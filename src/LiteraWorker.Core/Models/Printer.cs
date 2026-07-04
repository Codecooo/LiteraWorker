using LiteraWorker.Core.Models.Enums;

namespace LiteraWorker.Core.Models;

public partial record Printer
{
    public Guid Id { get; init; }
    public Guid DeviceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Uri? PrinterUri { get; init; }
    public PrinterStatus PrinterStatus { get; init; }
    public List<Media>? SupportedMedia { get; init; }
    public bool Shared { get; init; } = true;
    public bool IsDefault { get; init; }
}

public sealed class PrinterIdentityComparer : IEqualityComparer<Printer>
{
    public bool Equals(Printer? x, Printer? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        return x.Name == y.Name &&
               x.PrinterUri == y.PrinterUri &&
               x.DeviceId == y.DeviceId;
    }

    // Must produce the same hash for objects that are considered equal
    public int GetHashCode(Printer obj)
    {
        if (obj is null) return 0;

        return HashCode.Combine(obj.Name, obj.PrinterUri, obj.DeviceId);
    }
}