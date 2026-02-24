using RallyAPI.Users.Application.Abstractions;

namespace RallyAPI.Users.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    // Work factor - higher = slower but more secure
    // 11 is a good balance for 2024
    private const int WorkFactor = 11;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hashedPassword)
    {
        var hashedPass =  BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}