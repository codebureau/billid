using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories;

namespace WorkTracking.Tests.Data;

public class WorkCategoryRepositoryTests : IDisposable
{
    private readonly SqliteTestFixture _fixture = new();
    private readonly WorkCategoryRepository _repository;
    private readonly ClientRepository _clientRepository;

    public WorkCategoryRepositoryTests()
    {
        _repository = new WorkCategoryRepository(_fixture.ConnectionFactory, NullLogger<WorkCategoryRepository>.Instance);
        _clientRepository = new ClientRepository(_fixture.ConnectionFactory, NullLogger<ClientRepository>.Instance);
    }

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task GetAllAsync_WithNoCategories_ReturnsEmptyList()
    {
        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ValidCategory_AssignsIdAndPersists()
    {
        var category = await _repository.AddAsync(new WorkCategory { Name = "Development" });

        category.Id.Should().BeGreaterThan(0);
        var fetched = await _repository.GetByIdAsync(category.Id);
        fetched!.Name.Should().Be("Development");
    }

    [Fact]
    public async Task EnableForClientAsync_ThenGetByClient_ReturnsEnabledCategories()
    {
        var client = await _clientRepository.AddAsync(new Client { Name = "Client", HourlyRate = 100m });
        var cat1 = await _repository.AddAsync(new WorkCategory { Name = "Dev" });
        var cat2 = await _repository.AddAsync(new WorkCategory { Name = "Support" });

        await _repository.EnableForClientAsync(client.Id, cat1.Id);

        var result = await _repository.GetByClientAsync(client.Id);
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Dev");
    }

    [Fact]
    public async Task DisableForClientAsync_RemovesAssociation()
    {
        var client = await _clientRepository.AddAsync(new Client { Name = "Client", HourlyRate = 100m });
        var cat = await _repository.AddAsync(new WorkCategory { Name = "Dev" });
        await _repository.EnableForClientAsync(client.Id, cat.Id);

        await _repository.DisableForClientAsync(client.Id, cat.Id);

        var result = await _repository.GetByClientAsync(client.Id);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task EnableForClientAsync_CalledTwice_DoesNotDuplicate()
    {
        var client = await _clientRepository.AddAsync(new Client { Name = "Client", HourlyRate = 100m });
        var cat = await _repository.AddAsync(new WorkCategory { Name = "Dev" });

        await _repository.EnableForClientAsync(client.Id, cat.Id);
        await _repository.EnableForClientAsync(client.Id, cat.Id);

        var result = await _repository.GetByClientAsync(client.Id);
        result.Should().HaveCount(1);
    }
}
