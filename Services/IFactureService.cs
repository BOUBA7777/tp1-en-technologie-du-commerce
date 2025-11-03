using TP1.Models;

namespace TP1.Services
{
    public interface IFactureService
    {
        Task<Facture> CreerFactureAsync(int reservationId);
        Task<Facture?> GetFactureByReservationIdAsync(int reservationId);
        Task<IEnumerable<Facture>> GetFacturesByUtilisateurAsync(string utilisateurId);
    }
}


