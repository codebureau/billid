using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using Markdig;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Commands;
using WorkTracking.UI.Services;

namespace WorkTracking.UI.ViewModels;

public class TimesheetViewModel(
    IWorkEntryRepository workEntryRepository,
    IWorkCategoryRepository workCategoryRepository,
    IInvoiceRepository invoiceRepository,
    IDialogService dialogService) : ViewModelBase
{
    private static readonly WorkCategory AllCategoriesSentinel = new() { Id = 0, Name = "All categories" };

    private int _clientId;
    private decimal _hourlyRate;
    private decimal? _invoiceCapAmount;

    private ObservableCollection<WorkEntryRowViewModel> _entries = [];
    private ICollectionView? _entriesView;
    private ObservableCollection<WorkCategory> _categories = [];
    private WorkEntryRowViewModel? _selectedEntry;
    private WorkCategory? _selectedFilterCategory;
    private string _invoicedFilterText = "Uninvoiced";
    private DateOnly? _filterStartDate;
    private DateOnly? _filterEndDate;
    private bool _isNotesOpen = true;
    private bool _isLoading;
    private string _selectedGroupBy = "None";

    public ObservableCollection<WorkEntryRowViewModel> Entries
    {
        get => _entries;
        private set
        {
            SetField(ref _entries, value);
            _entriesView = CollectionViewSource.GetDefaultView(value);
            ApplyGrouping();
            OnPropertyChanged(nameof(EntriesView));
        }
    }

    public ICollectionView? EntriesView => _entriesView;

    public ObservableCollection<WorkCategory> Categories
    {
        get => _categories;
        private set
        {
            SetField(ref _categories, value);
            OnPropertyChanged(nameof(CategoriesWithAll));
        }
    }

    public IReadOnlyList<WorkCategory> CategoriesWithAll =>
        [AllCategoriesSentinel, .. _categories];

    public WorkEntryRowViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            SetField(ref _selectedEntry, value);
            OnPropertyChanged(nameof(HasSelectedEntry));
            OnPropertyChanged(nameof(RenderedNotesHtml));
        }
    }

    public bool HasSelectedEntry => _selectedEntry is not null;

    public WorkCategory SelectedFilterCategory
    {
        get => _selectedFilterCategory ?? AllCategoriesSentinel;
        set
        {
            var category = value?.Id == 0 ? null : value;
            if (SetField(ref _selectedFilterCategory, category))
                _ = ApplyFiltersAsync();
        }
    }

    public string InvoicedFilterText
    {
        get => _invoicedFilterText;
        set
        {
            if (SetField(ref _invoicedFilterText, value))
                _ = ApplyFiltersAsync();
        }
    }

    public IReadOnlyList<string> InvoicedFilterOptions { get; } = ["Uninvoiced", "Invoiced", "All"];

    public DateTime? FilterStartDate
    {
        get => _filterStartDate.HasValue ? _filterStartDate.Value.ToDateTime(TimeOnly.MinValue) : null;
        set
        {
            var d = value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null;
            if (!Equals(_filterStartDate, d))
            {
                _filterStartDate = d;
                OnPropertyChanged();
                _ = ApplyFiltersAsync();
            }
        }
    }

    public DateTime? FilterEndDate
    {
        get => _filterEndDate.HasValue ? _filterEndDate.Value.ToDateTime(TimeOnly.MinValue) : null;
        set
        {
            var d = value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null;
            if (!Equals(_filterEndDate, d))
            {
                _filterEndDate = d;
                OnPropertyChanged();
                _ = ApplyFiltersAsync();
            }
        }
    }

    public bool IsNotesOpen
    {
        get => _isNotesOpen;
        set => SetField(ref _isNotesOpen, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public IReadOnlyList<string> GroupByOptions { get; } = ["None", "Work category", "Invoice", "Month"];

    public string SelectedGroupBy
    {
        get => _selectedGroupBy;
        set
        {
            if (SetField(ref _selectedGroupBy, value))
                ApplyGrouping();
        }
    }

    public string RenderedNotesHtml
    {
        get
        {
            var md = _selectedEntry?.NotesMarkdown;
            if (string.IsNullOrWhiteSpace(md))
                return "<html><body></body></html>";
            var html = Markdown.ToHtml(md);
            return $"<html><body style='font-family:Segoe UI,sans-serif;font-size:12px;margin:6px'>{html}</body></html>";
        }
    }

    // --- Summary ---

    public decimal TotalUninvoicedHours =>
        Entries.Where(e => !e.IsInvoiced).Sum(e => e.Hours);

    public decimal TotalUninvoicedAmount =>
        TotalUninvoicedHours * _hourlyRate;

    public bool IsOverCap =>
        _invoiceCapAmount is > 0 && TotalUninvoicedAmount > _invoiceCapAmount;

    public IReadOnlyList<CategorySummaryLine> CategorySummaryLines =>
        Entries
            .Where(e => !e.IsInvoiced)
            .GroupBy(e => e.CategoryName)
            .Select(g => new CategorySummaryLine(
                string.IsNullOrEmpty(g.Key) ? "Uncategorised" : g.Key,
                g.Sum(e => e.Hours),
                g.Sum(e => e.Hours) * _hourlyRate))
            .OrderBy(l => l.CategoryName)
            .ToList();

    public bool HasAnySelectedUninvoiced =>
        Entries.Any(e => e.IsSelected && !e.IsInvoiced);

    // --- Commands ---

    public ICommand FilterThisMonthCommand => new RelayCommand(param =>
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        _filterStartDate = new DateOnly(today.Year, today.Month, 1);
        _filterEndDate = today;
        OnPropertyChanged(nameof(FilterStartDate));
        OnPropertyChanged(nameof(FilterEndDate));
        _ = ApplyFiltersAsync();
    });

    public ICommand FilterLastMonthCommand => new RelayCommand(param =>
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var first = new DateOnly(today.Year, today.Month, 1).AddMonths(-1);
        _filterStartDate = first;
        _filterEndDate = first.AddMonths(1).AddDays(-1);
        OnPropertyChanged(nameof(FilterStartDate));
        OnPropertyChanged(nameof(FilterEndDate));
        _ = ApplyFiltersAsync();
    });

    public ICommand FilterThisYearCommand => new RelayCommand(param =>
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        _filterStartDate = new DateOnly(today.Year, 1, 1);
        _filterEndDate = today;
        OnPropertyChanged(nameof(FilterStartDate));
        OnPropertyChanged(nameof(FilterEndDate));
        _ = ApplyFiltersAsync();
    });

    public ICommand FilterAllTimeCommand => new RelayCommand(param =>
    {
        _filterStartDate = null;
        _filterEndDate = null;
        OnPropertyChanged(nameof(FilterStartDate));
        OnPropertyChanged(nameof(FilterEndDate));
        _ = ApplyFiltersAsync();
    });

    public ICommand ToggleNotesCommand => new RelayCommand(param => IsNotesOpen = !IsNotesOpen);

    public ICommand PrepareInvoiceCommand => new RelayCommand(
        async param =>
        {
            var selected = Entries.Where(e => e.IsSelected && !e.IsInvoiced).ToList();
            if (selected.Count == 0) return;

            var prepVm = new InvoicePrepViewModel(selected, _hourlyRate, _invoiceCapAmount);
            if (!dialogService.ShowInvoicePrepDialog(prepVm))
                return;

            var invoice = new Invoice
            {
                ClientId = _clientId,
                InvoiceNumber = prepVm.InvoiceNumber,
                InvoiceDate = DateOnly.FromDateTime(prepVm.InvoiceDate),
                TotalAmount = prepVm.TotalAmount,
                PdfPath = prepVm.PdfPath,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var lines = prepVm.LinesByCategory.Select(l => new InvoiceLine
            {
                WorkCategoryId = l.WorkCategoryId,
                Hours = l.Hours,
                Rate = _hourlyRate,
                Amount = l.Amount,
                Description = l.CategoryName
            });

            var created = await invoiceRepository.AddWithLinesAsync(invoice, lines);
            await workEntryRepository.MarkInvoicedAsync(selected.Select(e => e.Id), created.Id);
            await ApplyFiltersAsync();
            InvoiceCreated?.Invoke(this, EventArgs.Empty);
        },
        param => HasAnySelectedUninvoiced);

    public ICommand RefreshCommand => new RelayCommand(async param => await ApplyFiltersAsync());

    public ICommand AddEntryCommand => new RelayCommand(async _ =>
    {
        var enabledCategories = CategoriesWithAll.Skip(1).ToList();
        var vm = new WorkEntryDialogViewModel(enabledCategories);
        if (!dialogService.ShowWorkEntryDialog(vm)) return;

        try
        {
            var entry = new WorkEntry
            {
                ClientId = _clientId,
                Date = DateOnly.FromDateTime(vm.Date),
                Description = vm.Description,
                Hours = vm.Hours,
                WorkCategoryId = vm.SelectedCategory?.Id,
                NotesMarkdown = vm.NotesMarkdown,
                InvoicedFlag = false
            };
            await workEntryRepository.AddAsync(entry);
            await ApplyFiltersAsync();
        }
        catch (Exception ex)
        {
            dialogService.ShowError($"Failed to add entry: {ex.Message}");
        }
    });

    public ICommand EditEntryCommand => new RelayCommand(
        async _ =>
        {
            if (_selectedEntry is null) return;
            var enabledCategories = CategoriesWithAll.Skip(1).ToList();
            var vm = new WorkEntryDialogViewModel(enabledCategories, _selectedEntry.Entry);
            if (!dialogService.ShowWorkEntryDialog(vm)) return;

            try
            {
                var entry = _selectedEntry.Entry;
                entry.Date = DateOnly.FromDateTime(vm.Date);
                entry.Description = vm.Description;
                entry.Hours = vm.Hours;
                entry.WorkCategoryId = vm.SelectedCategory?.Id;
                entry.NotesMarkdown = vm.NotesMarkdown;
                await workEntryRepository.UpdateAsync(entry);
                await ApplyFiltersAsync();
            }
            catch (Exception ex)
            {
                dialogService.ShowError($"Failed to update entry: {ex.Message}");
            }
        },
        _ => _selectedEntry is not null && !_selectedEntry.IsInvoiced);

    public ICommand DeleteEntryCommand => new RelayCommand(
        async _ =>
        {
            if (_selectedEntry is null) return;
            if (!dialogService.Confirm($"Delete entry '{_selectedEntry.Description}'?", "Delete Entry"))
                return;
            try
            {
                await workEntryRepository.DeleteAsync(_selectedEntry.Id);
                await ApplyFiltersAsync();
            }
            catch (Exception ex)
            {
                dialogService.ShowError($"Failed to delete entry: {ex.Message}");
            }
        },
        _ => _selectedEntry is not null && !_selectedEntry.IsInvoiced);

    public event EventHandler? InvoiceCreated;

    // --- Data loading ---

    public async Task LoadAsync(int clientId, decimal hourlyRate, decimal? invoiceCapAmount)
    {
        _clientId = clientId;
        _hourlyRate = hourlyRate;
        _invoiceCapAmount = invoiceCapAmount;

        var categories = await workCategoryRepository.GetByClientAsync(clientId);
        Categories = [.. categories];

        await ApplyFiltersAsync();
    }

    public async Task ApplyFiltersAsync()
    {
        if (_clientId == 0) return;

        IsLoading = true;
        try
        {
            bool? invoiced = InvoicedFilterText switch
            {
                "Invoiced" => true,
                "All" => null,
                _ => false
            };

            var entries = await workEntryRepository.GetFilteredAsync(
                _clientId,
                _filterStartDate,
                _filterEndDate,
                invoiced,
                _selectedFilterCategory?.Id);

            var categoryLookup = _categories.ToDictionary(c => c.Id, c => c.Name);

            Entries = [.. entries.Select(e => new WorkEntryRowViewModel(
                e,
                e.WorkCategoryId.HasValue && categoryLookup.TryGetValue(e.WorkCategoryId.Value, out var name)
                    ? name : null))];

            foreach (var row in Entries)
                row.PropertyChanged += (_, _) => RefreshSummary();

            RefreshSummary();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyGrouping()
    {
        if (_entriesView is null) return;
        _entriesView.GroupDescriptions.Clear();
        switch (_selectedGroupBy)
        {
            case "Work category":
                _entriesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(WorkEntryRowViewModel.CategoryName)));
                break;
            case "Invoice":
                _entriesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(WorkEntryRowViewModel.InvoiceRef)));
                break;
            case "Month":
                _entriesView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(WorkEntryRowViewModel.MonthLabel)));
                break;
        }
    }

    private void RefreshSummary()
    {
        OnPropertyChanged(nameof(TotalUninvoicedHours));
        OnPropertyChanged(nameof(TotalUninvoicedAmount));
        OnPropertyChanged(nameof(IsOverCap));
        OnPropertyChanged(nameof(CategorySummaryLines));
        OnPropertyChanged(nameof(HasAnySelectedUninvoiced));
    }
}
