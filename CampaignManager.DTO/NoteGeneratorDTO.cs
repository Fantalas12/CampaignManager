using CampaignManager.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.DTO
{
    public class NoteGeneratorDTO
    {
        public Guid Id { get; set; }
        public Guid? NoteId { get; set; }
        public Guid? GeneratorId { get; set; }
        public DateTime? NextRunInGameDate { get; set; }


        public static explicit operator NoteGeneratorDTO(NoteGenerator noteGenerators) => new NoteGeneratorDTO
        {
            Id = noteGenerators.Id,
            NoteId = noteGenerators.NoteId,
            GeneratorId = noteGenerators.GeneratorId,
            NextRunInGameDate = noteGenerators.NextRunInGameDate
        };

        public static explicit operator NoteGenerator(NoteGeneratorDTO noteGeneratorDTO) => new NoteGenerator
        {
            Id = noteGeneratorDTO.Id,
            NoteId = noteGeneratorDTO.NoteId,
            GeneratorId = noteGeneratorDTO.GeneratorId,
            NextRunInGameDate = noteGeneratorDTO.NextRunInGameDate
        };

        public static NoteGenerator ToNoteGenerator(NoteGeneratorDTO noteGeneratorDTO)
        {
            return new NoteGenerator
            {
                Id = noteGeneratorDTO.Id,
                NoteId = noteGeneratorDTO.NoteId,
                GeneratorId = noteGeneratorDTO.GeneratorId,
                NextRunInGameDate = noteGeneratorDTO.NextRunInGameDate
            };
        }

        public static NoteGeneratorDTO FromNoteGenerator(NoteGenerator noteGenerators)
        {
            return new NoteGeneratorDTO
            {
                Id = noteGenerators.Id,
                NoteId = noteGenerators.NoteId,
                GeneratorId = noteGenerators.GeneratorId,
                NextRunInGameDate = noteGenerators.NextRunInGameDate
            };
        }
    }
}
