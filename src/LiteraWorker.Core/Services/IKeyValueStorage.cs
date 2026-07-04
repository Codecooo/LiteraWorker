namespace LiteraWorker.Core.Services;

public interface IKeyValueStorage
{
    ValueTask ClearAsync(string key, CancellationToken ct);
    ValueTask<TValue?> GetAsync<TValue>(string key, CancellationToken ct);
    ValueTask SetAsync<TValue>(string key, TValue value, CancellationToken ct) where TValue : notnull;
}