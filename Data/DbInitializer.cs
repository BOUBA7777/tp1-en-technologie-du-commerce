using Microsoft.AspNetCore.Identity;
using TP1.Models;

namespace TP1.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<Utilisateur> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Créer la base de données si elle n'existe pas
            await context.Database.EnsureCreatedAsync();

            // Créer les rôles
            string[] roleNames = { "Admin", "Client", "Fournisseur" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Créer un admin par défaut
            var adminEmail = "admin@terrains.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new Utilisateur
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Nom = "Administrateur",
                    Role = "Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Créer des fournisseurs
            var fournisseur1Email = "fournisseur1@terrains.com";
            Utilisateur? fournisseur1 = await userManager.FindByEmailAsync(fournisseur1Email);
            if (fournisseur1 == null)
            {
                fournisseur1 = new Utilisateur
                {
                    UserName = fournisseur1Email,
                    Email = fournisseur1Email,
                    Nom = "Sports Lévis Inc.",
                    Role = "Fournisseur",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(fournisseur1, "Fournisseur123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(fournisseur1, "Fournisseur");
                }
            }

            var fournisseur2Email = "fournisseur2@terrains.com";
            Utilisateur? fournisseur2 = await userManager.FindByEmailAsync(fournisseur2Email);
            if (fournisseur2 == null)
            {
                fournisseur2 = new Utilisateur
                {
                    UserName = fournisseur2Email,
                    Email = fournisseur2Email,
                    Nom = "Québec Football Arena",
                    Role = "Fournisseur",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(fournisseur2, "Fournisseur123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(fournisseur2, "Fournisseur");
                }
            }

            // Créer un client par défaut
            var clientEmail = "client@terrains.com";
            if (await userManager.FindByEmailAsync(clientEmail) == null)
            {
                var client = new Utilisateur
                {
                    UserName = clientEmail,
                    Email = clientEmail,
                    Nom = "Client Test",
                    Role = "Client",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(client, "Client123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(client, "Client");
                }
            }

            // Vérifier si des terrains existent déjà
            if (context.Terrains.Any())
            {
                return; // La DB est déjà seeded
            }

            // S'assurer que les fournisseurs sont bien créés
            fournisseur1 = await userManager.FindByEmailAsync(fournisseur1Email);
            fournisseur2 = await userManager.FindByEmailAsync(fournisseur2Email);

            if (fournisseur1 == null || fournisseur2 == null)
            {
                throw new Exception("Impossible de créer les terrains : fournisseurs non trouvés");
            }

            // Créer les terrains - Lévis et Québec
            var terrains = new Terrain[]
            {
                // Terrains à Lévis (3) - Fournisseur 1
                new Terrain
                {
                    Nom = "Terrain Central de Lévis",
                    Type = "11-a-side",
                    Localisation = "Lévis",
                    ImageUrl = "https://images.unsplash.com/photo-1574629810360-7efbbe195018?w=800",
                    Description = "Grand terrain de football en gazon synthétique de dernière génération. Situé au cœur de Lévis avec vue sur le fleuve Saint-Laurent. Équipé d'éclairage nocturne LED et de vestiaires modernes.",
                    FournisseurId = fournisseur1.Id
                },
                new Terrain
                {
                    Nom = "Complexe Sportif Desjardins",
                    Type = "5-a-side",
                    Localisation = "Lévis",
                    ImageUrl = "https://images.unsplash.com/photo-1529900748604-07564a03e7a6?w=800",
                    Description = "Petit terrain couvert idéal pour les matchs rapides entre amis. Situé près du centre-ville de Lévis. Disponible 7j/7 avec réservation en ligne et stationnement gratuit.",
                    FournisseurId = fournisseur1.Id
                },
                new Terrain
                {
                    Nom = "Stade Saint-Romuald",
                    Type = "7-a-side",
                    Localisation = "Lévis",
                    ImageUrl = "https://images.unsplash.com/photo-1543351611-58f69d7c1781?w=800",
                    Description = "Terrain semi-professionnel avec tribunes. Situé dans le secteur de Saint-Romuald à Lévis. Idéal pour tournois et compétitions amicales. Vestiaires, cantine et terrasse disponibles.",
                    FournisseurId = fournisseur1.Id
                },
                // Terrains à Québec (3) - Fournisseur 2
                new Terrain
                {
                    Nom = "Centre Sportif Sainte-Foy",
                    Type = "11-a-side",
                    Localisation = "Québec",
                    ImageUrl = "https://images.unsplash.com/photo-1574629810360-7efbbe195018?w=800",
                    Description = "Grand stade professionnel situé à Sainte-Foy, Québec. Gazon synthétique haute qualité, éclairage professionnel et vestiaires avec douches. Parfait pour matchs officiels et entraînements sérieux.",
                    FournisseurId = fournisseur2.Id
                },
                new Terrain
                {
                    Nom = "Arena Indoor Québec",
                    Type = "5-a-side",
                    Localisation = "Québec",
                    ImageUrl = "https://images.unsplash.com/photo-1529900748604-07564a03e7a6?w=800",
                    Description = "Terrain intérieur climatisé situé dans le secteur Lebourgneuf. Parfait pour jouer toute l'année peu importe la météo. Stationnement intérieur inclus.",
                    FournisseurId = fournisseur2.Id
                },
                new Terrain
                {
                    Nom = "Complexe PEPS Université Laval",
                    Type = "7-a-side",
                    Localisation = "Québec",
                    ImageUrl = "https://images.unsplash.com/photo-1543351611-58f69d7c1781?w=800",
                    Description = "Terrain de qualité universitaire situé sur le campus de l'Université Laval. Installation moderne avec gradins, vestiaires premium et équipements sportifs complets.",
                    FournisseurId = fournisseur2.Id
                }
            };

            context.Terrains.AddRange(terrains);
            await context.SaveChangesAsync();

            // Créer les créneaux pour les 14 prochains jours (semaine en cours + semaine suivante)
            var creneaux = new List<Creneau>();
            var today = DateTime.Today;

            for (int day = 0; day < 14; day++)
            {
                var date = today.AddDays(day);

                foreach (var terrain in terrains)
                {
                    // Prix selon le type de terrain
                    decimal prixCreneau = terrain.Type switch
                    {
                        "11-a-side" => 90,
                        "7-a-side" => 55,
                        "5-a-side" => 35,
                        _ => 55
                    };

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
            }

            context.Creneaux.AddRange(creneaux);
            await context.SaveChangesAsync();
        }
    }
}


