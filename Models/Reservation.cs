using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TP1.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public string UtilisateurId { get; set; } = string.Empty;

        [Required]
        public int CreneauId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontantTotal { get; set; }

        [Required]
        [StringLength(50)]
        public string Statut { get; set; } = "EnAttente"; // EnAttente, Payee, Annulee

        public DateTime DateReservation { get; set; } = DateTime.Now;

        // Navigation properties
        public Utilisateur Utilisateur { get; set; } = null!;
        public Creneau Creneau { get; set; } = null!;
        public Paiement? Paiement { get; set; }
        public Facture? Facture { get; set; }
    }
}


