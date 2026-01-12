# Documentation Technique - RoomEase

## ?? Système de Réservation de Salles - Analyse Complète

---

## 1. Vue d'Ensemble et Architecture

### 1.1 Introduction au Projet

**RoomEase** est une application web de gestion et de réservation de salles de réunion développée avec ASP.NET Core 8 (Razor Pages). Elle permet aux utilisateurs de :
- Rechercher des salles disponibles selon des critères (capacité, équipements, créneaux horaires)
- Réserver des salles
- Gérer leurs réservations
- Recevoir des notifications sur l'état de leurs demandes

Les administrateurs peuvent :
- Gérer les salles et équipements
- Approuver ou refuser les réservations
- Superviser l'ensemble du système

---

### 1.2 Pattern Architecture : Razor Pages (Variante du MVC)

RoomEase utilise le pattern **Razor Pages**, qui est une variante simplifiée du pattern MVC dans ASP.NET Core.

#### ?? Comparaison avec MVC Traditionnel

| Composant | MVC Classique | Razor Pages (RoomEase) |
|-----------|---------------|------------------------|
| **Model** | Classes C# (Room, Reservation) | Classes C# (Room, Reservation) |
| **View** | Fichiers `.cshtml` | Fichiers `.cshtml` (Pages) |
| **Controller** | Classes Controller séparées | PageModel (`.cshtml.cs`) intégré |

#### ?? Explication des Composants

**1. Model (Modèle de Données)**
- **Rôle :** Représente les données et la logique métier
- **Exemples dans RoomEase :**
  ```csharp
  // Models/Room.cs
  public class Room
  {
      public int Id { get; set; }
      public string Name { get; set; }
      public int Capacity { get; set; }
      // ... Relations avec Equipment et Reservation
  }
  ```
  - `Room` : Représente une salle
  - `Reservation` : Représente une réservation
  - `AppUser` : Représente un utilisateur

**2. View (Vue)**
- **Rôle :** Affiche les données à l'utilisateur (interface HTML)
- **Exemples dans RoomEase :**
  ```razor
  <!-- Pages/Rooms/Index.cshtml -->
  @page
  @model RoomEase.Pages.Rooms.IndexModel
  
  <h1>Liste des Salles</h1>
  @foreach (var room in Model.AvailableRooms)
  {
      <div>@room.Name - Capacité: @room.Capacity</div>
  }
  ```

**3. PageModel (Contrôleur)**
- **Rôle :** Gère la logique de traitement des requêtes et prépare les données pour la vue
- **Exemples dans RoomEase :**
  ```csharp
  // Pages/Rooms/Index.cshtml.cs
  public class IndexModel : PageModel
  {
      public List<Room> AvailableRooms { get; set; }
      
      public async Task OnGetAsync()
      {
          // Récupère les salles depuis la base de données
          AvailableRooms = await _context.Rooms.ToListAsync();
      }
  }
  ```

#### ?? Flux de Données dans Razor Pages

```
Utilisateur ? Requête HTTP ? PageModel (OnGet/OnPost) 
    ?
PageModel interroge le Model (via DbContext)
    ?
Model retourne les données
    ?
PageModel prépare les données
    ?
Vue (.cshtml) affiche les données ? Réponse HTML ? Utilisateur
```

---

### 1.3 Stack Technologique

| Technologie | Version | Justification |
|-------------|---------|---------------|
| **.NET** | 8.0 | - Framework moderne et performant<br>- Support long terme (LTS)<br>- Performance optimisée |
| **Entity Framework Core** | 8.0 | - ORM puissant pour manipuler la base de données<br>- Code First (modèles ? base de données)<br>- LINQ pour requêtes type-safe |
| **SQL Server** | Express | - Base de données relationnelle fiable<br>- Intégration native avec .NET<br>- Transactions ACID |
| **ASP.NET Core Identity** | 8.0 | - Système d'authentification robuste<br>- Gestion des rôles (Admin/User)<br>- Sécurité des mots de passe (hashing) |
| **Bootstrap** | 5.3 | - Framework CSS responsive<br>- Interface moderne et professionnelle |

---

## 2. Structure de la Base de Données

### 2.1 Schéma Relationnel

