using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampaignManager.Persistence.Models
{
    /*
    public class ValidGuidAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || (value is string str && Guid.TryParse(str, out _)))
            {
                return ValidationResult.Success!;
            }
            return new ValidationResult("Invalid GUID format.");
        }
    } */

    public class NoteType
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? OwnerId { get; set; }
        public virtual ApplicationUser? Owner { get; set; }

        //[ValidGuid]
        public Guid? PlayerTemplateId { get; set; } = null;
        public virtual Template? PlayerTemplate { get; set; }
        //[ValidGuid]
        public Guid? GameMasterTemplateId { get; set; } = null;
        public virtual Template? GameMasterTemplate { get; set; }
        /*
        // Templates for displaying and viewing notes
        //[ValidGuid]
        public Guid? PlayerViewTemplateId { get; set; } = null;
        public virtual Template? PlayerViewTemplate { get; set; }

        //[ValidGuid]
        public Guid? GameMasterViewTemplateId { get; set; } = null;
        public virtual Template? GameMasterViewTemplate { get; set; }

        // Templates for editing notes
        //[ValidGuid]
        public Guid? PlayerEditTemplateId { get; set; } = null;
        public virtual Template? PlayerEditTemplate { get; set; }

        //[ValidGuid]
        public Guid? GameMasterEditTemplateId { get; set; } = null;
        public virtual Template? GameMasterEditTemplate { get; set; }
        */

        public virtual ICollection<Note> Notes { get; set; }
        public virtual ICollection<Generator> Generators { get; set; }

        public NoteType()
        {
            Name = string.Empty;
            Id = Guid.NewGuid();
            Notes = new List<Note>();
            Generators = new List<Generator>();
        }
    }
}

