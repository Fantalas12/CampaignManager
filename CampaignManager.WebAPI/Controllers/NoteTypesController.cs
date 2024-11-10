using CampaignManager.DTO;
using CampaignManager.Persistence.Models;
using CampaignManager.Persistence.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CampaignManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NoteTypesController : ControllerBase
    {
        private readonly CampaignManagerService _service;

        public NoteTypesController(CampaignManagerService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteTypeDTO>> GetNoteTypeById(Guid id)
        {
            NoteType? noteType = null;
            try
            {
                noteType = await _service.GetNoteTypeById(id);
                if (noteType == null)
                {
                    return NotFound();
                }
                return (NoteTypeDTO)noteType;
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostNoteType([FromBody] NoteTypeDTO noteTypeDTO)
        {
            var noteType = (NoteType)noteTypeDTO;
            var result = await _service.AddNoteType(noteType);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return CreatedAtAction(nameof(GetNoteTypeById), new { id = noteType.Id }, noteTypeDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutNoteType(Guid id, [FromBody] NoteTypeDTO noteTypeDTO)
        {
            if (id != noteTypeDTO.Id)
            {
                return BadRequest();
            }

            var noteType = (NoteType)noteTypeDTO;

            if (await _service.UpdateNoteType(noteType))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNoteTypeById(Guid id)
        {
            if (await _service.DeleteNoteTypeById(id))
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


