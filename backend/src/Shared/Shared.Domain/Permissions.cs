namespace Shared.Domain;

/// <summary>
/// Constants for permission strings used in authorization policies.
/// Format: "Module.Action" — must match the Permission entity's ToString() output.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permissions";

    public static class Auth
    {
        public const string View = "Auth.View";
        public const string Create = "Auth.Create";
        public const string Update = "Auth.Update";
        public const string Delete = "Auth.Delete";
        public const string Export = "Auth.Export";
        public const string Manage = "Auth.Manage";
    }

    public static class Patient
    {
        public const string View = "Patient.View";
        public const string Create = "Patient.Create";
        public const string Update = "Patient.Update";
        public const string Delete = "Patient.Delete";
        public const string Export = "Patient.Export";
        public const string Manage = "Patient.Manage";
    }

    public static class Clinical
    {
        public const string View = "Clinical.View";
        public const string Create = "Clinical.Create";
        public const string Update = "Clinical.Update";
        public const string Delete = "Clinical.Delete";
        public const string Export = "Clinical.Export";
        public const string Manage = "Clinical.Manage";
    }

    public static class Scheduling
    {
        public const string View = "Scheduling.View";
        public const string Create = "Scheduling.Create";
        public const string Update = "Scheduling.Update";
        public const string Delete = "Scheduling.Delete";
        public const string Export = "Scheduling.Export";
        public const string Manage = "Scheduling.Manage";
    }

    public static class Pharmacy
    {
        public const string View = "Pharmacy.View";
        public const string Create = "Pharmacy.Create";
        public const string Update = "Pharmacy.Update";
        public const string Delete = "Pharmacy.Delete";
        public const string Export = "Pharmacy.Export";
        public const string Manage = "Pharmacy.Manage";
    }

    public static class Optical
    {
        public const string View = "Optical.View";
        public const string Create = "Optical.Create";
        public const string Update = "Optical.Update";
        public const string Delete = "Optical.Delete";
        public const string Export = "Optical.Export";
        public const string Manage = "Optical.Manage";
    }

    public static class Billing
    {
        public const string View = "Billing.View";
        public const string Create = "Billing.Create";
        public const string Update = "Billing.Update";
        public const string Delete = "Billing.Delete";
        public const string Export = "Billing.Export";
        public const string Manage = "Billing.Manage";
    }

    public static class Treatment
    {
        public const string View = "Treatment.View";
        public const string Create = "Treatment.Create";
        public const string Update = "Treatment.Update";
        public const string Delete = "Treatment.Delete";
        public const string Export = "Treatment.Export";
        public const string Manage = "Treatment.Manage";
    }

    public static class Audit
    {
        public const string View = "Audit.View";
        public const string Create = "Audit.Create";
        public const string Update = "Audit.Update";
        public const string Delete = "Audit.Delete";
        public const string Export = "Audit.Export";
        public const string Manage = "Audit.Manage";
    }

    public static class Settings
    {
        public const string View = "Settings.View";
        public const string Create = "Settings.Create";
        public const string Update = "Settings.Update";
        public const string Delete = "Settings.Delete";
        public const string Export = "Settings.Export";
        public const string Manage = "Settings.Manage";
    }
}