```
???????????????????         ????????????????????
?   AspNetUsers   ?         ?   Rooms          ?
?  (AppUser)      ?         ?                  ?
???????????????????         ????????????????????
? Id (PK)         ?         ? Id (PK)          ?
? UserName        ?         ? Name             ?
? Email           ?         ? Capacity         ?
? FullName        ?         ? Description      ?
? Department      ?         ? IsAvailable      ?
???????????????????         ????????????????????
        ?                            ?
        ? 1                          ? 1
        ?                            ?
        ? *                          ? *
        ?                            ?
???????????????????         ????????????????????????
?  Reservations   ?         ?  RoomEquipments      ?
???????????????????         ????????????????????????
? Id (PK)         ?         ? RoomId (PK, FK)      ?
? UserId (FK)     ?         ? EquipmentId (PK, FK) ?
? RoomId (FK)     ?         ????????????????????????
? StartTime       ?                  ?
? EndTime         ?                  ? *
? Purpose         ?                  ?
? Status (Enum)   ?                  ? 1
???????????????????         ????????????????????
                            ?   Equipments     ?
???????????????????         ????????????????????
?  Notifications  ?         ? Id (PK)          ?
???????????????????         ? Name             ?
? Id (PK)         ?         ????????????????????
? UserId (FK)     ?
? Message         ?
? CreatedAt       ?
? IsRead          ?
???????????????????
```

---

### 2.2 Explication des Entités Clés

#### **1. AppUser (Utilisateur)**

```csharp
public class AppUser : IdentityUser
{
    public string FullName { get; set; }
    public string? Department { get; set; }
    
    // Relations
    public virtual ICollection<Reservation> Reservations { get; set; }
    public virtual ICollection<Notification> Notifications { get; set; }
}
```

**? Pourquoi hériter de `IdentityUser` ?**
- `IdentityUser` est une classe fournie par ASP.NET Core Identity
- Elle contient déjà toutes les propriétés nécessaires pour l'authentification :
  - `Id` (identifiant unique)
  - `UserName` (nom d'utilisateur)
  - `Email` (adresse email)
  - `PasswordHash` (mot de passe chiffré)
  - `SecurityStamp`, `ConcurrencyStamp`, etc.
- En héritant, on **étend** cette classe avec nos propres champs (`FullName`, `Department`)
- On évite de réinventer la roue pour la gestion de la sécurité

---

#### **2. Relation Many-to-Many : Room ? Equipment**

**?? Problématique :**
- Une salle peut avoir **plusieurs équipements** (ex: projecteur, tableau blanc)
- Un équipement peut être présent dans **plusieurs salles**

**?? Solution : Table de Jointure `RoomEquipment`**

```csharp
// Table de jointure
public class RoomEquipment
{
    public int RoomId { get; set; }
    public Room Room { get; set; }
    
    public int EquipmentId { get; set; }
    public Equipment Equipment { get; set; }
}
```

**Configuration dans `ApplicationDbContexte` :**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<RoomEquipment>(entity =>
    {
        // Clé composite (les deux colonnes forment la clé primaire)
        entity.HasKey(re => new { re.RoomId, re.EquipmentId });
        
        // Relation avec Room
        entity.HasOne(re => re.Room)
              .WithMany(r => r.RoomEquipments)
              .HasForeignKey(re => re.RoomId);
        
        // Relation avec Equipment
        entity.HasOne(re => re.Equipment)
              .WithMany(e => e.RoomEquipments)
              .HasForeignKey(re => re.EquipmentId);
    });
}
```

**?? Exemple de Données :**

| RoomId | EquipmentId | ? Signification |
|--------|-------------|-----------------|
| 1      | 1           | Salle A a un Projecteur |
| 1      | 3           | Salle A a un Tableau blanc |
| 2      | 1           | Salle B a un Projecteur |

---

#### **3. Relations One-to-Many**

**a) User ? Reservation (Un utilisateur a plusieurs réservations)**

```csharp
public class AppUser : IdentityUser
{
    public virtual ICollection<Reservation> Reservations { get; set; }
}

public class Reservation
{
    public string UserId { get; set; }
    public virtual AppUser User { get; set; }
}
```

**b) Room ? Reservation (Une salle a plusieurs réservations)**

```csharp
public class Room
{
    public virtual ICollection<Reservation> Reservations { get; set; }
}

public class Reservation
{
    public int RoomId { get; set; }
    public virtual Room Room { get; set; }
}
```

---

### 2.3 Enum `ReservationStatus`

```csharp
public enum ReservationStatus
{
    Pending,   // En attente de validation admin
    Approved,  // Validée par admin
    Rejected,  // Refusée par admin
    Cancelled  // Annulée par l'utilisateur
}
```

**Conversion en String dans la base de données :**
```csharp
modelBuilder.Entity<Reservation>()
    .Property(r => r.Status)
    .HasConversion<string>(); // Stocké comme "Pending", "Approved", etc.
```

---

## 3. Analyse du Code par Module

### 3.1 Authentification avec ASP.NET Core Identity

#### Configuration dans `Program.cs`

```csharp
// Ajout d'Identity au conteneur de services
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Règles de mot de passe
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    
    // Email unique requis
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContexte>()
.AddDefaultTokenProviders();

