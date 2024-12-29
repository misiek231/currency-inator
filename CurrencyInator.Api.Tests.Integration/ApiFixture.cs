using CurrencyInator.Api;
using CurrencyInator.Core.Data;
using CurrencyInator.Core.Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Testcontainers.MongoDb;

namespace CurrencyInator.Core.Tests.Integration;

public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder().Build();
    public string DbConnectionString { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IOptions<DbSettings>>();
            services.AddOptions<DbSettings>().Configure(p =>
            {
                p.ConnectionString = DbConnectionString;
                p.DatabaseName = "Test";
            });
        });

    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        DbConnectionString = _container.GetConnectionString();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _container.StopAsync();
    }
}
