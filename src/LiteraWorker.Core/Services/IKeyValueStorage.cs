namespace LiteraWorker.Core.Services;

public interface IKeyValueStorage
{
    ValueTask ClearAsync(string filePath, CancellationToken ct);
    ValueTask<TValue?> GetAsync<TValue>(string filePath, CancellationToken ct);
    ValueTask<TValue?> SetAsync<TValue>(string filePath, TValue value, CancellationToken ct) where TValue : notnull;
}