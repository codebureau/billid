namespace WorkTracking.UI.Services;

public interface IThemeService
{
    AppTheme CurrentTheme { get; }
    void Apply(AppTheme theme);
}

public enum AppTheme
{
    Light,
    Dark
}
