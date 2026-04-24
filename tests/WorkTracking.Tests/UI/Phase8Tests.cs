using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Services;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

// ── ClientListViewModel empty-state ──────────────────────────────────────────

public class ClientListViewModelEmptyStateTests
{
    private static ClientListViewModel MakeVm(List<Client> clients)
    {
        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(clients);
        var workEntryMock = new Mock<IWorkEntryRepository>();
        workEntryMock.Setup(r => r.GetUninvoicedHoursByClientAsync()).ReturnsAsync(new Dictionary<int, decimal>());
        return new ClientListViewModel(repo.Object, workEntryMock.Object, new Mock<IDialogService>().Object);
    }

    [Fact]
    public async Task HasClients_WithNoClients_ReturnsFalse()
    {
        var vm = MakeVm([]);
        await vm.LoadAsync();

        vm.HasClients.Should().BeFalse();
    }

    [Fact]
    public async Task HasClients_WithClients_ReturnsTrue()
    {
        var vm = MakeVm([new Client { Id = 1, Name = "Acme" }]);
        await vm.LoadAsync();

        vm.HasClients.Should().BeTrue();
    }
}

// ── ClientSettingsViewModel — IsDirty on category toggle ─────────────────────

public class ClientSettingsViewModelCategoryDirtyTests
{
    private static (ClientSettingsViewModel vm, Mock<IWorkCategoryRepository> categoryRepo) MakeVm()
    {
        var clientRepo = new Mock<IClientRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        var dialogService = new Mock<IDialogService>();
        var cat = new WorkCategory { Id = 1, Name = "Dev" };
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([cat]);
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([cat]);
        var vm = new ClientSettingsViewModel(clientRepo.Object, categoryRepo.Object, dialogService.Object);
        return (vm, categoryRepo);
    }

    [Fact]
    public async Task TogglingCategory_SetsIsDirtyTrue()
    {
        var (vm, _) = MakeVm();
        var client = new Client { Id = 1, Name = "Acme", HourlyRate = 100m };
        await vm.LoadAsync(client);
        vm.IsDirty.Should().BeFalse();

        vm.Categories[0].IsEnabled = !vm.Categories[0].IsEnabled;

        vm.IsDirty.Should().BeTrue();
    }
}

// ── ClientSettingsViewModel — error handling ─────────────────────────────────

public class ClientSettingsViewModelErrorHandlingTests
{
    [Fact]
    public async Task SaveAsync_WhenRepositoryThrows_ShowsError()
    {
        var clientRepo = new Mock<IClientRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        var dialogService = new Mock<IDialogService>();
        var vm = new ClientSettingsViewModel(clientRepo.Object, categoryRepo.Object, dialogService.Object);

        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        clientRepo.Setup(r => r.UpdateAsync(It.IsAny<Client>())).ThrowsAsync(new Exception("DB error"));

        var client = new Client { Id = 1, Name = "Acme", HourlyRate = 100m };
        await vm.LoadAsync(client);
        vm.Name = "Updated";

        await vm.SaveAsync();

        dialogService.Verify(d => d.ShowError(It.Is<string>(m => m.Contains("DB error")), It.IsAny<string>()), Times.Once);
        vm.IsDirty.Should().BeTrue();
    }
}

// ── TimesheetViewModel — grouping ────────────────────────────────────────────

public class TimesheetViewModelGroupingTests
{
    private static (TimesheetViewModel vm, Mock<IWorkEntryRepository> entryRepo) MakeVm()
    {
        var entryRepo = new Mock<IWorkEntryRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        var invoiceRepo = new Mock<IInvoiceRepository>();
        var dialogService = new Mock<IDialogService>();
        var vm = new TimesheetViewModel(entryRepo.Object, categoryRepo.Object, invoiceRepo.Object, dialogService.Object);
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        return (vm, entryRepo);
    }

    [Fact]
    public void GroupByOptions_ContainsExpectedValues()
    {
        var (vm, _) = MakeVm();

        vm.GroupByOptions.Should().BeEquivalentTo(["None", "Work category", "Invoice", "Month"]);
    }

