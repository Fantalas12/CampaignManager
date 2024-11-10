using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace CampaignManager.Persistence.Models
{
 
    //Note is an extension of NoteType, which is a type of note just like inheritance in OOP
    public class Note
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        [JsonString]
        [DataType(DataType.MultilineText)]
        public string? Content { get; set; } // JSON-formatted text field
        public string? OwnerId { get; set; }
        public virtual ApplicationUser? Owner { get; set; }
        public int? SessionId { get; set; }
        public virtual Session? Session { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime InGameDate { get; set; } // The date which the note is relevant to the game
        public virtual ICollection<string> Tags { get; set; }
        public Guid? NoteTypeId { get; set; }
        public virtual NoteType? NoteType { get; set; }
        public virtual ICollection<NoteLink> ToLinkedNotes { get; set; }
        public virtual ICollection<NoteLink> FromLinkedNotes { get; set; }
        //public virtual ICollection<NoteAccess> AccessControlList { get; set; }
        //Additional generators for this specific note that are not in the NoteType class
        public virtual ICollection<NoteGenerator> NoteGenerators { get; set; }

        public Note()
        {
            Id = Guid.NewGuid();
            Title = string.Empty;
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            InGameDate = DateTime.Now;
            Tags = new List<string>();
            ToLinkedNotes = new List<NoteLink>();
            FromLinkedNotes = new List<NoteLink>();
            //AccessControlList = new List<NoteAccess>();
            NoteGenerators = new List<NoteGenerator>();
        }
    }

    public class JsonStringAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            try
            {
                JsonDocument.Parse(value.ToString()!);
                return ValidationResult.Success;
            }
            catch (JsonException)
            {
                return new ValidationResult("The field must be a valid JSON string.");
            }
        }
    }
}