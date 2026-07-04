using LiteraWorker.Core.Models;
using LiteraWorker.Core.Models.Enums;

namespace LiteraWorker.Core.DTO;

public record SendPrintJobDto(Guid DeviceSenderId, Guid TargetDeviceId, Guid UserSenderId, Guid UserReceiverId, Printer Printer, string Filename, string FilePath, PrintRoute Route, SendPrintConfigDto Config);