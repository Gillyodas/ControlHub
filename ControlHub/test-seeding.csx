using Microsoft.EntityFrameworkCore;
using ControlHub.Infrastructure.Persistence;
using ControlHub.Infrastructure.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Create DbContext
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using var db = new AppDbContext(options);

Console.WriteLine("Testing seeding...");

try
{
    await ControlHubSeeder.SeedAsync(db);
    Console.WriteLine("Seeding completed successfully!");
    
    // Check accounts
    var accountCount = await db.Accounts.CountAsync();
    Console.WriteLine($"Total accounts: {accountCount}");
    
    var accounts = await db.Accounts
        .Include(a => a.Identifiers)
        .Include(a => a.User)
        .ToListAsync();
    
    foreach (var account in accounts)
    {
        var identifier = account.Identifiers.First();
        Console.WriteLine($"Account: {identifier.Value} (Type: {identifier.Type}) - User: {account.User?.Username}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack: {ex.StackTrace}");
}
