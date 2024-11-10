using CampaignManager.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.DTO
{
    public class NoteTypeDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? OwnerId { get; set; }
        //public Guid? PlayerViewTemplateId { get; set; }
        //public Guid? GameMasterViewTemplateId { get; set; }
        //public Guid? PlayerEditTemplateId { get; set; }
        //public Guid? GameMasterEditTemplateId { get; set; }
        public Guid? PlayerTemplateId { get; set; }
        public Guid? GameMasterTemplateId { get; set; }
        public virtual List<NoteDTO> Notes { get; set; } = new List<NoteDTO>();
        public List<GeneratorDTO> Generators { get; set; } = new List<GeneratorDTO>();


        public static explicit operator NoteTypeDTO(NoteType noteType) => new NoteTypeDTO
        {
            Id = noteType.Id,
            Name = noteType.Name,
            OwnerId = noteType.OwnerId,
            //PlayerViewTemplateId = noteType.PlayerViewTemplateId,
            //GameMasterViewTemplateId = noteType.GameMasterViewTemplateId,
            //PlayerEditTemplateId = noteType.PlayerEditTemplateId,
            //GameMasterEditTemplateId = noteType.GameMasterEditTemplateId,
            PlayerTemplateId = noteType.PlayerTemplateId,
            GameMasterTemplateId = noteType.GameMasterTemplateId,
            Notes = noteType.Notes.Select(n => (NoteDTO)n).ToList(),
            Generators = noteType.Generators.Select(g => (GeneratorDTO)g).ToList()
        };

        public static explicit operator NoteType(NoteTypeDTO noteTypeDTO) => new NoteType
        {
            Id = noteTypeDTO.Id,
            Name = noteTypeDTO.Name,
            OwnerId = noteTypeDTO.OwnerId,
            //PlayerViewTemplateId = noteTypeDTO.PlayerViewTemplateId,
            //GameMasterViewTemplateId = noteTypeDTO.GameMasterViewTemplateId,
            //PlayerEditTemplateId = noteTypeDTO.PlayerEditTemplateId,
            //GameMasterEditTemplateId = noteTypeDTO.GameMasterEditTemplateId,
            PlayerTemplateId = noteTypeDTO.PlayerTemplateId,
            GameMasterTemplateId = noteTypeDTO.GameMasterTemplateId,
            Notes = noteTypeDTO.Notes.Select(n => (Note)n).ToList(),
            Generators = noteTypeDTO.Generators.Select(g => (Generator)g).ToList()
        };

        public static NoteType ToNoteType(NoteTypeDTO noteTypeDTO)
        {
            return new NoteType
            {
                Id = noteTypeDTO.Id,
                Name = noteTypeDTO.Name,
                OwnerId = null,
                //PlayerViewTemplateId = noteTypeDTO.PlayerViewTemplateId,
                //GameMasterViewTemplateId = noteTypeDTO.GameMasterViewTemplateId,
                //PlayerEditTemplateId = noteTypeDTO.PlayerEditTemplateId,
                //GameMasterEditTemplateId = noteTypeDTO.GameMasterEditTemplateId,
                PlayerTemplateId = noteTypeDTO.PlayerTemplateId,
                GameMasterTemplateId = noteTypeDTO.GameMasterTemplateId,
                Notes = noteTypeDTO.Notes.Select(n => NoteDTO.ToNote(n)).ToList(),
                Generators = noteTypeDTO.Generators.Select(g => GeneratorDTO.ToGenerator(g)).ToList()
            };
        }

        //FromNoteType
        public static NoteTypeDTO FromNoteType(NoteType noteType)
        {
            return new NoteTypeDTO
            {
                Id = noteType.Id,
                Name = noteType.Name,
                OwnerId = null,
                //PlayerViewTemplateId = noteType.PlayerViewTemplateId,
                //GameMasterViewTemplateId = noteType.GameMasterViewTemplateId,
                //PlayerEditTemplateId = noteType.PlayerEditTemplateId,
                //GameMasterEditTemplateId = noteType.GameMasterEditTemplateId,
                PlayerTemplateId = noteType.PlayerTemplateId,
                GameMasterTemplateId = noteType.GameMasterTemplateId,
                Notes = noteType.Notes.Select(n => NoteDTO.FromNote(n)).ToList(),
                Generators = noteType.Generators.Select(g => GeneratorDTO.FromGenerator(g)).ToList()
            };
        }


    }
}

