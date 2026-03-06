using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using System.Reflection;

namespace Billing.Infrastructure.Documents.Shared;

/// <summary>
/// Manages font registration for QuestPDF billing documents.
/// Registers Noto Sans Regular and Bold for Vietnamese diacritic support.
/// Thread-safe, only executes once per application lifetime.
/// </summary>
public static class DocumentFontManager
{
    private static bool _isRegistered;
    private static readonly object _lock = new();

    /// <summary>
    /// Registers embedded Noto Sans fonts and configures QuestPDF settings.
    /// Thread-safe, only executes once.
    /// </summary>
    public static void RegisterFonts()
    {
        if (_isRegistered) return;

        lock (_lock)
        {
            if (_isRegistered) return;

            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                if (resourceName.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream is not null)
                    {
                        FontManager.RegisterFont(stream);
                    }
                }
            }

            _isRegistered = true;
        }
    }
}
