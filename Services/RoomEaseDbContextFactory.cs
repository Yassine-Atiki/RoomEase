using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RoomEase.Services;
// Assurez-vous d'importer votre modèle utilisateur si nécessaire
// using [Votre_Namespace_Pour_Models]; 

// La classe doit implémenter l'interface IDesignTimeDbContextFactory
public class RoomEaseDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContexte>
{
    public ApplicationDbContexte CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContexte>();

        // 1. Définissez ici la chaîne de connexion pour la migration
        // ATTENTION : Changez "DefaultConnection" par votre chaîne de connexion SQL Server
        // (Pour la production, il est préférable de lire ceci à partir d'un fichier settings)
        string connectionString = "Data Source=DESKTOP-LFOCPI1\\SQLEXPRESS;Initial Catalog=RoomEaseBD;Integrated Security=True;Encrypt=False;TrustServerCertificate=True";

        // 2. Utilisez UseSqlServer() comme vous l'avez configuré
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContexte(optionsBuilder.Options);
    }
}