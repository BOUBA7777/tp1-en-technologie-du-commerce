using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP1.Data;
using TP1.Services;

namespace TP1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDummyJsonService _dummyJsonService;

        public AdminController(ApplicationDbContext context, IDummyJsonService dummyJsonService)
        {
            _context = context;
            _dummyJsonService = dummyJsonService;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Statistiques générales
            var totalReservations = await _context.Reservations.CountAsync();
            var reservationsPayees = await _context.Reservations.CountAsync(r => r.Statut == "Payee");
            var revenuTotal = await _context.Reservations
                .Where(r => r.Statut == "Payee")
                .SumAsync(r => r.MontantTotal);

            // Top clients
            var topClients = await _context.Reservations
                .Include(r => r.Utilisateur)
                .Where(r => r.Statut == "Payee")
                .GroupBy(r => new { r.UtilisateurId, r.Utilisateur.Nom, r.Utilisateur.Email })
                .Select(g => new
                {
                    Nom = g.Key.Nom,
                    Email = g.Key.Email,
                    TotalReservations = g.Count(),
                    TotalDepense = g.Sum(r => r.MontantTotal)
                })
                .OrderByDescending(x => x.TotalDepense)
                .Take(10)
                .ToListAsync();

            // Taux de remplissage par terrain
            var terrains = await _context.Terrains
                .Include(t => t.Creneaux)
                .ToListAsync();

            var tauxRemplissage = terrains.Select(t => new
            {
                TerrainNom = t.Nom,
                Type = t.Type,
                TotalCreneaux = t.Creneaux.Count,
                CreneauxReserves = t.Creneaux.Count(c => !c.EstDisponible),
                TauxRemplissage = t.Creneaux.Any() 
                    ? (double)t.Creneaux.Count(c => !c.EstDisponible) / t.Creneaux.Count * 100 
                    : 0
            }).ToList();

            // Réservations récentes
            var reservationsRecentes = await _context.Reservations
                .Include(r => r.Utilisateur)
                .Include(r => r.Creneau)
                .ThenInclude(c => c.Terrain)
                .OrderByDescending(r => r.DateReservation)
                .Take(20)
                .ToListAsync();

            // Revenus par jour (7 derniers jours)
            var revenusParJour = await _context.Reservations
                .Where(r => r.Statut == "Payee" && r.DateReservation >= DateTime.Now.AddDays(-7))
                .GroupBy(r => r.DateReservation.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(r => r.MontantTotal),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.TotalReservations = totalReservations;
            ViewBag.ReservationsPayees = reservationsPayees;
            ViewBag.RevenuTotal = revenuTotal;
            ViewBag.TopClients = topClients;
            ViewBag.TauxRemplissage = tauxRemplissage;
            ViewBag.ReservationsRecentes = reservationsRecentes;
            ViewBag.RevenusParJour = revenusParJour;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Reservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Utilisateur)
                .Include(r => r.Creneau)
                .ThenInclude(c => c.Terrain)
                .OrderByDescending(r => r.DateReservation)
                .ToListAsync();

            return View(reservations);
        }

        [HttpGet]
        public async Task<IActionResult> Terrains()
        {
            var terrains = await _context.Terrains
                .Include(t => t.Creneaux)
                .ToListAsync();

            return View(terrains);
        }

        [HttpGet]
        public async Task<IActionResult> Utilisateurs()
        {
            var utilisateurs = await _context.Users
                .Include(u => u.Reservations)
                .OrderByDescending(u => u.DateInscription)
                .ToListAsync();

            var utilisateursFictifs = await _dummyJsonService.GetUsersAsync(30);

            ViewBag.UtilisateursFictifs = utilisateursFictifs;
            ViewBag.TotalFictifs = utilisateursFictifs.Count;

            return View(utilisateurs);
        }
    }
}


