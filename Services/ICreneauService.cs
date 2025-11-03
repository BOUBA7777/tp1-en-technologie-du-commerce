using TP1.Models;

namespace TP1.Services
{
    public interface ICreneauService
    {
        Task<IEnumerable<Creneau>> GetCreneauxDisponiblesAsync(DateTime? date = null, string? typeTerrain = null, string? localisation = null, string? recherche = null);
        Task<Creneau?> GetCreneauByIdAsync(int id);
        Task<bool> ReserverPlacesAsync(int creneauId, int quantite);
        Task LibererPlacesAsync(int creneauId, int quantite);
        Task<IEnumerable<Terrain>> GetTerrainsAsync();
    }
}


