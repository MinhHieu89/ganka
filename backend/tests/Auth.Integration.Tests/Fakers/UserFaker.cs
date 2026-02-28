using Bogus;

namespace Auth.Integration.Tests.Fakers;

/// <summary>
/// Bogus faker for generating test User data.
/// Will be populated with the full User entity builder pattern in plan 01-03
/// when Auth domain entities are implemented.
/// </summary>
public sealed class UserFaker : Faker<UserFakeDto>
{
    public UserFaker(int seed = 42)
    {
        UseSeed(seed);
        RuleFor(u => u.Email, f => f.Internet.Email());
        RuleFor(u => u.FirstName, f => f.Name.FirstName());
        RuleFor(u => u.LastName, f => f.Name.LastName());
        RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber());
    }
}

/// <summary>
/// Temporary DTO used until Auth.Domain User entity is available (plan 01-03).
/// </summary>
public class UserFakeDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
