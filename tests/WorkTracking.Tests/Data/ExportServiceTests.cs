using FluentAssertions;
using WorkTracking.Core.Models;
using WorkTracking.Core.Services;
using WorkTracking.Data.Services;
using WorkTracking.Tests.Data;

namespace WorkTracking.Tests.Data;

public class ExportServiceTests : IDisposable
{
    private readonly SqliteTestFixture _fixture = new();
    private readonly IExportService _service;
    private readonly string _tempFile;

    public ExportServiceTests()
    {
        var repo = new WorkTracking.Data.Repositories.SettingRepository(_fixture.ConnectionFactory);
        _service = new ExportService(repo);
        _tempFile = Path.Combine(Path.GetTempPath(), $"export_test_{Guid.NewGuid():N}.csv");
    }

    public void Dispose()
    {
        _fixture.Dispose();
        if (File.Exists(_tempFile)) File.Delete(_tempFile);
    }

    [Fact]
    public async Task LoadDefinitionAsync_NoSetting_ReturnsDefaults()
    {
        var definition = await _service.LoadDefinitionAsync();

        definition.Should().NotBeNull();
        definition.IncludeDate.Should().BeTrue();
        definition.IncludeDescription.Should().BeTrue();
        definition.IncludeHours.Should().BeTrue();
        definition.IncludeClientName.Should().BeTrue();
        definition.IncludeWorkEntryId.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAndLoadDefinitionAsync_PersistsCorrectly()
    {
        var definition = new ExportDefinition
        {
            IncludeDate = false,
            IncludeDescription = true,
            IncludeHours = false,
            IncludeClientName = false,
            IncludeClientAbn = true
        };

        await _service.SaveDefinitionAsync(definition);
        var loaded = await _service.LoadDefinitionAsync();

        loaded.IncludeDate.Should().BeFalse();
        loaded.IncludeHours.Should().BeFalse();
        loaded.IncludeClientName.Should().BeFalse();
        loaded.IncludeClientAbn.Should().BeTrue();
    }

    [Fact]
    public async Task ExportToCsvAsync_WithDefaultDefinition_WritesCorrectHeaders()
    {
        var entries = new List<WorkEntry>
        {
            new() { Id = 1, ClientId = 10, Date = new DateOnly(2025, 1, 15), Description = "Dev work", Hours = 3.5m, InvoicedFlag = false }
        };
        var clients = new Dictionary<int, Client>
        {
            [10] = new() { Id = 10, Name = "Acme", CompanyName = "Acme Corp", HourlyRate = 150m }
        };
        var categories = new Dictionary<int, string>();
        var definition = new ExportDefinition();

        await _service.ExportToCsvAsync(entries, clients, categories, definition, _tempFile);

        var lines = await File.ReadAllLinesAsync(_tempFile);
        lines.Length.Should().BeGreaterThanOrEqualTo(2);
        lines[0].Should().Contain("Date");
        lines[0].Should().Contain("Description");
        lines[0].Should().Contain("Hours");
        lines[0].Should().Contain("Client Name");
        lines[0].Should().Contain("Company Name");
        lines[0].Should().Contain("Hourly Rate");
        lines[0].Should().NotContain("Entry ID");
    }

    [Fact]
    public async Task ExportToCsvAsync_WithSubsetOfFields_OnlyIncludesSelectedColumns()
    {
        var entries = new List<WorkEntry>
        {
            new() { Id = 1, ClientId = 10, Date = new DateOnly(2025, 3, 1), Description = "Meeting", Hours = 1m }
        };
        var clients = new Dictionary<int, Client>
        {
            [10] = new() { Id = 10, Name = "ClientX", HourlyRate = 100m }
        };
        var definition = new ExportDefinition
        {
            IncludeDate = true,
            IncludeDescription = true,
            IncludeHours = false,
            IncludeWorkCategory = false,
            IncludeInvoicedFlag = false,
            IncludeClientName = false,
            IncludeClientCompanyName = false,
            IncludeClientHourlyRate = false
        };

        await _service.ExportToCsvAsync(entries, clients, new Dictionary<int, string>(), definition, _tempFile);

        var lines = await File.ReadAllLinesAsync(_tempFile);
        var header = lines[0];
        header.Should().Contain("Date");
        header.Should().Contain("Description");
        header.Should().NotContain("Hours");
        header.Should().NotContain("Client Name");
        header.Should().NotContain("Hourly Rate");
    }

    [Fact]
    public async Task ExportToCsvAsync_DescriptionWithComma_IsProperlyEscaped()
    {
        var entries = new List<WorkEntry>
        {
            new() { Id = 1, ClientId = 10, Date = new DateOnly(2025, 5, 1), Description = "Design, review", Hours = 2m }
        };
        var definition = new ExportDefinition { IncludeDescription = true, IncludeDate = false, IncludeHours = false,
            IncludeWorkCategory = false, IncludeInvoicedFlag = false, IncludeClientName = false,
            IncludeClientCompanyName = false, IncludeClientHourlyRate = false };

        await _service.ExportToCsvAsync(entries, new Dictionary<int, Client>(), new Dictionary<int, string>(), definition, _tempFile);

        var lines = await File.ReadAllLinesAsync(_tempFile);
        lines[1].Should().Contain("\"Design, review\"");
    }
}
