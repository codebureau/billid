using System.Windows.Input;
using WorkTracking.UI.Commands;

namespace WorkTracking.UI.ViewModels;

public class AddClientViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private decimal _hourlyRate;
    private bool _confirmed;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public decimal HourlyRate
    {
        get => _hourlyRate;
        set => SetField(ref _hourlyRate, value);
    }

    public bool Confirmed => _confirmed;

    public ICommand ConfirmCommand => new RelayCommand(
        _ => { _confirmed = true; CloseRequested?.Invoke(this, EventArgs.Empty); },
        _ => !string.IsNullOrWhiteSpace(_name) && _hourlyRate > 0);

    public ICommand CancelCommand => new RelayCommand(
        _ => CloseRequested?.Invoke(this, EventArgs.Empty));

    public event EventHandler? CloseRequested;
}
