using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Services;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class TimesheetViewModelTests
{
    private static (TimesheetViewModel vm, Mock<IWorkEntryRepository> entryRepo, Mock<IWorkCategoryRepository> categoryRepo) MakeVm()
    {
        var entryRepo = new Mock<IWorkEntryRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        var invoiceRepo = new Mock<IInvoiceRepository>();
        var dialogService = new Mock<IDialogService>();
        var vm = new TimesheetViewModel(entryRepo.Object, categoryRepo.Object, invoiceRepo.Object, dialogService.Object);
        return (vm, entryRepo, categoryRepo);
    }

    private static WorkEntry MakeEntry(int id, bool invoiced = false, decimal hours = 1m, int? categoryId = null) =>
        new()
        {
            Id = id, ClientId = 1,
            Date = DateOnly.FromDateTime(DateTime.Today),
            Description = "Test",
            Hours = hours,
            InvoicedFlag = invoiced,
            WorkCategoryId = categoryId
        };

    [Fact]
    public async Task LoadAsync_WithEntries_PopulatesEntries()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null))
                 .ReturnsAsync([MakeEntry(1), MakeEntry(2)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1, 100m, null);

        vm.Entries.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadAsync_WithNoEntries_ReturnsEmptyEntries()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null)).ReturnsAsync([]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1, 100m, null);

        vm.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task TotalUninvoicedHours_SumsUninvoicedEntries()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null))
                 .ReturnsAsync([MakeEntry(1, hours: 2m), MakeEntry(2, hours: 3m)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1, 100m, null);

        vm.TotalUninvoicedHours.Should().Be(5m);
    }

    [Fact]
    public async Task TotalUninvoicedAmount_ReturnsHoursTimesRate()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null))
                 .ReturnsAsync([MakeEntry(1, hours: 4m)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1, 150m, null);

        vm.TotalUninvoicedAmount.Should().Be(600m);
    }

    [Fact]
    public async Task IsOverCap_WhenAmountExceedsCap_ReturnsTrue()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null))
                 .ReturnsAsync([MakeEntry(1, hours: 10m)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1, 100m, 500m); // 10 × 100 = 1000 > 500

        vm.IsOverCap.Should().BeTrue();
    }

    [Fact]
    public async Task IsOverCap_WhenNoCap_ReturnsFalse()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null))
                 .ReturnsAsync([MakeEntry(1, hours: 100m)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1, 100m, null);

        vm.IsOverCap.Should().BeFalse();
    }

    [Fact]
    public async Task IsOverCap_WhenAmountUnderCap_ReturnsFalse()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null))
                 .ReturnsAsync([MakeEntry(1, hours: 2m)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1, 100m, 500m); // 2 × 100 = 200 < 500

        vm.IsOverCap.Should().BeFalse();
    }

    [Fact]
    public async Task CategorySummaryLines_GroupsUninvoicedByCategory()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        var cat = new WorkCategory { Id = 1, Name = "Development" };
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null))
                 .ReturnsAsync([MakeEntry(1, hours: 3m, categoryId: 1), MakeEntry(2, hours: 2m, categoryId: 1)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([cat]);

        await vm.LoadAsync(1, 100m, null);

        vm.CategorySummaryLines.Should().ContainSingle(l => l.CategoryName == "Development" && l.Hours == 5m);
    }

    [Fact]
    public async Task HasAnySelectedUninvoiced_WhenUninvoicedEntrySelected_ReturnsTrue()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null))
                 .ReturnsAsync([MakeEntry(1, invoiced: false)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1, 100m, null);
        vm.Entries[0].IsSelected = true;

        vm.HasAnySelectedUninvoiced.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnySelectedUninvoiced_WhenNothingSelected_ReturnsFalse()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null))
                 .ReturnsAsync([MakeEntry(1), MakeEntry(2)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        await vm.LoadAsync(1, 100m, null);

        vm.HasAnySelectedUninvoiced.Should().BeFalse();
    }

    [Fact]
    public async Task Entries_CategoryName_ResolvesFromLoadedCategories()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        var cat = new WorkCategory { Id = 5, Name = "Support" };
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null))
                 .ReturnsAsync([MakeEntry(1, categoryId: 5)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([cat]);

        await vm.LoadAsync(1, 100m, null);

        vm.Entries.Single().CategoryName.Should().Be("Support");
    }

    [Fact]
    public async Task InvoicedFilterText_Invoiced_PassesCorrectFilterToRepository()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, true, null))
                 .ReturnsAsync([MakeEntry(1, invoiced: true)]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        vm.InvoicedFilterText = "Invoiced";
        await vm.LoadAsync(1, 100m, null);

        entryRepo.Verify(r => r.GetFilteredAsync(1, null, null, true, null), Times.AtLeastOnce);
        vm.Entries.Should().ContainSingle(e => e.IsInvoiced);
    }

    [Fact]
    public async Task InvoicedFilterText_All_PassesNullToRepository()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, null, null)).ReturnsAsync([]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);

        vm.InvoicedFilterText = "All";
        await vm.LoadAsync(1, 100m, null);

        entryRepo.Verify(r => r.GetFilteredAsync(1, null, null, null, null), Times.AtLeastOnce);
    }
}

