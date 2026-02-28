namespace Audit.Domain.Enums;

/// <summary>
/// Types of access events captured by the access logging middleware.
/// </summary>
public enum AccessAction
{
    Login = 0,
    LoginFailed = 1,
    Logout = 2,
    ViewRecord = 3,
    ApiRequest = 4
}
