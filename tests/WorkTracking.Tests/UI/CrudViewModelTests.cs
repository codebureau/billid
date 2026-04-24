using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Services;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class AddClientViewModelTests
{
    [Fact]
    public void ConfirmCommand_WithEmptyName_CannotExecute()
    {
        var vm = new AddClientViewModel { HourlyRate = 100m };
        vm.ConfirmCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void ConfirmCommand_WithZeroRate_CannotExecute()
    {
        var vm = new AddClientViewModel { Name = "Acme" };
        vm.ConfirmCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void ConfirmCommand_WithNameAndRate_CanExecute()
    {
        var vm = new AddClientViewModel { Name = "Acme", HourlyRate = 100m };
        vm.ConfirmCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void ConfirmCommand_Execute_SetsConfirmedAndRaisesCloseRequested()
    {
        var vm = new AddClientViewModel { Name = "Acme", HourlyRate = 100m };
        bool closed = false;
        vm.CloseRequested += (_, _) => closed = true;

        vm.ConfirmCommand.Execute(null);

        vm.Confirmed.Should().BeTrue();
        closed.Should().BeTrue();
    }

    [Fact]
    public void CancelCommand_Execute_RaisesCloseRequestedWithoutConfirm()
    {
        var vm = new AddClientViewModel { Name = "Acme", HourlyRate = 100m };
        bool closed = false;
        vm.CloseRequested += (_, _) => closed = true;

        vm.CancelCommand.Execute(null);

        vm.Confirmed.Should().BeFalse();
        closed.Should().BeTrue();
    }
}

public class ClientListViewModelCrudTests
{
    private static ClientListViewModel MakeVm(
        Mock<IClientRepository> repo,
        Mock<IDialogService> dialog)
    {
        var workEntryMock = new Mock<IWorkEntryRepository>();
        workEntryMock.Setup(r => r.GetUninvoicedHoursByClientAsync()).ReturnsAsync(new Dictionary<int, decimal>());
        return new(repo.Object, workEntryMock.Object, dialog.Object);
    }

    [Fact]
    public async Task AddClientCommand_WhenDialogConfirmed_AddsAndReloads()
    {
        var repo = new Mock<IClientRepository>();
        var dialog = new Mock<IDialogService>();
        var added = new Client { Id = 99, Name = "New Co", HourlyRate = 120m };
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync([added]);
        repo.Setup(r => r.AddAsync(It.IsAny<Client>())).ReturnsAsync(added);
        dialog.Setup(d => d.ShowAddClientDialog(It.IsAny<AddClientViewModel>()))
              .Callback<AddClientViewModel>(vm => { vm.Name = "New Co"; vm.HourlyRate = 120m; })
              .Returns(true);

        var vm = MakeVm(repo, dialog);
        vm.AddClientCommand.Execute(null);
        await Task.Delay(100);

        repo.Verify(r => r.AddAsync(It.Is<Client>(c => c.Name == "New Co")), Times.Once);
    }

    [Fact]
    public async Task AddClientCommand_WhenDialogCancelled_DoesNotAdd()
    {
        var repo = new Mock<IClientRepository>();
        var dialog = new Mock<IDialogService>();
        dialog.Setup(d => d.ShowAddClientDialog(It.IsAny<AddClientViewModel>())).Returns(false);
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var vm = MakeVm(repo, dialog);
        vm.AddClientCommand.Execute(null);
        await Task.Delay(50);

        repo.Verify(r => r.AddAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task DeleteClientCommand_WhenConfirmed_DeletesAndReloads()
    {
        var client = new Client { Id = 1, Name = "Acme" };
        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        var dialog = new Mock<IDialogService>();
        dialog.Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var vm = MakeVm(repo, dialog);
        vm.SelectedClient = client;
        vm.DeleteClientCommand.Execute(null);
        await Task.Delay(50);

        repo.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteClientCommand_WhenDeclined_DoesNotDelete()
    {
        var client = new Client { Id = 1, Name = "Acme" };
        var repo = new Mock<IClientRepository>();
        var dialog = new Mock<IDialogService>();
        dialog.Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var vm = MakeVm(repo, dialog);
        vm.SelectedClient = client;
        vm.DeleteClientCommand.Execute(null);
        await Task.Delay(50);

        repo.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void DeleteClientCommand_WhenNoSelection_CannotExecute()
    {
        var vm = MakeVm(new Mock<IClientRepository>(), new Mock<IDialogService>());
        vm.DeleteClientCommand.CanExecute(null).Should().BeFalse();
    }
}

public class WorkEntryDialogViewModelTests
{
    private static WorkCategory MakeCat(int id, string name) =>
        new() { Id = id, Name = name };

    [Fact]
    public void Constructor_NewEntry_DefaultsToTodayAndIsEditFalse()
    {
        var vm = new WorkEntryDialogViewModel([]);

        vm.IsEdit.Should().BeFalse();
        vm.Date.Date.Should().Be(DateTime.Today);
        vm.Title.Should().Be("Add Work Entry");
    }

    [Fact]
    public void Constructor_ExistingEntry_PopulatesFieldsAndIsEditTrue()
    {
        var cats = new List<WorkCategory> { MakeCat(1, "Dev") };
        var entry = new WorkEntry
        {
            Id = 5, ClientId = 1,
            Date = new DateOnly(2025, 3, 10),
            Description = "Some work",
            Hours = 3.5m,
            WorkCategoryId = 1,
            NotesMarkdown = "note"
        };

        var vm = new WorkEntryDialogViewModel(cats, entry);

        vm.IsEdit.Should().BeTrue();
        vm.ExistingId.Should().Be(5);
        vm.Date.Should().Be(new DateTime(2025, 3, 10));
        vm.Description.Should().Be("Some work");
        vm.Hours.Should().Be(3.5m);
        vm.SelectedCategory!.Id.Should().Be(1);
        vm.NotesMarkdown.Should().Be("note");
        vm.Title.Should().Be("Edit Work Entry");
    }

    [Fact]
    public void ConfirmCommand_WithEmptyDescription_CannotExecute()
    {
        var vm = new WorkEntryDialogViewModel([]) { Hours = 1m };
        vm.ConfirmCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void ConfirmCommand_WithZeroHours_CannotExecute()
    {
        var vm = new WorkEntryDialogViewModel([]) { Description = "Work" };
        vm.ConfirmCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void ConfirmCommand_WithDescriptionAndHours_CanExecute()
    {
        var vm = new WorkEntryDialogViewModel([]) { Description = "Work", Hours = 1m };
        vm.ConfirmCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void ConfirmCommand_Execute_SetsConfirmedAndRaisesClose()
    {
        var vm = new WorkEntryDialogViewModel([]) { Description = "Work", Hours = 2m };
        bool closed = false;
        vm.CloseRequested += (_, _) => closed = true;

        vm.ConfirmCommand.Execute(null);

        vm.Confirmed.Should().BeTrue();
        closed.Should().BeTrue();
    }

    [Fact]
    public void CancelCommand_Execute_ClosesWithoutConfirm()
    {
        var vm = new WorkEntryDialogViewModel([]) { Description = "Work", Hours = 2m };
        bool closed = false;
        vm.CloseRequested += (_, _) => closed = true;

        vm.CancelCommand.Execute(null);

        vm.Confirmed.Should().BeFalse();
        closed.Should().BeTrue();
    }
}
