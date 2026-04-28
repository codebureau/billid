using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Core.Services;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class ExportViewModelTests
{
    private static (ExportViewModel vm, Mock<IExportService> exportService) MakeVm()
    {
        var exportService = new Mock<IExportService>();
        exportService.Setup(s => s.LoadDefinitionAsync()).ReturnsAsync(new ExportDefinition());
        var vm = new ExportViewModel(exportService.Object);
        return (vm, exportService);
    }

    [Fact]
    public async Task LoadAsync_LoadsDefinitionFromService()
    {
        var (vm, svc) = MakeVm();
        var definition = new ExportDefinition { IncludeDate = false, IncludeClientAbn = true };
        svc.Setup(s => s.LoadDefinitionAsync()).ReturnsAsync(definition);

        await vm.LoadAsync();

        vm.IncludeDate.Should().BeFalse();
        vm.IncludeClientAbn.Should().BeTrue();
    }

    [Fact]
    public async Task ExportCommand_SavesDefinitionAndRaisesCloseWithTrue()
    {
        var (vm, svc) = MakeVm();
        await vm.LoadAsync();

        bool? result = null;
        vm.CloseRequested += (_, confirmed) => result = confirmed;

        vm.ExportCommand.Execute(null);

        // Allow async to complete
        await Task.Delay(100);

        svc.Verify(s => s.SaveDefinitionAsync(It.IsAny<ExportDefinition>()), Times.Once);
        result.Should().BeTrue();
    }

    [Fact]
    public void CancelCommand_RaisesCloseWithFalse()
    {
        var (vm, _) = MakeVm();

        bool? result = null;
        vm.CloseRequested += (_, confirmed) => result = confirmed;

        vm.CancelCommand.Execute(null);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResetToDefaultsCommand_RestoresDefaultFieldSelections()
    {
        var (vm, svc) = MakeVm();
        var customDefinition = new ExportDefinition
        {
            IncludeDate = false,
            IncludeHours = false,
            IncludeClientName = false
        };
        svc.Setup(s => s.LoadDefinitionAsync()).ReturnsAsync(customDefinition);
        await vm.LoadAsync();

        vm.IncludeDate.Should().BeFalse();

        vm.ResetToDefaultsCommand.Execute(null);

        vm.IncludeDate.Should().BeTrue();
        vm.IncludeHours.Should().BeTrue();
        vm.IncludeClientName.Should().BeTrue();
    }

    [Fact]
    public async Task GetDefinition_ReturnsCurrentState()
    {
        var (vm, _) = MakeVm();
        await vm.LoadAsync();

        vm.IncludeClientAbn = true;
        vm.IncludeWorkEntryId = true;

        var def = vm.GetDefinition();
        def.IncludeClientAbn.Should().BeTrue();
        def.IncludeWorkEntryId.Should().BeTrue();
    }
}
