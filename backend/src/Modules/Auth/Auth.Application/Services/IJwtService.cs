// This file is kept for backward compatibility during migration.
// The canonical IJwtService interface has moved to Auth.Application.Interfaces.
// Existing code referencing Auth.Application.Services.IJwtService continues to compile
// via this type-forwarding alias. Plans 03-04 will remove this file.

using Auth.Application.Interfaces;

namespace Auth.Application.Services;

/// <summary>
/// Backward-compatible alias. See <see cref="Auth.Application.Interfaces.IJwtService"/>.
/// </summary>
public interface IJwtService : Interfaces.IJwtService;
