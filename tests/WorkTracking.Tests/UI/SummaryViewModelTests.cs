using FluentAssertions;
using Moq;
using WorkTracking.Core.Enums;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class SummaryViewModelTests
{
    private static Client MakeClient(decimal rate = 100m, decimal? cap = null, int? freqDays = null) =>
        new() { Id = 1, Name = "Acme", HourlyRate = rate, InvoiceCapAmount = cap, InvoiceFrequencyDays = freqDays };

    private static WorkEntry MakeEntry(int id, decimal hours, bool invoiced = false, DateOnly? date = null, int? categoryId = null) =>
        new() { Id = id, ClientId = 1, Hours = hours, InvoicedFlag = invoiced, Date = date ?? DateOnly.FromDateTime(DateTime.Today), Description = "Work", WorkCategoryId = categoryId };

    private static Invoice MakeInvoice(int id, decimal amount, DateOnly date) =>
        new() { Id = id, ClientId = 1, InvoiceNumber = $"INV-{id:D3}", InvoiceDate = date, TotalAmount = amount };

    private static SummaryViewModel MakeVm(
        IReadOnlyList<WorkEntry>? entries = null,
        IReadOnlyList<Invoice>? invoices = null,
        IReadOnlyList<WorkCategory>? categories = null)
    {
        var entryRepo = new Mock<IWorkEntryRepository>();
        entryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync(entries ?? []);
        var invoiceRepo = new Mock<IInvoiceRepository>();
        invoiceRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync(invoices ?? []);
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync(categories ?? []);
        return new SummaryViewModel(invoiceRepo.Object, entryRepo.Object, categoryRepo.Object);
    }

    [Fact]
    public async Task LoadAsync_WithNoData_AllZeroes()
    {
        var vm = MakeVm();

        await vm.LoadAsync(MakeClient());

        vm.TotalHoursThisYear.Should().Be(0);
        vm.TotalInvoicedAmountThisYear.Should().Be(0);
        vm.UninvoicedHours.Should().Be(0);
        vm.UninvoicedAmount.Should().Be(0);
    }

    [Fact]
    public async Task LoadAsync_SumsUninvoicedHoursAndAmount()
    {
        var entries = new List<WorkEntry>
        {
            MakeEntry(1, 3m, invoiced: false),
            MakeEntry(2, 2m, invoiced: true),
        };
        var vm = MakeVm(entries);

        await vm.LoadAsync(MakeClient(rate: 100m));

        vm.UninvoicedHours.Should().Be(3m);
        vm.UninvoicedAmount.Should().Be(300m);
    }

    [Fact]
    public async Task LoadAsync_SumsThisYearHours()
    {
        var thisYear = DateOnly.FromDateTime(DateTime.Today);
        var lastYear = new DateOnly(DateTime.Today.Year - 1, 1, 1);
        var entries = new List<WorkEntry>
        {
            MakeEntry(1, 4m, date: thisYear),
            MakeEntry(2, 6m, date: lastYear),
        };
        var vm = MakeVm(entries);

        await vm.LoadAsync(MakeClient());

        vm.TotalHoursThisYear.Should().Be(4m);
    }

    [Fact]
    public async Task LoadAsync_SumsThisYearInvoicedAmount()
    {
        var thisYear = new DateOnly(DateTime.Today.Year, 1, 15);
        var lastYear = new DateOnly(DateTime.Today.Year - 1, 6, 1);
        var invoices = new List<Invoice>
        {
            MakeInvoice(1, 500m, thisYear),
            MakeInvoice(2, 300m, lastYear),
        };
        var vm = MakeVm(invoices: invoices);

        await vm.LoadAsync(MakeClient());

        vm.TotalInvoicedAmountThisYear.Should().Be(500m);
    }

    [Fact]
    public async Task LoadAsync_WithCapExceeded_IsOverCapTrue()
    {
        var entries = new List<WorkEntry> { MakeEntry(1, 15m) };
        var vm = MakeVm(entries);

        await vm.LoadAsync(MakeClient(rate: 100m, cap: 1000m));

        vm.CapStatus.Should().Be(InvoiceCapStatus.OverCap);
        vm.IsOverCap.Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_WithNoCap_HasCapFalse()
    {
        var vm = MakeVm();

        await vm.LoadAsync(MakeClient(cap: null));

        vm.HasCap.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_PopulatesHoursPerMonth()
    {
        var date = new DateOnly(DateTime.Today.Year, 1, 10);
        var entries = new List<WorkEntry> { MakeEntry(1, 5m, date: date) };
        var vm = MakeVm(entries);

        await vm.LoadAsync(MakeClient());

        vm.HoursPerMonth.Should().NotBeEmpty();
        vm.HoursPerMonth[0].Month.Should().Be("Jan");
        vm.HoursPerMonth[0].Hours.Should().Be(5m);
    }

    [Fact]
    public async Task LoadAsync_PopulatesHoursByCategory()
    {
        var entries = new List<WorkEntry>
        {
            MakeEntry(1, 3m, categoryId: 1),
            MakeEntry(2, 2m, categoryId: 1),
            MakeEntry(3, 1m, categoryId: null),
        };
        var categories = new List<WorkCategory> { new() { Id = 1, Name = "Dev" } };
        var vm = MakeVm(entries, categories: categories);

        await vm.LoadAsync(MakeClient());

        vm.HoursByCategory.Should().HaveCount(2);
        vm.HoursByCategory.First(l => l.Category == "Dev").Hours.Should().Be(5m);
        vm.HoursByCategory.First(l => l.Category == "Uncategorised").Hours.Should().Be(1m);
    }

    [Fact]
    public async Task LoadAsync_SetsIsLoadingFalseWhenComplete()
    {
        var vm = MakeVm();

        await vm.LoadAsync(MakeClient());

        vm.IsLoading.Should().BeFalse();
    }
}
