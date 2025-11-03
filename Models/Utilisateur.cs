using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TP1.Models
{
    public class Utilisateur : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Client"; // Client, Fournisseur, Admin

        public DateTime DateInscription { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<PanierItem> PanierItems { get; set; } = new List<PanierItem>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}


