using Microsoft.EntityFrameworkCore;
using TP1.Data;
using TP1.Models;

namespace TP1.Services
{
    public class FactureService : IFactureService
    {
        private readonly ApplicationDbContext _context;

        public FactureService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Facture> CreerFactureAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Creneau)
                .ThenInclude(c => c.Terrain)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
                throw new Exception("Réservation introuvable");

            var facture = new Facture
            {
                ReservationId = reservationId,
                Montant = reservation.MontantTotal,
                Date = DateTime.Now,
                NumeroFacture = $"FAC-{DateTime.Now:yyyyMMdd}-{reservationId:D6}"
            };

            _context.Factures.Add(facture);
            await _context.SaveChangesAsync();

            return facture;
        }

        public async Task<Facture?> GetFactureByReservationIdAsync(int reservationId)
        {
            return await _context.Factures
                .Include(f => f.Reservation)
                    .ThenInclude(r => r.Utilisateur)
                .Include(f => f.Reservation)
                    .ThenInclude(r => r.Creneau)
                    .ThenInclude(c => c.Terrain)
                .FirstOrDefaultAsync(f => f.ReservationId == reservationId);
        }

        public async Task<IEnumerable<Facture>> GetFacturesByUtilisateurAsync(string utilisateurId)
        {
            return await _context.Factures
                .Include(f => f.Reservation)
                    .ThenInclude(r => r.Utilisateur)
                .Include(f => f.Reservation)
                    .ThenInclude(r => r.Creneau)
                    .ThenInclude(c => c.Terrain)
                .Where(f => f.Reservation.UtilisateurId == utilisateurId)
                .OrderByDescending(f => f.Date)
                .ToListAsync();
        }
    }
}


