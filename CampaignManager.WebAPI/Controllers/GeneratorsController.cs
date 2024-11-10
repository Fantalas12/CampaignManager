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
    public class GeneratorsController : ControllerBase
    {
        private readonly CampaignManagerService _service;

        public GeneratorsController(CampaignManagerService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GeneratorDTO>> GetGeneratorById(Guid id)
        {
            Generator? generator = null;
            try
            {
                generator = await _service.GetGeneratorById(id);
                if (generator == null)
                {
                    return NotFound();
                }
                return (GeneratorDTO)generator;
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostGenerator([FromBody] GeneratorDTO generatorDTO)
        {
            var generator = (Generator)generatorDTO;
            var result = await _service.AddGenerator(generator);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return CreatedAtAction(nameof(GetGeneratorById), new { id = generator.Id }, generatorDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutGenerator(Guid id, [FromBody] GeneratorDTO generatorDTO)
        {
            if (id != generatorDTO.Id)
            {
                return BadRequest();
            }

            var generator = (Generator)generatorDTO;

            if (await _service.UpdateGenerator(generator))
            {
                return Ok();
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteGeneratorById(Guid id)
        {
            if (await _service.DeleteGeneratorById(id))
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
