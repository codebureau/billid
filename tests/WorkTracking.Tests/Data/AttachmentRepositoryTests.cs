using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories;

namespace WorkTracking.Tests.Data;

public class AttachmentRepositoryTests : IDisposable
{
    private readonly SqliteTestFixture _fixture = new();
    private readonly AttachmentRepository _repository;
    private readonly WorkEntryRepository _workEntryRepository;
    private readonly ClientRepository _clientRepository;
    private int _workEntryId;

    public AttachmentRepositoryTests()
    {
        _repository = new AttachmentRepository(_fixture.ConnectionFactory);
        _workEntryRepository = new WorkEntryRepository(_fixture.ConnectionFactory, NullLogger<WorkEntryRepository>.Instance);
        _clientRepository = new ClientRepository(_fixture.ConnectionFactory, NullLogger<ClientRepository>.Instance);

        var client = _clientRepository.AddAsync(new Client { Name = "Test Client", HourlyRate = 100m })
            .GetAwaiter().GetResult();
        var entry = _workEntryRepository.AddAsync(new WorkEntry
        {
            ClientId = client.Id,
            Date = new DateOnly(2025, 1, 1),
            Description = "Test entry",
            Hours = 1m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).GetAwaiter().GetResult();
        _workEntryId = entry.Id;
    }

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task GetByWorkEntryAsync_WithNoAttachments_ReturnsEmptyList()
    {
        var result = await _repository.GetByWorkEntryAsync(_workEntryId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ValidAttachment_AssignsIdAndPersists()
    {
        var attachment = BuildAttachment(_workEntryId, "report.pdf");

        var added = await _repository.AddAsync(attachment);

        added.Id.Should().BeGreaterThan(0);
        var fetched = await _repository.GetByIdAsync(added.Id);
        fetched.Should().NotBeNull();
        fetched!.Filename.Should().Be("report.pdf");
        fetched.WorkEntryId.Should().Be(_workEntryId);
    }

    [Fact]
    public async Task GetByWorkEntryAsync_AfterAdd_ReturnsAttachment()
    {
        await _repository.AddAsync(BuildAttachment(_workEntryId, "invoice.pdf"));

        var result = await _repository.GetByWorkEntryAsync(_workEntryId);

        result.Should().HaveCount(1);
        result[0].Filename.Should().Be("invoice.pdf");
    }

    [Fact]
    public async Task GetByWorkEntryAsync_MultipleAttachments_ReturnsAll()
    {
        await _repository.AddAsync(BuildAttachment(_workEntryId, "a.pdf"));
        await _repository.AddAsync(BuildAttachment(_workEntryId, "b.png"));

        var result = await _repository.GetByWorkEntryAsync(_workEntryId);

        result.Should().HaveCount(2);
        result.Select(a => a.Filename).Should().BeEquivalentTo(["a.pdf", "b.png"]);
    }

    [Fact]
    public async Task DeleteAsync_RemovesAttachment()
    {
        var added = await _repository.AddAsync(BuildAttachment(_workEntryId, "delete-me.pdf"));

        await _repository.DeleteAsync(added.Id);

        var fetched = await _repository.GetByIdAsync(added.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);

        result.Should().BeNull();
    }

    private static Attachment BuildAttachment(int workEntryId, string filename) => new()
    {
        WorkEntryId = workEntryId,
        Filename = filename,
        MimeType = "application/pdf",
        FilePath = $@"C:\fake\{filename}",
    };
}