// Configuration des cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
```

**?? Explication du Flux d'Authentification :**

1. **Inscription (`Register.cshtml.cs`) :**
```csharp
var user = new AppUser
{
    UserName = Input.UserName,
    Email = Input.Email,
    FullName = Input.FullName
};

// Création de l'utilisateur avec mot de passe hashé
var result = await _userManager.CreateAsync(user, Input.Password);

if (result.Succeeded)
{
    // Attribution du rôle "User" par défaut
    await _userManager.AddToRoleAsync(user, "User");
    
    // Connexion automatique
    await _signInManager.SignInAsync(user, isPersistent: false);
}
```

2. **Connexion (`Login.cshtml.cs`) :**
```csharp
// Détection si l'input est un email ou username
string userName = Input.EmailOrUserName;
if (Input.EmailOrUserName.Contains("@"))
{
    var userByEmail = await _userManager.FindByEmailAsync(Input.EmailOrUserName);
    if (userByEmail != null)
        userName = userByEmail.UserName;
}

// Authentification
var result = await _signInManager.PasswordSignInAsync(
    userName, 
    Input.Password, 
    Input.RememberMe, 
    lockoutOnFailure: false
);

if (result.Succeeded)
{
    // Cookie créé automatiquement
    return LocalRedirect(returnUrl);
}
```

**?? Comment Fonctionnent les Cookies ?**
- Après connexion réussie, ASP.NET Core Identity crée un **cookie d'authentification**
- Ce cookie est envoyé à chaque requête HTTP
- Le serveur lit le cookie pour identifier l'utilisateur
- `User.Identity.Name` renvoie le username de l'utilisateur connecté
- `User.IsInRole("Admin")` vérifie si l'utilisateur a le rôle Admin

---

### 3.2 Gestion des Salles (Module Admin)

#### Flux Complet : Créer une Salle

**1. Affichage du Formulaire (GET) :**

```csharp
// Pages/Admin/Rooms/Create.cshtml.cs
[Authorize(Roles = "Admin")] // Seuls les admins peuvent accéder
public class CreateModel : PageModel
{
    public List<Equipment> AvailableEquipments { get; set; }
    
    public async Task<IActionResult> OnGetAsync()
    {
        // Charger tous les équipements pour les afficher dans le formulaire
        AvailableEquipments = await _context.Equipments.ToListAsync();
        return Page();
    }
}
```

**2. Soumission du Formulaire (POST) :**

```csharp
public async Task<IActionResult> OnPostAsync()
{
    // 1. Validation du modèle
    ModelState.Remove("Room.RoomEquipments"); // Ignorer les propriétés de navigation
    ModelState.Remove("Room.Reservations");
    
    if (!ModelState.IsValid)
    {
        // Recharger les équipements et réafficher le formulaire
        AvailableEquipments = await _context.Equipments.ToListAsync();
        return Page();
    }
    
    // 2. Créer l'objet Room
    var room = new Room
    {
        Name = Name,
        Capacity = Capacity,
        Description = Description,
        IsAvailable = IsAvailable
    };
    
    // 3. Sauvegarder dans la base de données
    _context.Rooms.Add(room);
    await _context.SaveChangesAsync(); // room.Id est maintenant généré
    
    // 4. Ajouter les équipements sélectionnés
    if (SelectedEquipmentIds != null && SelectedEquipmentIds.Any())
    {
        foreach (var equipmentId in SelectedEquipmentIds)
        {
            _context.RoomEquipments.Add(new RoomEquipment
            {
                RoomId = room.Id,
                EquipmentId = equipmentId
            });
        }
        await _context.SaveChangesAsync();
    }
    
    // 5. Rediriger vers la liste des salles
    return RedirectToPage("./Index");
}
```

**?? Diagramme du Flux :**
```
Utilisateur (Admin)
    ?
GET /Admin/Rooms/Create
    ?
OnGetAsync() ? Charge les équipements
    ?
Formulaire affiché (Create.cshtml)
    ?
Utilisateur remplit et soumet
    ?
POST /Admin/Rooms/Create
    ?
OnPostAsync() ? Validation ? Sauvegarde ? Redirection
    ?
