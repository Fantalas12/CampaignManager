using CampaignManager.DTO;
using CampaignManager.Persistence.Models;
using CampaignManager.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CampaignManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly CampaignManagerService _service;

        public SessionController(CampaignManagerService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SessionDTO>> GetSessionById(Guid id)
        {
            Session? session = null;
            try
            {
                session = await _service.GetSessionById(id);
                if (session == null)
                {
                    return NotFound();
                }
                return (SessionDTO)session;
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostSession([FromBody] SessionDTO sessionDTO)
        {
            var session = (Session)sessionDTO;
            var result = await _service.AddSession(session);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return CreatedAtAction(nameof(GetSessionById), new { id = session.Id }, sessionDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutSession(Guid id, [FromBody] SessionDTO sessionDTO)
        {
            if (id != sessionDTO.Id)
            {
                return BadRequest();
            }

            var session = (Session)sessionDTO;

            if (await _service.UpdateSession(session))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSessionById(Guid id)
        {
            if (await _service.DeleteSessionById(id))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("campaign/{campaignId}")]
        public async Task<ActionResult<IEnumerable<SessionDTO>>> GetSessionsForCampaign(int campaignId)
        {
            var sessions = await _service.GetSessionsForCampaign(campaignId);
            var sessionDTOs = sessions.Select(session => (SessionDTO)session).ToList();
            return sessionDTOs;
        }

        [HttpGet("campaign/{campaignId}/paginated")]
        public async Task<ActionResult<(List<SessionDTO> Sessions, int TotalCount)>> GetPaginatedSessionsForCampaign(int campaignId, [FromQuery] int page, [FromQuery] int pageSize)
        {
            var (sessions, totalCount) = await _service.GetPaginatedSessionsForCampaign(campaignId, page, pageSize);
            return Ok((sessions.Select(s => (SessionDTO)s).ToList(), totalCount));
        }

        [HttpGet("reserved/name")]
        public async Task<ActionResult<bool>> IsReservedSessionNameForCampaign([FromQuery] string name, [FromQuery] int campaignId)
        {
            var result = await _service.IsReservedSessionNameForCampaign(name, campaignId);
            return Ok(result);
        }

        [HttpGet("reserved/date")]
        public async Task<ActionResult<bool>> IsReservedSessionDateForCampaign([FromQuery] DateTime date, [FromQuery] int campaignId)
        {
            var result = await _service.IsReservedSessionDateForCampaign(date, campaignId);
            return Ok(result);
        }
    }
}
