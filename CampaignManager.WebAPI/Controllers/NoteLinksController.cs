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
    public class NoteLinksController : ControllerBase
    {
        private readonly ICampaignManagerService _service;

        public NoteLinksController(ICampaignManagerService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteLinkDTO>> GetNoteLinkById(Guid id)
        {
            NoteLink? noteLink = null;
            try
            {
                noteLink = await _service.GetNoteLinkById(id);
                if (noteLink == null)
                {
                    return NotFound();
                }
                return (NoteLinkDTO)noteLink;
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpGet("note/{noteId}/links")]
        public async Task<ActionResult<(List<NoteLinkDTO> NoteLinks, int TotalCount)>> GetPaginatedToNoteLinksForNote(Guid noteId, int page, int pageSize)
        {
            var result = await _service.GetPaginatedToNoteLinksForNote(noteId, page, pageSize);
            var noteLinkDTOs = result.NoteLinks.Select(nl => new NoteLinkDTO
            {
                Id = nl.Id,
                FromNoteId = nl.FromNoteId,
                ToNoteId = nl.ToNoteId,
                LinkType = nl.LinkType,
            }).ToList();

            return Ok((NoteLinks: noteLinkDTOs, TotalCount: result.TotalCount));
        }


        [HttpPost]
        public async Task<ActionResult> PostNoteLink([FromBody] NoteLinkDTO noteLinkDTO)
        {
            var noteLink = (NoteLink)noteLinkDTO;
            var result = await _service.AddNoteLink(noteLink);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return CreatedAtAction(nameof(GetNoteLinkById), new { id = noteLink.Id }, noteLinkDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutNoteLink(Guid id, [FromBody] NoteLinkDTO noteLinkDTO)
        {
            if (id != noteLinkDTO.Id)
            {
                return BadRequest();
            }

            var noteLink = (NoteLink)noteLinkDTO;

            if (await _service.UpdateNoteLink(noteLink))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNoteLinkById(Guid id)
        {
            if (await _service.DeleteNoteLinkById(id))
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
