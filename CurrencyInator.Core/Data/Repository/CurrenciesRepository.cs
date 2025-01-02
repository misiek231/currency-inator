using CurrencyInator.Core.Data.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OneOf;
using OneOf.Types;

namespace CurrencyInator.Core.Data.Repository;

public interface ICurrenciesRepository
{
    Task<OneOf<Success<CurrencyRate>, CurrencyRate, Error>> Create(CurrencyRate model, CancellationToken ct);
    Task<OneOf<CurrencyRate, NotFound>> Find(string currency, DateOnly date, CancellationToken ct);
}

public class CurrenciesRepository : ICurrenciesRepository
{
    private readonly IMongoDbContext db;
    private readonly ILogger<CurrenciesRepository> logger;
    private readonly SemaphoreSlim createLock = new(1);

    public CurrenciesRepository(IMongoDbContext db, ILogger<CurrenciesRepository> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    public async Task<OneOf<CurrencyRate, NotFound>> Find(string currency, DateOnly date, CancellationToken ct)
    {
        if (!db.Enabled) return new NotFound();

        var r = db.ReadableCollection<CurrencyRate>()
            .Where(p => p.Currency == currency)
            .Where(p => p.Date == date);

        var result = await r.FirstOrDefaultAsync(ct);

        if (result == null) return new NotFound();

        return result;
    }

    public async Task<OneOf<Success<CurrencyRate>, CurrencyRate, Error>> Create(CurrencyRate model, CancellationToken ct)
    {
        if (!db.Enabled) return model;

        await createLock.WaitAsync(cancellationToken: ct);

        try
        {
            var result = await Find(model.Currency, model.Date, ct);

            if (result.Value is CurrencyRate r) return r;

            await db.WritableCollection<CurrencyRate>().InsertOneAsync(model, new InsertOneOptions(), ct);

            return new Success<CurrencyRate>(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured while saving currency rate to db");
            return new Error();
        }
        finally
        {
            createLock.Release();
        }
    }
}
