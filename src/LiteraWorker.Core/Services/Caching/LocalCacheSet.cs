using System.Collections.Concurrent;
using System.Text.Json;
using LiteraWorker.Core.Helpers;

namespace LiteraWorker.Core.Services.Caching;

/// <summary>
/// Provides helper when operating for local cache
/// </summary>
public static class LocalCacheSet
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    private static SemaphoreSlim GetSemaphoreSlim(string path) => _locks.GetOrAdd(path, _ => new(1,1));

    public static string InitializeCachePath(string cacheDir)
    {
        if (!File.Exists(cacheDir))
        {
            // Create an empty file atomically
            File.WriteAllText(cacheDir, string.Empty);
        }
        return cacheDir;
    }

    public static async Task WriteCache<T>(T value, string path) 
    {
        var semaphore = GetSemaphoreSlim(path);

        await semaphore.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptionsDefault.Options);
            await File.WriteAllTextAsync(path, json);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public static async Task<T?> ReadCache<T>(string path)
    {
        var semaphore = GetSemaphoreSlim(path);

        await semaphore.WaitAsync();
        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(json, options: JsonOptionsDefault.Options);
        }
        finally
        {
            semaphore.Release();
        }    
    }

    public static async Task ClearCache(string path)
    {
        var semaphore = GetSemaphoreSlim(path);

        await semaphore.WaitAsync();
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    public static async Task<bool> IsCacheValid(string path, TimeSpan validTime)
    {
        if (!File.Exists(path)) return false;

        if (string.IsNullOrEmpty(await File.ReadAllTextAsync(path)))
        {
            return false;
        }
        
        var info = new FileInfo(path);
        
        if (DateTimeOffset.UtcNow - info.LastWriteTimeUtc < validTime)
        {
            return true;
        }

        return false;
    }
}