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
    [Authorize] 
    public class InvitationsController : Controller
    {
        private readonly ICampaignManagerService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CampaignsController> _logger;

        public InvitationsController(ICampaignManagerService service, UserManager<ApplicationUser> userManager, ILogger<CampaignsController> logger)
        {
            _service = service;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid? campaignid)
        {
            if (campaignid == null)
            {
                return NotFound();
            }

            var campaign = await _service.GetCampaignById((Guid)campaignid);
            if (campaign == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to invite players to a campaign.";
                return RedirectToAction("Login", "Account");
            }

            if (user.Id != campaign.OwnerId)
            {
                TempData["ErrorMessage"] = "You do not have permission to invite players to this campaign.";
                return RedirectToAction("Index", "Home");
            }

            var model = new CreateInvitationViewModel
            {
                CampaignId = (Guid)campaignid,
                Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Select(r => new SelectListItem
                {
                    Value = r.ToString(),
                    Text = r.ToString()
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInvitationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Select(r => new SelectListItem
                {
                    Value = r.ToString(),
                    Text = r.ToString()
                }).ToList();
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "User with this email does not exist.");
                model.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Select(r => new SelectListItem
                {
                    Value = r.ToString(),
                    Text = r.ToString()
                }).ToList();
                return View(model);
            }

            var campaign = await _service.GetCampaignById(model.CampaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            if (campaign.OwnerId == user.Id ||
                campaign.Participants.Any(p => p.ApplicationUserId == user.Id))
            {
                ModelState.AddModelError("Email", "User is already part of the campaign.");
                model.Roles = Enum.GetValues(typeof(Role)).Cast<Role>().Select(r => new SelectListItem
                {
                    Value = r.ToString(),
                    Text = r.ToString()
                }).ToList();
                return View(model);
            }

            var invitation = new Invitation
            {
                CampaignId = model.CampaignId,
                ApplicationUserId = user.Id,
                Role = model.Role
            };

            try
            {
                bool invitationSent = await _service.AddInvitation(invitation);
                if (invitationSent)
                {
                    TempData["SuccessMessage"] = "Invitation sent successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "There was an error sending the invitation.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invitation");
                TempData["ErrorMessage"] = "An error occurred while sending the invitation.";
            }

            return RedirectToAction("Details", "Campaigns", new { id = campaign.Id });
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You need to be logged in to view your invitations.";
                return RedirectToAction("Login", "Account");
            }

            List<Invitation> invitations = await _service.GetInvitationsForUserById(userId);
            return View(invitations);
        }

        public async Task<IActionResult> Accept(int id)
        {

            try
            {

                var invitation = await _service.GetInvitationById(id);
                if (invitation == null)
                {
                    return NotFound();
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null || user.Id != invitation.ApplicationUserId)
                {
                    TempData["ErrorMessage"] = "You do not have permission to accept this invitation.";
                    return RedirectToAction(nameof(Index));
                }

                var campaign = invitation.Campaign;
                if (campaign == null)
                {
                    TempData["ErrorMessage"] = "The campaign associated with this invitation has been deleted.";
                    return RedirectToAction(nameof(Index));
                }

                var participant = new CampaignParticipant
                {
                    ApplicationUserId = user.Id,
                    CampaignId = campaign.Id,
                    Role = invitation.Role
                };

                using (var transaction = await _service.BeginTransactionAsync())
                {
                    try
                    {
                        bool participantAdded = await _service.AddCampaignParticipant(participant);
                        if (participantAdded)
                        {
                            await _service.DeleteInvitationById(id);
                            await _service.DeleteOtherInvitationsForCampaign(invitation.CampaignId, user.Id);
                            await transaction.CommitAsync();
                            return RedirectToAction("Index", "Campaigns");
                        }
                        else
                        {
                            await transaction.RollbackAsync();
                            TempData["ErrorMessage"] = "There was an error accepting the invitation.";
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error accepting invitation");
                        TempData["ErrorMessage"] = "An error occurred while accepting the invitation.";
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting invitation");
                TempData["ErrorMessage"] = "An error occurred while accepting the invitation.";
            }

            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> Reject(int id)
        {
            var invitation = await _service.GetInvitationById(id);
            if (invitation == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Id != invitation.ApplicationUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to reject this invitation.";
                return RedirectToAction(nameof(Index));
            }


            await _service.DeleteInvitationById(id);
            return RedirectToAction(nameof(Index));
        }




    }
}