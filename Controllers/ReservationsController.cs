using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TP1.Data;
using TP1.Models;
using TP1.Services;

namespace TP1.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly IFactureService _factureService;

        public ReservationsController(
            ApplicationDbContext context,
            UserManager<Utilisateur> userManager,
            IFactureService factureService)
        {
            _context = context;
            _userManager = userManager;
            _factureService = factureService;
        }

        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User);
            var reservations = await _context.Reservations
                .Include(r => r.Creneau)
                .ThenInclude(c => c.Terrain)
                .Where(r => r.UtilisateurId == userId)
                .OrderByDescending(r => r.DateReservation)
                .ToListAsync();

            return View(reservations);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User);
            IEnumerable<Facture> factures;

            if (User.IsInRole("Fournisseur"))
            {
                // Fournisseur : voir les factures générées sur SES terrains
                var terrainIds = await _context.Terrains
                    .Where(t => t.FournisseurId == userId)
                    .Select(t => t.Id)
                    .ToListAsync();

                factures = await _context.Factures
                    .Include(f => f.Reservation)
                        .ThenInclude(r => r.Utilisateur)
                    .Include(f => f.Reservation)
                        .ThenInclude(r => r.Creneau)
                        .ThenInclude(c => c.Terrain)
                    .Where(f => terrainIds.Contains(f.Reservation.Creneau.TerrainId))
                    .OrderByDescending(f => f.Date)
                    .ToListAsync();
            }
            else
            {
                // Client ou Admin : voir ses propres factures
                factures = await _factureService.GetFacturesByUtilisateurAsync(userId!);
            }

            return View(factures);
        }

        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            var userId = _userManager.GetUserId(User);
            var facture = await _factureService.GetFactureByReservationIdAsync(id);

            if (facture == null)
            {
                return NotFound();
            }

            // Vérifier les droits d'accès
            bool hasAccess = false;
            
            if (User.IsInRole("Admin"))
            {
                hasAccess = true;
            }
            else if (User.IsInRole("Fournisseur"))
            {
                // Fournisseur peut voir les factures de SES terrains
                hasAccess = facture.Reservation.Creneau.Terrain.FournisseurId == userId;
            }
            else
            {
                // Client peut voir ses propres factures
                hasAccess = facture.Reservation.UtilisateurId == userId;
            }

            if (!hasAccess)
            {
                return Forbid();
            }

            return View(facture);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var reservation = await _context.Reservations
                .Include(r => r.Creneau)
                .Include(r => r.Facture)
                .FirstOrDefaultAsync(r => r.Id == id && r.UtilisateurId == userId);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Réservation introuvable.";
                return RedirectToAction("MyBookings");
            }

            // Vérifier que la réservation est payée
            if (reservation.Statut != "Payee")
            {
                TempData["ErrorMessage"] = "Cette réservation ne peut pas être annulée.";
                return RedirectToAction("MyBookings");
            }

            // Vérifier le délai d'annulation - DEUX conditions :
            var delaiAnnulation = TimeSpan.FromHours(24);
            
            // Condition 1 : Moins de 24h depuis la réservation
            var dateLimiteAnnulation = reservation.DateReservation.Add(delaiAnnulation);
            if (DateTime.Now > dateLimiteAnnulation)
            {
                TempData["ErrorMessage"] = "Impossible d'annuler : le délai d'annulation (24h après la réservation) est dépassé.";
                return RedirectToAction("MyBookings");
            }
            
            // Condition 2 : Plus de 24h avant le créneau
            var dateTimeCreneau = reservation.Creneau.Date.Add(reservation.Creneau.HeureDebut);
            if (DateTime.Now > dateTimeCreneau.Subtract(delaiAnnulation))
            {
                TempData["ErrorMessage"] = "Impossible d'annuler : le créneau est dans moins de 24h.";
                return RedirectToAction("MyBookings");
            }

            // Annuler la réservation
            reservation.Statut = "Annulee";
            
            // Libérer le créneau
            reservation.Creneau.EstDisponible = true;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Réservation annulée avec succès. Le créneau est à nouveau disponible.";
            return RedirectToAction("MyBookings");
        }

        [HttpGet]
        [Authorize(Roles = "Fournisseur,Admin")]
        public async Task<IActionResult> Revenue()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound();
            }

            // Récupérer les IDs des terrains du fournisseur connecté
            var terrainIds = await _context.Terrains
                .Where(t => t.FournisseurId == userId)
                .Select(t => t.Id)
                .ToListAsync();

            // Filtrer les réservations pour les terrains du fournisseur uniquement
            var reservations = await _context.Reservations
                .Include(r => r.Creneau)
                .ThenInclude(c => c.Terrain)
                .Include(r => r.Utilisateur)
                .Where(r => r.Statut == "Payee" && terrainIds.Contains(r.Creneau.TerrainId))
                .OrderByDescending(r => r.DateReservation)
                .ToListAsync();

            var totalRevenu = reservations.Sum(r => r.MontantTotal);
            var reservationsParMois = reservations
                .GroupBy(r => new { r.DateReservation.Year, r.DateReservation.Month })
                .Select(g => new
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Total = g.Sum(r => r.MontantTotal),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            ViewBag.TotalRevenu = totalRevenu;
            ViewBag.ReservationsParMois = reservationsParMois;
            ViewBag.TotalReservations = reservations.Count;

            return View(reservations);
        }
    }
}


