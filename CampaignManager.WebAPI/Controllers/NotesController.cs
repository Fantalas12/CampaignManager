﻿using CampaignManager.Persistence.Services;
using CampaignManager.DTO;
using CampaignManager.Persistence.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CampaignManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly CampaignManagerService _service;

        public NotesController(CampaignManagerService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteDTO>> GetNoteById(Guid id)
        {
            Note? note = null;
            try
            {
                note = await _service.GetNoteById(id);
                if (note == null)
                {
                    return NotFound();
                }
                return (NoteDTO)note;
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostNote([FromBody] NoteDTO noteDTO)
        {
            var note = (Note)noteDTO;
            var result = await _service.AddNote(note);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, noteDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutNote(Guid id, [FromBody] NoteDTO noteDTO)
        {
            if (id != noteDTO.Id)
            {
                return BadRequest();
            }

            var note = (Note)noteDTO;

            if (await _service.UpdateNote(note))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNoteById(Guid id)
        {
            if (await _service.DeleteNoteById(id))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /* Paginated Notes for Session 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NoteDTO>>> GetNotes([FromQuery] int sessionId)
        {
            // Implement filtering logic here
            // For now, return all notes
            var notes = await _service.GetNotesForSession(sessionId);
            var noteDTOs = notes.Select(note => (NoteDTO)note).ToList();
            return noteDTOs;
        } */

        /* Paginated Notes for Session  */
    }
}