GET /Admin/Rooms/Index (Liste des salles)
```

---

### 3.3 Système de Réservation (Module Utilisateur)

#### Logique de Vérification des Conflits

**Problématique :** Empêcher deux réservations qui se chevauchent pour la même salle.

**Solution Implémentée :**

```csharp
// Pages/Reservations/Reserve.cshtml.cs
public async Task<IActionResult> OnPostAsync()
{
    var requestedStart = Input.Date.Add(Input.StartTime);
    var requestedEnd = Input.Date.Add(Input.EndTime);
    
    // Vérifier s'il existe déjà une réservation qui chevauche
    var hasConflict = await _context.Reservations
        .AnyAsync(r => 
            r.RoomId == RoomId &&
            r.Status != ReservationStatus.Rejected &&
            r.Status != ReservationStatus.Cancelled &&
            r.StartTime < requestedEnd &&  // Commence avant la fin demandée
            r.EndTime > requestedStart);   // Finit après le début demandé
    
    if (hasConflict)
    {
        ModelState.AddModelError("", "Créneau non disponible");
        return Page();
    }
    
    // Créer la réservation
    var reservation = new Reservation
    {
        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        RoomId = RoomId,
        StartTime = requestedStart,
        EndTime = requestedEnd,
        Purpose = Input.Purpose,
        Status = ReservationStatus.Pending // En attente de validation
    };
    
    _context.Reservations.Add(reservation);
    await _context.SaveChangesAsync();
    
    // Notifier l'utilisateur
    await _notificationService.CreateNotificationAsync(
        reservation.UserId,
        $"Votre demande pour {room.Name} est en attente de validation."
    );
    
    return RedirectToPage("/Reservations/MyReservations");
}
```

**?? Exemple de Détection de Conflit :**

```
Réservation existante : 14:00 ? 16:00
Demande 1 : 15:00 ? 17:00 ? CONFLIT (chevauche 15:00-16:00)
Demande 2 : 16:00 ? 18:00 ? OK (commence exactement à la fin)
Demande 3 : 12:00 ? 14:00 ? OK (finit exactement au début)
Demande 4 : 13:00 ? 15:00 ? CONFLIT (chevauche 14:00-15:00)
```

**Condition Logique :**
```csharp
// Une réservation chevauche SI :
r.StartTime < requestedEnd   ET   r.EndTime > requestedStart

// Expliqué :
// - Elle commence AVANT la fin demandée
// - ET elle finit APRÈS le début demandé
```

---

### 3.4 Système de Notifications

**Interface `INotificationService` :**

```csharp
public interface INotificationService
{
    Task CreateNotificationAsync(string userId, string message);
    Task<List<Notification>> GetUserNotificationsAsync(string userId);
    Task MarkAsReadAsync(int notificationId);
}
```

**Implémentation :**

```csharp
public class NotificationService : INotificationService
{
    private readonly ApplicationDbContexte _context;
    
    public async Task CreateNotificationAsync(string userId, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            CreatedAt = DateTime.Now,
            IsRead = false
        };
        
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
    
