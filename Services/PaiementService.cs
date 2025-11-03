using Microsoft.EntityFrameworkCore;
using Stripe;
using TP1.Data;
using TP1.Models;

namespace TP1.Services
{
    public class PaiementService : IPaiementService
    {
        private readonly ApplicationDbContext _context;

        public PaiementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(string ClientSecret, string PaymentIntentId)> CreerPaymentIntentAsync(decimal montant, string? description = null, string devise = "eur")
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(montant * 100), // Stripe utilise les centimes
                Currency = devise,
                Description = description ?? "Réservation de terrain(s) de football",
                PaymentMethodTypes = new List<string> { "card" } // Uniquement les cartes bancaires
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);
            return (paymentIntent.ClientSecret, paymentIntent.Id);
        }

        public async Task<Paiement> EnregistrerPaiementAsync(int reservationId, string paymentIntentId, decimal montant)
        {
            var paiement = new Paiement
            {
                ReservationId = reservationId,
                StripePaymentIntentId = paymentIntentId,
                Montant = montant,
                Statut = "EnAttente",
                DatePaiement = DateTime.Now
            };

            _context.Paiements.Add(paiement);
            await _context.SaveChangesAsync();
            return paiement;
        }

        public async Task<bool> ConfirmerPaiementAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                // Vérifier si le paiement a réussi
                if (paymentIntent.Status == "succeeded")
                {
                    return true;
                }
                
                return false;
            }
            catch (StripeException ex)
            {
                // Logger l'erreur
                Console.WriteLine($"Erreur Stripe: {ex.Message}");
                return false;
            }
        }
    }
}


