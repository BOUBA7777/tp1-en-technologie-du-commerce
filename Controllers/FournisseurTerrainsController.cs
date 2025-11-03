using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP1.Data;
using TP1.Models;

namespace TP1.Controllers
{
    [Authorize(Roles = "Fournisseur")]
    public class FournisseurTerrainsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;

        public FournisseurTerrainsController(ApplicationDbContext context, UserManager<Utilisateur> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Liste des terrains du fournisseur
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var terrains = await _context.Terrains
                .Include(t => t.Creneaux)
                .Where(t => t.FournisseurId == userId)
                .OrderBy(t => t.Nom)
                .ToListAsync();

            return View(terrains);
        }

        // GET: Ajouter un terrain
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ajouter un terrain
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Terrain terrain)
        {
            var userId = _userManager.GetUserId(User);
            terrain.FournisseurId = userId!;

            // Retirer FournisseurId de la validation car il est assigné automatiquement
            ModelState.Remove("FournisseurId");
            ModelState.Remove("Fournisseur");
            ModelState.Remove("Creneaux");

            if (ModelState.IsValid)
            {
                _context.Terrains.Add(terrain);
                await _context.SaveChangesAsync();

                // Créer automatiquement des créneaux pour les 14 prochains jours
                var creneaux = new List<Creneau>();
                var today = DateTime.Today;

                // Prix selon le type de terrain
                decimal prixCreneau = terrain.Type switch
                {
                    "11-a-side" => 90,
                    "7-a-side" => 55,
                    "5-a-side" => 35,
                    _ => 55
                };

                for (int day = 0; day < 14; day++)
                {
                    var date = today.AddDays(day);
                    
                    // Horaires de 8h à 21h30 avec 30 min de pause entre chaque créneau - 7 créneaux par jour
                    var horaires = new[]
                    {
                        (new TimeSpan(8, 0, 0), new TimeSpan(9, 30, 0)),
                        (new TimeSpan(10, 0, 0), new TimeSpan(11, 30, 0)),
                        (new TimeSpan(12, 0, 0), new TimeSpan(13, 30, 0)),
                        (new TimeSpan(14, 0, 0), new TimeSpan(15, 30, 0)),
                        (new TimeSpan(16, 0, 0), new TimeSpan(17, 30, 0)),
                        (new TimeSpan(18, 0, 0), new TimeSpan(19, 30, 0)),
                        (new TimeSpan(20, 0, 0), new TimeSpan(21, 30, 0))
                    };

                    foreach (var (debut, fin) in horaires)
                    {
                        creneaux.Add(new Creneau
                        {
                            TerrainId = terrain.Id,
                            Date = date,
                            HeureDebut = debut,
                            HeureFin = fin,
                            Prix = prixCreneau,
                            EstDisponible = true
                        });
                    }
                }

                _context.Creneaux.AddRange(creneaux);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"✅ Le terrain '{terrain.Nom}' a été ajouté avec succès ! {creneaux.Count} créneaux ont été générés automatiquement.";
                return RedirectToAction(nameof(Index));
            }

            // Afficher les erreurs en console pour debug
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Erreur validation : {error.ErrorMessage}");
            }

            return View(terrain);
        }

        // GET: Modifier un terrain
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var terrain = await _context.Terrains
                .FirstOrDefaultAsync(t => t.Id == id && t.FournisseurId == userId);

            if (terrain == null)
            {
                return NotFound();
            }

            return View(terrain);
        }

        // POST: Modifier un terrain
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Terrain terrain)
        {
            var userId = _userManager.GetUserId(User);

            if (id != terrain.Id)
            {
                return NotFound();
            }

            // Vérifier que le terrain appartient au fournisseur
            var existingTerrain = await _context.Terrains
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.FournisseurId == userId);

            if (existingTerrain == null)
            {
                return Forbid();
            }

            terrain.FournisseurId = userId!;

            // Retirer de la validation
            ModelState.Remove("FournisseurId");
            ModelState.Remove("Fournisseur");
            ModelState.Remove("Creneaux");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(terrain);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"✅ Le terrain '{terrain.Nom}' a été modifié avec succès !";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TerrainExists(terrain.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(terrain);
        }

        // GET: Supprimer un terrain
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var terrain = await _context.Terrains
                .Include(t => t.Creneaux)
                .FirstOrDefaultAsync(t => t.Id == id && t.FournisseurId == userId);

            if (terrain == null)
            {
                return NotFound();
            }

            return View(terrain);
        }

        // POST: Confirmer suppression
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var terrain = await _context.Terrains
                .Include(t => t.Creneaux)
                .FirstOrDefaultAsync(t => t.Id == id && t.FournisseurId == userId);

            if (terrain == null)
            {
                return NotFound();
            }

            // Vérifier s'il y a des créneaux avec des réservations
            var hasReservations = await _context.Reservations
                .AnyAsync(r => terrain.Creneaux.Select(c => c.Id).Contains(r.CreneauId));

            if (hasReservations)
            {
                TempData["ErrorMessage"] = "❌ Impossible de supprimer ce terrain car il a des réservations existantes.";
                return RedirectToAction(nameof(Index));
            }

            _context.Terrains.Remove(terrain);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"✅ Le terrain '{terrain.Nom}' a été supprimé avec succès !";
            return RedirectToAction(nameof(Index));
        }

        // GET: Gains cumulés
        public async Task<IActionResult> Gains()
        {
            var userId = _userManager.GetUserId(User);
            
            // Récupérer les terrains du fournisseur
            var terrainsIds = await _context.Terrains
                .Where(t => t.FournisseurId == userId)
                .Select(t => t.Id)
                .ToListAsync();

            // Récupérer toutes les réservations payées des terrains du fournisseur
            var reservations = await _context.Reservations
                .Include(r => r.Utilisateur)
                .Include(r => r.Creneau)
                    .ThenInclude(c => c.Terrain)
                .Where(r => r.Statut == "Payee" && terrainsIds.Contains(r.Creneau.TerrainId))
                .OrderByDescending(r => r.DateReservation)
                .ToListAsync();

            // Calculer les statistiques
            var totalRevenu = reservations.Sum(r => r.MontantTotal);
            var totalReservations = reservations.Count;

            // Gains par terrain
            var gainsParTerrain = reservations
                .GroupBy(r => new { r.Creneau.Terrain.Id, r.Creneau.Terrain.Nom })
                .Select(g => new
                {
                    TerrainNom = g.Key.Nom,
                    TotalReservations = g.Count(),
                    TotalGains = g.Sum(r => r.MontantTotal)
                })
                .OrderByDescending(x => x.TotalGains)
                .ToList();

            ViewBag.TotalRevenu = totalRevenu;
            ViewBag.TotalReservations = totalReservations;
            ViewBag.GainsParTerrain = gainsParTerrain;

            return View(reservations);
        }

        private bool TerrainExists(int id)
        {
            return _context.Terrains.Any(e => e.Id == id);
        }
    }
}

