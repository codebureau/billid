using FluentAssertions;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories;
using WorkTracking.Tests.Data;

namespace WorkTracking.Tests.Data;

public class ClientRepositoryTests : IDisposable
{
    private readonly SqliteTestFixture _fixture = new();
    private readonly ClientRepository _repository;

    public ClientRepositoryTests() => _repository = new ClientRepository(_fixture.ConnectionFactory);

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task GetAllAsync_WithNoClients_ReturnsEmptyList()
    {
        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ValidClient_AssignsIdAndPersists()
    {
        var client = BuildClient("Acme Corp");

        var added = await _repository.AddAsync(client);

        added.Id.Should().BeGreaterThan(0);
        var fetched = await _repository.GetByIdAsync(added.Id);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleClients_ReturnsAll()
    {
        await _repository.AddAsync(BuildClient("Alpha"));
        await _repository.AddAsync(BuildClient("Beta"));

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ChangesName_PersistsChange()
    {
        var client = await _repository.AddAsync(BuildClient("Old Name"));
        client.Name = "New Name";

        await _repository.UpdateAsync(client);

        var fetched = await _repository.GetByIdAsync(client.Id);
        fetched!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task DeleteAsync_ExistingClient_RemovesIt()
    {
        var client = await _repository.AddAsync(BuildClient("To Delete"));

        await _repository.DeleteAsync(client.Id);

        var fetched = await _repository.GetByIdAsync(client.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_WithOptionalFields_RoundTripsCorrectly()
    {
        var client = BuildClient("Full Client");
        client.InvoiceCapAmount = 5000m;
        client.InvoiceCapBehavior = "warn";
        client.InvoiceFrequencyDays = 90;
        client.LastInvoiceDate = new DateOnly(2025, 1, 1);

        var added = await _repository.AddAsync(client);
        var fetched = await _repository.GetByIdAsync(added.Id);

        fetched!.InvoiceCapAmount.Should().Be(5000m);
        fetched.InvoiceCapBehavior.Should().Be("warn");
        fetched.InvoiceFrequencyDays.Should().Be(90);
        fetched.LastInvoiceDate.Should().Be(new DateOnly(2025, 1, 1));
    }

    private static Client BuildClient(string name) => new()
    {
        Name = name,
        HourlyRate = 150m,
    };
}
