namespace MaintenancePortal.Core.Abstractions;

/// <summary>
/// Contract for turning an authenticated user into a bearer token. Core
/// declares this interface (it needs the concept "we can issue a token for
/// a user") without knowing HOW - that JWT-specific implementation detail
/// lives in Infrastructure. This is one of the few abstractions in Core that
/// is "justified": the Api layer depends on this interface, not directly on
/// a JWT library.
/// </summary>
public interface ITokenService
{
    string GenerateToken(Guid userId, string email, IEnumerable<string> roles);
}
