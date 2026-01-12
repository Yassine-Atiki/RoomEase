using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoomEase.Models;

namespace RoomEase.Services
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContexte>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed Roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@roomease.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            
            if (adminUser == null)
            {
                // Create new admin user
                var admin = new AppUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FullName = "Administrateur RoomEase",
                    Department = "Administration",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
            else
            {
                // Update existing admin user's username if it's still the email
                if (adminUser.UserName == adminEmail)
                {
                    adminUser.UserName = "admin";
                    adminUser.NormalizedUserName = "ADMIN";
                    await userManager.UpdateAsync(adminUser);
                }
                
                // Ensure admin role is assigned
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Equipment
            if (!await context.Equipments.AnyAsync())
            {
                var equipments = new List<Equipment>
                {
                    new Equipment { Name = "Projecteur" },
                    new Equipment { Name = "Écran de projection" },
                    new Equipment { Name = "Tableau blanc" },
                    new Equipment { Name = "Visioconférence" },
                    new Equipment { Name = "Téléphone de conférence" }
                };

                await context.Equipments.AddRangeAsync(equipments);
                await context.SaveChangesAsync();
            }

            // Seed Rooms
            if (!await context.Rooms.AnyAsync())
            {
                var equipments = await context.Equipments.ToListAsync();
                
                var rooms = new List<Room>
                {
                    new Room
                    {
                        Name = "Salle de Conférence A",
                        Capacity = 20,
                        Description = "Grande salle de conférence avec équipement audiovisuel complet",
                        IsAvailable = true
                    },
                    new Room
                    {
                        Name = "Salle de Réunion B",
                        Capacity = 10,
                        Description = "Salle de réunion moyenne équipée pour les présentations",
                        IsAvailable = true
                    },
                    new Room
                    {
                        Name = "Bureau Projet C",
                        Capacity = 6,
                        Description = "Petit espace pour les réunions d'équipe",
                        IsAvailable = true
                    },
                    new Room
                    {
                        Name = "Auditorium",
                        Capacity = 50,
                        Description = "Grand auditorium pour les événements et formations",
                        IsAvailable = true
                    },
                    new Room
                    {
                        Name = "Salle de Formation",
                        Capacity = 15,
                        Description = "Salle équipée pour les sessions de formation",
                        IsAvailable = true
                    }
                };

                await context.Rooms.AddRangeAsync(rooms);
                await context.SaveChangesAsync();

                // Link equipment to rooms
                var room1 = rooms[0]; // Conference A
                var room2 = rooms[1]; // Meeting B
                var room4 = rooms[3]; // Auditorium

                if (equipments.Count >= 5)
                {
                    var roomEquipments = new List<RoomEquipment>
                    {
                        // Conference Room A - Full equipment
                        new RoomEquipment { RoomId = room1.Id, EquipmentId = equipments[0].Id }, // Projecteur
                        new RoomEquipment { RoomId = room1.Id, EquipmentId = equipments[1].Id }, // Écran
                        new RoomEquipment { RoomId = room1.Id, EquipmentId = equipments[3].Id }, // Visio
                        
                        // Meeting Room B
                        new RoomEquipment { RoomId = room2.Id, EquipmentId = equipments[2].Id }, // Tableau blanc
                        new RoomEquipment { RoomId = room2.Id, EquipmentId = equipments[4].Id }, // Téléphone
                        
                        // Auditorium - Full equipment
                        new RoomEquipment { RoomId = room4.Id, EquipmentId = equipments[0].Id }, // Projecteur
                        new RoomEquipment { RoomId = room4.Id, EquipmentId = equipments[1].Id }, // Écran
                        new RoomEquipment { RoomId = room4.Id, EquipmentId = equipments[3].Id }  // Visio
                    };

                    await context.RoomEquipments.AddRangeAsync(roomEquipments);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
