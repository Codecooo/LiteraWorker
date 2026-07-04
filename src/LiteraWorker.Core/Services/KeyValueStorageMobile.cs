#if ANDROID

using System.Text.Json;
using LiteraWorker.Core.Helpers;
using Plugin.SecureStorage;

namespace LiteraWorker.Core.Services;

public class KeyValueStorageMobile : IKeyValueStorage
{
    public ValueTask ClearAsync(string key, CancellationToken ct)
    {
        CrossSecureStorage.Current.DeleteKey(key);
        return ValueTask.CompletedTask;
    }

    public ValueTask<TValue?> GetAsync<TValue>(string key, CancellationToken ct)
    {
        var valueJson = CrossSecureStorage.Current.GetValue(key);

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
        
        var value = JsonSerializer.Deserialize<TValue>(valueJson, JsonOptionsDefault.Options);

        return ValueTask.FromResult(value);
    }

    public ValueTask SetAsync<TValue>(string key, TValue value, CancellationToken ct) where TValue : notnull
    {
        var valueJson = JsonSerializer.Serialize(value, JsonOptionsDefault.Options);

        CrossSecureStorage.Current.SetValue(key, valueJson);
        return ValueTask.CompletedTask;
    }

#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
}

#endif