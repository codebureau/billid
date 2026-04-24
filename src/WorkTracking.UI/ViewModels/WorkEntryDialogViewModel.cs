using System.Windows.Input;
using WorkTracking.Core.Models;
using WorkTracking.UI.Commands;

namespace WorkTracking.UI.ViewModels;

public class WorkEntryDialogViewModel : ViewModelBase
{
    private DateTime _date = DateTime.Today;
    private string _description = string.Empty;
    private decimal _hours;
    private WorkCategory? _selectedCategory;
    private string? _notesMarkdown;
    private bool _confirmed;

    public WorkEntryDialogViewModel(IReadOnlyList<WorkCategory> categories, WorkEntry? existing = null)
    {
        Categories = categories;
        IsEdit = existing is not null;

        if (existing is not null)
        {
            ExistingId = existing.Id;
            _date = existing.Date.ToDateTime(TimeOnly.MinValue);
            _description = existing.Description;
            _hours = existing.Hours;
            _notesMarkdown = existing.NotesMarkdown;
            _selectedCategory = categories.FirstOrDefault(c => c.Id == existing.WorkCategoryId);
        }

        ConfirmCommand = new RelayCommand(
            _ => { _confirmed = true; CloseRequested?.Invoke(this, EventArgs.Empty); },
            _ => !string.IsNullOrWhiteSpace(_description) && _hours > 0);

        CancelCommand = new RelayCommand(
            _ => CloseRequested?.Invoke(this, EventArgs.Empty));
    }

    public bool IsEdit { get; }
    public int ExistingId { get; }
    public string Title => IsEdit ? "Edit Work Entry" : "Add Work Entry";
    public IReadOnlyList<WorkCategory> Categories { get; }

    public DateTime Date
    {
        get => _date;
        set => SetField(ref _date, value);
    }

    public string Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    public decimal Hours
    {
        get => _hours;
        set => SetField(ref _hours, value);
    }

    public WorkCategory? SelectedCategory
    {
        get => _selectedCategory;
        set => SetField(ref _selectedCategory, value);
    }

    public string? NotesMarkdown
    {
        get => _notesMarkdown;
        set => SetField(ref _notesMarkdown, value);
    }

    public bool Confirmed => _confirmed;

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler? CloseRequested;
}
