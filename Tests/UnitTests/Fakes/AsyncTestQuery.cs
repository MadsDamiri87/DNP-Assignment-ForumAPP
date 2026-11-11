using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Tests.UnitTests.Fakes;

// Understøtter EF's async-kald i unit tests, men tester ikke oversættelse til SQL.
internal sealed class AsyncTestQuery<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public AsyncTestQuery(IEnumerable<T> items) : base(items) { }
    public AsyncTestQuery(Expression expression) : base(expression) { }

    IQueryProvider IQueryable.Provider => new AsyncTestQueryProvider(this);

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new AsyncTestEnumerator<T>(this.AsEnumerable().GetEnumerator(), cancellationToken);
}

internal sealed class AsyncTestQueryProvider(IQueryProvider inner) : IAsyncQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
    {
        Type elementType = expression.Type.GetGenericArguments()[0];
        return (IQueryable)Activator.CreateInstance(typeof(AsyncTestQuery<>).MakeGenericType(elementType), expression)!;
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new AsyncTestQuery<TElement>(expression);
    public object? Execute(Expression expression) => inner.Execute(expression);
    public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Type resultType = typeof(TResult).GetGenericArguments()[0];
        object? value = inner.Execute(expression);
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType).Invoke(null, [value])!;
    }
}

internal sealed class AsyncTestEnumerator<T>(IEnumerator<T> inner, CancellationToken cancellationToken) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;
    public ValueTask<bool> MoveNextAsync()
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(inner.MoveNext());
    }
    public ValueTask DisposeAsync()
    {
        inner.Dispose();
        return ValueTask.CompletedTask;
    }
}