// -- DeleteEntryCommand confirm gate (issue #7) --------------------------------

public class TimesheetViewModelDeleteConfirmTests
{
    private static (TimesheetViewModel vm, Mock<IWorkEntryRepository> entryRepo, Mock<IDialogService> dialogService) MakeVm()
    {
        var entryRepo = new Mock<IWorkEntryRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        var invoiceRepo = new Mock<IInvoiceRepository>();
        var dialogService = new Mock<IDialogService>();
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        entryRepo.Setup(r => r.GetFilteredAsync(It.IsAny<int>(), null, null, false, null))
                 .ReturnsAsync([]);
        var vm = new TimesheetViewModel(entryRepo.Object, categoryRepo.Object, invoiceRepo.Object, dialogService.Object);
        return (vm, entryRepo, dialogService);
    }

    private static WorkEntry MakeEntry(int id) => new()
    {
        Id = id, ClientId = 1,
        Date = DateOnly.FromDateTime(DateTime.Today),
        Description = "Test entry", Hours = 1m
    };

    [Fact]
    public async Task DeleteEntryCommand_WhenConfirmed_DeletesEntry()
    {
        var (vm, entryRepo, dialogService) = MakeVm();
        dialogService.Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        entryRepo.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        await vm.LoadAsync(1, 100m, null);
        vm.SelectedEntry = new WorkEntryRowViewModel(MakeEntry(42), null);
        vm.DeleteEntryCommand.Execute(null);

        entryRepo.Verify(r => r.DeleteAsync(42), Times.Once);
    }

    [Fact]
    public async Task DeleteEntryCommand_WhenDeclined_DoesNotDelete()
    {
        var (vm, entryRepo, dialogService) = MakeVm();
        dialogService.Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        await vm.LoadAsync(1, 100m, null);
        vm.SelectedEntry = new WorkEntryRowViewModel(MakeEntry(42), null);
        vm.DeleteEntryCommand.Execute(null);

        entryRepo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteEntryCommand_AlwaysShowsConfirmPrompt()
    {
        var (vm, entryRepo, dialogService) = MakeVm();
        dialogService.Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        entryRepo.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

        await vm.LoadAsync(1, 100m, null);
        vm.SelectedEntry = new WorkEntryRowViewModel(MakeEntry(1), null);
        vm.DeleteEntryCommand.Execute(null);

        dialogService.Verify(d => d.Confirm(It.Is<string>(m => m.Contains("Test entry")), It.IsAny<string>()), Times.Once);
    }
}

// -- Issue #4: Lock invoiced entries from edit/delete -------------------------

public class TimesheetViewModelInvoicedLockTests
{
    private static (TimesheetViewModel vm, Mock<IWorkEntryRepository> entryRepo, Mock<IDialogService> dialogService) MakeVm()
    {
        var entryRepo = new Mock<IWorkEntryRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        var invoiceRepo = new Mock<IInvoiceRepository>();
        var dialogService = new Mock<IDialogService>();
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        entryRepo.Setup(r => r.GetFilteredAsync(It.IsAny<int>(), null, null, false, null))
                 .ReturnsAsync([]);
        var vm = new TimesheetViewModel(entryRepo.Object, categoryRepo.Object, invoiceRepo.Object, dialogService.Object);
        return (vm, entryRepo, dialogService);
    }

    private static WorkEntry MakeEntry(int id, bool invoiced = false) => new()
    {
        Id = id, ClientId = 1,
        Date = DateOnly.FromDateTime(DateTime.Today),
        Description = "Test entry", Hours = 1m,
        InvoicedFlag = invoiced
    };

    [Fact]
    public async Task EditOrViewEntryCommand_WhenEntryIsInvoiced_CanExecuteReturnsTrue()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(1, 100m, null);
        vm.SelectedEntry = new WorkEntryRowViewModel(MakeEntry(1, invoiced: true), null);

        vm.EditOrViewEntryCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task EditOrViewEntryCommand_WhenEntryIsUninvoiced_CanExecuteReturnsTrue()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(1, 100m, null);
        vm.SelectedEntry = new WorkEntryRowViewModel(MakeEntry(1, invoiced: false), null);

        vm.EditOrViewEntryCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteEntryCommand_WhenEntryIsInvoiced_CanExecuteReturnsFalse()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(1, 100m, null);
        vm.SelectedEntry = new WorkEntryRowViewModel(MakeEntry(1, invoiced: true), null);

