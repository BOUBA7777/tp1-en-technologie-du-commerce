using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TP1.Models
{
    public class Paiement
    {
        public int Id { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [Required]
        public string StripePaymentIntentId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        [Required]
        [StringLength(50)]
        public string Statut { get; set; } = "EnAttente"; // EnAttente, Reussi, Echoue

        public DateTime DatePaiement { get; set; } = DateTime.Now;

        // Navigation properties
        public Reservation Reservation { get; set; } = null!;
    }
}


