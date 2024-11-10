using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using CampaignManager.Persistence.Models;

namespace CampaignManager.DTO
{
    public class NoteDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? OwnerId { get; set; }
        public int? SessionId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime InGameDate { get; set; }
        public virtual ICollection<string> Tags { get; set; } = new List<string>();
        public Guid? NoteTypeId { get; set; }
        public List<NoteLinkDTO> ToLinkedNotes { get; set; } = new List<NoteLinkDTO>();
        public List<NoteLinkDTO> FromLinkedNotes { get; set; } = new List<NoteLinkDTO>();
        //public List<NoteAccessDTO> AccessControlList { get; set; } = new List<NoteAccessDTO>();
        //Different generators then the ones in the NoteType class, we extend the NoteType class with additional generators
        public List<NoteGeneratorDTO> NoteGenerators { get; set; } = new List<NoteGeneratorDTO>();


        public static explicit operator NoteDTO(Note note) => new NoteDTO
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            OwnerId = note.OwnerId,
            SessionId = note.SessionId,
            CreatedDate = note.CreatedDate,
            ModifiedDate = note.ModifiedDate,
            InGameDate = note.InGameDate,
            NoteTypeId = note.NoteTypeId,
            ToLinkedNotes = note.ToLinkedNotes.Select(nl => (NoteLinkDTO)nl).ToList(),
            FromLinkedNotes = note.FromLinkedNotes.Select(nl => (NoteLinkDTO)nl).ToList(),
            //AccessControlList = note.AccessControlList.Select(ace => (NoteAccessDTO)ace).ToList(),
            NoteGenerators = note.NoteGenerators.Select(g => (NoteGeneratorDTO)g).ToList()
        };

        public static explicit operator Note(NoteDTO noteDTO) => new Note
        {
            Id = noteDTO.Id,
            Title = noteDTO.Title,
            Content = noteDTO.Content,
            OwnerId = noteDTO.OwnerId,
            SessionId = noteDTO.SessionId,
            CreatedDate = noteDTO.CreatedDate,
            ModifiedDate = noteDTO.ModifiedDate,
            InGameDate = noteDTO.InGameDate,
            NoteTypeId = noteDTO.NoteTypeId,
            ToLinkedNotes = noteDTO.ToLinkedNotes.Select(nl => (NoteLink)nl).ToList(),
            FromLinkedNotes = noteDTO.FromLinkedNotes.Select(nl => (NoteLink)nl).ToList(),
            //AccessControlList = noteDTO.AccessControlList.Select(ace => (NoteAccess)ace).ToList(),
            NoteGenerators = noteDTO.NoteGenerators.Select(g => (NoteGenerator)g).ToList()
        };

        public static Note ToNote(NoteDTO noteDTO)
        {
            return new Note
            {
                Id = noteDTO.Id,
                Title = noteDTO.Title,
                Content = noteDTO.Content,
                OwnerId = null,
                SessionId = noteDTO.SessionId,
                CreatedDate = noteDTO.CreatedDate,
                ModifiedDate = noteDTO.ModifiedDate,
                InGameDate = noteDTO.InGameDate,
                NoteTypeId = noteDTO.NoteTypeId,
                ToLinkedNotes = noteDTO.ToLinkedNotes.Select(nl => NoteLinkDTO.ToNoteLink(nl)).ToList(),
                FromLinkedNotes = noteDTO.FromLinkedNotes.Select(nl => NoteLinkDTO.ToNoteLink(nl)).ToList(),
                //ccessControlList = noteDTO.AccessControlList.Select(ace => (NoteAccess)ace).ToList(),
                NoteGenerators = noteDTO.NoteGenerators.Select(g => NoteGeneratorDTO.ToNoteGenerator(g)).ToList()
            };
        }

        //FromNote
        public static NoteDTO FromNote(Note note)
        {
            return new NoteDTO
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                OwnerId = null,
                SessionId = note.SessionId,
                CreatedDate = note.CreatedDate,
                ModifiedDate = note.ModifiedDate,
                InGameDate = note.InGameDate,
                NoteTypeId = note.NoteTypeId,
                ToLinkedNotes = note.ToLinkedNotes.Select(nl => NoteLinkDTO.FromNoteLink(nl)).ToList(),
                FromLinkedNotes = note.FromLinkedNotes.Select(nl => NoteLinkDTO.FromNoteLink(nl)).ToList(),
                //AccessControlList = note.AccessControlList.Select(ace => (NoteAccessDTO)ace).ToList(),
                NoteGenerators = note.NoteGenerators.Select(g => NoteGeneratorDTO.FromNoteGenerator(g)).ToList()
            };
        }



    }
}
