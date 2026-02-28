namespace Auth.Application.Interfaces;

/// <summary>
/// Abstraction for password hashing and verification.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string storedHash);
}
