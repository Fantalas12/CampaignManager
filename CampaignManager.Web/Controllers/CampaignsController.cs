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
using CampaignManager.DTO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Text.Json;

namespace CampaignManager.Web.Controllers
{
    public enum SortOrder { NAME_DESC, NAME_ASC, CREATED_DESC, CREATED_ASC, EDITED_DECS, EDITED_ASC }

    [Authorize] //TODO - Add custom policy and roles
    public class CampaignsController : Controller
    {
        private readonly ICampaignManagerService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CampaignsController> _logger;

        [TempData]
        public string[] SuccessNotifications { get; set; } = Array.Empty<string>();

        [TempData]
        public string[] ErrorNotifications { get; set; } = Array.Empty<string>();


        public CampaignsController(ICampaignManagerService service, UserManager<ApplicationUser> userManager, ILogger<CampaignsController> logger)
        {
            _service = service;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult?> DisplayImage(int id)
        {
            var campaign = await _service.GetCampaignById(id);
            if (campaign != null && campaign.Image != null)
            {
                return File(campaign.Image, "image/png");
            }
            return null;
        }

        #region Create Methods

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,GameTime")] Campaign campaign, IFormFile? image)
        {
            if (image != null && image.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    image.CopyTo(stream);
                    campaign.Image = stream.ToArray();
                }
            }

            if (!ModelState.IsValid)
            {
                return View(campaign);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to create a campaign.";
                return RedirectToAction("Login", "Account");
            }

            bool existWithName = await _service.IsReservedCampaignNameForUser(campaign.Name, userId);
            if (existWithName)
            {
                ModelState.AddModelError("Name", "Campaign with this name already exists.");
                return View(campaign);
            }

            campaign.OwnerId = userId;
            campaign.Created = DateTime.Now;
            campaign.Edited = DateTime.Now;

            // Use transactions to ensure that both the campaign and the game master are added
            using (var transaction = await _service.BeginTransactionAsync())
            {
                try
                {
                    bool campaignAdded = await _service.AddCampaign(campaign);
                    if (campaignAdded)
                    {
                        // Add the owner as a game master
                        var GM = new CampaignParticipant
                        {
                            ApplicationUserId = userId,
                            CampaignId = campaign.Id, // Ensure CampaignId is set correctly
                            Role = Role.GameMaster, // Use the correct role
                        };

                        bool participantAdded = await _service.AddCampaignParticipant(GM);
                        if (participantAdded)
                        {
                            await transaction.CommitAsync();
                            return RedirectToAction(nameof(IndexOwned));
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                            ModelState.AddModelError("", "There was an error during saving the participant");
                        }
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "There was an error during saving the campaign");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Exception during transaction");
                    ModelState.AddModelError("", "There was an error during saving");
                }
            }


            return View(campaign);


        }

        [HttpGet]
        public IActionResult LoadCampaign()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoadCampaign(IFormFile jsonFile)
        {
            if (jsonFile == null || jsonFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid JSON file.";
                return View();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to load a campaign.";
                return RedirectToAction("Login", "Account");
            }

            string filePath = Path.Combine(Path.GetTempPath(), jsonFile.FileName);


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await jsonFile.CopyToAsync(stream);
            }

            var campaign = await _service.LoadCampaignFromFile(filePath);
            if (campaign == null)
            {
                TempData["ErrorMessage"] = "Failed to load campaign from the JSON file.";
                return View();
            }

            bool existWithName = await _service.IsReservedCampaignNameForUser(campaign.Name, userId);
            if (existWithName)
            {
                TempData["ErrorMessage"] = "Campaign with this name already exists.";
                return View();
            }

