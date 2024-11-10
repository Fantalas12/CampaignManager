/* using CampaignManager.DTO;
using CampaignManager.Persistence.Models;
using CampaignManager.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CampaignManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessControlsController : ControllerBase
    {
        private readonly CampaignManagerService _service;

        public AccessControlsController(CampaignManagerService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteAccessDTO>> GetAccessControl(Guid id)
        {
            NoteAccess? ace = null;
            try
            {
                ace = await _service.GetAccessControlById(id);
                if (ace == null)
                {
                    return NotFound();
                }
                return (NoteAccessDTO)ace;
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostAccessControl([FromBody] NoteAccessDTO aceDTO)
        {
            var ace = (NoteAccess)aceDTO;
            var result = await _service.AddAccessControl(ace);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return CreatedAtAction(nameof(GetAccessControl), new { id = ace.Id }, aceDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAccessControl(Guid id, [FromBody] NoteAccessDTO aceDTO)
        {
            if (id != aceDTO.Id)
            {
                return BadRequest();
            }

            var ace = (NoteAccess)aceDTO;

            if (await _service.UpdateAccessControl(ace))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAccessControl(Guid id)
        {
            if (await _service.DeleteAccessControlById(id))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
*/