    // ... autres méthodes
}
```

**Enregistrement dans `Program.cs` :**
```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
```

**?? Concept Clé : Injection de Dépendances**
- Le service est enregistré dans le conteneur DI
- ASP.NET Core l'injecte automatiquement dans les constructeurs
- Permet de facilement remplacer l'implémentation (tests, etc.)

---

## 4. Structure des Dossiers

```
RoomEase/
?
??? wwwroot/                          # Fichiers statiques (CSS, JS, images)
?   ??? css/
?   ?   ??? site.css                  # Styles personnalisés
?   ??? js/
?   ?   ??? site.js                   # Scripts JavaScript
?   ??? lib/                          # Bibliothèques externes (Bootstrap, jQuery)
?
??? Pages/                            # Pages Razor
?   ??? Account/                      # Authentification
?   ?   ??? Login.cshtml
?   ?   ??? Login.cshtml.cs
?   ?   ??? Register.cshtml
?   ?   ??? Register.cshtml.cs
?   ??? Admin/                        # Zone administration
?   ?   ??? Rooms/                    # Gestion des salles
?   ?   ?   ??? Index.cshtml
?   ?   ?   ??? Create.cshtml
?   ?   ?   ??? Edit.cshtml
?   ?   ?   ??? Delete.cshtml
?   ?   ??? Equipment/                # Gestion des équipements
?   ??? Rooms/                        # Recherche de salles (utilisateur)
?   ?   ??? Index.cshtml
?   ?   ??? Reserve.cshtml
?   ??? Reservations/                 # Mes réservations
?   ?   ??? MyReservations.cshtml
?   ??? Notifications/                # Notifications
?   ?   ??? Index.cshtml
?   ??? Shared/                       # Layouts et composants partagés
?   ?   ??? _Layout.cshtml            # Layout principal
?   ?   ??? _ValidationScriptsPartial.cshtml
?   ??? Index.cshtml                  # Page d'accueil
?   ??? _ViewImports.cshtml           # Imports globaux
?   ??? _ViewStart.cshtml             # Configuration de démarrage des vues
?
??? Models/                           # Modèles de données (entités)
?   ??? AppUser.cs
?   ??? Room.cs
?   ??? Equipment.cs
?   ??? RoomEquipment.cs
?   ??? Reservation.cs
?   ??? ReservationStatus.cs
?   ??? Notification.cs
?
??? ViewModels/                       # ViewModels pour les formulaires
?   ??? LoginViewModel.cs
?   ??? RegisterViewModel.cs
?   ??? RoomSearchViewModel.cs
?   ??? ReservationViewModel.cs
?
??? Services/                         # Services et logique métier
?   ??? ApplicationDbContexte.cs      # Contexte EF Core
?   ??? INotificationService.cs       # Interface du service de notifications
?   ??? NotificationService.cs        # Implémentation
?   ??? SeedData.cs                   # Données initiales (admin, salles)
?
??? Migrations/                       # Historique des modifications de la base
?   ??? 20251206194610_initialDatabasrScripts.cs
?
??? Program.cs                        # Point d'entrée de l'application
??? appsettings.json                  # Configuration (chaîne de connexion, etc.)
??? RoomEase.csproj                   # Fichier de projet .NET
```

### ?? Explication des Dossiers Clés

| Dossier | Rôle |
|---------|------|
| **wwwroot/** | Contient tous les fichiers statiques (CSS, JS, images) accessibles publiquement |
| **Pages/** | Pages Razor (vue + logique). Chaque `.cshtml` a un `.cshtml.cs` associé (PageModel) |
| **Models/** | Classes qui représentent les tables de la base de données |
| **ViewModels/** | Classes utilisées uniquement pour le transfert de données entre formulaires et PageModels |
| **Services/** | Services métier (DbContext, services de notification, etc.) |
| **Migrations/** | Historique des modifications de la structure de la base de données |

---

### ?? Fichiers Clés Expliqués

#### **Program.cs** (Point d'Entrée)

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Enregistrement des services dans le conteneur DI
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContexte>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddIdentity<AppUser, IdentityRole>(...);
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// 2. Configuration du pipeline de middleware
app.UseHttpsRedirection();
app.UseStaticFiles();        // Sert les fichiers wwwroot
app.UseRouting();
app.UseAuthentication();     // Active l'authentification
app.UseAuthorization();      // Active l'autorisation (roles)
app.MapRazorPages();         // Mappe les routes des pages

app.Run();                   // Démarre le serveur
```

**?? Concepts Importants :**
- **Conteneur DI :** Enregistre tous les services (DbContext, Identity, services personnalisés)
- **Middleware Pipeline :** Chaîne de traitements des requêtes HTTP
- **Ordre important :** `UseAuthentication()` AVANT `UseAuthorization()`

---

#### **appsettings.json** (Configuration)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=RoomEaseDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

- **ConnectionStrings** : Chaîne de connexion à SQL Server
- **Logging** : Niveau de logs (Information, Warning, Error)

---

#### **Migrations/** (Historique de la Base de Données)

**Commandes EF Core utilisées :**

```bash
# Créer une migration (génère le code pour modifier la DB)
dotnet ef migrations add InitialCreate

# Appliquer les migrations à la base de données
dotnet ef database update

# Supprimer la dernière migration (si pas encore appliquée)
dotnet ef migrations remove

# Supprimer complètement la base de données
dotnet ef database drop
```

**?? À quoi sert `Update-Database` ?**
- Lit les fichiers dans `Migrations/`
- Exécute le code SQL correspondant pour créer/modifier les tables
- Met à jour le schéma de la base de données pour correspondre aux modèles C#

---

## 5. Questions Types pour la Soutenance

### Question 1 : Qu'est-ce que l'Injection de Dépendances et où est-elle utilisée ?

**Réponse :**

L'**Injection de Dépendances (DI)** est un design pattern qui permet de fournir les dépendances d'une classe depuis l'extérieur plutôt que de les créer à l'intérieur.

**Exemple dans RoomEase :**

```csharp
public class IndexModel : PageModel
{
    private readonly ApplicationDbContexte _context;
    
    // Le DbContext est injecté via le constructeur
    public IndexModel(ApplicationDbContexte context)
    {
        _context = context;
    }
    
    public async Task OnGetAsync()
    {
        // On utilise _context qui a été injecté
        var rooms = await _context.Rooms.ToListAsync();
    }
}
```

**Avantages :**
- ? **Testabilité** : On peut injecter un mock du DbContext pour les tests
- ? **Réutilisabilité** : Le même `ApplicationDbContexte` peut être injecté dans plusieurs pages
- ? **Gestion du cycle de vie** : ASP.NET Core gère automatiquement la création/destruction

**Configuration dans `Program.cs` :**
```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
// "Scoped" = une instance par requête HTTP
```

---