            using (var transaction = await _service.BeginTransactionAsync())
            {
                try
                {
                    campaign.OwnerId = userId;

                    bool campaignAdded = await _service.AddCampaign(campaign);
                    if (!campaignAdded)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Failed to add the loaded campaign.";
                        return View();
                    }

                    var campaignParticipant = new CampaignParticipant
                    {
                        ApplicationUserId = userId,
                        CampaignId = campaign.Id,
                        Role = Role.GameMaster
                    };

                    bool participantAdded = await _service.AddCampaignParticipant(campaignParticipant);
                    if (!participantAdded)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Failed to add the loaded campaign.";
                        return View();
                    }

                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = "Campaign loaded successfully.";
                    return RedirectToAction(nameof(IndexOwned));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding campaign with ownership");
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Failed to add the loaded campaign.";
                    return View();
                }
            }
        }

        /*
        [HttpGet]
        public async Task<IActionResult> AddNoteAdmin(int? campaignId)
        {
            if (campaignId == null)
            {
                return NotFound();
            }

            var campaign = await _service.GetCampaignById(campaignId.Value);
            if (campaign == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to add note admins to this campaign.";
                return RedirectToAction("Login", "Account");
            }

            if (campaign.OwnerId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to add note admins to this campaign.";
                return RedirectToAction("Details", new { id = campaignId });
            }

            var participants = await _service.GetParticipantsForCampaign((int)campaignId);
            var noteAdmins = await _service.GetNoteAdminsForCampaign((int)campaignId);

            var filteredParticipants = participants
                .Where(p => !noteAdmins.Any(na => na.ApplicationUserId == p.ApplicationUserId))
                .ToList();

            var model = new ManageNoteAdminViewModel
            {
                CampaignId = (int)campaignId,
                Participants = new SelectList(filteredParticipants, "ApplicationUserId", "ApplicationUser.UserName")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNoteAdmin(ManageNoteAdminViewModel model)
        {

            var campaign = await _service.GetCampaignById(model.CampaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to add note admins to this campaign.";
                return RedirectToAction("Login", "Account");
            }

            if (campaign.OwnerId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to add note admins to this campaign.";
                return RedirectToAction("Details", new { id = model.CampaignId });
            }

            var existingNoteAdmin = await _service.GetNoteAdminByUserId(model.SelectedParticipantId, model.CampaignId);
            if (existingNoteAdmin != null)
            {
                ModelState.AddModelError("", "This user is already a NoteAdmin.");
                var participants = await _service.GetParticipantsForCampaign(model.CampaignId);
                var noteAdmins = await _service.GetNoteAdminsForCampaign(model.CampaignId);
                model.Participants = new SelectList(participants.Where(p => !noteAdmins.Any(na => na.ApplicationUserId == p.ApplicationUserId)), "ApplicationUserId", "ApplicationUser.UserName");
                return View(model);
            }

            var noteAdmin = new NoteAdmin
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = model.SelectedParticipantId,
                CampaignId = model.CampaignId
            };

            var result = await _service.AddNoteAdmin(noteAdmin);
            if (result)
            {
                TempData["SuccessMessage"] = "Note Admin added successfully.";
                return RedirectToAction(nameof(Details), new { id = model.CampaignId });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add Note Admin.";
                return RedirectToAction(nameof(Details), new { id = model.CampaignId });
            }
        }
        */



        #endregion

        #region Read Methods

        public async Task<IActionResult> Index(SortOrder sortOrder = SortOrder.NAME_ASC)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to view your campaigns.";
                return RedirectToAction("Login", "Account");
            }

            ViewData["NameSortParam"] = sortOrder == SortOrder.NAME_DESC ? SortOrder.NAME_ASC : SortOrder.NAME_DESC;
            ViewData["CreatedSortParam"] = sortOrder == SortOrder.CREATED_DESC ? SortOrder.CREATED_ASC : SortOrder.CREATED_DESC;
            ViewData["EditedSortParam"] = sortOrder == SortOrder.EDITED_DECS ? SortOrder.EDITED_ASC : SortOrder.EDITED_DECS;

            var campaigns = await _service.GetCampaignsForUserById(user.Id);

            switch (sortOrder)
            {
                case SortOrder.NAME_DESC:
                    campaigns = campaigns.OrderByDescending(c => c.Name).ToList();
                    break;
                case SortOrder.NAME_ASC:
                    campaigns = campaigns.OrderBy(c => c.Name).ToList();
                    break;
                case SortOrder.CREATED_DESC:
                    campaigns = campaigns.OrderByDescending(c => c.Created).ToList();
                    break;
                case SortOrder.CREATED_ASC:
                    campaigns = campaigns.OrderBy(c => c.Created).ToList();
                    break;
                case SortOrder.EDITED_DECS:
                    campaigns = campaigns.OrderByDescending(c => c.Edited).ToList();
                    break;
                case SortOrder.EDITED_ASC:
                    campaigns = campaigns.OrderBy(c => c.Edited).ToList();
                    break;
            }

            var gameMasterCampaignIds = new List<int>();

            foreach (var campaign in campaigns)
            {
                if (await IsUserGameMaster(campaign.Id, user.Id))
                {
                    gameMasterCampaignIds.Add(campaign.Id);
                }
            }

            ViewBag.GameMasterCampaignIds = gameMasterCampaignIds;

            return View(campaigns);
        }

        public async Task<IActionResult> IndexOwned(SortOrder sortOrder = SortOrder.NAME_ASC)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to view your campaigns.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                ViewData["NameSortParam"] = sortOrder == SortOrder.NAME_DESC ? SortOrder.NAME_ASC : SortOrder.NAME_DESC;
                ViewData["CreatedSortParam"] = sortOrder == SortOrder.CREATED_DESC ? SortOrder.CREATED_ASC : SortOrder.CREATED_DESC;
                ViewData["EditedSortParam"] = sortOrder == SortOrder.EDITED_DECS ? SortOrder.EDITED_ASC : SortOrder.EDITED_DECS;


                List<Campaign> campaigns = await _service.GetOwnedCampaignsForUserById(userId);

                switch (sortOrder)
                {
                    case SortOrder.NAME_DESC:
                        campaigns = campaigns.OrderByDescending(c => c.Name).ToList();
                        break;
                    case SortOrder.NAME_ASC:
                        campaigns = campaigns.OrderBy(c => c.Name).ToList();
                        break;
                    case SortOrder.CREATED_DESC:
                        campaigns = campaigns.OrderByDescending(c => c.Created).ToList();
                        break;
                    case SortOrder.CREATED_ASC:
                        campaigns = campaigns.OrderBy(c => c.Created).ToList();
                        break;
                    case SortOrder.EDITED_DECS:
                        campaigns = campaigns.OrderByDescending(c => c.Edited).ToList();
                        break;
                    case SortOrder.EDITED_ASC:
                        campaigns = campaigns.OrderBy(c => c.Edited).ToList();
                        break;
                }

                return View(campaigns);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

        }

        //The details are private so only relevant users can see the campaign details
        public async Task<IActionResult> Details(int? id, int page = 1, int pageSize = 5)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campaign = await _service.GetCampaignById((int)id);
            if (campaign == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to view the details of this campaign.";
                return RedirectToAction("Login", "Account");
            }

            //Check if the user is a player or a game master or owner in the campaign
            var userCampaigns = await _service.GetCampaignsForUserById(userId);
            if (!userCampaigns.Any(c => c.Id == id))
            {
                TempData["ErrorMessage"] = "You do not have permission to view the details of this campaign.";
                return RedirectToAction("Index", "Home");
            }

            var (sessions, totalSessions) = await _service.GetPaginatedSessionsForCampaign((int)id, page, pageSize);

            var viewModel = new CampaignDetailsViewModel
            {
                Campaign = campaign,
                Sessions = sessions,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalSessions / (double)pageSize)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SaveCampaign(int id, bool useFromCampaign)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to save a campaign.";
                return RedirectToAction("Login", "Account");
            }

            var campaign = await _service.GetCampaignById(id);
            if (campaign == null || campaign.OwnerId != userId)
            {
                //Console.WriteLine($"User {userId} is saving campaign {campaignId}");
                //_logger.LogInformation($"User {userId} is saving campaign {campaignId}");
                TempData["ErrorMessage"] = "You do not have access to save this campaign.";
                return RedirectToAction("Index", "Home");
            }

            CampaignDTO campaignDto;
            if (useFromCampaign)
            {
                campaignDto = CampaignDTO.FromCampaign(campaign);
            }
            else
            {
                campaignDto = (CampaignDTO)campaign;
            }


            // Get the current directory of the Web project
            string currentDirectory = Directory.GetCurrentDirectory();
            // Combine the root directory with the desired folder and file name
            string directoryPath = Path.Combine(currentDirectory, "SavedCampaigns");
            string filePath = Path.Combine(directoryPath, $"campaign_{id}.json");

            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            //string filePath = $"campaign_{id}.json";
            Console.WriteLine($"Saving file to: {filePath}");
            bool success = await _service.SaveCampaignToFile(campaignDto, filePath);

            if (success)
            {
                TempData["SuccessMessage"] = "Campaign saved successfully.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to save the campaign.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

        }

        #endregion

        #region Update Methods

        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var campaign = await _service.GetCampaignById((int)id);
            if (campaign == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if(userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to edit campaigns";
                return RedirectToAction("Login", "Account");
            }

            var isGameMaster = await IsUserGameMaster((int)id, userId);
            if (campaign.OwnerId != userId && !isGameMaster)
            {
                TempData["ErrorMessage"] = "You do not have access to edit this campaign.";
                return RedirectToAction("Index", "Home");
            }

            return View(campaign);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, Campaign campaign, IFormFile? image)
        {
            if (id == null || campaign == null)
            {
                _logger.LogWarning("Edit method called with null id or campaign.");
                return NotFound();
            }

            var existingCampaign = await _service.GetCampaignById(id.Value);
            if (existingCampaign == null)
            {
                _logger.LogWarning($"Campaign with id {id} not found.");
                return NotFound();
            }

            // Update campaign properties
            existingCampaign.Name = campaign.Name;
            existingCampaign.Description = campaign.Description;
            var oldGameTime = existingCampaign.GameTime;
            existingCampaign.GameTime = campaign.GameTime;

            // Handle image upload if provided
            if (image != null && image.Length > 0)
            {
                // Process image upload
                // ...
            }

            try
            {
                // Save changes to the campaign
                await _service.UpdateCampaign(existingCampaign);
                _logger.LogInformation($"Campaign {existingCampaign.Id} updated successfully.");

                // Run scripts if the GameTime has changed
                if (oldGameTime != campaign.GameTime)
                {
                    _logger.LogInformation($"GameTime changed for campaign {existingCampaign.Id}. Running scripts.");
                    await RunScript(existingCampaign, oldGameTime, campaign.GameTime);
                }

                return RedirectToAction(nameof(Details), new { id = existingCampaign.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating campaign {existingCampaign.Id}.");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the campaign.");
                return View(campaign);
            }
        }


        public async Task<IActionResult> EditRole(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campaign = await _service.GetCampaignById((int)id);
            if (campaign == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null || campaign.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have access to edit the roles of this campaign.";
                return RedirectToAction("Index", "Home");
            }

            var participants = campaign.Participants.Select(p => new SelectListItem
            {
                Value = p.ApplicationUserId,
                Text = p.ApplicationUser.UserName
            }).ToList();

            var roles = Enum.GetValues(typeof(Role)).Cast<Role>().Select(r => new SelectListItem
            {
                Value = r.ToString(),
                Text = r.ToString()
            }).ToList();

            var model = new EditRoleViewModel
            {
                CampaignId = (int)id,
                Participants = participants,
                Roles = roles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var campaign = await _service.GetCampaignById(model.CampaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Participants = campaign.Participants.Select(p => new SelectListItem
                {
                    Value = p.ApplicationUserId,
                    Text = p.ApplicationUser.UserName
                }).ToList();

                model.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Select(r => new SelectListItem
                {
                    Value = r.ToString(),
                    Text = r.ToString()
                }).ToList();

                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null || campaign.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have access to edit the roles of this campaign.";
                return RedirectToAction("Index", "Home");
            }

            var participant = campaign.Participants.FirstOrDefault(p => p.ApplicationUserId == model.ParticipantId);
            if (participant == null)
            {
                return NotFound();
            }

            var existingParticipant = await _service.GetParticipantForCampaignByUserId(model.ParticipantId, model.CampaignId);
            if (existingParticipant == null)
            {
                return NotFound();
            }
            existingParticipant.Role = model.Role;

            bool result = await _service.UpdateCampaignParticipant(existingParticipant);
            if (result)
            {
                return RedirectToAction(nameof(Details), new { id = model.CampaignId });
            }
            else
            {
                ModelState.AddModelError("", "There was an error during saving");
            }


            return View(model);
        }

        #endregion

        #region Delete Methods

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campaign = await _service.GetCampaignById((int)id);
            if (campaign == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null || campaign.OwnerId != userId)
            {
                TempData["ErrorMessage"] = "You do not have access to delete this campaign.";
                return RedirectToAction("Index", "Home");
            }

            return View(campaign);

        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to delete a campaign.";
                return RedirectToAction("Login", "Account");
            }

            var campaign = await _service.GetCampaignById((int)id);
            if (campaign == null)
            {
                return NotFound();
            }

            var result = await _service.DeleteCampaignById((int)id);
            if (!result)
            {
                TempData["ErrorMessage"] = "An error occurred while trying to delete the campaign.";
                return RedirectToAction("Details", new { id = campaign.Id });
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> LeaveCampaign(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campaign = await _service.GetCampaignById((int)id);
            if (campaign == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null || !campaign.Participants.Any(p => p.ApplicationUserId == userId))
            {
                TempData["ErrorMessage"] = "You do not have permission to leave this campaign.";
                return RedirectToAction("Index", "Home");
            }

            return View(campaign);
        }

        [HttpPost, ActionName("LeaveCampaign")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveCampaignConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to leave a campaign.";
                return RedirectToAction("Login", "Account");
            }

            var campaign = await _service.GetCampaignById((int)id);
            if (campaign == null)
            {
                return NotFound();
            }

            var participant = campaign.Participants.FirstOrDefault(p => p.ApplicationUserId == user.Id);
            if (participant == null)
            {
                TempData["ErrorMessage"] = "You are not a participant in this campaign.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _service.DeleteCampaignParticipantById(participant.Id);
            if (!result)
            {
                TempData["ErrorMessage"] = "An error occurred while trying to leave the campaign.";
                return RedirectToAction("Details", new { id = campaign.Id });
            }

            return RedirectToAction(nameof(Index));
        }

        /*
        [HttpGet]
        public async Task<IActionResult> RemoveNoteAdmin(int? campaignId)
        {
            if (campaignId == null)
            {
                return NotFound();
            }

            var campaign = await _service.GetCampaignById((int)campaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to remove note admins from this campaign.";
                return RedirectToAction("Login", "Account");
            }

            if (campaign.OwnerId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to remove note admins from this campaign.";
                return RedirectToAction("Details", new { id = campaignId });
            }

            // Fetch NoteAdmins for the given campaign
            var noteAdmins = await _service.GetNoteAdminsForCampaign((int)campaignId);

            // Create a SelectList from the NoteAdmins
            var participants = new SelectList(noteAdmins.Select(na => new
            {
                na.ApplicationUserId,
                UserName = na.ApplicationUser.UserName
            }), "ApplicationUserId", "UserName");

            var model = new ManageNoteAdminViewModel
            {
                CampaignId = campaignId.Value,
                Participants = participants
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveNoteAdmin(ManageNoteAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var campaign = await _service.GetCampaignById(model.CampaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to remove note admins from this campaign.";
                return RedirectToAction("Login", "Account");
            }

            if (campaign.OwnerId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to remove note admins from this campaign.";
                return RedirectToAction("Details", new { id = model.CampaignId });
            }

            var noteAdmin = await _service.GetNoteAdminByUserId(model.SelectedParticipantId, model.CampaignId);
            if (noteAdmin == null)
            {
                ModelState.AddModelError("", "NoteAdmin not found.");
                return View(model);
            }

            await _service.DeleteNoteAdminById(noteAdmin.Id);
            TempData["SuccessMessage"] = "NoteAdmin removed successfully.";
            return RedirectToAction("Details", new { id = model.CampaignId });

        }
        */


        #endregion


        /*
        private async Task RunScript(Campaign campaign, DateTime oldGameTime, DateTime newGameTime)
        {
            var notifications = Notifications?.ToList() ?? new List<string>();

            foreach (var session in campaign.Sessions)
            {
               
                foreach (var note in session.Notes)
                {
                    
                    foreach (var noteGenerator in note.NoteGenerators)
                    {
                        
                        if (!noteGenerator.NextRunInGameDate.HasValue)
                        {
                            continue;
                        }

                        var nextRunDate = noteGenerator.NextRunInGameDate.Value;
                        if (nextRunDate > oldGameTime && nextRunDate <= newGameTime)
                        {
                            // Run the script

                            // Check if the generator has a valid script and skip if it doesn't
                            if (string.IsNullOrWhiteSpace(noteGenerator.Generator?.Script))
                            {
                                continue;
                            }

                            try
                            {
                                // Execute the script
                                var scriptOptions = ScriptOptions.Default
                                    .AddReferences(
                                        typeof(object).Assembly,
                                        typeof(Enumerable).Assembly,
                                        typeof(JsonSerializer).Assembly,
                                        typeof(Random).Assembly,
                                        typeof(List<>).Assembly,
                                        typeof(Array).Assembly,
                                        typeof(String).Assembly
                                    )
                                    .AddImports(
                                        "System",
                                        "System.Linq",
                                        "System.Collections.Generic",
                                        "System.Text.Json",
                                        "System.Text"
                                    );

                                var globals = new ScriptGlobals { Note = note };

                                var result = await CSharpScript.EvaluateAsync<string>(noteGenerator.Generator.Script, scriptOptions, globals);


                                // Check if Generator is null and use GeneratorId if necessary
                                var generatorName = noteGenerator.Generator?.Name ?? noteGenerator.GeneratorId.ToString();
                                // Trigger notification
                                notifications.Add($"{campaign.Name}/{session.Name}/{note.Title}/{generatorName} successfully run.");
                            }
                            catch (CompilationErrorException ex)
                            {
                                // Log compilation errors
                                //_logger.LogError(ex, $"Script compilation errors for NoteGenerator {noteGenerator.Id}: {string.Join(Environment.NewLine, ex.Diagnostics)}");
                                notifications.Add($"Error compiling script for {campaign.Name}/{session.Name}/{note.Title}/{noteGenerator.Generator?.Name ?? noteGenerator.GeneratorId.ToString()}: {ex.Message}");
                            }
                            catch (Exception ex)
                            {
                                // Log runtime errors
                                //_logger.LogError(ex, $"Error executing script for NoteGenerator {noteGenerator.Id}");
                                notifications.Add($"Error executing script for {campaign.Name}/{session.Name}/{note.Title}/{noteGenerator.Generator?.Name ?? noteGenerator.GeneratorId.ToString()}: {ex.Message}");
                            }

                        }
                    }
                }
            }

            await Task.CompletedTask;
           
        } */


        /*
        private async Task RunScript(Campaign campaign, DateTime oldGameTime, DateTime newGameTime)
        {
            var notifications = Notifications?.ToList() ?? new List<string>();

            _logger.LogInformation($"Running scripts for campaign {campaign.Id} from {oldGameTime} to {newGameTime}.");

            foreach (var session in campaign.Sessions)
            {
                foreach (var note in session.Notes)
                {
                    foreach (var noteGenerator in note.NoteGenerators)
                    {
                        if (!noteGenerator.NextRunInGameDate.HasValue)
                        {
                            _logger.LogInformation($"Skipping NoteGenerator {noteGenerator.Id} as NextRunInGameDate is not set.");
                            continue;
                        }

                        var nextRunDate = noteGenerator.NextRunInGameDate.Value;
                        if (nextRunDate > oldGameTime && nextRunDate <= newGameTime)
                        {
                            // Run the script
                            if (string.IsNullOrWhiteSpace(noteGenerator.Generator?.Script))
                            {
                                _logger.LogInformation($"Skipping NoteGenerator {noteGenerator.Id} as script is empty.");
                                continue;
                            }

                            try
                            {
                                // Execute the script
                                var scriptOptions = ScriptOptions.Default
                                    .AddReferences(
                                        typeof(object).Assembly,
                                        typeof(Enumerable).Assembly,
                                        typeof(JsonSerializer).Assembly,
                                        typeof(Random).Assembly,
                                        typeof(List<>).Assembly,
                                        typeof(Array).Assembly,
                                        typeof(String).Assembly
                                    )
                                    .AddImports(
                                        "System",
                                        "System.Linq",
                                        "System.Collections.Generic",
                                        "System.Text.Json",
                                        "System.Text"
                                    );

                                var globals = new ScriptGlobals { Note = note };

                                _logger.LogInformation($"Running script for NoteGenerator {noteGenerator.Id} in Note {note.Id}.");
                                var result = await CSharpScript.EvaluateAsync<string>(noteGenerator.Generator.Script, scriptOptions, globals);
                                _logger.LogInformation($"Script result for NoteGenerator {noteGenerator.Id}: {result}");

                                // Check if Generator is null and use GeneratorId if necessary
                                var generatorName = noteGenerator.Generator?.Name ?? noteGenerator.GeneratorId.ToString();
                                // Trigger notification
                                notifications.Add($"{campaign.Name}/{session.Name}/{note.Title}/{generatorName} successfully run.");
                            }
                            catch (CompilationErrorException ex)
                            {
                                // Log compilation errors
                                _logger.LogError(ex, $"Script compilation errors for NoteGenerator {noteGenerator.Id}: {string.Join(Environment.NewLine, ex.Diagnostics)}");
                                notifications.Add($"Error compiling script for {campaign.Name}/{session.Name}/{note.Title}/{noteGenerator.Generator?.Name ?? noteGenerator.GeneratorId.ToString()}: {ex.Message}");
                            }
                            catch (Exception ex)
                            {
                                // Log runtime errors
                                _logger.LogError(ex, $"Error executing script for NoteGenerator {noteGenerator.Id}");
                                notifications.Add($"Error executing script for {campaign.Name}/{session.Name}/{note.Title}/{noteGenerator.Generator?.Name ?? noteGenerator.GeneratorId.ToString()}: {ex.Message}");
                            }
                        }
                    }
                }
            }

            Notifications = notifications.ToArray();
            await Task.CompletedTask;
        } */

        private async Task RunScript(Campaign campaign, DateTime oldGameTime, DateTime newGameTime)
        {
            var successNotifications = SuccessNotifications?.ToList() ?? new List<string>();
            var errorNotifications = ErrorNotifications?.ToList() ?? new List<string>();

            _logger.LogInformation($"Running scripts for campaign {campaign.Id} from {oldGameTime} to {newGameTime}.");

            foreach (var session in campaign.Sessions)
            {
                foreach (var note in session.Notes)
                {
                    foreach (var noteGenerator in note.NoteGenerators)
                    {
                        if (!noteGenerator.NextRunInGameDate.HasValue)
                        {
                            _logger.LogInformation($"Skipping NoteGenerator {noteGenerator.Id} as NextRunInGameDate is not set.");
                            continue;
                        }

                        var nextRunDate = noteGenerator.NextRunInGameDate.Value;
                        if (nextRunDate > oldGameTime && nextRunDate <= newGameTime)
                        {
                            // Run the script
                            if (string.IsNullOrWhiteSpace(noteGenerator.Generator?.Script))
                            {
                                _logger.LogInformation($"Skipping NoteGenerator {noteGenerator.Id} as script is empty.");
                                continue;
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
                                _logger.LogInformation($"Running script for NoteGenerator {noteGenerator.Id} in Note {note.Id}.");
                                _logger.LogInformation($"Script content: {noteGenerator.Generator.Script}");
                                var result = await CSharpScript.EvaluateAsync<string>(noteGenerator.Generator.Script, scriptOptions, globals);
                                _logger.LogInformation($"Script result for NoteGenerator {noteGenerator.Id}: {result}");

                                if(result!= null)
                                {
                                    note.Content = result;
                                    var updateResult = await _service.UpdateNote(note);
                                    if (updateResult)
                                    {
                                        // Check if Generator is null and use GeneratorId if necessary
                                        var generatorName = noteGenerator.Generator?.Name ?? noteGenerator.GeneratorId.ToString();
                                        successNotifications.Add($"{campaign.Name}/{session.Name}/{note.Title}/{generatorName} successfully run!");
                                    } else
                                    {
                                        var generatorName = noteGenerator.Generator?.Name ?? noteGenerator.GeneratorId.ToString();
                                        //Should use an error message here
                                        errorNotifications.Add($"{campaign.Name}/{session.Name}/{note.Title}/{generatorName} didn't run successfully!");
                                    }
                                }
                              
                            }
                            catch (CompilationErrorException ex)
                            {
                                // Log compilation errors
                                _logger.LogError(ex, $"Script compilation errors for NoteGenerator {noteGenerator.Id}: {string.Join(Environment.NewLine, ex.Diagnostics)}!");
                                errorNotifications.Add($"Error compiling script for {campaign.Name}/{session.Name}/{note.Title}/{noteGenerator.Generator?.Name ?? noteGenerator.GeneratorId.ToString()}: {ex.Message}!");
                            }
                            catch (Exception ex)
                            {
                                // Log runtime errors
                                _logger.LogError(ex, $"Error executing script for NoteGenerator {noteGenerator.Id}");
                                errorNotifications.Add($"Error executing script for {campaign.Name}/{session.Name}/{note.Title}/{noteGenerator.Generator?.Name ?? noteGenerator.GeneratorId.ToString()}: {ex.Message}");
                            }
                        }
                    }
                }
            }

            SuccessNotifications = successNotifications.ToArray();
            ErrorNotifications = errorNotifications.ToArray();
        }



        private async Task<bool> IsUserGameMaster(int campaignId, string userId)
        {
            var gameMasters = await _service.GetGMsForCampaign(campaignId);
            return gameMasters.Any(gm => gm.ApplicationUserId == userId);
        }

        public class ScriptGlobals
        {
            public Note Note { get; set; } = null!;
            public ILogger<CampaignsController> Logger { get; set;} = null!;
        }


    }
}