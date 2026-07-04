using System.Collections.Immutable;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Mappers;

public static class DeviceMapper
{
    public static Device ToDevice(this DeviceDto device)
    {
        return new Device
        {
            Id = device.Id,
            Name = device.Name,
            OS = device.OS,
            DeviceStatus = device.DeviceStatus,
            IpAddress = device.IpAddress,
            AllowedPeerToPeer = device.AllowedPeerToPeer,
            AllowedToInternet = device.AllowedToInternet,
            CanReceive = device.CanReceive,
            CanSend = device.CanSend,
            UserId = device.UserId
        };
    }

    public static List<Device> ToDeviceList(this IImmutableList<DeviceDto> deviceDtos)
    {
        var devices = new List<Device>();

        foreach (var deviceDto in deviceDtos)
        {
            var device = deviceDto.ToDevice();
            devices.Add(device);
        }

        return devices;
    }
}