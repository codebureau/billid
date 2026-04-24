using FluentAssertions;
using WorkTracking.Core.Enums;
using WorkTracking.Core.Models;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class InvoicePrepViewModelTests
{
    private static WorkEntryRowViewModel MakeRow(int id, decimal hours, int? categoryId = null, string categoryName = "")
    {
        var entry = new WorkEntry
        {
            Id = id, ClientId = 1,
            Hours = hours,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Description = "Work",
            WorkCategoryId = categoryId
        };
        return new WorkEntryRowViewModel(entry, categoryName);
    }

    [Fact]
    public void Constructor_CalculatesTotalHoursAndAmount()
    {
        var rows = new List<WorkEntryRowViewModel>
        {
            MakeRow(1, 3m),
            MakeRow(2, 2m),
        };

        var vm = new InvoicePrepViewModel(rows, 100m, null);

        vm.TotalHours.Should().Be(5m);
        vm.TotalAmount.Should().Be(500m);
    }

    [Fact]
    public void Constructor_WithNoCap_ReturnsNoCap()
    {
        var rows = new List<WorkEntryRowViewModel> { MakeRow(1, 5m) };

        var vm = new InvoicePrepViewModel(rows, 100m, null);

        vm.CapStatus.Should().Be(InvoiceCapStatus.NoCap);
        vm.IsOverCap.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WhenAmountExceedsCap_IsOverCapTrue()
    {
        var rows = new List<WorkEntryRowViewModel> { MakeRow(1, 15m) };

        var vm = new InvoicePrepViewModel(rows, 100m, 1000m);

        vm.CapStatus.Should().Be(InvoiceCapStatus.OverCap);
        vm.IsOverCap.Should().BeTrue();
    }

    [Fact]
    public void Constructor_GroupsLinesByCategory()
    {
        var rows = new List<WorkEntryRowViewModel>
        {
            MakeRow(1, 2m, categoryId: 1, categoryName: "Dev"),
            MakeRow(2, 1m, categoryId: 1, categoryName: "Dev"),
            MakeRow(3, 3m, categoryId: null, categoryName: ""),
        };

        var vm = new InvoicePrepViewModel(rows, 100m, null);

        vm.LinesByCategory.Should().HaveCount(2);
        vm.LinesByCategory.First(l => l.CategoryName == "Dev").Hours.Should().Be(3m);
        vm.LinesByCategory.First(l => l.CategoryName == "Uncategorised").Hours.Should().Be(3m);
    }

    [Fact]
    public void ConfirmCommand_WithEmptyInvoiceNumber_CannotExecute()
    {
        var rows = new List<WorkEntryRowViewModel> { MakeRow(1, 1m) };
        var vm = new InvoicePrepViewModel(rows, 100m, null);
        vm.InvoiceNumber = string.Empty;

        vm.ConfirmCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void ConfirmCommand_WithInvoiceNumber_CanExecute()
    {
        var rows = new List<WorkEntryRowViewModel> { MakeRow(1, 1m) };
        var vm = new InvoicePrepViewModel(rows, 100m, null);
        vm.InvoiceNumber = "INV-001";

        vm.ConfirmCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void ConfirmCommand_Execute_SetsConfirmedAndRaisesCloseRequested()
    {
        var rows = new List<WorkEntryRowViewModel> { MakeRow(1, 1m) };
        var vm = new InvoicePrepViewModel(rows, 100m, null);
        vm.InvoiceNumber = "INV-001";
        bool? closeResult = null;
        vm.CloseRequested += (_, result) => closeResult = result;

        vm.ConfirmCommand.Execute(null);

        vm.Confirmed.Should().BeTrue();
        closeResult.Should().BeTrue();
    }

    [Fact]
    public void CancelCommand_Execute_RaisesCloseRequestedWithFalse()
    {
        var rows = new List<WorkEntryRowViewModel> { MakeRow(1, 1m) };
        var vm = new InvoicePrepViewModel(rows, 100m, null);
        bool? closeResult = null;
        vm.CloseRequested += (_, result) => closeResult = result;

        vm.CancelCommand.Execute(null);

        closeResult.Should().BeFalse();
    }

    [Fact]
    public void InvoiceDate_DefaultsToToday()
    {
        var rows = new List<WorkEntryRowViewModel> { MakeRow(1, 1m) };
        var vm = new InvoicePrepViewModel(rows, 100m, null);

        vm.InvoiceDate.Date.Should().Be(DateTime.Today);
    }
}
