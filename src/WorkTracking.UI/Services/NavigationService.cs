namespace WorkTracking.UI.Services;

public class NavigationService : INavigationService
{
    public string? CurrentDestination { get; private set; }

    public void NavigateTo(string destination) => CurrentDestination = destination;
}
