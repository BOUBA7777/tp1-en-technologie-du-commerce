using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TP1.Services;

namespace TP1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ApiDataController : Controller
    {
        private readonly IDummyJsonService _dummyJsonService;
        private readonly ILogger<ApiDataController> _logger;

        public ApiDataController(IDummyJsonService dummyJsonService, ILogger<ApiDataController> logger)
        {
            _dummyJsonService = dummyJsonService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Récupérer 30 utilisateurs depuis l'API Dummy JSON
                var users = await _dummyJsonService.GetUsersAsync(30);
                
                ViewBag.TotalUsers = users.Count;
                ViewBag.ApiSource = "Dummy JSON API (https://dummyjson.com/users)";
                
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des données de l'API");
                TempData["ErrorMessage"] = "Impossible de récupérer les données de l'API externe.";
                return View(new List<Models.DummyJsonUser>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _dummyJsonService.GetUserByIdAsync(id);
                
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Utilisateur non trouvé dans l'API.";
                    return RedirectToAction(nameof(Index));
                }
                
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération de l'utilisateur {id} depuis l'API");
                TempData["ErrorMessage"] = "Impossible de récupérer les détails de cet utilisateur.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}