    [Fact]
    public void SelectedGroupBy_DefaultsToNone()
    {
        var (vm, _) = MakeVm();

        vm.SelectedGroupBy.Should().Be("None");
    }

    [Fact]
    public async Task SelectedGroupBy_ChangingValue_SetsProperty()
    {
        var (vm, entryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(
            It.IsAny<int>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<bool?>(), It.IsAny<int?>()))
            .ReturnsAsync([]);
        await vm.LoadAsync(1, 100m, null);

        vm.SelectedGroupBy = "Month";

        vm.SelectedGroupBy.Should().Be("Month");
    }
}

// ── TimesheetViewModel — RenderedNotesHtml ────────────────────────────────────

public class TimesheetViewModelMarkdownTests
{
    private static TimesheetViewModel MakeVm()
    {
        var entryRepo = new Mock<IWorkEntryRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        var invoiceRepo = new Mock<IInvoiceRepository>();
        var dialogService = new Mock<IDialogService>();
        return new TimesheetViewModel(entryRepo.Object, categoryRepo.Object, invoiceRepo.Object, dialogService.Object);
    }

    [Fact]
    public void RenderedNotesHtml_WhenNoEntrySelected_ReturnsEmptyBody()
    {
        var vm = MakeVm();

        vm.RenderedNotesHtml.Should().Contain("<body>");
    }

    [Fact]
    public void RenderedNotesHtml_WithMarkdownNotes_ConvertsToHtml()
    {
        var vm = MakeVm();
        var entry = new WorkEntry
        {
            Id = 1, ClientId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Description = "Test",
            Hours = 1m,
            NotesMarkdown = "**Bold text**"
        };
        vm.SelectedEntry = new WorkEntryRowViewModel(entry, null);

        vm.RenderedNotesHtml.Should().Contain("<strong>Bold text</strong>");
    }
}

// ── WorkEntryRowViewModel — MonthLabel ────────────────────────────────────────

public class WorkEntryRowViewModelMonthLabelTests
{
    [Fact]
    public void MonthLabel_ReturnsFormattedMonth()
    {
        var entry = new WorkEntry
        {
            Id = 1, ClientId = 1,
            Date = new DateOnly(2024, 3, 15),
            Description = "Work",
            Hours = 2m
        };
        var vm = new WorkEntryRowViewModel(entry, null);

        vm.MonthLabel.Should().Be("March 2024");
    }
}


// -- WorkEntryRowViewModel � IsInvoiced is read-only --------------------------
// Regression: WPF DataGridCheckBoxColumn binds TwoWay by default, which throws
// InvalidOperationException when the property has no setter. IsInvoiced must
// remain a computed read-only property (no public setter).

public class WorkEntryRowViewModelIsInvoicedTests
{
    [Fact]
    public void IsInvoiced_Property_HasNoPublicSetter()
    {
        var prop = typeof(WorkEntryRowViewModel).GetProperty(nameof(WorkEntryRowViewModel.IsInvoiced));

        prop.Should().NotBeNull();
        prop!.GetSetMethod(nonPublic: false).Should().BeNull("IsInvoiced must be read-only to avoid TwoWay binding exceptions in WPF DataGrid");
    }

    [Fact]
    public void IsInvoiced_WhenEntryNotInvoiced_ReturnsFalse()
    {
        var entry = new WorkEntry { Id = 1, ClientId = 1, Date = DateOnly.FromDateTime(DateTime.Today), Description = "Test", Hours = 1m, InvoicedFlag = false };
        var vm = new WorkEntryRowViewModel(entry, null);

        vm.IsInvoiced.Should().BeFalse();
    }

    [Fact]
    public void IsInvoiced_WhenEntryIsInvoiced_ReturnsTrue()
    {
        var entry = new WorkEntry { Id = 1, ClientId = 1, Date = DateOnly.FromDateTime(DateTime.Today), Description = "Test", Hours = 1m, InvoicedFlag = true };
        var vm = new WorkEntryRowViewModel(entry, null);

        vm.IsInvoiced.Should().BeTrue();
    }
}
