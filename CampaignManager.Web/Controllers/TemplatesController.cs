using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CampaignManager.Persistence.Models;
using CampaignManager.Persistence.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace CampaignManager.Web.Controllers
{
    public class TemplatesController : Controller
    {
        private readonly ICampaignManagerService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CampaignsController> _logger;

        public TemplatesController(ICampaignManagerService service, UserManager<ApplicationUser> userManager, ILogger<CampaignsController> logger)
        {
            _service = service;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Templates/Create
        [HttpGet]
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to create a template.";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Templates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content,Name,IsVerified")] Template template)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to create a template.";
                    return RedirectToAction("Login", "Account");
                }

                template.Id = Guid.NewGuid();
                template.OwnerId = userId;
                var result = await _service.AddTemplate(template);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Unable to create template. Please try again.");
            }
            return View(template);
        }

        // GET: Templates
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var (templates, totalCount) = await _service.GetPaginatedTemplates(page, pageSize);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(templates);
        }

        // GET: Templates/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _service.GetTemplateById(id.Value);
            if (template == null)
            {
                return NotFound();
            }

            return View(template);
        }

        // GET: Templates/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to edit this template.";
                return RedirectToAction("Login", "Account");
            }

            var template = await _service.GetTemplateById(id.Value);
            if (template == null)
            {
                return NotFound();
            }

            if (template.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this template.";
                return RedirectToAction(nameof(Index));
            }


            return View(template);
        }

        // POST: Templates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, [Bind("Id,Name,Content,IsVerified")] Template template)
        {
            if (id == null || id != template.Id)
            {
                return NotFound();
            }

            if(id != template.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to edit this template.";
                    return RedirectToAction("Login", "Account");
                }

                var existingTemplate = await _service.GetTemplateById((Guid)id);
                if (existingTemplate == null)
                {
                    return NotFound();
                }

                if (existingTemplate.OwnerId != userId)
                {
                    TempData["ErrorMessage"] = "You do not have permission to edit this template";
                    return RedirectToAction(nameof(Index));
                }

                //existingTemplate.Id = template.Id;
                existingTemplate.Content = template.Content;
                existingTemplate.Name = template.Name;
                existingTemplate.IsVerified = template.IsVerified;

                var result = await _service.UpdateTemplate(existingTemplate);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "There was an error during saving");
                }
                return RedirectToAction(nameof(Index));
            }
            return View(template);
        }

        // GET: Templates/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to delete this template.";
                return RedirectToAction("Login", "Account");
            }

            var template = await _service.GetTemplateById(id.Value);
            if (template == null)
            {
                return NotFound();
            }

            if (template.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this template.";
                return RedirectToAction(nameof(Index));
            }

            return View(template);
        }

        // POST: Templates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to delete this template.";
                return RedirectToAction("Login", "Account");
            }

            var template = await _service.GetTemplateById(id.Value);
            if (template == null)
            {
                return NotFound();
            }

            if (template.OwnerId != userId) {
                TempData["ErrorMessage"] = "You do not have permission to delete this template.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _service.DeleteTemplateById(id.Value);
            if (!result)
            {
                TempData["ErrorMessage"] = "Unable to delete template. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
