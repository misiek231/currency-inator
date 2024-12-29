using CurrencyInator.Core.Data;
using CurrencyInator.Core.Data.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Core.Configuration;
using Moq;
using Testcontainers.MongoDb;

namespace CurrencyInator.Core.Tests.Integration;

public class DbFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder().Build();
    private readonly Mock<IOptions<DbSettings>> dbSettingsMock = new();

    public IMongoDbContext Context { get; private set; } = null!;
    public string DbConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        DbConnectionString = _container.GetConnectionString();

        dbSettingsMock.SetupGet(p => p.Value).Returns(new DbSettings { ConnectionString = DbConnectionString, DatabaseName = "Test" });

        Context = new MongoDbContext(dbSettingsMock.Object);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _container.StopAsync();
    }
}
