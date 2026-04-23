using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class InvoicesViewModelTests
{
    private static Invoice MakeInvoice(int id, string number = "INV-001") =>
        new() { Id = id, ClientId = 1, InvoiceNumber = number, InvoiceDate = new DateOnly(2025, 1, 1), TotalAmount = 1000m };

    private static (InvoicesViewModel vm, Mock<IInvoiceRepository> invoiceRepo, Mock<IWorkEntryRepository> entryRepo) MakeVm()
    {
        var invoiceRepo = new Mock<IInvoiceRepository>();
        var entryRepo = new Mock<IWorkEntryRepository>();
        return (new InvoicesViewModel(invoiceRepo.Object, entryRepo.Object), invoiceRepo, entryRepo);
    }

    [Fact]
    public async Task LoadAsync_WithInvoices_PopulatesInvoices()
    {
        var (vm, invoiceRepo, _) = MakeVm();
        invoiceRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([MakeInvoice(1), MakeInvoice(2, "INV-002")]);

        await vm.LoadAsync(1);

        vm.Invoices.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAsync_WithNoInvoices_ReturnsEmptyList()
    {
        var (vm, invoiceRepo, _) = MakeVm();
        invoiceRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1);

        vm.Invoices.Should().BeEmpty();
    }

    [Fact]
    public async Task HasInvoices_WhenInvoicesLoaded_ReturnsTrue()
    {
        var (vm, invoiceRepo, _) = MakeVm();
        invoiceRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([MakeInvoice(1)]);

        await vm.LoadAsync(1);

        vm.HasInvoices.Should().BeTrue();
    }

    [Fact]
    public async Task HasInvoices_WhenNoInvoices_ReturnsFalse()
    {
        var (vm, invoiceRepo, _) = MakeVm();
        invoiceRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1);

        vm.HasInvoices.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_SetsIsLoadingFalseWhenComplete()
    {
        var (vm, invoiceRepo, _) = MakeVm();
        invoiceRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1);

        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_ClearsSelectedInvoice()
    {
        var (vm, invoiceRepo, entryRepo) = MakeVm();
        invoiceRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([MakeInvoice(1)]);
        invoiceRepo.Setup(r => r.GetLinesAsync(It.IsAny<int>())).ReturnsAsync([]);
        entryRepo.Setup(r => r.GetByInvoiceIdAsync(It.IsAny<int>())).ReturnsAsync([]);
        await vm.LoadAsync(1);
        vm.SelectedInvoice = vm.Invoices[0];

        await vm.LoadAsync(1);

        vm.SelectedInvoice.Should().BeNull();
    }

    [Fact]
    public async Task SettingSelectedInvoice_LoadsLinesAndEntries()
    {
        var (vm, invoiceRepo, entryRepo) = MakeVm();
        var line = new InvoiceLine { Id = 1, InvoiceId = 1, Hours = 2m, Rate = 100m, Amount = 200m };
        var entry = new WorkEntry { Id = 1, ClientId = 1, Date = DateOnly.FromDateTime(DateTime.Today), Hours = 2m, Description = "Work" };
        invoiceRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([MakeInvoice(1)]);
        invoiceRepo.Setup(r => r.GetLinesAsync(1)).ReturnsAsync([line]);
        entryRepo.Setup(r => r.GetByInvoiceIdAsync(1)).ReturnsAsync([entry]);
        await vm.LoadAsync(1);

        vm.SelectedInvoice = vm.Invoices[0];
        await Task.Delay(50); // allow async detail load

        vm.SelectedInvoice!.Lines.Should().HaveCount(1);
        vm.SelectedInvoice.LinkedEntries.Should().HaveCount(1);
    }
}
