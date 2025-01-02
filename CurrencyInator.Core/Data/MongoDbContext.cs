using CurrencyInator.Core.Data.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CurrencyInator.Core.Data;

public interface IMongoDbContext
{
    bool Enabled { get; }
    IMongoCollection<T> WritableCollection<T>();
    IQueryable<T> ReadableCollection<T>();

}

public class MongoDbContext : IMongoDbContext
{
    public bool Enabled { get; }
    private readonly IMongoDatabase db;

    public MongoDbContext(IOptions<DbSettings> dbSettings)
    {
        var client = new MongoClient(dbSettings.Value.ConnectionString);
        db = client.GetDatabase(dbSettings.Value.DatabaseName);
        Enabled = dbSettings.Value.Enabled;
    }

    public IMongoCollection<T> WritableCollection<T>() => db.GetCollection<T>(typeof(T).Name);

    public IQueryable<T> ReadableCollection<T>() => WritableCollection<T>().AsQueryable();
}
