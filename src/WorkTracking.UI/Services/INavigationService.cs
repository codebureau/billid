namespace WorkTracking.UI.Services;

public interface INavigationService
{
    void NavigateTo(string destination);
    string? CurrentDestination { get; }
}
