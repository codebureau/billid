using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class HomeViewModelTests
{
    private static HomeViewModel MakeVm(
        List<Client>? clients = null,
        Dictionary<int, decimal>? uninvoicedHours = null,
        List<WorkEntry>? allEntries = null,
        List<Invoice>? allInvoices = null,
        List<WorkCategory>? categories = null)
    {
        clients ??= [];
        allEntries ??= [];
        allInvoices ??= [];
        categories ??= [];

        var clientRepo = new Mock<IClientRepository>();
        clientRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(clients);

        var entryRepo = new Mock<IWorkEntryRepository>();
        entryRepo.Setup(r => r.GetUninvoicedHoursByClientAsync())
                 .ReturnsAsync(uninvoicedHours ?? new Dictionary<int, decimal>());
        foreach (var client in clients)
        {
            var clientId = client.Id;
            var entries = allEntries.Where(e => e.ClientId == clientId).ToList();
            entryRepo.Setup(r => r.GetByClientAsync(clientId)).ReturnsAsync(entries);
        }

        var invoiceRepo = new Mock<IInvoiceRepository>();
        foreach (var client in clients)
        {
            var clientId = client.Id;
            var invoices = allInvoices.Where(i => i.ClientId == clientId).ToList();
            invoiceRepo.Setup(r => r.GetByClientAsync(clientId)).ReturnsAsync(invoices);
        }

        var categoryRepo = new Mock<IWorkCategoryRepository>();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        return new HomeViewModel(clientRepo.Object, entryRepo.Object, invoiceRepo.Object, categoryRepo.Object);
    }

    [Fact]
    public async Task LoadAsync_WithNoData_AllZeroes()
    {
        var vm = MakeVm();

        await vm.LoadAsync();

        vm.TotalUnbilledHours.Should().Be(0);
        vm.InvoicedRolling30Days.Should().Be(0);
        vm.ActiveClientCount.Should().Be(0);
    }

    [Fact]
    public async Task LoadAsync_ActiveClientCount_ReflectsClientList()
    {
        var clients = new List<Client>
        {
            new() { Id = 1, Name = "Alpha" },
            new() { Id = 2, Name = "Beta" }
        };
        var vm = MakeVm(clients: clients);

        await vm.LoadAsync();

        vm.ActiveClientCount.Should().Be(2);
    }

    [Fact]
    public async Task LoadAsync_TotalUnbilledHours_SumsAcrossClients()
    {
        var clients = new List<Client>
        {
            new() { Id = 1, Name = "Alpha" },
            new() { Id = 2, Name = "Beta" }
        };
        var uninvoiced = new Dictionary<int, decimal> { [1] = 3.5m, [2] = 6.0m };
        var vm = MakeVm(clients: clients, uninvoicedHours: uninvoiced);

        await vm.LoadAsync();

        vm.TotalUnbilledHours.Should().Be(9.5m);
    }

    [Fact]
    public async Task LoadAsync_InvoicedRolling30Days_SumsRecentInvoices()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var clients = new List<Client> { new() { Id = 1, Name = "Alpha" } };
        var invoices = new List<Invoice>
        {
            new() { Id = 1, ClientId = 1, InvoiceDate = today.AddDays(-5),  TotalAmount = 1000m },
            new() { Id = 2, ClientId = 1, InvoiceDate = today.AddDays(-15), TotalAmount = 500m  },
            new() { Id = 3, ClientId = 1, InvoiceDate = today.AddDays(-60), TotalAmount = 999m  } // outside window
        };
        var vm = MakeVm(clients: clients, allInvoices: invoices);

        await vm.LoadAsync();

        vm.InvoicedRolling30Days.Should().Be(1500m);
    }

    [Fact]
    public async Task LoadAsync_ClientStatuses_OneRowPerClient()
    {
        var clients = new List<Client>
        {
            new() { Id = 1, Name = "Alpha", HourlyRate = 100m },
            new() { Id = 2, Name = "Beta",  HourlyRate = 120m }
        };
        var vm = MakeVm(clients: clients);

        await vm.LoadAsync();

        vm.ClientStatuses.Should().HaveCount(2);
        vm.ClientStatuses.Select(s => s.Name).Should().BeEquivalentTo(["Alpha", "Beta"]);
    }

    [Fact]
    public async Task LoadAsync_MonthlyReport_HasTwelveEntries()
    {
        var vm = MakeVm();

        await vm.LoadAsync();

        vm.MonthlyReport.Should().HaveCount(12);
    }

    [Fact]
    public async Task LoadAsync_MonthlyReport_AggregatesHoursCorrectly()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var clients = new List<Client> { new() { Id = 1, Name = "Alpha" } };
        var entries = new List<WorkEntry>
        {
            new() { ClientId = 1, Date = today, Hours = 4m },
            new() { ClientId = 1, Date = today, Hours = 2m }
        };
        var vm = MakeVm(clients: clients, allEntries: entries);

        await vm.LoadAsync();

        var thisMonth = vm.MonthlyReport.Last();
        thisMonth.Hours.Should().Be(6m);
    }

    [Fact]
    public async Task LoadAsync_CategoryDistribution_GroupsByCategory()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var clients = new List<Client> { new() { Id = 1, Name = "Alpha" } };
        var categories = new List<WorkCategory>
        {
            new() { Id = 10, Name = "Dev" },
            new() { Id = 20, Name = "Design" }
        };
        var entries = new List<WorkEntry>
        {
            new() { ClientId = 1, Date = today, Hours = 5m, WorkCategoryId = 10 },
            new() { ClientId = 1, Date = today, Hours = 3m, WorkCategoryId = 10 },
            new() { ClientId = 1, Date = today, Hours = 2m, WorkCategoryId = 20 }
        };
        var vm = MakeVm(clients: clients, allEntries: entries, categories: categories);

        await vm.LoadAsync();

        vm.CategoryDistribution.Should().HaveCount(2);
        vm.CategoryDistribution.First(c => c.Category == "Dev").Hours.Should().Be(8m);
        vm.CategoryDistribution.First(c => c.Category == "Design").Hours.Should().Be(2m);
    }

    [Fact]
    public async Task LoadAsync_HoursByClient_SumsAllEntriesPerClient()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var clients = new List<Client>
        {
            new() { Id = 1, Name = "Alpha" },
            new() { Id = 2, Name = "Beta"  }
        };
        var entries = new List<WorkEntry>
        {
            new() { ClientId = 1, Date = today, Hours = 3m },
            new() { ClientId = 1, Date = today, Hours = 2m },
            new() { ClientId = 2, Date = today, Hours = 7m }
        };
        var vm = MakeVm(clients: clients, allEntries: entries);

        await vm.LoadAsync();

        vm.HoursByClient.Should().HaveCount(2);
        vm.HoursByClient.First(c => c.ClientName == "Alpha").Hours.Should().Be(5m);
        vm.HoursByClient.First(c => c.ClientName == "Beta").Hours.Should().Be(7m);
    }

    [Fact]
    public async Task LoadAsync_SetsIsLoadingFalseWhenComplete()
    {
        var vm = MakeVm();

        await vm.LoadAsync();

        vm.IsLoading.Should().BeFalse();
    }
}