### Question 2 : Quelle est la différence entre un ViewModel et un Model ?

**Réponse :**

| Aspect | Model | ViewModel |
|--------|-------|-----------|
| **Rôle** | Représente une table de la base de données | Représente les données d'un formulaire |
| **Mapping** | Mappé directement à une table SQL | PAS mappé à la base de données |
| **Utilisation** | Requêtes LINQ, relations EF Core | Validation de formulaires, transfert de données |

**Exemple Concret :**

**Model (Room.cs) :**
```csharp
public class Room
{
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<RoomEquipment> RoomEquipments { get; set; }
}
```
? Représente exactement la table `Rooms` dans SQL Server

**ViewModel (RegisterViewModel.cs) :**
```csharp
public class RegisterViewModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; } // N'existe PAS dans la table
}
```
? Utilisé uniquement pour le formulaire d'inscription

**Pourquoi séparer ?**
- Le formulaire d'inscription a besoin de `ConfirmPassword` pour la validation
- Mais on ne veut PAS stocker `ConfirmPassword` dans la base de données
- On utilise donc un ViewModel pour le formulaire, puis on crée un Model `AppUser` pour la sauvegarde

---

### Question 3 : Comment fonctionne l'autorisation par rôles dans votre projet ?

**Réponse :**

L'autorisation par rôles permet de restreindre l'accès à certaines pages selon le rôle de l'utilisateur.

**1. Définition des Rôles (SeedData.cs) :**
```csharp
string[] roleNames = { "Admin", "User" };
foreach (var roleName in roleNames)
{
    if (!await roleManager.RoleExistsAsync(roleName))
    {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}
```

**2. Attribution du Rôle à un Utilisateur :**
```csharp
// Lors de l'inscription
await _userManager.AddToRoleAsync(user, "User");

// Pour l'admin
await _userManager.AddToRoleAsync(admin, "Admin");
```

**3. Protection des Pages avec `[Authorize(Roles = "Admin")]` :**
```csharp
[Authorize(Roles = "Admin")] // Seuls les admins peuvent accéder
public class CreateModel : PageModel
{
    // ...
}
```

**4. Affichage Conditionnel dans les Vues :**
```razor
@if (User.IsInRole("Admin"))
{
    <a asp-page="/Admin/Rooms/Index">Gérer les salles</a>
}
```

**Flux d'Autorisation :**
```
Utilisateur ? Requête ? Middleware d'Autorisation
    ?
Vérifie si User.IsInRole("Admin")
    ?
    OUI ? Accès autorisé
    ?
    NON ? Redirection vers AccessDenied
```

---

### Question 4 : Expliquez le cycle de vie d'une requête HTTP dans votre application

**Réponse :**

**Exemple : Afficher la liste des salles**

```
1. Utilisateur saisit l'URL : https://localhost:5001/Rooms/Index
   ?
2. Requête HTTP GET arrive au serveur
   ?
3. Pipeline de Middleware :
   - UseHttpsRedirection ? Redirige HTTP vers HTTPS
   - UseStaticFiles ? Vérifie si la requête demande un fichier statique (CSS/JS)
   - UseRouting ? Détermine quelle page appeler
   - UseAuthentication ? Lit le cookie d'authentification
   - UseAuthorization ? Vérifie les autorisations (rôles)
   ?
4. La route mappe vers Pages/Rooms/Index.cshtml
   ?
5. ASP.NET Core crée une instance de IndexModel
   ?
6. Injection de Dépendances :
   - Le DbContext est injecté dans le constructeur
   ?
7. Exécution de OnGetAsync() :
   - Requête LINQ vers la base de données
   - Récupération des données
   ?
8. Le PageModel prépare les données (propriété AvailableRooms)
   ?
9. La vue Index.cshtml est rendue :
   - Razor génère du HTML en utilisant Model.AvailableRooms
   ?
10. Réponse HTTP envoyée au navigateur
   ?
11. Le navigateur affiche la page HTML
```

---

### Question 5 : Comment gérez-vous la sécurité des mots de passe ?

**Réponse :**

ASP.NET Core Identity gère automatiquement la sécurité des mots de passe avec plusieurs niveaux de protection :

**1. Règles de Complexité (Program.cs) :**
```csharp
options.Password.RequireDigit = true;           // Au moins 1 chiffre
options.Password.RequireLowercase = true;       // Au moins 1 minuscule
options.Password.RequireNonAlphanumeric = true; // Au moins 1 caractère spécial
options.Password.RequireUppercase = true;       // Au moins 1 majuscule
options.Password.RequiredLength = 6;            // Minimum 6 caractères
```

