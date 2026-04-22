using FluentAssertions;
using WorkTracking.Core.Models;
using WorkTracking.Data.Repositories;

namespace WorkTracking.Tests.Data;

public class SettingRepositoryTests : IDisposable
{
    private readonly SqliteTestFixture _fixture = new();
    private readonly SettingRepository _repository;

    public SettingRepositoryTests() => _repository = new SettingRepository(_fixture.ConnectionFactory);

    public void Dispose() => _fixture.Dispose();

    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        var result = await _repository.GetAsync("missing_key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ThenGet_ReturnsValue()
    {
        await _repository.SetAsync("theme", "dark");

        var result = await _repository.GetAsync("theme");

        result.Should().Be("dark");
    }

    [Fact]
    public async Task SetAsync_CalledTwice_UpdatesValue()
    {
        await _repository.SetAsync("theme", "dark");
        await _repository.SetAsync("theme", "light");

        var result = await _repository.GetAsync("theme");

        result.Should().Be("light");
    }

    [Fact]
    public async Task SetAsync_WithNullValue_StoresAndReturnsNull()
    {
        await _repository.SetAsync("key", "value");
        await _repository.SetAsync("key", null);

        var result = await _repository.GetAsync("key");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnAllSettings()
    {
        await _repository.SetAsync("a", "1");
        await _repository.SetAsync("b", "2");

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Key == "a" && s.Value == "1");
    }
}
