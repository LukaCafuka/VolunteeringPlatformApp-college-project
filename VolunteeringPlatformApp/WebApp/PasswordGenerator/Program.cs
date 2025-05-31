using WebApp.Security;

namespace PasswordGenerator;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter password: ");
        string password = Console.ReadLine() ?? "admin123";

        string salt = PasswordHashProvider.GetSalt();
        string hash = PasswordHashProvider.GetHash(password, salt);

        Console.WriteLine("\nGenerated values for database:");
        Console.WriteLine($"PswdSalt: {salt}");
        Console.WriteLine($"PswdHash: {hash}");
    }
}
