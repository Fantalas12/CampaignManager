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
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using CampaignManager.Web.Services;
using CampaignManager.DTO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json.Linq;

namespace CampaignManager.Web.Controllers
{
    public class NotesController : Controller
    {
        private readonly ICampaignManagerService _service;
        private readonly NotesHttpClientService _notesHttpClientService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CampaignsController> _logger;

        public NotesController(ICampaignManagerService service, NotesHttpClientService notesHttpClientService, UserManager<ApplicationUser> userManager, ILogger<CampaignsController> logger)
        {
            _service = service;
            _notesHttpClientService = notesHttpClientService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Notes/Create
        [HttpGet]
        public IActionResult Create(Guid sessionId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to create a note.";
                return RedirectToAction("Login", "Account");
            }

            NoteViewModel model = new NoteViewModel
            {
                SessionId = sessionId
            };

            return View(model);

        }

        // POST: Notes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,SessionId,InGameDate,NoteTypeId")] NoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to create a note.";
                    return RedirectToAction("Login", "Account");
                }

                if (!await IsValidNoteTypeId(model.NoteTypeId))
                {
                    ModelState.AddModelError("NoteTypeId", "Invalid Note Type ID.");
                    return View(model);
                }

                var note = new Note
                {
                    Id = Guid.NewGuid(),
                    OwnerId = userId,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    SessionId = model.SessionId,
                    Title = model.Title,
                    Content = model.Content,
                    InGameDate = model.InGameDate,
                    NoteTypeId = model.NoteTypeId
                };
                
                if (note.NoteTypeId == null)
                {
                    return NotFound();
                }

                var noteType = await _service.GetNoteTypeById((Guid)note.NoteTypeId);
                if (noteType != null)
                {
                    foreach (var generator in noteType.Generators)
                    {
                        var noteGenerator = new NoteGenerator
                        {
                            NoteId = note.Id,
                            GeneratorId = generator.Id
                        };
                        note.NoteGenerators.Add(noteGenerator);
                    }
                }

                var sessionId = note.SessionId;
                if (sessionId == null)
                {
                    TempData["ErrorMessage"] = "Note does not belong to a session";
                    return RedirectToAction("Index", "Home");
                }

                var result = await _service.AddNote(note);
                //var result = await _notesHttpClientService.CreateNoteAsync((NoteDTO)note);
                if (result)
                {
                    return RedirectToAction("Details", "Sessions", new { id = sessionId });
                }
                ModelState.AddModelError("", "Unable to create note. Please try again.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid? id, int fromPage = 1, int toPage = 1, int pageSize = 10)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to view this note";
                return RedirectToAction("Login", "Account");
            }

            var note = await _service.GetNoteById((Guid)id);
            if (note == null)
            //var noteDTO = await _notesHttpClientService.GetNoteByIdAsync((Guid)id);
            //if (noteDTO == null)
            {
                _logger.LogWarning("No note found with id: {Id}", id);
                TempData["ErrorMessage"] = "Note does not belong to a session";
                return RedirectToAction("Index", "Home");
            }

            //var note = (Note)noteDTO;

            var sessionId = note.SessionId;
            if (sessionId == null)
            {
                TempData["ErrorMessage"] = "Note does not belong to a session";
                return RedirectToAction("Index", "Home");
            }

            var session = await _service.GetSessionById((Guid)sessionId);
            if (session == null)
            {
                TempData["ErrorMessage"] = "Session not found";
                return RedirectToAction("Index", "Home");
            }

            var campaignId = session.CampaignId;

