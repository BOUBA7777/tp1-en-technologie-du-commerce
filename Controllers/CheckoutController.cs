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
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilisateur> _userManager;
        private readonly IPaiementService _paiementService;
        private readonly IFactureService _factureService;
        private readonly ICreneauService _creneauService;
        private readonly IConfiguration _configuration;

        public CheckoutController(
            ApplicationDbContext context,
            UserManager<Utilisateur> userManager,
            IPaiementService paiementService,
            IFactureService factureService,
            ICreneauService creneauService,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _paiementService = paiementService;
            _factureService = factureService;
            _creneauService = creneauService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var panierItems = await _context.PanierItems
                .Include(p => p.Creneau)
                .ThenInclude(c => c.Terrain)
                .Where(p => p.UtilisateurId == userId)
                .ToListAsync();

            if (!panierItems.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction("Index", "Cart");
            }

            var total = panierItems.Sum(p => p.Creneau.Prix);
            
            ViewBag.Total = total;
            ViewBag.StripePublishableKey = _configuration["Stripe:PublishableKey"];
            
            return View(panierItems);
        }

        [HttpPost]
        [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreatePaymentIntent()
        {
            var userId = _userManager.GetUserId(User);
            var panierItems = await _context.PanierItems
                .Include(p => p.Creneau)
                    .ThenInclude(c => c.Terrain)
                .Where(p => p.UtilisateurId == userId)
                .ToListAsync();

            if (!panierItems.Any())
            {
                return BadRequest(new { error = "Panier vide" });
            }

            var total = panierItems.Sum(p => p.Creneau.Prix);

            // Créer la description du paiement avec les noms des terrains
            var terrainsNoms = panierItems
                .Select(p => p.Creneau.Terrain.Nom)
                .Distinct()
                .ToList();
            
            string description = panierItems.Count == 1
                ? $"Réservation - {panierItems[0].Creneau.Terrain.Nom} - {panierItems[0].Creneau.Date:dd/MM/yyyy} {panierItems[0].Creneau.HeureDebut:hh\\:mm}"
                : $"Réservation de {panierItems.Count} créneaux - {string.Join(", ", terrainsNoms)}";

            // Créer le PaymentIntent Stripe AVANT de créer les réservations
            var (clientSecret, paymentIntentId) = await _paiementService.CreerPaymentIntentAsync(total, description);

            // Stocker le PaymentIntentId en session pour le lier aux réservations
            HttpContext.Session.SetString("CurrentPaymentIntentId", paymentIntentId);

            return Ok(new { clientSecret });
        }

        [HttpPost]
        [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken]
        public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmationModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                
                // Confirmer le paiement Stripe
                var confirmed = await _paiementService.ConfirmerPaiementAsync(model.PaymentIntentId);

                if (!confirmed)
                {
                    return BadRequest(new { error = "Le paiement n'a pas pu être confirmé avec Stripe" });
                }

                // Récupérer les articles du panier
                var panierItems = await _context.PanierItems
                    .Include(p => p.Creneau)
                    .Where(p => p.UtilisateurId == userId)
                    .ToListAsync();

                if (!panierItems.Any())
                {
                    return BadRequest(new { error = "Panier vide" });
                }

                // Créer les réservations MAINTENANT (après confirmation du paiement)
                var reservations = new List<Reservation>();
                foreach (var item in panierItems)
                {
                    var reservation = new Reservation
                    {
                        UtilisateurId = userId!,
                        CreneauId = item.CreneauId,
                        MontantTotal = item.Creneau.Prix,
                        Statut = "Payee",
                        DateReservation = DateTime.Now
                    };
                    _context.Reservations.Add(reservation);
                    reservations.Add(reservation);
                }

                await _context.SaveChangesAsync();

                // Enregistrer le paiement lié à la première réservation
                var total = panierItems.Sum(p => p.Creneau.Prix);
                await _paiementService.EnregistrerPaiementAsync(reservations.First().Id, model.PaymentIntentId, total);

                // Créer les factures
                foreach (var reservation in reservations)
                {
                    await _factureService.CreerFactureAsync(reservation.Id);
                }

                // Vider le panier
                _context.PanierItems.RemoveRange(panierItems);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Paiement confirmé avec succès" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Erreur lors de la confirmation: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }
    }

    public class PaymentConfirmationModel
    {
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}


