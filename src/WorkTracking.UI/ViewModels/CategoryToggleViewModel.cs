using WorkTracking.Core.Models;

namespace WorkTracking.UI.ViewModels;

public class CategoryToggleViewModel(WorkCategory category, bool isEnabled) : ViewModelBase
{
    private bool _isEnabled = isEnabled;

    public WorkCategory Category { get; } = category;
    public int Id => Category.Id;
    public string Name => Category.Name;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetField(ref _isEnabled, value);
    }
}
