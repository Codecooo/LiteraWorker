using System.Text.Json;
using LiteraWorker.Core.Helpers;

namespace LiteraWorker.Core.Services;

public class KeyValueStorage : IKeyValueStorage
{
    public ValueTask ClearAsync(string filePath, CancellationToken ct)
    {
        File.Delete(filePath);
        return ValueTask.CompletedTask;
    }

    public ValueTask<TValue?> GetAsync<TValue>(string filePath, CancellationToken ct)
    {
        if (!File.Exists(filePath)) return ValueTask.FromResult<TValue?>(default);

        var value = FileSecurity.Decrypt(filePath);
        var result = JsonSerializer.Deserialize<TValue>(value, JsonOptionsDefault.Options);

        return ValueTask.FromResult(result)!;
    }

    public ValueTask<TValue?> SetAsync<TValue>(string filePath, TValue value, CancellationToken ct) where TValue : notnull
    {
        var stringValue = JsonSerializer.Serialize(value, JsonOptionsDefault.Options);
        FileSecurity.Encrypt(filePath, stringValue);
        return ValueTask.FromResult<TValue?>(default);
    }
}