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
    public class GeneratorsController : Controller
    {
        private readonly ICampaignManagerService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CampaignsController> _logger;

        public GeneratorsController(ICampaignManagerService service, UserManager<ApplicationUser> userManager, ILogger<CampaignsController> logger)
        {
            _service = service;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Generators/Create
        [HttpGet]
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to create a generator.";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Generators/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Script")] Generator generator)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to create a template.";
                    return RedirectToAction("Login", "Account");
                }

                generator.Id = Guid.NewGuid();
                generator.OwnerId = userId;
                var result = await _service.AddGenerator(generator);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Unable to create generator. Please try again.");
            }
            return View(generator);
        }

        // GET: Generators
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var (generators, totalCount) = await _service.GetPaginatedGenerators(page, pageSize);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(generators);
        }

        // GET: Generators/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generator = await _service.GetGeneratorById((Guid)id);
            if (generator == null)
            {
                return NotFound();
            }

            return View(generator);
        }



        // GET: Generators/Edit/5
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
                TempData["ErrorMessage"] = "You need to be logged in to edit this generator.";
                return RedirectToAction("Login", "Account");
            }

            var generator = await _service.GetGeneratorById((Guid)id);
            if (generator == null)
            {
                return NotFound();
            }

            if (generator.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this generator.";
                return RedirectToAction(nameof(Index));
            }

            return View(generator);
        }

        // POST: Generator/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, [Bind("Id,Name,Script,NextRunInGameDate")] Generator generator)
        {
            if (id == null || id != generator.Id)
            {
                return NotFound();
            }

            if (id != generator.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to edit this generator.";
                    return RedirectToAction("Login", "Account");
                }

                var existingGenerator = await _service.GetGeneratorById((Guid)id);
                if (existingGenerator == null)
                {
                    return NotFound();
                }

                if (existingGenerator.OwnerId != userId)
                {
                    TempData["ErrorMessage"] = "You do not have permission to edit this generator";
                    return RedirectToAction(nameof(Index));
                }

                //existingTemplate.Id = template.Id;
                existingGenerator.Name = generator.Name;
                existingGenerator.Script = generator.Script;

                var result = await _service.UpdateGenerator(existingGenerator);
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
            return View(generator);
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
                TempData["ErrorMessage"] = "You need to be logged in to delete this generator.";
                return RedirectToAction("Login", "Account");
            }

            var template = await _service.GetGeneratorById((Guid)id);
            if (template == null)
            {
                return NotFound();
            }

            if (template.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this generator.";
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
                TempData["ErrorMessage"] = "You need to be logged in to delete this generator.";
                return RedirectToAction("Login", "Account");
            }

            var generator = await _service.GetGeneratorById((Guid)id);
            if (generator == null)
            {
                return NotFound();
            }

            if (generator.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this generator.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _service.DeleteGeneratorById((Guid)id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Unable to delete generator. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}