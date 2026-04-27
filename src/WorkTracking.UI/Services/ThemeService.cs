using System.Windows;
using System.Windows.Media;

namespace WorkTracking.UI.Services;

public class ThemeService : IThemeService
{
    // Brush keys must match exactly the x:Key values in ModernTheme.xaml.
    private static readonly IReadOnlyDictionary<string, Color> LightColours =
        new Dictionary<string, Color>
        {
            ["AccentBrush"]            = Color.FromArgb(0xFF, 0x00, 0x67, 0xC0),
            ["AccentHoverBrush"]       = Color.FromArgb(0xFF, 0x00, 0x5A, 0xA3),
            ["AccentPressedBrush"]     = Color.FromArgb(0xFF, 0x00, 0x4D, 0x8C),
            ["AccentDisabledBrush"]    = Color.FromArgb(0xFF, 0xA8, 0xC8, 0xEB),
            ["BackgroundBrush"]        = Color.FromArgb(0xFF, 0xF3, 0xF3, 0xF3),
            ["SurfaceBrush"]           = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF),
            ["SurfaceAltBrush"]        = Color.FromArgb(0xFF, 0xFA, 0xFA, 0xFA),
            ["BorderBrush"]            = Color.FromArgb(0xFF, 0xE0, 0xE0, 0xE0),
            ["TextPrimaryBrush"]       = Color.FromArgb(0xFF, 0x1A, 0x1A, 0x1A),
            ["TextSecondaryBrush"]     = Color.FromArgb(0xFF, 0x6E, 0x6E, 0x6E),
            ["DangerBrush"]            = Color.FromArgb(0xFF, 0xC4, 0x2B, 0x1C),
            ["DangerHoverBrush"]       = Color.FromArgb(0xFF, 0xFD, 0xEC, 0xEC),
            ["HoverBrush"]             = Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8),
            ["SecondaryHoverBrush"]    = Color.FromArgb(0xFF, 0xEF, 0xF0, 0xF1),
            ["ListSelectedBrush"]      = Color.FromArgb(0xFF, 0xD0, 0xE6, 0xF8),
            ["ListSelectedHoverBrush"] = Color.FromArgb(0xFF, 0xBC, 0xDB, 0xF5),
            ["DataGridAltRowBrush"]    = Color.FromArgb(0xFF, 0xF8, 0xF8, 0xF8),
        };

    private static readonly IReadOnlyDictionary<string, Color> DarkColours =
        new Dictionary<string, Color>
        {
            ["AccentBrush"]            = Color.FromArgb(0xFF, 0x4F, 0xA3, 0xE3),
            ["AccentHoverBrush"]       = Color.FromArgb(0xFF, 0x3D, 0x8F, 0xCC),
            ["AccentPressedBrush"]     = Color.FromArgb(0xFF, 0x2D, 0x7A, 0xB5),
            ["AccentDisabledBrush"]    = Color.FromArgb(0xFF, 0x3A, 0x5A, 0x78),
            ["BackgroundBrush"]        = Color.FromArgb(0xFF, 0x1E, 0x1E, 0x1E),
            ["SurfaceBrush"]           = Color.FromArgb(0xFF, 0x25, 0x25, 0x26),
            ["SurfaceAltBrush"]        = Color.FromArgb(0xFF, 0x2D, 0x2D, 0x30),
            ["BorderBrush"]            = Color.FromArgb(0xFF, 0x3F, 0x3F, 0x46),
            ["TextPrimaryBrush"]       = Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8),
            ["TextSecondaryBrush"]     = Color.FromArgb(0xFF, 0x9D, 0x9D, 0x9D),
            ["DangerBrush"]            = Color.FromArgb(0xFF, 0xFF, 0x6B, 0x6B),
            ["DangerHoverBrush"]       = Color.FromArgb(0xFF, 0x3D, 0x1A, 0x1A),
            ["HoverBrush"]             = Color.FromArgb(0xFF, 0x3E, 0x3E, 0x42),
            ["SecondaryHoverBrush"]    = Color.FromArgb(0xFF, 0x2F, 0x2F, 0x33),
            ["ListSelectedBrush"]      = Color.FromArgb(0xFF, 0x1C, 0x3A, 0x52),
            ["ListSelectedHoverBrush"] = Color.FromArgb(0xFF, 0x1A, 0x48, 0x70),
            ["DataGridAltRowBrush"]    = Color.FromArgb(0xFF, 0x2A, 0x2A, 0x2E),
        };

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public void Apply(AppTheme theme)
    {
        CurrentTheme = theme;
        var colours = theme == AppTheme.Dark ? DarkColours : LightColours;
        var resources = Application.Current.Resources;

        foreach (var (key, color) in colours)
        {
            if (resources[key] is SolidColorBrush)
                resources[key] = new SolidColorBrush(color);
        }
    }
}
