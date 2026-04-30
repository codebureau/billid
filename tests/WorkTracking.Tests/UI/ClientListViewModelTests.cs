using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Services;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class ClientListViewModelTests
{
    private static List<Client> SampleClients() =>
    [
        new() { Id = 1, Name = "Alpha Ltd",   CompanyName = "Alpha" },
        new() { Id = 2, Name = "Beta Corp",   CompanyName = "Beta" },
        new() { Id = 3, Name = "Gamma Inc",   CompanyName = "Gamma" },
    ];

    private static Mock<IClientRepository> RepoWith(List<Client> clients)
    {
        var mock = new Mock<IClientRepository>();
        mock.Setup(r => r.GetAllAsync(It.IsAny<bool>())).ReturnsAsync(clients);
        return mock;
    }

    private static Mock<IWorkEntryRepository> WorkEntryRepoWithHours(Dictionary<int, decimal>? hours = null)
    {
        var mock = new Mock<IWorkEntryRepository>();
        mock.Setup(r => r.GetUninvoicedHoursByClientAsync()).ReturnsAsync(hours ?? []);
        return mock;
    }

    private static AppSettingsViewModel MakeAppSettings() { var theme = new Mock<IThemeService>(); theme.Setup(t => t.CurrentTheme).Returns(AppTheme.Light); var settings = new Mock<ISettingRepository>(); settings.Setup(s => s.GetAsync(It.IsAny<string>())).ReturnsAsync((string?)null); return new AppSettingsViewModel(theme.Object, settings.Object); }
    private static ClientListViewModel MakeVm(List<Client> clients, Dictionary<int, decimal>? hours = null) =>
        new(RepoWith(clients).Object, WorkEntryRepoWithHours(hours).Object, new Mock<IDialogService>().Object, MakeAppSettings());

    [Fact]
    public async Task LoadAsync_WithClients_PopulatesClients()
    {
        var vm = MakeVm(SampleClients());

        await vm.LoadAsync();

        vm.Clients.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoadAsync_WithNoClients_ReturnsEmptyList()
    {
        var vm = MakeVm([]);

        await vm.LoadAsync();

        vm.Clients.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchText_FiltersByName_CaseInsensitive()
    {
        var vm = MakeVm(SampleClients());
        await vm.LoadAsync();

        vm.SearchText = "alpha";

        vm.Clients.Should().ContainSingle(c => c.Name == "Alpha Ltd");
    }

    [Fact]
    public async Task SearchText_WhenCleared_ShowsAllClients()
    {
        var vm = MakeVm(SampleClients());
        await vm.LoadAsync();
        vm.SearchText = "alpha";

        vm.SearchText = string.Empty;

        vm.Clients.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchText_NoMatch_ReturnsEmptyList()
    {
        var vm = MakeVm(SampleClients());
        await vm.LoadAsync();

        vm.SearchText = "zzz";

        vm.Clients.Should().BeEmpty();
    }

    [Fact]
    public async Task SelectedClient_SetAndGet_RaisesPropertyChanged()
    {
        var vm = MakeVm(SampleClients());
        await vm.LoadAsync();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        var row = vm.Clients[0];

        vm.SelectedClient = row.Client;

        vm.SelectedClient.Should().Be(row.Client);
        raised.Should().Contain(nameof(ClientListViewModel.SelectedClient));
    }

    [Fact]
    public async Task IsCapExceeded_WhenUninvoicedTotalExceedsCap_IsTrue()
    {
        var clients = new List<Client>
        {
            new() { Id = 1, Name = "CapClient", HourlyRate = 100, InvoiceCapAmount = 500 }
        };
        // 6 hours * $100 = $600 > $500 cap
        var vm = MakeVm(clients, new Dictionary<int, decimal> { [1] = 6m });
        await vm.LoadAsync();

        vm.Clients[0].IsCapExceeded.Should().BeTrue();
    }

    [Fact]
    public async Task IsCapExceeded_WhenUninvoicedTotalUnderCap_IsFalse()
    {
        var clients = new List<Client>
        {
            new() { Id = 1, Name = "CapClient", HourlyRate = 100, InvoiceCapAmount = 500 }
        };
        // 4 hours * $100 = $400 < $500 cap
        var vm = MakeVm(clients, new Dictionary<int, decimal> { [1] = 4m });
        await vm.LoadAsync();

        vm.Clients[0].IsCapExceeded.Should().BeFalse();
    }

    [Fact]
    public async Task IsCapExceeded_WhenNoCap_IsFalse()
    {
        var clients = new List<Client>
        {
            new() { Id = 1, Name = "NoCap", HourlyRate = 100, InvoiceCapAmount = null }
        };
        var vm = MakeVm(clients, new Dictionary<int, decimal> { [1] = 100m });
        await vm.LoadAsync();

        vm.Clients[0].IsCapExceeded.Should().BeFalse();
    }

    [Fact]
    public async Task IsInvoiceOverdue_WhenNextDueDateInPast_IsTrue()
    {
        var clients = new List<Client>
        {
            new() { Id = 1, Name = "OverdueClient", HourlyRate = 100,
                NextInvoiceDueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) }
        };
        var vm = MakeVm(clients);
        await vm.LoadAsync();

        vm.Clients[0].IsInvoiceOverdue.Should().BeTrue();
    }

    [Fact]
    public async Task IsInvoiceOverdue_WhenNextDueDateInFuture_IsFalse()
    {
        var clients = new List<Client>
        {
            new() { Id = 1, Name = "OnTrackClient", HourlyRate = 100,
                NextInvoiceDueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)) }
        };
        var vm = MakeVm(clients);
        await vm.LoadAsync();

        vm.Clients[0].IsInvoiceOverdue.Should().BeFalse();
    }

    [Fact]
    public async Task IsInvoiceOverdue_WhenNoNextDueDate_IsFalse()
    {
        var clients = new List<Client>
        {
            new() { Id = 1, Name = "NoFreqClient", HourlyRate = 100, NextInvoiceDueDate = null }
        };
        var vm = MakeVm(clients);
        await vm.LoadAsync();

        vm.Clients[0].IsInvoiceOverdue.Should().BeFalse();
    }
}