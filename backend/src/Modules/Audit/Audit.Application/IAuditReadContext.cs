// This file is kept for backward compatibility during migration.
// The canonical interface has moved to Audit.Application.Interfaces.IAuditReadRepository.
// Existing Wolverine.Http endpoints referencing IAuditReadContext continue to compile
// via this type-forwarding alias. Plan 02 will remove this file.

using Audit.Application.Interfaces;

namespace Audit.Application;

/// <summary>
/// Backward-compatible alias. See <see cref="IAuditReadRepository"/>.
/// </summary>
public interface IAuditReadContext : IAuditReadRepository;
