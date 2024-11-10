using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CampaignManager.Persistence.Models
{
    public class LoginViewModel
    {
        //[DisplayName("Name")]
        //public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [DisplayName("Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }

    public class RegisterViewModel
    {
        //[DisplayName("Name")]
        //public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [DisplayName("Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [DisplayName("Password confirmation")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string PasswordRepeat { get; set; } = null!;
    }
}
