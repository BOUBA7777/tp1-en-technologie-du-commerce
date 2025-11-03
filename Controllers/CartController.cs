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
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly ICreneauService _creneauService;

        public CartController(ApplicationDbContext context, UserManager<Utilisateur> userManager, ICreneauService creneauService)
        {
            _context = context;
            _userManager = userManager;
            _creneauService = creneauService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var panierItems = await _context.PanierItems
                .Include(p => p.Creneau)
                .ThenInclude(c => c.Terrain)
                .Where(p => p.UtilisateurId == userId)
                .ToListAsync();

            ViewBag.Total = panierItems.Sum(p => p.Creneau.Prix);
            return View(panierItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int creneauId, string returnUrl = "")
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var creneau = await _creneauService.GetCreneauByIdAsync(creneauId);
            if (creneau == null || !creneau.EstDisponible)
            {
                TempData["ErrorMessage"] = "Ce créneau n'est plus disponible.";
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Details", "Home", new { id = creneauId });
            }

            // Vérifier si le créneau est déjà dans le panier
            var existingItem = await _context.PanierItems
                .FirstOrDefaultAsync(p => p.UtilisateurId == userId && p.CreneauId == creneauId);

            if (existingItem != null)
            {
                TempData["ErrorMessage"] = "Ce créneau est déjà dans votre panier.";
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Details", "Home", new { id = creneauId });
            }

            // Ajouter au panier
            var panierItem = new PanierItem
            {
                UtilisateurId = userId,
                CreneauId = creneauId,
                DateAjout = DateTime.Now
            };
            _context.PanierItems.Add(panierItem);

            // Marquer le créneau comme non disponible immédiatement
            await _creneauService.ReserverPlacesAsync(creneauId, 1);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "✅ Créneau ajouté au panier ! Vous pouvez continuer à réserver d'autres créneaux ou aller au panier pour payer.";
            
            // Rediriger vers la page de recherche pour continuer à réserver
            return RedirectToAction("Reserver", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var userId = _userManager.GetUserId(User);
            var panierItem = await _context.PanierItems
                .Include(p => p.Creneau)
                .FirstOrDefaultAsync(p => p.Id == id && p.UtilisateurId == userId);

            if (panierItem == null)
            {
                return NotFound();
            }

            // Libérer le créneau (le rendre disponible à nouveau)
            await _creneauService.LibererPlacesAsync(panierItem.CreneauId, 1);

            _context.PanierItems.Remove(panierItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Article retiré du panier.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var userId = _userManager.GetUserId(User);
            var panierItems = await _context.PanierItems
                .Include(p => p.Creneau)
                .Where(p => p.UtilisateurId == userId)
                .ToListAsync();

            // Libérer tous les créneaux
            foreach (var item in panierItems)
            {
                await _creneauService.LibererPlacesAsync(item.CreneauId, 1);
            }

            _context.PanierItems.RemoveRange(panierItems);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Panier vidé.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Json(new { count = 0 });
            }

            var count = await _context.PanierItems
                .Where(p => p.UtilisateurId == userId)
                .CountAsync();

            return Json(new { count });
        }
    }
}


