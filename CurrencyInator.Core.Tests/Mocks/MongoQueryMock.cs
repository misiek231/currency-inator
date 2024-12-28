using MongoDB.Bson;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;

namespace CurrencyInator.Core.Tests.Mocks;

public class MongoQueryMock<T> : EnumerableQuery<T>, IOrderedQueryable<T>, IQueryProvider, IMongoQueryProvider
{
    IQueryProvider IQueryable.Provider => this;

    public BsonDocument[] LoggedStages => [];

    public MongoQueryMock(IEnumerable<T> enumerable) : base(enumerable) { }

    public MongoQueryMock(Expression expression) : base(expression) { }

    IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        if (!typeof(IQueryable<TElement>).IsAssignableFrom(expression.Type))
        {
            throw new ArgumentException(nameof(expression));
        }

        return new MongoQueryMock<TElement>(expression);
    }

    public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken); // simulating I/O delay for thread safety tests
        return (this as IQueryProvider).Execute<TResult>(expression);
    }
}