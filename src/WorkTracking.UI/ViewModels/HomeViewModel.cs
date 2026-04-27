using System.Collections.ObjectModel;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;

namespace WorkTracking.UI.ViewModels;

public record MonthlyReportLine(string Month, decimal Hours, decimal InvoicedAmount);
public record ClientHourLine(string ClientName, decimal Hours);

public class HomeViewModel(
    IClientRepository clientRepository,
    IWorkEntryRepository workEntryRepository,
    IInvoiceRepository invoiceRepository,
    IWorkCategoryRepository workCategoryRepository) : ViewModelBase
{
    private decimal _totalUnbilledHours;
    private decimal _invoicedRolling30Days;
    private int _activeClientCount;
    private bool _isLoading;
    private IReadOnlyList<ClientStatusRowViewModel> _clientStatuses = [];
    private IReadOnlyList<MonthlyReportLine> _monthlyReport = [];
    private IReadOnlyList<CategoryHourLine> _categoryDistribution = [];
    private IReadOnlyList<ClientHourLine> _hoursByClient = [];

    public decimal TotalUnbilledHours
    {
        get => _totalUnbilledHours;
        private set => SetField(ref _totalUnbilledHours, value);
    }

    public decimal InvoicedRolling30Days
    {
        get => _invoicedRolling30Days;
        private set => SetField(ref _invoicedRolling30Days, value);
    }

    public int ActiveClientCount
    {
        get => _activeClientCount;
        private set => SetField(ref _activeClientCount, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value);
    }

    public IReadOnlyList<ClientStatusRowViewModel> ClientStatuses
    {
        get => _clientStatuses;
        private set => SetField(ref _clientStatuses, value);
    }

    public IReadOnlyList<MonthlyReportLine> MonthlyReport
    {
        get => _monthlyReport;
        private set => SetField(ref _monthlyReport, value);
    }

    public IReadOnlyList<CategoryHourLine> CategoryDistribution
    {
        get => _categoryDistribution;
        private set => SetField(ref _categoryDistribution, value);
    }

    public IReadOnlyList<ClientHourLine> HoursByClient
    {
        get => _hoursByClient;
        private set => SetField(ref _hoursByClient, value);
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var thirtyDaysAgo = today.AddDays(-30);

            var clients = await clientRepository.GetAllAsync();
            ActiveClientCount = clients.Count;

            // Load uninvoiced hours in one query
            var uninvoicedByClient = await workEntryRepository.GetUninvoicedHoursByClientAsync();
            TotalUnbilledHours = uninvoicedByClient.Values.Sum();

            // Load all entries and invoices per client in parallel
            var entryTasks   = clients.Select(c => workEntryRepository.GetByClientAsync(c.Id)).ToArray();
            var invoiceTasks = clients.Select(c => invoiceRepository.GetByClientAsync(c.Id)).ToArray();
            await Task.WhenAll([.. entryTasks, .. invoiceTasks]);

            var allEntries  = entryTasks.SelectMany(t => t.Result).ToList();
            var allInvoices = invoiceTasks.SelectMany(t => t.Result).ToList();

            // Rolling 30-day invoiced total
            InvoicedRolling30Days = allInvoices
                .Where(i => i.InvoiceDate >= thirtyDaysAgo)
                .Sum(i => i.TotalAmount);

            // Per-client status rows
            ClientStatuses = clients
                .Select(c =>
                {
                    uninvoicedByClient.TryGetValue(c.Id, out var hours);
                    return new ClientStatusRowViewModel(c, hours, today);
                })
                .ToList();

            // Monthly report: last 12 rolling months
            MonthlyReport = Enumerable.Range(0, 12)
                .Select(i =>
                {
                    var month = today.AddMonths(-11 + i);
                    var label = new DateOnly(month.Year, month.Month, 1).ToString("MMM yyyy");
                    var hours = allEntries
                        .Where(e => e.Date.Year == month.Year && e.Date.Month == month.Month)
                        .Sum(e => e.Hours);
                    var invoiced = allInvoices
                        .Where(inv => inv.InvoiceDate.Year == month.Year && inv.InvoiceDate.Month == month.Month)
                        .Sum(inv => inv.TotalAmount);
                    return new MonthlyReportLine(label, hours, invoiced);
                })
                .ToList();

            // Hours by client (all time)
            var clientNameLookup = clients.ToDictionary(c => c.Id, c => c.Name);
            HoursByClient = allEntries
                .GroupBy(e => e.ClientId)
                .Select(g => new ClientHourLine(
                    clientNameLookup.TryGetValue(g.Key, out var name) ? name : "Unknown",
                    g.Sum(e => e.Hours)))
                .OrderByDescending(l => l.Hours)
                .ToList();

            // Cross-client category distribution
            var allCategories = await workCategoryRepository.GetAllAsync();
            var categoryLookup = allCategories.ToDictionary(c => c.Id, c => c.Name);
            CategoryDistribution = allEntries
                .GroupBy(e => e.WorkCategoryId)
                .Select(g => new CategoryHourLine(
                    g.Key.HasValue && categoryLookup.TryGetValue(g.Key.Value, out var catName)
                        ? catName : "Uncategorised",
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