            if (!await _service.IsUserParticipant(campaignId, userId))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this note";
                return RedirectToAction("Index", "Home");
            }

            (List<NoteLink> noteLinks, int totalCount) = await _service.GetPaginatedToNoteLinksForNote((Guid)id, toPage, pageSize);

            var viewModel = new NoteDetailsViewModel
            {
                Note = note,
                ToNotes = noteLinks,
                ToTotalCount = totalCount,
                ToPage = toPage,
                PageSize = pageSize
            };

            //log the Note's NoteType and Template and GameMaster Template
            _logger.LogInformation("Note {NoteId} has NoteType {NoteType} and Template {Template}", note.Id, note.NoteType?.Name, note.NoteType?.PlayerTemplate?.Name);
            //The GameMAterTemplate as well
            _logger.LogInformation("Note {NoteId} has NoteType {NoteType} and Template {Template}", note.Id, note.NoteType?.Name, note.NoteType?.GameMasterTemplate?.Name);





            var isGameMaster = note.Session?.GameMasterId == userId;
            var templateContent = isGameMaster ? note.NoteType?.GameMasterTemplate?.Content : note.NoteType?.PlayerTemplate?.Content;

            if (templateContent == null)
            {
                TempData["ErrorMessage"] = "Template content not found! Please set the template content manualy or make sure to use templates which has non null or empty content!";
                return RedirectToAction("Details", "Sessions", new { id = sessionId });
            }

            var renderedContent = RenderNoteContent(note.Content, templateContent);
            ViewData["RenderedContent"] = renderedContent;

            return View(viewModel);
        }


        // GET: Notes/Edit/5
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
                TempData["ErrorMessage"] = "You need to be logged in to edit this note";
                return RedirectToAction("Login", "Account");
            }

            var note = await _service.GetNoteById((Guid)id);
            if (note == null)
            //var noteDTO = await _notesHttpClientService.GetNoteByIdAsync((Guid)id);
            //if (noteDTO == null)
            {
                return NotFound();
            }

            //var note = (Note)noteDTO;

            if (note.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit this note";
                return RedirectToAction("Index", "Home");
            }

            return View(note);
        }

        // POST: Notes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, [Bind("Id,Title,Content,InGameDate","SessionId")] Note note)
        {
            if (id == null || id != note.Id)
            {
                return NotFound();
            }

            if (id != note.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to edit this note";
                    return RedirectToAction("Login", "Account");
                }

                var existingNote = await _service.GetNoteById((Guid)id);
                if (existingNote == null)
                {
                    return NotFound();
                }

                if (existingNote.OwnerId != userId)
                {
                    TempData["ErrorMessage"] = "You do not have permission to edit this note";
                    return RedirectToAction(nameof(Index));
                }

                existingNote.Title = note.Title;
                existingNote.Content = note.Content;
                existingNote.InGameDate = note.InGameDate;
                existingNote.ModifiedDate = DateTime.Now;


                var result = await _service.UpdateNote(existingNote);
                //var result = await _notesHttpClientService.UpdateNoteAsync((Guid)id, (NoteDTO)existingNote);
                if (result)
                {
                    TempData["SuccessMessage"] = "Note updated successfully.";
                    return RedirectToAction("Details", "Sessions", new { id = existingNote.SessionId });
                }
                else
                {
                    ModelState.AddModelError("", "There was an error during saving");
                }
            }
            return View(note);
        }

        // GET: Notes/Delete/5
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
                TempData["ErrorMessage"] = "You need to be logged in to delete this note";
                return RedirectToAction("Login", "Account");
            }

            var note = await _service.GetNoteById((Guid)id);
            if(note == null)
            //var noteDTO = await _notesHttpClientService.GetNoteByIdAsync((Guid)id);
            //if (noteDTO == null)
            {
                return NotFound();
            }

            //var note = (Note)noteDTO;

            if (note.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this note";
                return RedirectToAction(nameof(Index));
            }

            return View(note);
        }

        // POST: Notes/Delete/5
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
                TempData["ErrorMessage"] = "You need to be logged in to delete this note";
                return RedirectToAction("Login", "Account");
            }

            var note = await _service.GetNoteById((Guid)id);
            if (note == null)
            {
                return NotFound();
            }

            if (note.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this note";
                return RedirectToAction(nameof(Index));
            }

            var result = await _service.DeleteNoteById((Guid)id);
            //var result = await _notesHttpClientService.DeleteNoteByIdAsync((Guid)id);
            if (!result)
            {
                TempData["ErrorMessage"] = "Unable to delete note. Please try again.";
                return RedirectToAction("Details", "Sessions", new { id = note.SessionId });
            }

            TempData["SuccessMessage"] = "Note deleted successfully.";
            return RedirectToAction("Details", "Sessions", new { id = note.SessionId });
        }

        // GET: NoteTypes/AddGenerator/{noteId}
        [HttpGet]
        public async Task<IActionResult> AddGenerator(Guid? noteId)
        {
            if (noteId == null)
            {
                return NotFound();
            }

            var note = await _service.GetNoteById((Guid)noteId);
            if (note == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to add generators to the note";
                return RedirectToAction("Login", "Account");
            }

            if (note.OwnerId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to add generators to the note";
                return RedirectToAction(nameof(Index));
            }

            var model = new ManageNoteGeneratorViewModel
            {
                Id = note.Id,
                NoteId = noteId
            };

            return View(model);
        }

        // POST: Sessions/AddPlayer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGenerator([Bind("Id,NoteId,GeneratorId,NextRunInGameDate")] ManageNoteGeneratorViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.NoteId == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to add generators to the note";
                return RedirectToAction("Login", "Account");
            }

            if (!await IsValidGeneratorId(model.GeneratorId))
            {
                ModelState.AddModelError("GeneratorId", "Invalid Generator ID");
                return View(model);
            }

            var generator = await _service.GetGeneratorById((Guid)model.GeneratorId);
            if (generator == null)
            {
                ModelState.AddModelError("", "Generator not found");
                return View(model);
            }

            var note = await _service.GetNoteById((Guid)model.NoteId);
            if (note == null)
            {
                ModelState.AddModelError("", "Note not found");
                return View(model);
            }

            if (note.OwnerId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to add generators to the note";
                return RedirectToAction("Details", new { id = generator.NoteTypeId });
            }

            if (note.NoteGenerators.Any(g => g.GeneratorId == generator.Id))
            {
                TempData["ErrorMessage"] = "Generator is already in the note";
                return RedirectToAction("Details", new { id = model.NoteId });
            }

            //Create new NoteGenerator with the note and generator IDs
            var noteGenerator = new NoteGenerator
            {
                Id = model.Id,
                NoteId = model.NoteId,
                GeneratorId = model.GeneratorId,
                NextRunInGameDate = model.NextRunInGameDate
            };

            var result = await _service.AddNoteGenerator(noteGenerator);
            if (result)
            {
                TempData["SuccessMessage"] = "Generator added successfully.";
                return RedirectToAction(nameof(Details), new { id = note.Id });
            }
            else
            {

                TempData["ErrorMessage"] = "There was an error adding the generator to the note";
                return RedirectToAction(nameof(Details), new { id = note.Id });
            }
        }


        // GET: Notes/RemoveGenerator/{noteId}/
        [HttpGet]
        public async Task<IActionResult> RemoveGenerator(Guid? noteId)
        {
            if (noteId == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to remove a generator.";
                return RedirectToAction("Login", "Account");
            }

            var note = await _service.GetNoteById((Guid)noteId);
            if (note == null)
            {
                return NotFound();
            }

            if (note.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to remove a generator from this note";
                return RedirectToAction(nameof(Index));
            }

            var model = new ManageNoteGeneratorViewModel
            {
                Id = new Guid(),
                NoteId = noteId
            };

            return View(model);
        }

        // POST: NoteTypes/RemoveGenerator
        [HttpPost, ActionName("RemoveGenerator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveGeneratorConfirmed(ManageNoteGeneratorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.NoteId == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to remove a generator";
                return RedirectToAction("Login", "Account");
            }

            var note = await _service.GetNoteById((Guid)model.NoteId);
            if (note == null)
            {
                return NotFound();
            }

            if (note.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have permission to remove a generator from this note";
                return RedirectToAction(nameof(Index));
            }

            if (!await IsValidGeneratorId(model.GeneratorId))
            {
                ModelState.AddModelError("GeneratorId", "Invalid Generator ID.");
                return View(model);
            }

            var generator = note.NoteGenerators.FirstOrDefault(g => g.GeneratorId == model.GeneratorId);
            if (generator == null)
            {
                TempData["ErrorMessage"] = "Generator not found in the note type";
                return RedirectToAction(nameof(Details), new { id = model.NoteId });
            }

            note.NoteGenerators.Remove(generator);
            var result = await _service.UpdateNote(note);
            if (result)
            {
                return RedirectToAction(nameof(Details), new { id = model.NoteId });
            }

            TempData["ErrorMessage"] = "Unable to remove generator. Please try again.";
            return RedirectToAction(nameof(RemoveGenerator), model);
        }


        [HttpGet]
        public async Task<IActionResult> SetGeneratorNextRunDate(Guid? noteGeneratorId)
        {
            if (noteGeneratorId == null)
            {
                TempData["ErrorMessage"] = "Invalid generator ID";
                return RedirectToAction("Details");
            }

            var noteGenerator = await _service.GetNoteGeneratorById(noteGeneratorId.Value);
            if (noteGenerator == null)
            {
                TempData["ErrorMessage"] = "Generator not found";
                return RedirectToAction("Details");
            }

            return View(noteGenerator);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetGeneratorNextRunDate([Bind("Id,NoteId,GeneratorId,NextRunInGameDate")] NoteGenerator model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input data.";
                return View(model);
            }

            var noteGenerator = await _service.GetNoteGeneratorById(model.Id);
            if (noteGenerator == null)
            {
                TempData["ErrorMessage"] = "Generator not found.";
                return RedirectToAction("Details", new { id = model.NoteId });
            }

            noteGenerator.NextRunInGameDate = model.NextRunInGameDate;
            await _service.UpdateNoteGenerator(noteGenerator);

            TempData["SuccessMessage"] = "Generator run date updated successfully.";
            return RedirectToAction("Details", new { id = model.NoteId });
        }

        [HttpGet]
        public async Task<IActionResult> LinkNote(Guid? fromNoteId)
        {
            if (fromNoteId == null) {
                TempData["ErrorMessage"] = "Invalid note ID";
                return RedirectToAction("Details");
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to link a note";
                return RedirectToAction("Login", "Account");
            }

            var note = await _service.GetNoteById((Guid)fromNoteId);
            if (note == null)
            {
                TempData["ErrorMessage"] = "Note not found";
                return RedirectToAction("Details");
            }

            if (userId != note.OwnerId)
            {
                TempData["ErrorMessage"] = "You do not have permission to link notes";
                return RedirectToAction("Details");
            }

            if (fromNoteId == null)
            {
                TempData["ErrorMessage"] = "Invalid note ID";
                return RedirectToAction("Details");
            }

            var fromNote = await _service.GetNoteById(fromNoteId.Value);

            if (fromNote == null)
            {
                TempData["ErrorMessage"] = "Note not found";
                return RedirectToAction("Details");
            }

            var noteLink = new NoteLink
            {
                Id = Guid.NewGuid(),
                FromNoteId = fromNoteId.Value
            };

            return View(noteLink);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkNote([Bind("Id,FromNoteId,ToNoteId,LinkType")] NoteLink noteLink)
        {
            if (noteLink == null)
            {
                TempData["ErrorMessage"] = "NoteLink cannot be null.";
                return RedirectToAction("Index", "Home");
            }

            if (noteLink.FromNoteId == Guid.Empty || noteLink.ToNoteId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Note id cannot be empty and must be in a correct note id format";
                return RedirectToAction("Details", "Notes", new { id = noteLink.FromNoteId });

            }

            if (!ModelState.IsValid)
            {
                return View(noteLink);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to link a note";
                return RedirectToAction("Login", "Account");
            }

            var fromNote = await _service.GetNoteById((Guid)noteLink.FromNoteId);
            if (fromNote == null)
            {
                TempData["ErrorMessage"] = "Note not found";
                return RedirectToAction("Details");
            }

            if (userId != fromNote.OwnerId)
            {
                TempData["ErrorMessage"] = "You do not have permission to link notes";
                return RedirectToAction("Details", new { id = noteLink.FromNoteId });
            }

            if (!await IsValidNoteId(noteLink.ToNoteId))
            {
                ModelState.AddModelError("ToNoteId", "Invalid note ID");
                return View(noteLink);
            }

            var toNote = await _service.GetNoteById((Guid)noteLink.ToNoteId);
            if (toNote == null)
            {
                ModelState.AddModelError("", "Note to link not found");
                return View(noteLink);
            }

            /*
            //Ther can only be one link between two notes with the same link type
            var existingLink = await _service.GetNoteLinkByFromAndToNoteIds(noteLink.FromNoteId, noteLink.ToNoteId);
            if (existingLink == null)
            {
                var result = await _service.AddNoteLink(noteLink);
                if (result)
                {
                    TempData["SuccessMessage"] = "Notes linked successfully";
                    return RedirectToAction("Details", new { id = noteLink.FromNoteId });
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Notes are already linked";
                return View(noteLink);
            } */


            var fromNoteSession = fromNote.Session;
            var toNoteSession = toNote.Session;
            if(fromNoteSession != null && toNoteSession != null && fromNoteSession.CampaignId != toNoteSession.CampaignId)
            {
                TempData["ErrorMessage"] = "Notes are not in the same campaign";
                return RedirectToAction("Details", new { id = noteLink.FromNoteId });
            }

            if (string.IsNullOrEmpty(noteLink.LinkType))
            {
                noteLink.LinkType = "default";
            }

            _logger.LogInformation("Linking note {FromNoteId} to note {ToNoteId} with link type {LinkType}", noteLink.FromNoteId, noteLink.ToNoteId, noteLink.LinkType);

            var result = await _service.AddNoteLink(noteLink);
            if (result)
            {
                TempData["SuccessMessage"] = "Notes linked successfully";
                return RedirectToAction("Details", new { id = noteLink.FromNoteId });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to link notes";
                return View(noteLink);
            }
        }

        [HttpPost, ActionName("UnlinkNote")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlinkNoteConfirmed(Guid? fromNoteId, Guid? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid note link ID";
                return RedirectToAction("Index", "Home");
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to unlink a note";
                return RedirectToAction("Login", "Account");
            }

            var noteLink = await _service.GetNoteLinkById(id.Value);
            if (noteLink == null)
            {
                TempData["ErrorMessage"] = "Note link not found";
                return RedirectToAction("Index", "Home");
            }

            var fromNote = await _service.GetNoteById(noteLink.FromNoteId);
            if (fromNote == null)
            {
                TempData["ErrorMessage"] = "Note not found";
                return RedirectToAction("Index", "Home");
            }

            if (userId != fromNote.OwnerId)
            {
                TempData["ErrorMessage"] = "You do not have permission to unlink notes";
                return RedirectToAction("Details", "Notes", new { id = noteLink.FromNoteId });
            }

            var result = await _service.DeleteNoteLinkById((Guid)id);
            if (result)
            {
                TempData["SuccessMessage"] = "Note unlinked successfully";
                return RedirectToAction("Details", new { id = noteLink.FromNoteId });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to unlink notes";
                return RedirectToAction("Details", new { id = noteLink.FromNoteId });
            }
        }


        private async Task<bool> IsValidTemplateId(Guid? templateId)
        {
            if (templateId == null)
            {
                //_logger.LogError("Template ID is null.");
                return false;
            }

            var template = await _service.GetTemplateById((Guid)templateId);
            if (template == null)
            {
                _logger.LogError($"Template not found for ID: {templateId}");
            }
            return template != null;
        }

        private async Task<bool> IsValidGeneratorId(Guid? generatorId)
        {
            if (generatorId == null)
            {
                return false;
            }

            var generator = await _service.GetGeneratorById((Guid)generatorId);
            if (generator == null)
            {
                _logger.LogError($"Generator not found for ID: {generatorId}");
            } 
            return generator != null;
        }

        private async Task<bool> IsValidNoteTypeId(Guid? noteTypeId)
        {
            if (noteTypeId == null)
            {
                return false;
            }

            var noteType = await _service.GetNoteTypeById((Guid)noteTypeId);
            return noteType != null;
        }

        private async Task<bool> IsValidNoteId(Guid? noteId)
        {
            if (noteId == null)
            {
                return false;
            }

            var note = await _service.GetNoteById((Guid)noteId);
            return note != null;
        }

        private string RenderNoteContent(string? noteContent, string templateContent)
        {
            if (string.IsNullOrEmpty(noteContent))
            {
                return templateContent;
            }

            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(noteContent);
                string pattern = @"<---(.*?)--->";
                string result = Regex.Replace(templateContent, pattern, match =>
                {
                    string key = match.Groups[1].Value.Trim();
                    if (data != null)
                    {
                        // Check if the key contains an array index
                        var arrayMatch = Regex.Match(key, @"(\w+)\[(\d+)\](\.\w+)?");
                        if (arrayMatch.Success)
                        {
                            string arrayKey = arrayMatch.Groups[1].Value;
                            int index = int.Parse(arrayMatch.Groups[2].Value);
                            string? property = arrayMatch.Groups[3].Value.TrimStart('.');

                            if (data.ContainsKey(arrayKey) && data[arrayKey] is JArray array && array.Count > index)
                            {
                                var arrayItem = array[index];
                                if (property != null && arrayItem is JObject jObject && jObject.TryGetValue(property, out var jValue))
                                {
                                    return jValue.ToString();
                                }
                                return arrayItem.ToString();
                            }
                        }
                        else
                        {
                            return GetValueFromJson(data, key) ?? match.Value;
                        }
                    }
                    return match.Value;
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid JSON content: {NoteContent}", noteContent);
                return $"Error rendering content: Invalid JSON format. Raw content: {noteContent}";
            }
        }

        private string? GetValueFromJson(Dictionary<string, object> data, string key)
        {
            var keys = key.Split('.');
            object? current = data;

            foreach (var k in keys)
            {
                if (current is Dictionary<string, object> dict && dict.TryGetValue(k, out var value))
                {
                    current = value;
                }
                else if (current is JObject jObject && jObject.TryGetValue(k, out var jValue))
                {
                    current = jValue;
                }
                else if (current is JArray jArray)
                {
                    var arrayKey = k.Trim('[', ']');
                    if (int.TryParse(arrayKey, out int index) && index < jArray.Count)
                    {
                        current = jArray[index];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return current?.ToString();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunScript(Guid? noteId, Guid? noteGeneratorId)
        {

            if (noteId == null || noteGeneratorId == null)
            {
                TempData["ErrorMessage"] = "Invalid note or note generator ID";
                return RedirectToAction("Index", "Home");
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to run a generator.";
                return RedirectToAction("Login", "Account");
            }

            var note = await _service.GetNoteById((Guid)noteId);
            if (note == null)
            {
                TempData["ErrorMessage"] = "Note not found";
                return RedirectToAction("Index", "Home");
            }
            var noteGenerator = await _service.GetNoteGeneratorById((Guid)noteGeneratorId);
            if (noteGenerator == null)
            {
                TempData["ErrorMessage"] = "Note generator not found";
                return RedirectToAction("Details", new { id = noteId });
            }


            if(userId != note.OwnerId)
            {
                TempData["ErrorMessage"] = "You do not have permission to run the note's generators";
                return RedirectToAction("Details", new { id = noteId });
            }
            

            if (string.IsNullOrWhiteSpace(noteGenerator.Generator?.Script))
            {
                TempData["ErrorMessage"] = "Note generator script is empty!";
                return RedirectToAction("Details", new { id = noteId });
            }

            var scriptOptions = ScriptOptions.Default
                .AddReferences(
                    typeof(object).Assembly,
                    typeof(Enumerable).Assembly,
                    typeof(JsonSerializer).Assembly,
                    typeof(Random).Assembly,
                    typeof(List<>).Assembly,
                    typeof(Array).Assembly,
                    typeof(String).Assembly,
                    typeof(ILogger).Assembly,
                    typeof(Note).Assembly,
                    typeof(DateTime).Assembly,
                    AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "System.Runtime")
                )
                .AddImports(
                    "System",
                    "System.Linq",
                    "System.Collections.Generic",
                    "System.Text.Json",
                    "System.Text.Json.Nodes",
                    "Microsoft.Extensions.Logging",
                    "CampaignManager.Persistence.Models"
                );

            var globals = new ScriptGlobals { Note = note, Logger = _logger };

            try
            {
                _logger.LogInformation($"Script content: {noteGenerator.Generator.Script}");
                var result = await CSharpScript.EvaluateAsync<string>(noteGenerator.Generator.Script, scriptOptions, globals);
                _logger.LogInformation($"Script result for NoteGenerator {noteGenerator.Id}: {result}");

                if (result != null)
                {
                    note.Content = result;
                    var updateResult = await _service.UpdateNote(note);
                    if (updateResult)
                    {
                        TempData["SuccessMessage"] = "Generator ran successfully!";
                        return RedirectToAction("Details", new { id = noteId });
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Generator failed to run!";
                        return RedirectToAction("Details", new { id = noteId });
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Script returned null result!";
                    return RedirectToAction("Details", new { id = noteId });
                }
            }
            catch (CompilationErrorException ex)
            {
                // Log compilation errors
                _logger.LogError(ex, $"Script compilation errors for NoteGenerator {noteGeneratorId}: {string.Join(Environment.NewLine, ex.Diagnostics)}!");
                TempData["ErrorMessage"] = $"Error compiling script: {string.Join(Environment.NewLine, ex.Diagnostics.Select(d => d.ToString()))}";
                return RedirectToAction("Details", new { id = noteId });
            }
            catch (Exception ex)
            {
                // Log runtime errors
                _logger.LogError(ex, $"Error executing script for NoteGenerator {noteGeneratorId}");
                TempData["ErrorMessage"] = $"Error executing script: {ex.Message}";
                return RedirectToAction("Details", new { id = noteId });
            }
        }
    }

        public class ScriptGlobals
        {
        public Note Note { get; set; } = null!;
        public ILogger Logger { get; set; } = null!;
        }
}