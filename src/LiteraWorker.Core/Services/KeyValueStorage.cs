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
        var value = FileSecurity.Decrypt(filePath);
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

        var result = JsonSerializer.Deserialize<TValue>(value, JsonOptionsDefault.Options);

        return ValueTask.FromResult(result)!;
    }

    public ValueTask<TValue?> SetAsync<TValue>(string filePath, TValue value, CancellationToken ct) where TValue : notnull
    {
        var stringValue = JsonSerializer.Serialize(value, JsonOptionsDefault.Options);
        FileSecurity.Encrypt(filePath, stringValue);
        return ValueTask.FromResult<TValue?>(default);
    }

#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
}