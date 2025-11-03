using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TP1.Models;
using TP1.Services;

namespace TP1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICreneauService _creneauService;

        public HomeController(ILogger<HomeController> logger, ICreneauService creneauService)
        {
            _logger = logger;
            _creneauService = creneauService;
        }

        public async Task<IActionResult> Index()
        {
            // Page d'accueil : affiche uniquement les terrains
            var terrains = await _creneauService.GetTerrainsAsync();
            return View(terrains);
        }

        public async Task<IActionResult> DetailsTerrain(int id)
        {
            // Récupérer le terrain avec ses créneaux
            var terrains = await _creneauService.GetTerrainsAsync();
            var terrain = terrains.FirstOrDefault(t => t.Id == id);
            
            if (terrain == null)
            {
                return NotFound();
            }

            return View(terrain);
        }

        public async Task<IActionResult> ReserverTerrain(int id, DateTime? date)
        {
            // Récupérer le terrain spécifique
            var terrains = await _creneauService.GetTerrainsAsync();
            var terrain = terrains.FirstOrDefault(t => t.Id == id);
            
            if (terrain == null)
            {
                return NotFound();
            }

            ViewBag.Date = date;

            // Récupérer TOUS les créneaux disponibles
            var tousCreneaux = await _creneauService.GetCreneauxDisponiblesAsync(date, null, null, null);
            
            // Filtrer pour garder SEULEMENT les créneaux de CE terrain
            var creneaux = tousCreneaux.Where(c => c.TerrainId == id).ToList();
            
            ViewBag.Creneaux = creneaux;

            return View(terrain);
        }

        public async Task<IActionResult> Reserver(DateTime? date, string? ville, string? terrain, string? type, string? plageHoraire, string? recherche)
        {
            ViewBag.Date = date;
            ViewBag.Ville = ville;
            ViewBag.Terrain = terrain;
            ViewBag.Type = type;
            ViewBag.PlageHoraire = plageHoraire;
            ViewBag.Recherche = recherche;

            // Récupérer tous les terrains pour les dropdowns
            var terrains = await _creneauService.GetTerrainsAsync();
            ViewBag.Terrains = terrains;

            // Récupérer les créneaux avec les filtres
            IEnumerable<Creneau> creneaux;
            
            // Si dropdown terrain sélectionné, l'utiliser (correspondance exacte)
            if (!string.IsNullOrEmpty(terrain))
            {
                creneaux = await _creneauService.GetCreneauxDisponiblesAsync(
                    date: date,
                    typeTerrain: type,
                    localisation: ville,
                    recherche: terrain
                );
            }
            // Sinon, si recherche textuelle, l'utiliser (recherche partielle)
            else if (!string.IsNullOrEmpty(recherche))
            {
                var tousCreneaux = await _creneauService.GetCreneauxDisponiblesAsync(
                    date: date,
                    typeTerrain: type,
                    localisation: ville,
                    recherche: null
                );
                
                // Recherche partielle dans le nom et la description du terrain
                creneaux = tousCreneaux.Where(c => 
                    c.Terrain.Nom.Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
                    (c.Terrain.Description != null && c.Terrain.Description.Contains(recherche, StringComparison.OrdinalIgnoreCase))
                );
            }
            else
            {
                creneaux = await _creneauService.GetCreneauxDisponiblesAsync(
                    date: date,
                    typeTerrain: type,
                    localisation: ville,
                    recherche: null
                );
            }

            // Filtrer par plage horaire si spécifié
            if (!string.IsNullOrEmpty(plageHoraire))
            {
                creneaux = plageHoraire.ToLower() switch
                {
                    "matin" => creneaux.Where(c => c.HeureDebut.Hours >= 6 && c.HeureDebut.Hours < 12),
                    "apres-midi" => creneaux.Where(c => c.HeureDebut.Hours >= 12 && c.HeureDebut.Hours < 18),
                    "soir" => creneaux.Where(c => c.HeureDebut.Hours >= 18 && c.HeureDebut.Hours < 23),
                    _ => creneaux
                };
            }

            return View(creneaux);
        }

        public async Task<IActionResult> Details(int id)
        {
            var creneau = await _creneauService.GetCreneauByIdAsync(id);
            if (creneau == null)
            {
                return NotFound();
            }

            return View(creneau);
        }

        public async Task<IActionResult> Terrains()
        {
            var terrains = await _creneauService.GetTerrainsAsync();
            return View(terrains);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
