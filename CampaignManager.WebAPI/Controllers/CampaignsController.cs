using CampaignManager.DTO;
using CampaignManager.Persistence.Models;
using CampaignManager.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CampaignManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignManagerService _service;

        public CampaignController(ICampaignManagerService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CampaignDTO>> GetCampaignById(Guid id)
        {
            Campaign? campaign = null;
            try
            {
                campaign = await _service.GetCampaignById(id);
                if (campaign == null)
                {
                    return NotFound();
                }
                return (CampaignDTO)campaign;
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostCampaign([FromBody] CampaignDTO campaignDTO)
        {
            var campaign = (Campaign)campaignDTO;
            var result = await _service.AddCampaign(campaign);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return CreatedAtAction(nameof(GetCampaignById), new { id = campaign.Id }, campaignDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutCampaign(Guid id, [FromBody] CampaignDTO campaignDTO)
        {
            if (id != campaignDTO.Id)
            {
                return BadRequest();
            }

            var campaign = (Campaign)campaignDTO;

            if (await _service.UpdateCampaign(campaign))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCampaignById(Guid id)
        {
            if (await _service.DeleteCampaignById(id))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("owned/{userId}")]
        public async Task<ActionResult<List<CampaignDTO>>> GetOwnedCampaignsForUserById(string userId)
        {
            var campaigns = await _service.GetOwnedCampaignsForUserById(userId);
            return Ok(campaigns.Select(c => (CampaignDTO)c).ToList());
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<CampaignDTO>>> GetCampaignsForUserById(string userId)
        {
            var campaigns = await _service.GetCampaignsForUserById(userId);
            return Ok(campaigns.Select(c => (CampaignDTO)c).ToList());
        }

        [HttpGet("reserved")]
        public async Task<ActionResult<bool>> IsReservedCampaignNameForUser([FromQuery] string name, [FromQuery] string userId)
        {
            var result = await _service.IsReservedCampaignNameForUser(name, userId);
            return Ok(result);
        }
    }
}
