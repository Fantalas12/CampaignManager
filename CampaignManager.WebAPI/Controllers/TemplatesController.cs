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
    public class TemplatesController : ControllerBase
    {
        private readonly CampaignManagerService _service;

        public TemplatesController(CampaignManagerService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TemplateDTO>> GetTemplateById(Guid id)
        {
            Template? template = null;
            try
            {
                template = await _service.GetTemplateById(id);
                if (template == null)
                {
                    return NotFound();
                }
                return (TemplateDTO)template;
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostTemplate([FromBody] TemplateDTO templateDTO)
        {
            var template = (Template)templateDTO;
            var result = await _service.AddTemplate(template);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return CreatedAtAction(nameof(GetTemplateById), new { id = template.Id }, templateDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutTemplate(Guid id, [FromBody] TemplateDTO templateDTO)
        {
            if (id != templateDTO.Id)
            {
                return BadRequest();
            }

            var template = (Template)templateDTO;

            if (await _service.UpdateTemplate(template))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTemplateById(Guid id)
        {
            if (await _service.DeleteTemplateById(id))
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
