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
using Microsoft.AspNetCore.Routing;
using static System.Collections.Specialized.BitVector32;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CampaignManager.Web.Controllers
{
    public class SessionsController : Controller
    {
        private readonly ICampaignManagerService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CampaignsController> _logger;

        public SessionsController(ICampaignManagerService service, UserManager<ApplicationUser> userManager, ILogger<CampaignsController> logger)
        {
            _service = service;
            _userManager = userManager;
            _logger = logger;
        }

        #region Create Methods

        [HttpGet]
        public async Task<IActionResult> Create(Guid campaignId)
        {
            _logger.LogInformation("Create GET method called with campaignId: {CampaignId}", campaignId);
            var campaign = await _service.GetCampaignById(campaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            var sessionViewModel = new SessionViewModel
            {
                CampaignId = campaignId,
                Date = DateTime.Now.Date, // Set the default date to the current date
                //GameTime = DateTime.Now // Set the default time to the current time
            };
            return View(sessionViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SessionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError(error.ErrorMessage);
                }
                return View(vm);
            }

            try
            {
                var session = (Session)vm;

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to create a session.";
                    return RedirectToAction("Login", "Account");
                }

                bool existWithName = await _service.IsReservedSessionNameForCampaign(session.Name, session.CampaignId);
                if (existWithName)
                {
                    ModelState.AddModelError("Name", "Session with this name for the campaign already exists.");
                    return View(vm);
                }

                bool existWithDate = await _service.IsReservedSessionDateForCampaign(session.Date, session.CampaignId);
                if (existWithDate)
                {
                    ModelState.AddModelError("Date", "Session with this date for the campaign already exists.");
                    return View(vm);
                }

                //Check if the user is a game master in the campaign, if not, redirect to the campaign details page
                var gameMasters = await _service.GetGMsForCampaign(session.CampaignId);
                if (!gameMasters.Any(gm => gm.ApplicationUserId == user.Id))
                {
                    TempData["ErrorMessage"] = "You do not have permission to create a session for this campaign.";
                    return RedirectToAction("Details", "Campaigns", new { id = session.CampaignId });
                }

                session.GameMasterId = user.Id;

                var result = await _service.AddSession(session);
                if (result)
                {
                    return RedirectToAction("Details", "Campaigns", new { id = session.CampaignId });
                } else
                {
                    ModelState.AddModelError("", "There was an error creating the session.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "There was an error creating the session.");
            }

            return View(vm);
        }


        // GET: Sessions/AddPlayer/5
        [HttpGet]
        public async Task<IActionResult> AddPlayer(Guid? sessionId)
        {
            if (sessionId == null)
            {
                return NotFound();
            }

            var session = await _service.GetSessionById((Guid)sessionId);
            if (session == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to add players to a session.";
                return RedirectToAction("Login", "Account");
            }

            if (session.GameMasterId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to add players to this session.";
                return RedirectToAction("Details", "Campaigns", new { id = session.CampaignId });
            }

            var campaignParticipants = await _service.GetParticipantsForCampaign(session.CampaignId);
            var players = campaignParticipants
                .Where(p => p.Role == Role.Player || p.Role == Role.PlayerAndGameMaster)
                .Select(p => new SelectListItem
                {
                    Value = p.ApplicationUserId,
                    Text = p.ApplicationUser.UserName
                }).ToList();

           
            if (!players.Any())
            {
                TempData["ErrorMessage"] = "There are no players in the campaign to add to the session.";
                return RedirectToAction("Details", new { id = sessionId });
            } 

            var model = new ManagePlayerViewModel
            {
                SessionId = (Guid)sessionId,
                Players = players
            };

            return View(model);
        }

        // POST: Sessions/AddPlayer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPlayer(ManagePlayerViewModel model)
        {
            var session = await _service.GetSessionById(model.SessionId);
            if (session == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                var campaignParticipants = await _service.GetParticipantsForCampaign(session.CampaignId);
                model.Players = campaignParticipants
                    .Where(p => p.Role == Role.Player || p.Role == Role.PlayerAndGameMaster)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ApplicationUserId,
                        Text = p.ApplicationUser.UserName
                    }).ToList();

                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to add players to a session.";
                return RedirectToAction("Login", "Account");
            }

            if (session.GameMasterId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to add players to this session.";
                return RedirectToAction("Details", new { id = model.SessionId });
            }

            if (session.SessionPlayers.Any(sp => sp.ApplicationUserId == model.SelectedPlayerId))
            {
                TempData["ErrorMessage"] = "Player is already in the session.";
                return RedirectToAction("Details", new { id = model.SessionId });
            }

            var sessionPlayer = new SessionPlayer
            {
                SessionId = model.SessionId,
                ApplicationUserId = model.SelectedPlayerId,
                //SessonPlayerRole = "Player", //TODO - SessionPlayerRole
            };

            var result = await _service.AddSessionPlayer(sessionPlayer);
            if (result)
            {
                TempData["SuccessMessage"] = "Player added successfully.";
                return RedirectToAction("Details", new { id = model.SessionId });
            }

            TempData["ErrorMessage"] = "There was an error adding the player to the session.";
            return RedirectToAction("Details", new { id = model.SessionId });
        }

        #endregion

        #region Read Methods

        public async Task<IActionResult> Details(Guid? id, int page = 1, int pageSize = 5)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _service.GetSessionById((Guid)id);

            if (session == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to view session details.";
                return RedirectToAction("Login", "Account");
            }

            //Check if the user is part of the campaign, if not, redirect the user 
            var userCampaigns = await _service.GetCampaignsForUserById(user.Id);
            if (!userCampaigns.Any(c => c.Id == session.CampaignId))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this session.";
                return RedirectToAction("Index", "Campaigns");
            }

            if (session == null)
            {
                return NotFound();
            }

            var (notes, totalNotes) = await _service.GetPaginatedNotesForSession((Guid)id, page, pageSize);

            var viewModel = new SessionDetailsViewModel
            {
                Session = session,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalNotes / (double)pageSize),
                Notes = notes
            };

            return View(viewModel);
        }


        #endregion

        #region Update Methods

        // GET: Sessions/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _service.GetSessionById((Guid)id);
            if (session == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to view session details.";
                return RedirectToAction("Login", "Account");
            }

            //Check if the user is part of the campaign, if not, redirect the user 
            var userCampaigns = await _service.GetCampaignsForUserById(user.Id);
            if (!userCampaigns.Any(c => c.Id == session.CampaignId))
            {
                TempData["ErrorMessage"] = "You do not have permission to view this session.";
                return RedirectToAction("Index", "Campaigns");
            }

            var viewModel = (SessionViewModel)session;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, [Bind("Id,Name,Description,Date,GameTime,CampaignId,GameMasterId")] SessionViewModel viewModel)
        {

            if (id == null)
            {
                return NotFound();
            }

            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                var userId = _userManager.GetUserId(User);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in to edit a campaign.";
                    return RedirectToAction("Login", "Account");
                }

                var session = (Session)viewModel;


                // We need existingCampaign variable to access the owner id so we can check if the user is the owner of the campaign
                var existingSession = await _service.GetSessionById((Guid)id);
                if (existingSession == null)
                {
                    return NotFound();
                }

                if (existingSession.GameMasterId != userId)
                {
                    TempData["ErrorMessage"] = "You do not have access to edit this campaign.";
                    return RedirectToAction("Index", "Home");
                }

                if (existingSession.Name != session.Name) {
                    bool existWithName = await _service.IsReservedSessionNameForCampaign(session.Name, session.CampaignId);
                    if (existWithName)
                    {
                        ModelState.AddModelError("Name", "Session with this name for the campaign already exists.");
                        return View(viewModel);
                    }
                }

                if (existingSession.Date != session.Date)
                {
                    bool existWithDate = await _service.IsReservedSessionDateForCampaign(session.Date, session.CampaignId);
                    if (existWithDate)
                    {
                        ModelState.AddModelError("Date", "Session with this date for the campaign already exists.");
                        return View(viewModel);
                    }
                }

                // Update the properties of the existing campaign with the new values from the form
                existingSession.Name = session.Name;
                existingSession.Description = session.Description;
                existingSession.Date = session.Date;
                //existingSession.GameTime = session.GameTime;

                bool result = await _service.UpdateSession(existingSession);
                if (result)
                {
                    return RedirectToAction("Details", new { id = existingSession.Id });
                }
                else
                {
                    ModelState.AddModelError("", "There was an error during saving");
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeGameMaster(Guid? sessionId)
        {
            if(sessionId == null) 
            { 
                return NotFound(); 
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to change the gamemaster";
                return RedirectToAction("Login", "Account");
            }

            var session = await _service.GetSessionById((Guid)sessionId);
            if (session == null)
            {
                return NotFound();
            }

            var campaign = await _service.GetCampaignById(session.CampaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            if(userId != campaign.OwnerId)
            {
                TempData["ErrorMessage"] = "You do not have permission to change the gamemaster for this session.";
                return RedirectToAction("Details", new { id = session.Id });
            }

            var gameMasters = await _service.GetGMsForCampaign(campaign.Id);
            //var gameMasters = campaign.Participants.Where(p => p.Role == Role.GameMaster);
            //var gameMasterSelectList = new SelectList(gameMasters, "UserId", "UserName");
            var gameMasterSelectList = new SelectList(gameMasters.Select(gm => new SelectListItem
            {
                Value = gm.ApplicationUserId,
                Text = gm.ApplicationUser.UserName
            }).ToList(), "Value", "Text");

            if (!gameMasterSelectList.Any())
            {
                TempData["ErrorMessage"] = "There are no gamemasters in the campaign";
                return RedirectToAction("Details", new { id = session.Id });
            }

            var viewModel = new ChangeGameMasterViewModel
            {
                SessionId = session.Id,
                CurrentGameMasterId = session.GameMasterId,
                GameMasters = gameMasterSelectList
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeGameMaster(ChangeGameMasterViewModel viewModel)
        {

            var session = await _service.GetSessionById(viewModel.SessionId);
            if (session == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {

                var gameMasters = await _service.GetGMsForCampaign(session.CampaignId);
                viewModel.GameMasters = new SelectList(gameMasters.Select(gm => new SelectListItem
                {
                    Value = gm.ApplicationUserId,
                    Text = gm.ApplicationUser.UserName
                }).ToList(), "Value", "Text");
                return View(viewModel);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to change the gamemaster";
                return RedirectToAction("Login", "Account");
            }

            if (userId != session.Campaign.OwnerId)
            {
                TempData["ErrorMessage"] = "You do not have permission to change the Gamemaster for this session.";
                return RedirectToAction("Details", new { id = session.Id });
            }

            session.GameMasterId = viewModel.SelectedGameMasterId;
            var updateResult = await _service.UpdateSession(session);

            if (!updateResult)
            {
                //_logger.LogError("Failed to update GameMaster for Session {SessionId}", session.Id);
                ModelState.AddModelError("", "Failed to update Gamemaster.");
                TempData["ErrorMessage"] = "Failed to update Gamemaster.";
                return View(viewModel);
            }

            TempData["SuccessMessage"] = "Gamemaster added successfully.";
            return RedirectToAction(nameof(Details), new { id = session.Id });
        }


        #endregion

        #region Delete Methods

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _service.GetSessionById((Guid)id);
            if (session == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user?.Id == null || session.GameMasterId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have access to delete this session.";
                return RedirectToAction("Index", "Home");
            }

            return View(session);

        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _service.GetSessionById((Guid)id);
            if (session == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to delete a session.";
                return RedirectToAction("Login", "Account");
            }

            // Check if the user is the game master of the session
            if (session.GameMasterId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this session.";
                return RedirectToAction("Details", "Campaigns", new { id = session.CampaignId });
            }

            var result = await _service.DeleteSessionById((Guid)id);
            if (result)
            {
                return RedirectToAction("Details", "Campaigns", new { id = session.CampaignId });
            }

            TempData["ErrorMessage"] = "There was an error deleting the session.";
            return RedirectToAction("Details", "Campaigns", new { id = session.CampaignId });
        }



        [HttpGet]
        public async Task<IActionResult> RemovePlayer(Guid? sessionId)
        {
            if (sessionId == null)
            {
                return NotFound();
            }

            var session = await _service.GetSessionById((Guid)sessionId);
            if (session == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to remove players from a session.";
                return RedirectToAction("Login", "Account");
            }

            if (session.GameMasterId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to remove players from this session.";
                return RedirectToAction("Details", new { id = sessionId });
            }


            var players = session.SessionPlayers
            .Select(sp => new SelectListItem
            {
                Value = sp.ApplicationUser.Id,
                Text = sp.ApplicationUser.UserName
            }).ToList();

            if (!players.Any())
            {
                TempData["ErrorMessage"] = "There are no players in the session to remove.";
                return RedirectToAction("Details", new { id = sessionId });
            }

            var model = new ManagePlayerViewModel
            {
                SessionId = (Guid)sessionId,
                Players = players
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePlayer(ManagePlayerViewModel model)
        {

            var session = await _service.GetSessionById(model.SessionId);
            if (session == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to remove players from a session.";
                return RedirectToAction("Login", "Account");
            }

            if (session.GameMasterId != user.Id)
            {
                TempData["ErrorMessage"] = "You do not have permission to remove players from this session.";
                return RedirectToAction("Details", new { id = model.SessionId });
            }

            var sessionPlayer = session.SessionPlayers.FirstOrDefault(sp => sp.ApplicationUserId == model.SelectedPlayerId);
            if (sessionPlayer == null)
            {
                TempData["ErrorMessage"] = "Player not found in the session.";
                return RedirectToAction("Details", new { id = model.SessionId });
            }

            var result = await _service.RemoveSessionPlayer(sessionPlayer);
            if (result)
            {
                TempData["SuccessMessage"] = "Player removed successfully.";
                return RedirectToAction("Details", new { id = model.SessionId });
            }

            TempData["ErrorMessage"] = "There was an error removing the player from the session.";
            return RedirectToAction("Details", new { id = model.SessionId });

        }


        #endregion

        #region Other methods

        #endregion


    }
}
