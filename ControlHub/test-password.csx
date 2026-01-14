using Microsoft.EntityFrameworkCore;
using ControlHub.Infrastructure.Persistence;
using ControlHub.Infrastructure.Accounts.Security;
using ControlHub.Domain.Accounts.Security;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using var db = new AppDbContext(options);

// Get the first account
var account = await db.Accounts
    .Include(a => a.Identifiers)
    .FirstOrDefaultAsync();

if (account != null)
{
    Console.WriteLine($"Found account: {account.Id}");
    
    var identifier = account.Identifiers.First();
    Console.WriteLine($"Identifier: {identifier.Value} (Normalized: {identifier.NormalizedValue})");
    
    // Test password verification
    var runtimeHasher = new Argon2PasswordHasher(Microsoft.Extensions.Options.Options.Create<Argon2Options>(new Argon2Options
    {
        SaltSize = 16,
        HashSize = 32,
        MemorySizeKB = 65536,
        Iterations = 3,
        DegreeOfParallelism = 4
    }));
    
    var isValid = runtimeHasher.Verify("Admin@123", account.Password);
    Console.WriteLine($"Password verification result: {isValid}");
    
    // Test creating new hash
    var newHash = runtimeHasher.Hash("Admin@123");
    Console.WriteLine($"New hash created: {newHash.Hash.Length} bytes, salt: {newHash.Salt.Length} bytes");
    
    Console.WriteLine($"Stored hash: {account.Password.Hash.Length} bytes, salt: {account.Password.Salt.Length} bytes");
}
else
{
    Console.WriteLine("No accounts found!");
}
