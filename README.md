# 🏟️ Système de Réservation de Terrains de Football

Application web complète de réservation de terrains de football développée avec ASP.NET Core MVC.

## 📋 Description

Cette application permet aux clients de réserver des créneaux horaires pour des terrains de football, aux fournisseurs de gérer leurs terrains et consulter leurs revenus, et aux administrateurs de superviser l'ensemble du système.

## ✨ Fonctionnalités Principales

### Pour les Clients
- 🔍 Recherche avancée de terrains avec filtres (ville, type, date, plage horaire)
- 🛒 Panier de réservation
- 💳 Paiement sécurisé via Stripe
- 📅 Consultation des réservations
- 🧾 Historique des factures

### Pour les Fournisseurs
- ➕ Ajout, modification et suppression de terrains
- 📊 Consultation des gains cumulés
- 💰 Suivi des revenus par terrain
- 📈 Statistiques détaillées

### Pour les Administrateurs
- 📊 Dashboard avec statistiques globales
- 👥 Gestion des utilisateurs (avec intégration API DummyJSON)
- 🏟️ Vue d'ensemble de tous les terrains
- 📋 Gestion des réservations

## 🛠️ Technologies Utilisées

- **Backend** : ASP.NET Core 8.0 MVC
- **Base de données** : SQL Server avec Entity Framework Core
- **Authentification** : ASP.NET Core Identity
- **Paiement** : Stripe API
- **Frontend** : Razor Views, Bootstrap 5, JavaScript
- **API externe** : DummyJSON (utilisateurs fictifs)

## 📦 Installation

### Prérequis
- .NET 8.0 SDK
- SQL Server (LocalDB ou SQL Server Express)
- Compte Stripe (pour les paiements)

### Étapes d'installation

1. **Cloner le dépôt**
```bash
git clone https://github.com/BOUBA7777/Tp-technologie-du-commerce-lectronique.git
cd Tp-technologie-du-commerce-lectronique
```

2. **Configurer les paramètres**
```bash
cp appsettings.Template.json appsettings.json
```

Éditez `appsettings.json` et ajoutez vos clés Stripe :
```json
{
  "Stripe": {
    "PublishableKey": "VOTRE_CLE_PUBLIQUE_STRIPE",
    "SecretKey": "VOTRE_CLE_SECRETE_STRIPE"
  }
}
```

3. **Restaurer les packages**
```bash
dotnet restore
```

4. **Créer la base de données**
```bash
dotnet ef database update
```

5. **Lancer l'application**
```bash
dotnet run
```

L'application sera accessible sur `https://localhost:7186`

## 👥 Comptes de Test

Après l'initialisation de la base de données, vous pouvez utiliser ces comptes :

- **Admin** : `admin@terrains.com` / `Admin123!`
- **Client** : `client@terrains.com` / `Client123!`
- **Fournisseur 1** : `fournisseur1@terrains.com` / `Fournisseur123!`
- **Fournisseur 2** : `fournisseur2@terrains.com` / `Fournisseur123!`

## 💳 Tests de Paiement Stripe

Utilisez ces numéros de carte de test :
- **Succès** : `4242 4242 4242 4242`
- **Échec** : `4000 0000 0000 9995`
- Date d'expiration : n'importe quelle date future
- CVC : n'importe quel code à 3 chiffres

## 📊 Tarification

| Type de Terrain | Prix par Créneau (1h30) |
|-----------------|-------------------------|
| 5-a-side        | $35.00                  |
| 7-a-side        | $55.00                  |
| 11-a-side       | $90.00                  |

## 🏗️ Structure du Projet

```
├── Controllers/          # Contrôleurs MVC
├── Models/              # Modèles de données
├── Views/               # Vues Razor
├── Services/            # Services métier
├── Data/                # Contexte et initialisation DB
├── Migrations/          # Migrations Entity Framework
├── wwwroot/             # Fichiers statiques (CSS, JS)
└── appsettings.json     # Configuration (non versionné)
```

## 👨‍💻 Auteurs

- **Aboubacar Tounkara** - Développement Backend
- **Eli Daniel Senyo** - Développement Frontend

## 📄 Licence

Projet académique - Technologies du Commerce Électronique (2 novembre 2025)

## 🔗 Liens Utiles

- [Documentation ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Stripe Documentation](https://stripe.com/docs)
- [Bootstrap 5](https://getbootstrap.com/)

---

⚠️ **Note** : Ce projet est à des fins éducatives. N'utilisez pas les clés API en production sans sécurisation appropriée.

