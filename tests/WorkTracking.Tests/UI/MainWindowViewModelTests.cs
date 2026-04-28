using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Services;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class MainWindowViewModelTests
{
    private static ClientListViewModel MakeClientListVm(List<Client>? clients = null)
    {
        var mock = new Mock<IClientRepository>();
        mock.Setup(r => r.GetAllAsync()).ReturnsAsync(clients ?? []);
        var workEntryMock = new Mock<IWorkEntryRepository>();
        workEntryMock.Setup(r => r.GetUninvoicedHoursByClientAsync()).ReturnsAsync(new Dictionary<int, decimal>());
        return new ClientListViewModel(mock.Object, workEntryMock.Object, new Mock<IDialogService>().Object);
    }

    private static TimesheetViewModel MakeTimesheetVm()
    {
        var entryRepo = new Mock<IWorkEntryRepository>();
        entryRepo.Setup(r => r.GetFilteredAsync(It.IsAny<int>(), null, null, It.IsAny<bool?>(), null))
                 .ReturnsAsync([]);
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        var invoiceRepo = new Mock<IInvoiceRepository>();
        var dialogService = new Mock<IDialogService>();
        return new TimesheetViewModel(entryRepo.Object, categoryRepo.Object, invoiceRepo.Object, new Mock<IAttachmentRepository>().Object, dialogService.Object);
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

    private static ClientDetailViewModel MakeDetailVm() => new(MakeTimesheetVm(), MakeInvoicesVm(), MakeSummaryVm(), MakeSettingsVm());
    private static AppSettingsViewModel MakeAppSettingsVm()
    {
        var themeService = new Mock<IThemeService>();
        themeService.SetupGet(t => t.CurrentTheme).Returns(AppTheme.Light);
        var settingRepo = new Mock<ISettingRepository>();
        settingRepo.Setup(r => r.GetAsync(It.IsAny<string>())).ReturnsAsync((string?)null);
        return new AppSettingsViewModel(themeService.Object, settingRepo.Object);
    }
    private static HomeViewModel MakeHomeVm()
    {
        var clientRepo = new Mock<IClientRepository>();
        clientRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        var entryRepo = new Mock<IWorkEntryRepository>();
        entryRepo.Setup(r => r.GetUninvoicedHoursByClientAsync()).ReturnsAsync(new Dictionary<int, decimal>());
        var invoiceRepo = new Mock<IInvoiceRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        return new HomeViewModel(clientRepo.Object, entryRepo.Object, invoiceRepo.Object, categoryRepo.Object);
    }

    [Fact]
    public async Task InitializeAsync_LoadsClientList()
    {
        var clients = new List<Client> { new() { Id = 1, Name = "Acme" } };
        var listVm = MakeClientListVm(clients);
        var detailVm = MakeDetailVm();
        var vm = new MainWindowViewModel(listVm, detailVm, MakeHomeVm(), MakeAppSettingsVm());

        await vm.InitializeAsync();

        vm.ClientList.Clients.Should().HaveCount(1);
    }

    [Fact]
    public async Task SelectingClient_PopulatesClientDetail()
    {
        var client = new Client { Id = 1, Name = "Acme" };
        var listVm = MakeClientListVm([client]);
        var detailVm = MakeDetailVm();
        var vm = new MainWindowViewModel(listVm, detailVm, MakeHomeVm(), MakeAppSettingsVm());
        await vm.InitializeAsync();

        listVm.SelectedClient = client;

        detailVm.HasClient.Should().BeTrue();
        detailVm.Client.Should().Be(client);
    }

    [Fact]
    public async Task DeselectingClient_ClearsClientDetail()
    {
        var client = new Client { Id = 1, Name = "Acme" };
        var listVm = MakeClientListVm([client]);
        var detailVm = MakeDetailVm();
        var vm = new MainWindowViewModel(listVm, detailVm, MakeHomeVm(), MakeAppSettingsVm());
        await vm.InitializeAsync();
        listVm.SelectedClient = client;

        listVm.SelectedClient = null;

        detailVm.HasClient.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WithNoClients_ClientDetailHasNoClient()
    {
        var listVm = MakeClientListVm([]);
        var detailVm = MakeDetailVm();
        var vm = new MainWindowViewModel(listVm, detailVm, MakeHomeVm(), MakeAppSettingsVm());

        await vm.InitializeAsync();

        detailVm.HasClient.Should().BeFalse();
    }
}
