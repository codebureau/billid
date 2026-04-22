using FluentAssertions;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories;

namespace WorkTracking.Tests.Data;

public class WorkEntryRepositoryTests : IDisposable
{
    private readonly SqliteTestFixture _fixture = new();
    private readonly WorkEntryRepository _repository;
    private readonly ClientRepository _clientRepository;
    private readonly InvoiceRepository _invoiceRepository;
    private int _clientId;

    public WorkEntryRepositoryTests()
    {
        _repository = new WorkEntryRepository(_fixture.ConnectionFactory);
        _clientRepository = new ClientRepository(_fixture.ConnectionFactory);
        _invoiceRepository = new InvoiceRepository(_fixture.ConnectionFactory);
        _clientId = _clientRepository.AddAsync(new Client { Name = "Test Client", HourlyRate = 100m })
            .GetAwaiter().GetResult().Id;
    }

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task GetByClientAsync_WithNoEntries_ReturnsEmptyList()
    {
        var result = await _repository.GetByClientAsync(_clientId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ValidEntry_AssignsIdAndPersists()
    {
        var entry = BuildEntry(_clientId, new DateOnly(2025, 3, 1), 3m);

        var added = await _repository.AddAsync(entry);

        added.Id.Should().BeGreaterThan(0);
        var fetched = await _repository.GetByIdAsync(added.Id);
        fetched!.Description.Should().Be("Test work");
        fetched.Hours.Should().Be(3m);
    }

    [Fact]
    public async Task GetFilteredAsync_ByDateRange_ReturnsMatchingEntries()
    {
        await _repository.AddAsync(BuildEntry(_clientId, new DateOnly(2025, 1, 10), 1m));
        await _repository.AddAsync(BuildEntry(_clientId, new DateOnly(2025, 2, 15), 2m));
        await _repository.AddAsync(BuildEntry(_clientId, new DateOnly(2025, 3, 20), 3m));

        var result = await _repository.GetFilteredAsync(_clientId,
            from: new DateOnly(2025, 2, 1),
            to: new DateOnly(2025, 2, 28));

        result.Should().HaveCount(1);
        result[0].Hours.Should().Be(2m);
    }

    [Fact]
    public async Task GetFilteredAsync_ByInvoicedFlag_ReturnsMatchingEntries()
    {
        var e1 = await _repository.AddAsync(BuildEntry(_clientId, new DateOnly(2025, 1, 1), 1m));
        var e2 = await _repository.AddAsync(BuildEntry(_clientId, new DateOnly(2025, 1, 2), 2m));
        e1.InvoicedFlag = true;
        await _repository.UpdateAsync(e1);

        var uninvoiced = await _repository.GetFilteredAsync(_clientId, invoiced: false);
        var invoiced = await _repository.GetFilteredAsync(_clientId, invoiced: true);

        uninvoiced.Should().HaveCount(1).And.Contain(e => e.Id == e2.Id);
        invoiced.Should().HaveCount(1).And.Contain(e => e.Id == e1.Id);
    }

    [Fact]
    public async Task MarkInvoicedAsync_SetsInvoicedFlagAndInvoiceId()
    {
        var invoice = await _invoiceRepository.AddWithLinesAsync(
            new Invoice { ClientId = _clientId, InvoiceNumber = "INV-001", InvoiceDate = new DateOnly(2025, 1, 1), TotalAmount = 500m },
            []);
        var e1 = await _repository.AddAsync(BuildEntry(_clientId, new DateOnly(2025, 1, 1), 2m));
        var e2 = await _repository.AddAsync(BuildEntry(_clientId, new DateOnly(2025, 1, 2), 3m));

        await _repository.MarkInvoicedAsync([e1.Id, e2.Id], invoice.Id);

        var fetched1 = await _repository.GetByIdAsync(e1.Id);
        var fetched2 = await _repository.GetByIdAsync(e2.Id);
        fetched1!.InvoicedFlag.Should().BeTrue();
        fetched1.InvoiceId.Should().Be(invoice.Id);
        fetched2!.InvoicedFlag.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ExistingEntry_RemovesIt()
    {
        var entry = await _repository.AddAsync(BuildEntry(_clientId, new DateOnly(2025, 1, 1), 1m));

        await _repository.DeleteAsync(entry.Id);

        var fetched = await _repository.GetByIdAsync(entry.Id);
        fetched.Should().BeNull();
    }

    private static WorkEntry BuildEntry(int clientId, DateOnly date, decimal hours) => new()
    {
        ClientId = clientId,
        Date = date,
        Description = "Test work",
        Hours = hours,
    };
}
