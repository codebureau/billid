using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.Services;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class ClientSettingsViewModelTests
{
    private static Client MakeClient() => new()
    {
        Id = 1,
        Name = "Acme",
        ContactName = "Jane",
        CompanyName = "Acme Corp",
        Address = "1 Main St",
        Abn = "12345678901",
        Email = "jane@acme.com",
        Phone = "0400000000",
        HourlyRate = 150m,
        InvoiceCapAmount = 5000m,
        InvoiceCapBehavior = "warn",
        InvoiceFrequencyDays = 30
    };

    private static WorkCategory MakeCat(int id, string name) =>
        new() { Id = id, Name = name };

    private static (ClientSettingsViewModel vm, Mock<IClientRepository> clientRepo, Mock<IWorkCategoryRepository> categoryRepo) MakeVm()
    {
        var clientRepo = new Mock<IClientRepository>();
        var categoryRepo = new Mock<IWorkCategoryRepository>();
        var dialogService = new Mock<IDialogService>();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);
        return (new ClientSettingsViewModel(clientRepo.Object, categoryRepo.Object, dialogService.Object), clientRepo, categoryRepo);
    }

    [Fact]
    public async Task LoadAsync_PopulatesAllClientFields()
    {
        var (vm, _, _) = MakeVm();
        var client = MakeClient();

        await vm.LoadAsync(client);

        vm.Name.Should().Be("Acme");
        vm.ContactName.Should().Be("Jane");
        vm.CompanyName.Should().Be("Acme Corp");
        vm.Address.Should().Be("1 Main St");
        vm.Abn.Should().Be("12345678901");
        vm.Email.Should().Be("jane@acme.com");
        vm.Phone.Should().Be("0400000000");
        vm.HourlyRate.Should().Be(150m);
        vm.InvoiceCapAmount.Should().Be(5000m);
        vm.InvoiceCapBehavior.Should().Be("warn");
        vm.InvoiceFrequencyDays.Should().Be(30);
    }

    [Fact]
    public async Task LoadAsync_ClearsIsDirty()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(MakeClient());
        vm.Name = "Changed";

        await vm.LoadAsync(MakeClient());

        vm.IsDirty.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_PopulatesCategories_WithEnabledFlag()
    {
        var (vm, _, categoryRepo) = MakeVm();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([MakeCat(1, "Dev"), MakeCat(2, "Support")]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([MakeCat(1, "Dev")]);

        await vm.LoadAsync(MakeClient());

        vm.Categories.Should().HaveCount(2);
        vm.Categories.Single(c => c.Id == 1).IsEnabled.Should().BeTrue();
        vm.Categories.Single(c => c.Id == 2).IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_OrdersCategoriesAlphabetically()
    {
        var (vm, _, categoryRepo) = MakeVm();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([MakeCat(2, "Support"), MakeCat(1, "Dev")]);
        categoryRepo.Setup(r => r.GetByClientAsync(It.IsAny<int>())).ReturnsAsync([]);

        await vm.LoadAsync(MakeClient());

        vm.Categories[0].Name.Should().Be("Dev");
        vm.Categories[1].Name.Should().Be("Support");
    }

    [Fact]
    public async Task ChangingName_SetsIsDirtyTrue()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(MakeClient());

        vm.Name = "New Name";

        vm.IsDirty.Should().BeTrue();
    }

    [Fact]
    public async Task SaveCommand_WithEmptyName_CannotExecute()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(MakeClient());

        vm.Name = string.Empty;

        vm.SaveCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task SaveCommand_WhenDirtyAndNameSet_CanExecute()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(MakeClient());

        vm.Name = "Updated";

        vm.SaveCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_CallsClientRepositoryUpdateAsync()
    {
        var (vm, clientRepo, _) = MakeVm();
        await vm.LoadAsync(MakeClient());
        vm.Name = "Updated";

        await vm.SaveAsync();

        clientRepo.Verify(r => r.UpdateAsync(It.Is<Client>(c => c.Name == "Updated")), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_ClearsIsDirty()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(MakeClient());
        vm.Name = "Updated";

        await vm.SaveAsync();

        vm.IsDirty.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_RaisesClientUpdatedEvent()
    {
        var (vm, _, _) = MakeVm();
        await vm.LoadAsync(MakeClient());
        vm.Name = "Updated";
        Client? raised = null;
        vm.ClientUpdated += (_, c) => raised = c;

        await vm.SaveAsync();

        raised.Should().NotBeNull();
        raised!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task SaveAsync_EnablesNewlyCheckedCategory()
    {
        var (vm, _, categoryRepo) = MakeVm();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([MakeCat(1, "Dev")]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([]);
        await vm.LoadAsync(MakeClient());

        vm.Categories[0].IsEnabled = true;
        await vm.SaveAsync();

        categoryRepo.Verify(r => r.EnableForClientAsync(1, 1), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_DisablesUncheckedCategory()
    {
        var (vm, _, categoryRepo) = MakeVm();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([MakeCat(1, "Dev")]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([MakeCat(1, "Dev")]);
        await vm.LoadAsync(MakeClient());

        vm.Categories[0].IsEnabled = false;
        await vm.SaveAsync();

        categoryRepo.Verify(r => r.DisableForClientAsync(1, 1), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_DoesNotToggleUnchangedCategory()
    {
        var (vm, _, categoryRepo) = MakeVm();
        categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([MakeCat(1, "Dev")]);
        categoryRepo.Setup(r => r.GetByClientAsync(1)).ReturnsAsync([MakeCat(1, "Dev")]);
        await vm.LoadAsync(MakeClient());
        // Dev remains enabled — no change

        await vm.SaveAsync();

        categoryRepo.Verify(r => r.EnableForClientAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        categoryRepo.Verify(r => r.DisableForClientAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void CapBehaviorOptions_ContainsExpectedValues()
    {
        ClientSettingsViewModel.CapBehaviorOptions.Should().BeEquivalentTo(["warn", "block", "allow"]);
    }
}
