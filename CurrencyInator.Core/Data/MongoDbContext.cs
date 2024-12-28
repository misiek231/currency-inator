﻿using CurrencyInator.Core.Data.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CurrencyInator.Core.Data;

public interface IMongoDbContext
{
    IMongoCollection<T> WritableCollection<T>();
    IQueryable<T> ReadableCollection<T>();
}

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase db;

    public MongoDbContext(IOptions<DbSettings> dbSettings)
    {
        var client = new MongoClient(dbSettings.Value.ConnectionString);
        db = client.GetDatabase(dbSettings.Value.DatabaseName);
    }

    public IMongoCollection<T> WritableCollection<T>() => db.GetCollection<T>(nameof(T));

    public IQueryable<T> ReadableCollection<T>() => WritableCollection<T>().AsQueryable();
}