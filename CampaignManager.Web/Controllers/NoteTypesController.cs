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
    public class NoteTypesController : Controller
    {
        private readonly ICampaignManagerService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CampaignsController> _logger;

        public NoteTypesController(ICampaignManagerService service, UserManager<ApplicationUser> userManager, ILogger<CampaignsController> logger)
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
                TempData["ErrorMessage"] = "You need to be logged in to create a note type.";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: NoteTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,PlayerTemplateId,GameMasterTemplateId")] NoteType noteType)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to create a note type.";
                return RedirectToAction("Login", "Account");
            }
            
            if (noteType.PlayerTemplateId == null)
            {
                ModelState.AddModelError("PlayerTemplateId", "Player Template ID is required.");
            }
            if(noteType.GameMasterTemplateId == null)
            {
                ModelState.AddModelError("GameMasterTemplateId", "Game Master Template ID is required.");
            }

            if (ModelState.IsValid)
            {
                if (!await IsValidTemplateId(noteType.PlayerTemplateId))
                {
                    ModelState.AddModelError("PlayerTemplateId", "Invalid Player Template ID.");
                }
                if (!await IsValidTemplateId(noteType.GameMasterTemplateId))
                {
                    ModelState.AddModelError("GameMasterTemplateId", "Invalid Game Master Template ID.");
                }

                noteType.Id = Guid.NewGuid();
                noteType.OwnerId = userId;
                var result = await _service.AddNoteType(noteType);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Unable to create note type. Please try again.");
            }
            return View(noteType);
        }

        // GET: NoteTypes
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var (noteTypes, totalCount) = await _service.GetPaginatedNoteTypes(page, pageSize);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(noteTypes);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to view this note type.";
                return RedirectToAction("Login", "Account");
            }

            var noteType = await _service.GetNoteTypeById((Guid)id);
            if (noteType == null)
            {
                return NotFound();
            }

            return View(noteType);
        }


        // GET: NoteTypes/Edit/5
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
                TempData["ErrorMessage"] = "You need to be logged in to edit this note type.";
                return RedirectToAction("Login", "Account");
            }

            var noteType = await _service.GetNoteTypeById((Guid)id);
            if (noteType == null)
            {
                return NotFound();
            }

            if (noteType.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this note type.";
                return RedirectToAction(nameof(Index));
            }

            return View(noteType);
        }

        // POST: NoteType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, [Bind("Id,Name,PlayerTemplateId,GameMasterTemplateId")] NoteType noteType)
        {
            if (id == null || id != noteType.Id)
            {
                return NotFound();
            }

            if (!await IsValidTemplateId(noteType.PlayerTemplateId))
            {
                ModelState.AddModelError("PlayerTemplateId", "Invalid Player Template ID.");
            }
            if (!await IsValidTemplateId(noteType.GameMasterTemplateId))
            {
                ModelState.AddModelError("GameMasterTemplateId", "Invalid Game Master Template ID.");
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to edit this note type.";
                    return RedirectToAction("Login", "Account");
                }

                var existingNoteType = await _service.GetNoteTypeById((Guid)id);
                if (existingNoteType == null)
                {
                    return NotFound();
                }

                if (existingNoteType.OwnerId != userId)
                {
                    TempData["ErrorMessage"] = "You do not have permission to edit this note type";
                    return RedirectToAction(nameof(Index));
                }

                existingNoteType.Name = noteType.Name;
                existingNoteType.PlayerTemplateId = noteType.PlayerTemplateId;
                existingNoteType.GameMasterTemplateId = noteType.GameMasterTemplateId;
                //existingNoteType.PlayerViewTemplateId = noteType.PlayerViewTemplateId;
                //existingNoteType.GameMasterViewTemplateId = noteType.GameMasterViewTemplateId;
                //existingNoteType.PlayerEditTemplateId = noteType.PlayerEditTemplateId;
                //existingNoteType.GameMasterEditTemplateId = noteType.GameMasterEditTemplateId;


                var result = await _service.UpdateNoteType(existingNoteType);
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
            return View(noteType);
        }

        // GET: NoteType/Delete/5
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
                TempData["ErrorMessage"] = "You need to be logged in to delete this note type.";
                return RedirectToAction("Login", "Account");
            }

            var noteType = await _service.GetNoteTypeById((Guid)id);
            if (noteType == null)
            {
                return NotFound();
            }

            if (noteType.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this not type.";
                return RedirectToAction(nameof(Index));
            }

            return View(noteType);
        }

        // POST: NoteType/Delete/5
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
                TempData["ErrorMessage"] = "You need to be logged in to delete this note type.";
                return RedirectToAction("Login", "Account");
            }

            var noteType = await _service.GetNoteTypeById((Guid)id);
            if (noteType == null)
            {
                return NotFound();
            }

            if (noteType.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this note type.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _service.DeleteNoteTypeById((Guid)id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Unable to delete note type. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: NoteTypes/AddGenerator/{noteTypeId}
        [HttpGet]
        public async Task<IActionResult> AddGenerator(Guid? noteTypeId)
        {
            if (noteTypeId == null)
            {
                return NotFound();
            }

            var noteType = await _service.GetNoteTypeById((Guid)noteTypeId);
            if (noteType == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to add generators to the note type";
                return RedirectToAction("Login", "Account");
            }

            if (noteType.OwnerId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to add generators to the note type";
                //return RedirectToAction("Details", "NoteTypes", new { id = noteType.CampaignId });
                return RedirectToAction(nameof(Index));
            }

            var model = new ManageNoteTypeGeneratorViewModel
            {
                NoteTypeId = noteTypeId.Value
            };

            return View(model);
        }

        // POST: Sessions/AddPlayer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGenerator(ManageNoteTypeGeneratorViewModel model)
        {
            if (model.NoteTypeId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to add generators to the note type";
                return RedirectToAction("Login", "Account");
            }

            
            if (!await IsValidGeneratorId(model.GeneratorId))
            {
                ModelState.AddModelError("GeneratorId", "Invalid Generator ID.");
                return View(model);
            }

            var generator = await _service.GetGeneratorById((Guid)model.GeneratorId);
            if (generator == null)
            {
                ModelState.AddModelError("", "Generator not found.");
                return View(model);
            }

            var noteType = await _service.GetNoteTypeById((Guid)model.NoteTypeId);
            if (noteType == null)
            {
                ModelState.AddModelError("", "NoteType not found.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (noteType.OwnerId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to add generators to the note type";
                return RedirectToAction("Details", new { id = generator.NoteTypeId });
            }

            //Check if the generator with ID exists


            if (noteType.Generators.Any(g => g.Id == generator.Id))
            {
                TempData["ErrorMessage"] = "Generator is already in the note type.";
                return RedirectToAction("Details", new { id = generator.NoteTypeId });
            }

            noteType.Generators.Add(generator);
            var result = await _service.UpdateNoteType(noteType);
            if (result)
            {
                TempData["SuccessMessage"] = "Generator added successfully.";
                return RedirectToAction(nameof(Details), new { id = noteType.Id });
            } else
            {

                TempData["ErrorMessage"] = "There was an error adding the generator to the note type.";
                return RedirectToAction(nameof(Details), new { id = noteType.Id });
            }
        }


        // GET: NoteTypes/RemoveGenerator/{noteTypeId}/{generatorId}
        [HttpGet]
        public async Task<IActionResult> RemoveGenerator(Guid? noteTypeId)
        {
            if (noteTypeId == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to remove a generator.";
                return RedirectToAction("Login", "Account");
            }

            var noteType = await _service.GetNoteTypeById((Guid)noteTypeId);
            if (noteType == null)
            {
                return NotFound();
            }

            if (noteType.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to remove a generator from this note type.";
                return RedirectToAction(nameof(Index));
            }

            var model = new ManageNoteTypeGeneratorViewModel
            {
                NoteTypeId = noteTypeId.Value
            };

            return View(model);
        }

        // POST: NoteTypes/RemoveGenerator
        [HttpPost, ActionName("RemoveGenerator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveGeneratorConfirmed(ManageNoteTypeGeneratorViewModel model)
        {
            if (!await IsValidGeneratorId(model.GeneratorId))
            {
                ModelState.AddModelError("GeneratorId", "Invalid Generator ID.");
                return View(model);
            }

            if (!ModelState.IsValid) {
                return View(model);
            }

            if (model.NoteTypeId == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to remove a generator.";
                return RedirectToAction("Login", "Account");
            }

            var noteType = await _service.GetNoteTypeById((Guid)model.NoteTypeId);
            if (noteType == null)
            {
                return NotFound();
            }

            if (noteType.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to remove a generator from this note type.";
                return RedirectToAction(nameof(Index));
            }

            if (!await IsValidGeneratorId(model.GeneratorId))
            {
                ModelState.AddModelError("GeneratorId", "Invalid Generator ID.");
                return View(model);
            }

            var generator = noteType.Generators.FirstOrDefault(g => g.Id == model.GeneratorId);
            if (generator == null)
            {
                TempData["ErrorMessage"] = "Generator not found in the note type.";
                return RedirectToAction(nameof(Details), new { id = model.NoteTypeId });
            }

            noteType.Generators.Remove(generator);
            var result = await _service.UpdateNoteType(noteType);
            if (result)
            {
                return RedirectToAction(nameof(Details), new { id = model.NoteTypeId });
            }

            TempData["ErrorMessage"] = "Unable to remove generator. Please try again.";
            return RedirectToAction(nameof(RemoveGenerator), model);
        }


        private async Task<bool> IsValidTemplateId(Guid? templateId)
        {
            if (templateId == null)
            {
                return false;
            }

            var template = await _service.GetTemplateById((Guid)templateId);
            return template != null;
        }

        private async Task<bool> IsValidGeneratorId(Guid? generatorId)
        {
            if (generatorId == null)
            {
                return false;
            }

            var generator = await _service.GetGeneratorById((Guid)generatorId);
            return generator != null;
        }
    }
}
