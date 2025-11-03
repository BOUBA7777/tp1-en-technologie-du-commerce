using System.ComponentModel.DataAnnotations;

namespace TP1.Models
{
    public class PanierItem
    {
        public int Id { get; set; }

        [Required]
        public string UtilisateurId { get; set; } = string.Empty;

        [Required]
        public int CreneauId { get; set; }

        public DateTime DateAjout { get; set; } = DateTime.Now;

        // Navigation properties
        public Utilisateur Utilisateur { get; set; } = null!;
        public Creneau Creneau { get; set; } = null!;
    }
}