**2. Hashing (Chiffrement Unidirectionnel) :**
```csharp
// Lors de l'inscription
await _userManager.CreateAsync(user, Input.Password);

// En interne, Identity :
// - Génère un "salt" aléatoire
// - Combine Password + Salt
// - Applique un algorithme de hashing (PBKDF2)
// - Stocke UNIQUEMENT le hash (pas le mot de passe en clair)
```

**3. Stockage dans la Base de Données :**
```
Table AspNetUsers :
| UserName | PasswordHash                                          |
|----------|------------------------------------------------------|
| admin    | AQAAAAIAAYagAAAAEL8... (chaîne de 84 caractères)   |
```

**4. Vérification lors de la Connexion :**
```csharp
// Utilisateur saisit : "Admin123!"
// Identity :
// - Récupère le PasswordHash de la base
// - Applique le même algorithme de hashing au mot de passe saisi
// - Compare les deux hash
// - Si identiques ? Connexion réussie
```

**?? Avantages :**
- ? **Impossible de retrouver** le mot de passe original depuis le hash
- ? **Salt unique** pour chaque utilisateur (empêche les attaques par rainbow table)
- ? **Algorithme robuste** (PBKDF2 avec 100 000 itérations)

---

### Question 6 : Qu'est-ce qu'Entity Framework Core et quel est son rôle ?

**Réponse :**

**Entity Framework Core (EF Core)** est un ORM (**Object-Relational Mapper**) qui fait le pont entre le code C# orienté objet et la base de données relationnelle (SQL Server).

**Rôle :**
- Transforme les classes C# en tables SQL
- Convertit les requêtes LINQ en requêtes SQL
- Gère automatiquement les connexions à la base de données

**Exemple Concret :**

**Code C# :**
```csharp
var rooms = await _context.Rooms
    .Where(r => r.Capacity >= 10)
    .OrderBy(r => r.Name)
    .ToListAsync();
```

**SQL Généré par EF Core :**
```sql
SELECT [r].[Id], [r].[Name], [r].[Capacity], [r].[Description], [r].[IsAvailable]
FROM [Rooms] AS [r]
WHERE [r].[Capacity] >= 10
ORDER BY [r].[Name]
```

**Avantages :**
- ? **Productivité** : Pas besoin d'écrire du SQL manuellement
- ? **Type-safety** : Le compilateur détecte les erreurs (ex: typo dans un nom de propriété)
- ? **Migrations** : Gestion automatique de l'évolution de la base de données
- ? **LINQ** : Requêtes expressives et lisibles

**Code First vs Database First :**
- **Code First** (utilisé dans RoomEase) : On écrit les classes C#, EF Core génère la base
- **Database First** : On crée la base, EF Core génère les classes

---

### Question 7 : Expliquez la différence entre `AddScoped`, `AddTransient` et `AddSingleton`

**Réponse :**

Ces méthodes définissent le **cycle de vie** d'un service dans le conteneur d'injection de dépendances.

| Méthode | Durée de Vie | Quand l'utiliser |
|---------|--------------|------------------|
| **AddTransient** | Créé à CHAQUE injection | Services légers, sans état (ex: calculateurs) |
| **AddScoped** | Créé UNE FOIS par requête HTTP | Services avec état par requête (ex: DbContext) |
| **AddSingleton** | Créé UNE SEULE FOIS pour toute l'application | Services partagés (ex: configuration) |

**Exemple Visuel :**

```
Requête 1 :
    PageA ? DbContext1
    PageB ? DbContext1 (même instance - Scoped)

Requête 2 :
    PageC ? DbContext2 (nouvelle instance)
    PageD ? DbContext2 (même instance - Scoped)
```

**Dans RoomEase :**
```csharp
// DbContext ? Scoped (une instance par requête)
builder.Services.AddDbContext<ApplicationDbContexte>(options => ...);

// Service de notifications ? Scoped
builder.Services.AddScoped<INotificationService, NotificationService>();
```

**?? Pourquoi Scoped pour le DbContext ?**
- Le DbContext garde un cache des entités chargées
- On veut un cache propre pour chaque requête
- Évite les problèmes de concurrence entre requêtes

---

## 6. Concepts Avancés

### 6.1 Eager Loading vs Lazy Loading

**Problème N+1 :**
```csharp
// ? MAUVAIS : Lazy Loading
var rooms = await _context.Rooms.ToListAsync();
foreach (var room in rooms)
{
    // Déclenche une requête SQL pour CHAQUE room
    var equipments = room.RoomEquipments.ToList();
}
// Total : 1 + N requêtes (N = nombre de salles)
```

**Solution : Eager Loading avec `.Include()` :**
```csharp
// ? BON : Eager Loading
var rooms = await _context.Rooms
    .Include(r => r.RoomEquipments)         // JOIN sur RoomEquipments
    .ThenInclude(re => re.Equipment)        // JOIN sur Equipment
    .ToListAsync();
// Total : 1 seule requête SQL avec JOIN
```

