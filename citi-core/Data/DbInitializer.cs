using Microsoft.EntityFrameworkCore;

namespace citi_core.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                if (!env.IsDevelopment())
                {
                    Console.WriteLine("Skipping automatic migrations outside of Development");
                    return;
                }

                Console.WriteLine("Starting database migration...");

                await context.Database.CanConnectAsync();
                Console.WriteLine("Database connection successful");

                await context.Database.MigrateAsync();
                Console.WriteLine("Database migration completed successfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

    }
}
