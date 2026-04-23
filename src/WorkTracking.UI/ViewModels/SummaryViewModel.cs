using WorkTracking.Core.Enums;
using WorkTracking.Core.Models;
using WorkTracking.Core.Services;
using WorkTracking.Data.Repositories.Interfaces;

namespace WorkTracking.UI.ViewModels;

public record MonthlyHourLine(string Month, decimal Hours);
public record CategoryHourLine(string Category, decimal Hours);

public class SummaryViewModel(
    IInvoiceRepository invoiceRepository,
    IWorkEntryRepository workEntryRepository,
    IWorkCategoryRepository workCategoryRepository) : ViewModelBase
{
    private decimal _totalHoursThisYear;
    private decimal _totalInvoicedAmountThisYear;
    private decimal _uninvoicedHours;
    private decimal _uninvoicedAmount;
    private InvoiceCapStatus _capStatus;
    private decimal? _invoiceCapAmount;
    private DateOnly? _nextDueDate;
    private InvoiceFrequencyStatus _frequencyStatus;
    private IReadOnlyList<MonthlyHourLine> _hoursPerMonth = [];
    private IReadOnlyList<CategoryHourLine> _hoursByCategory = [];
    private bool _isLoading;

    public decimal TotalHoursThisYear
    {
        get => _totalHoursThisYear;
        private set => SetField(ref _totalHoursThisYear, value);
    }

    public decimal TotalInvoicedAmountThisYear
    {
        get => _totalInvoicedAmountThisYear;
        private set => SetField(ref _totalInvoicedAmountThisYear, value);
    }

    public decimal UninvoicedHours
    {
        get => _uninvoicedHours;
        private set => SetField(ref _uninvoicedHours, value);
    }

    public decimal UninvoicedAmount
    {
        get => _uninvoicedAmount;
        private set => SetField(ref _uninvoicedAmount, value);
    }

    public InvoiceCapStatus CapStatus
    {
        get => _capStatus;
        private set => SetField(ref _capStatus, value);
    }

    public bool HasCap => _invoiceCapAmount is > 0;
    public bool IsOverCap => _capStatus == InvoiceCapStatus.OverCap;
    public bool HasFrequency => _frequencyStatus != InvoiceFrequencyStatus.NoFrequency;

    public DateOnly? NextDueDate
    {
        get => _nextDueDate;
        private set => SetField(ref _nextDueDate, value);
    }

    public InvoiceFrequencyStatus FrequencyStatus
    {
        get => _frequencyStatus;
        private set => SetField(ref _frequencyStatus, value);
    }

    public string FrequencyStatusLabel => _frequencyStatus switch
    {
        InvoiceFrequencyStatus.Due => "Due",
        InvoiceFrequencyStatus.Overdue => "Overdue",
        InvoiceFrequencyStatus.OnTrack => "On track",
        _ => string.Empty
    };

    public IReadOnlyList<MonthlyHourLine> HoursPerMonth
    {
        get => _hoursPerMonth;
        private set => SetField(ref _hoursPerMonth, value);
    }

    public IReadOnlyList<CategoryHourLine> HoursByCategory
    {
        get => _hoursByCategory;
        private set => SetField(ref _hoursByCategory, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public async Task LoadAsync(Client client)
    {
        IsLoading = true;
        try
        {
            _invoiceCapAmount = client.InvoiceCapAmount;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var yearStart = new DateOnly(today.Year, 1, 1);

            var allEntries = await workEntryRepository.GetByClientAsync(client.Id);
            var thisYearEntries = allEntries.Where(e => e.Date >= yearStart).ToList();
            var uninvoicedEntries = allEntries.Where(e => !e.InvoicedFlag).ToList();

            TotalHoursThisYear = thisYearEntries.Sum(e => e.Hours);
            UninvoicedHours = uninvoicedEntries.Sum(e => e.Hours);
            UninvoicedAmount = UninvoicedHours * client.HourlyRate;
            CapStatus = InvoiceCapCalculator.Calculate(client.InvoiceCapAmount, UninvoicedAmount);
            OnPropertyChanged(nameof(HasCap));
            OnPropertyChanged(nameof(IsOverCap));

            var invoices = await invoiceRepository.GetByClientAsync(client.Id);
            TotalInvoicedAmountThisYear = invoices
                .Where(i => i.InvoiceDate >= yearStart)
                .Sum(i => i.TotalAmount);

            var lastInvoiceDate = invoices.Count > 0 ? invoices.Max(i => i.InvoiceDate) : (DateOnly?)null;
            NextDueDate = InvoiceFrequencyCalculator.CalculateNextDueDate(lastInvoiceDate, client.InvoiceFrequencyDays);
            FrequencyStatus = InvoiceFrequencyCalculator.CalculateStatus(NextDueDate, today);
            OnPropertyChanged(nameof(HasFrequency));
            OnPropertyChanged(nameof(FrequencyStatusLabel));

            HoursPerMonth = Enumerable.Range(1, today.Month)
                .Select(m =>
                {
                    var label = new DateOnly(today.Year, m, 1).ToString("MMM");
                    var hours = thisYearEntries.Where(e => e.Date.Month == m).Sum(e => e.Hours);
                    return new MonthlyHourLine(label, hours);
                })
                .ToList();

            var categories = await workCategoryRepository.GetByClientAsync(client.Id);
            var categoryLookup = categories.ToDictionary(c => c.Id, c => c.Name);

            HoursByCategory = allEntries
                .GroupBy(e => e.WorkCategoryId)
                .Select(g => new CategoryHourLine(
                    g.Key.HasValue && categoryLookup.TryGetValue(g.Key.Value, out var name) ? name : "Uncategorised",
                    g.Sum(e => e.Hours)))
                .OrderByDescending(l => l.Hours)
                .ToList();
        }
        finally
        {
            IsLoading = false;
        }
    }
}