---

### 6.2 Async/Await

**Pourquoi utiliser `async`/`await` ?**

```csharp
// ? SYNCHRONE (bloque le thread)
public IActionResult OnGet()
{
    var rooms = _context.Rooms.ToList(); // Attend la base de données
    return Page();
}

// ? ASYNCHRONE (libère le thread)
public async Task<IActionResult> OnGetAsync()
{
    var rooms = await _context.Rooms.ToListAsync(); // Le thread peut traiter d'autres requêtes
    return Page();
}
```

**Avantages :**
- Meilleure **scalabilité** (le serveur peut gérer plus de requêtes simultanées)
- Le thread n'est pas bloqué pendant l'attente de la base de données

---

## 7. Bonnes Pratiques Appliquées

### ? 1. Séparation des Responsabilités
- **Models** : Données pures
- **ViewModels** : Formulaires
- **Services** : Logique métier
- **PageModels** : Orchestration

### ? 2. Validation
```csharp
[Required(ErrorMessage = "Le nom est requis")]
[StringLength(100, ErrorMessage = "Maximum 100 caractères")]
public string Name { get; set; }
```

### ? 3. Logging
```csharp
_logger.LogInformation("Room created: {RoomName}", room.Name);
_logger.LogWarning("Validation Error: {Error}", error.ErrorMessage);
```

### ? 4. Sécurité
- Mots de passe hashés (Identity)
- Protection CSRF (antiforgery tokens)
- Autorisation par rôles
- HTTPS forcé

### ? 5. Architecture Testable
- Injection de dépendances
- Interfaces (INotificationService)
- Séparation logique/présentation

---

## 8. Glossaire des Termes Techniques

| Terme | Définition |
|-------|------------|
| **ORM** | Object-Relational Mapper - Outil qui mappe objets ? tables |
| **Migration** | Fichier de code qui modifie le schéma de la base de données |
| **DbContext** | Classe qui représente une session avec la base de données |
| **LINQ** | Language Integrated Query - Requêtes type-safe en C# |
| **ViewModel** | Classe pour transférer des données entre formulaire et contrôleur |
| **PageModel** | Classe code-behind d'une page Razor (équivalent du Controller) |
| **Razor** | Moteur de templates pour générer du HTML dynamique |
| **Identity** | Framework d'authentification et gestion des utilisateurs |
| **DI** | Dependency Injection - Pattern pour injecter des dépendances |
| **Middleware** | Composant dans le pipeline de traitement des requêtes |

---

## 9. Commandes Utiles

### Entity Framework Core
```bash
# Créer une migration
dotnet ef migrations add <NomMigration>

# Appliquer les migrations
dotnet ef database update

# Supprimer la dernière migration
dotnet ef migrations remove

# Générer un script SQL
dotnet ef migrations script

# Supprimer la base de données
dotnet ef database drop
```

### Projet .NET
```bash
# Compiler le projet
dotnet build

# Lancer l'application
dotnet run

# Restaurer les packages NuGet
dotnet restore

# Nettoyer les fichiers de build
dotnet clean
```

---

## 10. Points Clés à Retenir pour la Soutenance

### ?? Architecture
- ? Razor Pages = Variante simplifiée de MVC
- ? Séparation Model / ViewModel / PageModel
- ? Injection de Dépendances omniprésente

### ?? Base de Données
- ? Code First avec EF Core
- ? Relations : 1-N (User ? Reservations), N-N (Room ? Equipment)
- ? Migrations pour gérer l'évolution du schéma

### ?? Sécurité
- ? ASP.NET Core Identity pour l'authentification
- ? Mots de passe hashés avec salt
- ? Autorisation par rôles (Admin/User)

### ?? Fonctionnalités
- ? Gestion des salles et équipements (CRUD)
- ? Réservation avec détection de conflits
- ? Système de notifications
- ? Interface responsive (Bootstrap 5)

---

## Conclusion

Ce document vous donne une compréhension complète de votre projet RoomEase. Pour la soutenance :

1. **Maîtrisez les concepts clés** : MVC/Razor Pages, EF Core, Identity
2. **Soyez capable d'expliquer vos choix** : Pourquoi tel pattern ? Pourquoi telle technologie ?
3. **Préparez des exemples concrets** : "Voici comment fonctionne la réservation..."
4. **Anticipez les questions** : Utilisez la section "Questions Types"

**Bonne soutenance !** ????

---

*Document généré pour le projet RoomEase - Système de Réservation de Salles*
*Auteur : Yassine Atiki*
*Date : Décembre 2025*
