using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Services;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class ClientDetailViewModelTests
{
    private static Client MakeClient(string name = "Acme") =>
        new() { Id = 1, Name = name, CompanyName = "Acme Corp" };

    private static TimesheetViewModel MakeTimesheetVm()
    {
        var entryRepo = new Mock<IWorkEntryRepository>();
        entryRepo.Setup(r => r.GetFilteredAsync(It.IsAny<int>(), null, null, It.IsAny<bool?>(), null))
                 .ReturnsAsync([]);
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        var invoiceRepo = new Mock<IInvoiceRepository>();
        var dialogService = new Mock<IDialogService>();
        return new TimesheetViewModel(entryRepo.Object, categoryRepo.Object, invoiceRepo.Object, dialogService.Object);
    }

    private static InvoicesViewModel MakeInvoicesVm()
    {
        var invoiceRepo = new Mock<IInvoiceRepository>();
        invoiceRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        var entryRepo = new Mock<IWorkEntryRepository>();
        return new InvoicesViewModel(invoiceRepo.Object, entryRepo.Object);
    }

    private static SummaryViewModel MakeSummaryVm()
    {
        var invoiceRepo = new Mock<IInvoiceRepository>();
        invoiceRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        var entryRepo = new Mock<IWorkEntryRepository>();
        entryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        return new SummaryViewModel(invoiceRepo.Object, entryRepo.Object, categoryRepo.Object);
    }

    private static ClientSettingsViewModel MakeSettingsVm()
    {
        var clientRepo = new Mock<IClientRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        var dialogService = new Mock<IDialogService>();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        return new ClientSettingsViewModel(clientRepo.Object, categoryRepo.Object, dialogService.Object);
    }

    private static ClientDetailViewModel MakeVm() =>
        new(MakeTimesheetVm(), MakeInvoicesVm(), MakeSummaryVm(), MakeSettingsVm());

    [Fact]
    public void HasClient_WhenNew_ReturnsFalse()
    {
        var vm = MakeVm();

        vm.HasClient.Should().BeFalse();
    }

    [Fact]
    public void LoadClient_SetsClientAndHasClientTrue()
    {
        var vm = MakeVm();
        var client = MakeClient();

        vm.LoadClient(client);

        vm.Client.Should().Be(client);
        vm.HasClient.Should().BeTrue();
    }

    [Fact]
    public void LoadClient_ResetsSelectedTabIndex()
    {
        var vm = MakeVm();
        vm.LoadClient(MakeClient());
        vm.SelectedTabIndex = 2;

        vm.LoadClient(MakeClient("Other"));

        vm.SelectedTabIndex.Should().Be(0);
    }

    [Fact]
    public void Clear_RemovesClientAndHasClientFalse()
    {
        var vm = MakeVm();
        vm.LoadClient(MakeClient());

        vm.Clear();

        vm.Client.Should().BeNull();
        vm.HasClient.Should().BeFalse();
    }

    [Fact]
    public void LoadClient_RaisesPropertyChangedForHasClient()
    {
        var vm = MakeVm();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.LoadClient(MakeClient());

        raised.Should().Contain(nameof(ClientDetailViewModel.HasClient));
    }

    [Fact]
    public void Clear_RaisesPropertyChangedForHasClient()
    {
        var vm = MakeVm();
        vm.LoadClient(MakeClient());
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.Clear();

        raised.Should().Contain(nameof(ClientDetailViewModel.HasClient));
    }
}