        vm.DeleteEntryCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteEntryCommand_WhenEntryIsUninvoiced_CanExecuteReturnsTrue()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(1, 100m, null);
        vm.SelectedEntry = new WorkEntryRowViewModel(MakeEntry(1, invoiced: false), null);

        vm.DeleteEntryCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task ViewEntryCommand_WhenNoEntrySelected_CanExecuteReturnsFalse()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(1, 100m, null);

        vm.ViewEntryCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task ViewEntryCommand_WhenUninvoicedEntrySelected_CanExecuteReturnsTrue()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(1, 100m, null);
        vm.SelectedEntry = new WorkEntryRowViewModel(MakeEntry(1, invoiced: false), null);

        vm.ViewEntryCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task ViewEntryCommand_WhenInvoicedEntrySelected_CanExecuteReturnsTrue()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(1, 100m, null);
        vm.SelectedEntry = new WorkEntryRowViewModel(MakeEntry(1, invoiced: true), null);

        vm.ViewEntryCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task ViewEntryCommand_WhenExecuted_OpensDialogInReadOnlyMode()
    {
        var (vm, _, dialogService) = MakeVm();
        WorkEntryDialogViewModel? capturedVm = null;
        dialogService.Setup(d => d.ShowWorkEntryDialog(It.IsAny<WorkEntryDialogViewModel>()))
                     .Callback<WorkEntryDialogViewModel>(v => capturedVm = v)
                     .Returns(false);

        await vm.LoadAsync(1, 100m, null);
        vm.SelectedEntry = new WorkEntryRowViewModel(MakeEntry(1, invoiced: true), null);
        vm.ViewEntryCommand.Execute(null);

        capturedVm.Should().NotBeNull();
        capturedVm!.IsReadOnly.Should().BeTrue();
    }
}

// -- Issue #5: Category filter on timesheet ----------------------------------

public class TimesheetViewModelCategoryFilterTests
{
    private static (TimesheetViewModel vm, Mock<IWorkEntryRepository> entryRepo, Mock<IWorkCategoryRepository> categoryRepo) MakeVm()
    {
        var entryRepo = new Mock<IWorkEntryRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        var invoiceRepo = new Mock<IInvoiceRepository>();
        var dialogService = new Mock<IDialogService>();
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        entryRepo.Setup(r => r.GetFilteredAsync(It.IsAny<int>(), null, null, false, null))
                 .ReturnsAsync([]);
        var vm = new TimesheetViewModel(entryRepo.Object, categoryRepo.Object, invoiceRepo.Object, dialogService.Object);
        return (vm, entryRepo, categoryRepo);
    }

    [Fact]
    public async Task SelectedFilterCategory_WhenSetToSpecificCategory_PassesCategoryIdToRepository()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        var cat = new WorkCategory { Id = 7, Name = "Development" };
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([cat]);
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, 7)).ReturnsAsync([]);

        await vm.LoadAsync(1, 100m, null);
        vm.SelectedFilterCategory = cat;

        entryRepo.Verify(r => r.GetFilteredAsync(1, null, null, false, 7), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SelectedFilterCategory_WhenResetToAll_PassesNullCategoryToRepository()
    {
        var (vm, entryRepo, categoryRepo) = MakeVm();
        var cat = new WorkCategory { Id = 7, Name = "Development" };
        var all = new WorkCategory { Id = 0, Name = "All categories" };
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([cat]);
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, null)).ReturnsAsync([]);
        entryRepo.Setup(r => r.GetFilteredAsync(1, null, null, false, 7)).ReturnsAsync([]);

        await vm.LoadAsync(1, 100m, null);
        vm.SelectedFilterCategory = cat;
        vm.SelectedFilterCategory = all;

        entryRepo.Verify(r => r.GetFilteredAsync(1, null, null, false, null), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CategoriesWithAll_AlwaysHasAllCategoriesSentinelFirst()
    {
        var (vm, _, categoryRepo) = MakeVm();
        var cat = new WorkCategory { Id = 3, Name = "Support" };
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([cat]);

        await vm.LoadAsync(1, 100m, null);

        vm.CategoriesWithAll.Should().HaveCount(2);
        vm.CategoriesWithAll[0].Id.Should().Be(0);
        vm.CategoriesWithAll[0].Name.Should().Be("All categories");
        vm.CategoriesWithAll[1].Name.Should().Be("Support");
    }

    [Fact]
    public async Task SelectedFilterCategory_DefaultsToAllCategories()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(1, 100m, null);

        vm.SelectedFilterCategory.Id.Should().Be(0);
        vm.SelectedFilterCategory.Name.Should().Be("All categories");
    }
}
