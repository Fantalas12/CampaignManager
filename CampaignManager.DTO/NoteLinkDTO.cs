using CampaignManager.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.DTO
{

    public class NoteLinkDTO
    {
        public Guid Id { get; set; }
        public Guid FromNoteId { get; set; }
        public Guid ToNoteId { get; set; }
        public string? LinkType { get; set; }
        //public int Weight { get; set; }

        public static explicit operator NoteLinkDTO(NoteLink noteLink) => new NoteLinkDTO
        {
            Id = noteLink.Id,
            FromNoteId = noteLink.FromNoteId,
            ToNoteId = noteLink.ToNoteId,
            LinkType = noteLink.LinkType,
            //Weight = noteLink.Weight,
        };

        public static explicit operator NoteLink(NoteLinkDTO noteLinkDTO) => new NoteLink
        {
            Id = noteLinkDTO.Id,
            FromNoteId = noteLinkDTO.FromNoteId,
            ToNoteId = noteLinkDTO.ToNoteId,
            LinkType = noteLinkDTO.LinkType,
            //Weight = noteLinkDTO.Weight,
        };

        public static NoteLink ToNoteLink(NoteLinkDTO notelinkDTO)
        {
            return (NoteLink)notelinkDTO;
        }

        public static NoteLinkDTO FromNoteLink(NoteLink notelink)
        {
            return (NoteLinkDTO)notelink;
        }

    }
}