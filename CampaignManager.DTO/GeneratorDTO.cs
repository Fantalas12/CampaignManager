using CampaignManager.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CampaignManager.DTO
{
    public class GeneratorDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Script { get; set; }
        public string? OwnerId { get; set; }
        public Guid? NoteId { get; set; }
        public Guid? NoteTypeId { get; set; }
        public List<NoteGeneratorDTO> NoteGenerators { get; set; } = new List<NoteGeneratorDTO>();
        //public bool IsBuiltIn { get; set; }

        public static explicit operator GeneratorDTO(Generator generator) => new GeneratorDTO
        {
            Id = generator.Id,
            Name = generator.Name,
            Script = generator.Script,
            OwnerId = generator.OwnerId,
            NoteId = generator.NoteId,
            NoteTypeId = generator.NoteTypeId,
            NoteGenerators = generator.NoteGenerators.Select(ng => (NoteGeneratorDTO)ng).ToList(),
            //IsBuiltIn = generator.IsBuiltIn
        };

        public static explicit operator Generator(GeneratorDTO generatorDTO) => new Generator
        {
            Id = generatorDTO.Id,
            Name = generatorDTO.Name,
            Script = generatorDTO.Script,
            OwnerId = generatorDTO.OwnerId,
            NoteId = generatorDTO.NoteId,
            NoteTypeId = generatorDTO.NoteTypeId,
            NoteGenerators = generatorDTO.NoteGenerators.Select(ng => (NoteGenerator)ng).ToList(),
            //IsBuiltIn = generatorDTO.IsBuiltIn
        };

        public static Generator ToGenerator(GeneratorDTO generatorDTO)
        {
            return new Generator
            {
                Id = generatorDTO.Id,
                Name = generatorDTO.Name,
                Script = generatorDTO.Script,
                OwnerId = null,
                NoteId = generatorDTO.NoteId,
                NoteTypeId = generatorDTO.NoteTypeId,
                NoteGenerators = generatorDTO.NoteGenerators.Select(ng => NoteGeneratorDTO.ToNoteGenerator(ng)).ToList(),
                //IsBuiltIn = generatorDTO.IsBuiltIn
            };
        }

        public static GeneratorDTO FromGenerator(Generator generator)
        {
            return new GeneratorDTO
            {
                Id = generator.Id,
                Name = generator.Name,
                Script = generator.Script,
                OwnerId = null,
                NoteId = generator.NoteId,
                NoteTypeId = generator.NoteTypeId,
                NoteGenerators = generator.NoteGenerators.Select(ng => NoteGeneratorDTO.FromNoteGenerator(ng)).ToList(),
                //IsBuiltIn = generator.IsBuiltIn
            };
        }
    }
}
