using TP1.Models;

namespace TP1.Services
{
    public interface IPaiementService
    {
        Task<(string ClientSecret, string PaymentIntentId)> CreerPaymentIntentAsync(decimal montant, string? description = null, string devise = "eur");
        Task<Paiement> EnregistrerPaiementAsync(int reservationId, string paymentIntentId, decimal montant);
        Task<bool> ConfirmerPaiementAsync(string paymentIntentId);
    }
}


