using InventoryManagment.DTOs;
using InventoryManagment.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagment.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder app, bool isProd)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<AppDbContext>();
                var userRepo = serviceScope.ServiceProvider.GetService<IUserAccount>();

                SeedData(context, userRepo, isProd);
            }
        }

        private static void SeedData(AppDbContext context, IUserAccount userRepo, bool isProd)
        {
            if (isProd)
            {
                Console.WriteLine("--> Attempting to apply migrations...");
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not run migrations: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("--> Not in Production mode, so no migrations will be run...");
            }

            if (!context.Users.Any())
            {
                Console.WriteLine("--> No users found, creating default user...");

                var defaultUser = new UserDTO
                {
                    Id = Guid.NewGuid().ToString(), 
                    Name = "Omer",
                    Email = "omer@example.com",
                    Password = "Omer@123",  
                    ConfirmPassword = "Omer@123", 
                    FirstName = "Omer",
                    LastName = "Sadikovic",
                    Address = "123 Example Street",
                    DateOfBirth = DateTime.Parse("1990-05-01"),  
                    DateOfHire = DateTime.Now, 
                    Salary = 55000.00f 
                };

                var result = userRepo.CreateAccount(defaultUser).Result;
                Console.WriteLine($"--> {result}");

                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("--> Users already exist");
            }
        }
    }
}
