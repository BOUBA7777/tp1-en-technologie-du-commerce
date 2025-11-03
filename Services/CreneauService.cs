using Microsoft.EntityFrameworkCore;
using TP1.Data;
using TP1.Models;

namespace TP1.Services
{
    public class CreneauService : ICreneauService
    {
        private readonly ApplicationDbContext _context;

        public CreneauService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Creneau>> GetCreneauxDisponiblesAsync(
            DateTime? date = null, 
            string? typeTerrain = null, 
            string? localisation = null, 
            string? recherche = null)
        {
            var query = _context.Creneaux
                .Include(c => c.Terrain)
                .Where(c => c.EstDisponible && c.Date >= DateTime.Today)
                .AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(c => c.Date.Date == date.Value.Date);
            }

            if (!string.IsNullOrEmpty(typeTerrain))
            {
                query = query.Where(c => c.Terrain.Type == typeTerrain);
            }

            if (!string.IsNullOrEmpty(localisation))
            {
                query = query.Where(c => c.Terrain.Localisation.Contains(localisation));
            }

            if (!string.IsNullOrEmpty(recherche))
            {
                // Correspondance exacte pour le nom du terrain (filtre dropdown)
                // Ou recherche partielle si c'est une vraie recherche libre
                query = query.Where(c => c.Terrain.Nom == recherche);
            }

            return await query.OrderBy(c => c.Date).ThenBy(c => c.HeureDebut).ToListAsync();
        }

        public async Task<Creneau?> GetCreneauByIdAsync(int id)
        {
            return await _context.Creneaux
                .Include(c => c.Terrain)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> ReserverPlacesAsync(int creneauId, int quantite)
        {
            var creneau = await _context.Creneaux.FindAsync(creneauId);
            if (creneau == null || !creneau.EstDisponible)
            {
                return false;
            }

            creneau.EstDisponible = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task LibererPlacesAsync(int creneauId, int quantite)
        {
            var creneau = await _context.Creneaux.FindAsync(creneauId);
            if (creneau != null)
            {
                creneau.EstDisponible = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Terrain>> GetTerrainsAsync()
        {
            return await _context.Terrains
                .Include(t => t.Creneaux)
                .OrderBy(t => t.Nom)
                .ToListAsync();
        }
    }
}


