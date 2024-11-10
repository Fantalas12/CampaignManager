using CampaignManager.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.DTO
{
    public class TemplateDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Content { get; set; }
        public bool IsVerified { get; set; }
        public string? OwnerId { get; set; }
        public virtual ICollection<NoteTypeDTO> PlayerNoteTypes { get; set; } = new List<NoteTypeDTO>();
        public virtual ICollection<NoteTypeDTO> GameMasterNoteTypes { get; set; } = new List<NoteTypeDTO>();

        //public virtual ICollection<NoteTypeDTO> PlayerViewNoteTypes { get; set; } = new List<NoteTypeDTO>();
        //public virtual ICollection<NoteTypeDTO> GameMasterViewNoteTypes { get; set; } = new List<NoteTypeDTO>();
        //public virtual ICollection<NoteTypeDTO> PlayerEditNoteTypes { get; set; } = new List<NoteTypeDTO>();
        //public virtual ICollection<NoteTypeDTO> GameMasterEditNoteTypes { get; set; } = new List<NoteTypeDTO>();
        //public virtual List<NoteTypeDTO> NoteTypes { get; set; } = new List<NoteTypeDTO>();

        public static explicit operator TemplateDTO(Template template) => new TemplateDTO
        {
            Id = template.Id,
            Name = template.Name,
            Content = template.Content,
            IsVerified = template.IsVerified,
            OwnerId = template.OwnerId,
            PlayerNoteTypes = template.PlayerNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList(),
            GameMasterNoteTypes = template.GameMasterNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList(),
            //PlayerViewNoteTypes = template.PlayerViewNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList(),
            //GameMasterViewNoteTypes = template.GameMasterViewNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList(),
            //PlayerEditNoteTypes = template.PlayerEditNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList(),
            //GameMasterEditNoteTypes = template.GameMasterEditNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList()
        };

        public static explicit operator Template(TemplateDTO templateDTO) => new Template
        {
            Id = templateDTO.Id,
            Name = templateDTO.Name,
            Content = templateDTO.Content,
            IsVerified = templateDTO.IsVerified,
            OwnerId = templateDTO.OwnerId,
            PlayerNoteTypes = templateDTO.PlayerNoteTypes.Select(nt => (NoteType)nt).ToList(),
            GameMasterNoteTypes = templateDTO.GameMasterNoteTypes.Select(nt => (NoteType)nt).ToList(),
            //PlayerViewNoteTypes = templateDTO.PlayerViewNoteTypes.Select(nt => (NoteType)nt).ToList(),
            //GameMasterViewNoteTypes = templateDTO.GameMasterViewNoteTypes.Select(nt => (NoteType)nt).ToList(),
            //PlayerEditNoteTypes = templateDTO.PlayerEditNoteTypes.Select(nt => (NoteType)nt).ToList(),
            //GameMasterEditNoteTypes = templateDTO.GameMasterEditNoteTypes.Select(nt => (NoteType)nt).ToList()
        };

        public static Template ToTemplate(TemplateDTO templateDTO)
        {
            return new Template
            {
                Id = templateDTO.Id,
                Name = templateDTO.Name,
                Content = templateDTO.Content,
                IsVerified = templateDTO.IsVerified,
                OwnerId = null,
                PlayerNoteTypes = templateDTO.PlayerNoteTypes.Select(nt => (NoteType)nt).ToList(),
                //PlayerViewNoteTypes = templateDTO.PlayerViewNoteTypes.Select(nt => (NoteType)nt).ToList(),
                //GameMasterViewNoteTypes = templateDTO.GameMasterViewNoteTypes.Select(nt => (NoteType)nt).ToList(),
                //PlayerEditNoteTypes = templateDTO.PlayerEditNoteTypes.Select(nt => (NoteType)nt).ToList(),
                //GameMasterEditNoteTypes = templateDTO.GameMasterEditNoteTypes.Select(nt => (NoteType)nt).ToList()
            };
        }

        public static TemplateDTO FromTemplate(Template template)
        {
            return new TemplateDTO
            {
                Id = template.Id,
                Name = template.Name,
                Content = template.Content,
                IsVerified = template.IsVerified,
                OwnerId = null,
                PlayerNoteTypes = template.PlayerNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList(),
                GameMasterNoteTypes = template.GameMasterNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList(),
                //PlayerViewNoteTypes = template.PlayerViewNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList(),
                //GameMasterViewNoteTypes = template.GameMasterViewNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList(),
                //PlayerEditNoteTypes = template.PlayerEditNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList(),
                //GameMasterEditNoteTypes = template.GameMasterEditNoteTypes.Select(nt => (NoteTypeDTO)nt).ToList()
            };
        }

    }
}