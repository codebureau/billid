using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories;

namespace WorkTracking.Tests.Data;

public class InvoiceRepositoryTests : IDisposable
{
    private readonly SqliteTestFixture _fixture = new();
    private readonly InvoiceRepository _repository;
    private readonly ClientRepository _clientRepository;
    private int _clientId;

    public InvoiceRepositoryTests()
    {
        _repository = new InvoiceRepository(_fixture.ConnectionFactory, NullLogger<InvoiceRepository>.Instance);
        _clientRepository = new ClientRepository(_fixture.ConnectionFactory, NullLogger<ClientRepository>.Instance);
        _clientId = _clientRepository.AddAsync(new Client { Name = "Test Client", HourlyRate = 100m })
            .GetAwaiter().GetResult().Id;
    }

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task GetByClientAsync_WithNoInvoices_ReturnsEmptyList()
    {
        var result = await _repository.GetByClientAsync(_clientId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddWithLinesAsync_CreatesInvoiceAndLines()
    {
        var invoice = BuildInvoice(_clientId, "INV-001");
        var lines = new List<InvoiceLine>
        {
            new() { WorkCategoryId = null, Hours = 5m, Rate = 100m, Amount = 500m, Description = "Dev work" },
            new() { WorkCategoryId = null, Hours = 2m, Rate = 100m, Amount = 200m, Description = "Support" },
        };

        var added = await _repository.AddWithLinesAsync(invoice, lines);

        added.Id.Should().BeGreaterThan(0);
        var fetchedLines = await _repository.GetLinesAsync(added.Id);
        fetchedLines.Should().HaveCount(2);
        fetchedLines.Sum(l => l.Amount).Should().Be(700m);
    }

    [Fact]
    public async Task AddWithLinesAsync_SetsInvoiceIdOnLines()
    {
        var invoice = BuildInvoice(_clientId, "INV-002");
        var lines = new List<InvoiceLine>
        {
            new() { Hours = 3m, Rate = 100m, Amount = 300m },
        };

        var added = await _repository.AddWithLinesAsync(invoice, lines);

        var fetchedLines = await _repository.GetLinesAsync(added.Id);
        fetchedLines[0].InvoiceId.Should().Be(added.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ChangesPdfPath_PersistsChange()
    {
        var invoice = await _repository.AddWithLinesAsync(BuildInvoice(_clientId, "INV-003"), []);
        invoice.PdfPath = "/path/to/invoice.pdf";

        await _repository.UpdateAsync(invoice);

        var fetched = await _repository.GetByIdAsync(invoice.Id);
        fetched!.PdfPath.Should().Be("/path/to/invoice.pdf");
    }

    [Fact]
    public async Task DeleteAsync_RemovesInvoice()
    {
        var invoice = await _repository.AddWithLinesAsync(BuildInvoice(_clientId, "INV-004"), []);

        await _repository.DeleteAsync(invoice.Id);

        var fetched = await _repository.GetByIdAsync(invoice.Id);
        fetched.Should().BeNull();
    }

    private static Invoice BuildInvoice(int clientId, string number) => new()
    {
        ClientId = clientId,
        InvoiceNumber = number,
        InvoiceDate = new DateOnly(2025, 4, 1),
        TotalAmount = 700m,
    };
}
