using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Services.Caching;

namespace LiteraWorker.Core.Services.ApiService;

public class AuthService(IPersistentIdentity persistentIdentity, DeviceService deviceService, HttpService httpService)
{
    public async ValueTask<Result<EmptyRecord>> RegisterUser(UserDto userDto, CancellationToken token)
    {
        return await httpService.PostAsync("api/auth/register", userDto, token);
    }

    public async ValueTask<Result<LoginResponseDto>> Login(LoginRequestDto loginRequest, CancellationToken token = default)
    {
        return await httpService.PostAsync("api/auth/login", loginRequest, JsonContext.Default.LoginResponseDto, token);
    }

    public async ValueTask<Result<EmptyRecord>> Logout(Guid userId, CancellationToken token)
    {
        var result = await httpService.PostAsync($"api/auth/logout/{userId}", EmptyRecord.Empty, JsonContext.Default.EmptyRecord, token);
        await persistentIdentity.ClearIdentity(token);
        return result;
    }

    public async ValueTask RegisterUserDevice(Guid userId, CancellationToken token = default)
    {
        var identity = await persistentIdentity.LoadIdentity(token);
        if (identity != null && identity.DeviceId != Guid.Empty)
        {
            return;
        }

        await deviceService.RegisterDevice(userId, token);
    }
}