using FluentAssertions;
using Moq;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories.Interfaces;
using WorkTracking.UI.ViewModels;

namespace WorkTracking.Tests.UI;

public class ClientListViewModelTests
{
    private static List<Client> SampleClients() =>
    [
        new() { Id = 1, Name = "Alpha Ltd",   CompanyName = "Alpha" },
        new() { Id = 2, Name = "Beta Corp",   CompanyName = "Beta" },
        new() { Id = 3, Name = "Gamma Inc",   CompanyName = "Gamma" },
    ];

    private static Mock<IClientRepository> RepoWith(List<Client> clients)
    {
        var mock = new Mock<IClientRepository>();
        mock.Setup(r => r.GetAllAsync()).ReturnsAsync(clients);
        return mock;
    }

    [Fact]
    public async Task LoadAsync_WithClients_PopulatesClients()
    {
        var vm = new ClientListViewModel(RepoWith(SampleClients()).Object);

        await vm.LoadAsync();

        vm.Clients.Should().HaveCount(3);
    }

    [Fact]
    public async Task LoadAsync_WithNoClients_ReturnsEmptyList()
    {
        var vm = new ClientListViewModel(RepoWith([]).Object);

        await vm.LoadAsync();

        vm.Clients.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchText_FiltersByName_CaseInsensitive()
    {
        var vm = new ClientListViewModel(RepoWith(SampleClients()).Object);
        await vm.LoadAsync();

        vm.SearchText = "alpha";

        vm.Clients.Should().ContainSingle(c => c.Name == "Alpha Ltd");
    }

    [Fact]
    public async Task SearchText_WhenCleared_ShowsAllClients()
    {
        var vm = new ClientListViewModel(RepoWith(SampleClients()).Object);
        await vm.LoadAsync();
        vm.SearchText = "alpha";

        vm.SearchText = string.Empty;

        vm.Clients.Should().HaveCount(3);
    }

    [Fact]
    public async Task SearchText_NoMatch_ReturnsEmptyList()
    {
        var vm = new ClientListViewModel(RepoWith(SampleClients()).Object);
        await vm.LoadAsync();

        vm.SearchText = "zzz";

        vm.Clients.Should().BeEmpty();
    }

    [Fact]
    public async Task SelectedClient_SetAndGet_RaisesPropertyChanged()
    {
        var vm = new ClientListViewModel(RepoWith(SampleClients()).Object);
        await vm.LoadAsync();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        var client = vm.Clients[0];

        vm.SelectedClient = client;

        vm.SelectedClient.Should().Be(client);
        raised.Should().Contain(nameof(ClientListViewModel.SelectedClient));
    }
}
