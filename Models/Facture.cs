using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TP1.Models
{
    public class Facture
    {
        public int Id { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        public string? UrlPdf { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroFacture { get; set; } = string.Empty;

        // Navigation properties
        public Reservation Reservation { get; set; } = null!;
    }
}


