using System.Net;
using LiteraWorker.Core.DTO;

namespace LiteraWorker.Core.Services.Auth;

public class LocalAuthHandler(IKeyValueStorage keyValueStorage)
{
    private string _storageKey = OperatingSystem.IsWindows() ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiteraWorker", "local_credentials.json")
        : "/var/lib/litera/local_credentials.json";

    public async Task SaveUser(string password)
    {
        var credentials = new LocalStoredCredential(Environment.UserName, password);
        await keyValueStorage.SetAsync(_storageKey, credentials, CancellationToken.None);
    }

    public async Task<NetworkCredential?> GetUser()
    {
        var credentials = await keyValueStorage.GetAsync<LocalStoredCredential>(_storageKey, CancellationToken.None);
        return new NetworkCredential(credentials?.Username, credentials?.Password);
    }

    public async Task<bool> IsUserSaved()
    {
        var credentials = await keyValueStorage.GetAsync<NetworkCredential>(_storageKey, CancellationToken.None);
        return credentials != null;
    }
}