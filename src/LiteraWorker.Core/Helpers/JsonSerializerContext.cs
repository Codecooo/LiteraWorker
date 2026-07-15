using System.Collections.Immutable;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Services.ApiService;

namespace LiteraWorker.Core.Helpers;

[JsonSerializable(typeof(AuthTokens))]
[JsonSerializable(typeof(List<Media>))]
[JsonSerializable(typeof(ImmutableArray<Device>))]
[JsonSerializable(typeof(ImmutableArray<DeviceDto>))]
[JsonSerializable(typeof(DeviceDto))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(List<GetConnectedUserDto>))]
[JsonSerializable(typeof(List<PrinterDto>))]
[JsonSerializable(typeof(List<CreatePrinterDto>))]
[JsonSerializable(typeof(UpdatePrinterDto))]
[JsonSerializable(typeof(UserDto))]
[JsonSerializable(typeof(Identity))]
[JsonSerializable(typeof(ImmutableArray<User>))]
[JsonSerializable(typeof(IImmutableList<Printer>))]
[JsonSerializable(typeof(IImmutableList<PrinterDto>))]
[JsonSerializable(typeof(Result<IImmutableList<SendPrinterDto>>))]
[JsonSerializable(typeof(IImmutableList<Device>))]
[JsonSerializable(typeof(IImmutableList<DeviceDto>))]
[JsonSerializable(typeof(Result<IImmutableList<Device>>))]
[JsonSerializable(typeof(IImmutableList<User>))]
[JsonSerializable(typeof(IImmutableList<UserDto>))]
[JsonSerializable(typeof(Result<IImmutableList<Device>>))]
[JsonSerializable(typeof(Result<Device>))]
[JsonSerializable(typeof(Result<EmptyRecord>))]
[JsonSerializable(typeof(Result<User>))]
[JsonSerializable(typeof(IImmutableList<PrintJob>))]
[JsonSerializable(typeof(Result<IImmutableList<ServerPrintJobDto>>))]
[JsonSerializable(typeof(IEnumerable<ServerPrintJobDto>))]
[JsonSerializable(typeof(IImmutableList<ServerPrintJobDto>))]
[JsonSerializable(typeof(IImmutableList<SendPrintJobDto>))]
[JsonSerializable(typeof(TokenResponseDto))]
[JsonSerializable(typeof(LoginRequestDto))]
[JsonSerializable(typeof(MultipartFormDataContent))]
[JsonSerializable(typeof(LocalStoredCredential))]
[JsonSerializable(typeof(LoginResponseDto))]
[JsonSerializable(typeof(CreateDeviceDto))]
[JsonSerializable(typeof(Dictionary<string, Media>))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(bool))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class JsonContext : JsonSerializerContext;