/* using CampaignManager.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.DTO
{
    public class NoteAccessDTO
    {
        public Guid Id { get; set; }
        public Guid? NoteId { get; set; }
        public string? UserId { get; set; }
        public string AccessLevel { get; set; } = "Read";


        public static explicit operator NoteAccessDTO(NoteAccess ace) => new NoteAccessDTO
        {
            Id = ace.Id,
            NoteId = ace.NoteId,
            UserId = ace.UserId,
            AccessLevel = ace.AccessLevel
        };

        public static explicit operator NoteAccess(NoteAccessDTO aceDTO) => new NoteAccess
        {
            Id = aceDTO.Id,
            NoteId = aceDTO.NoteId,
            UserId = aceDTO.UserId,
            AccessLevel = aceDTO.AccessLevel
        };

    }
}
*/
