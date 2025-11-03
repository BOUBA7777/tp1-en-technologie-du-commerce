using System.ComponentModel.DataAnnotations;

namespace TP1.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime DateInscription { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string? ConfirmNewPassword { get; set; }
    }
}


