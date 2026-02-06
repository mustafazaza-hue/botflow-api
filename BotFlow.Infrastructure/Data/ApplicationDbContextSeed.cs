using Microsoft.EntityFrameworkCore;

namespace BotFlow.Infrastructure.Data
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Check if database exists
            await context.Database.EnsureCreatedAsync();
            
            // Add initial data here
            // Example: Create default admin user
            
            await context.SaveChangesAsync();
        }
    }
}