#define LINUX
#if WINDOWS || MACOS || LINUX

using System.Text.Json;
using KeySharp;
using LiteraWorker.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Services;

public class KeyValueStorageDesktop(ILogger<KeyValueStorageDesktop> logger) : IKeyValueStorage
{
    private const string PackageName = "com.codecoo.litera";
    private const string Label = "Litera Secure Storage";

    public ValueTask ClearAsync(string key, CancellationToken ct)
    {
        Keyring.DeletePassword(PackageName, key, Label);
        return ValueTask.CompletedTask;
    }

    public ValueTask<TValue?> GetAsync<TValue>(string key, CancellationToken ct)
    {
        string value;
        try
        {
            value = Keyring.GetPassword(PackageName, key, Label);
        }
        catch (Exception ex)
        {
            logger.LogDebug("There is a problem while extracting password from a vault: {msg}", ex.Message);
            return ValueTask.FromResult<TValue?>(default);
        }

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.

        var result = JsonSerializer.Deserialize<TValue>(value, JsonOptionsDefault.Options);

        return ValueTask.FromResult(result)!;
    }

    public ValueTask SetAsync<TValue>(string key, TValue value, CancellationToken ct) where TValue : notnull
    {
        var stringValue = JsonSerializer.Serialize(value, JsonOptionsDefault.Options);
        // save the keys to the secret
        Keyring.SetPassword(PackageName, key, Label, stringValue);

        return ValueTask.CompletedTask;
    }

#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
}

#endif