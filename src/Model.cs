// GENERATED CODE - DO NOT MODIFY BY HAND

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global

namespace Litera;

public enum PrinterStatus {
  Idle,
  Processing,
  Stopped,
  Offline,
}
  
public enum DeviceStatus {
  Online,
  Offline,
  Unknown,
}
  
public partial record Device  {
  public required string Id { get; init; }
  public required string Name { get; init; }
  public required string Os { get; init; }
  public required string IpAddress { get; init; }
  public required DeviceStatus Status { get; init; }
  public required bool AllowedToInternet { get; init; }
  public required bool AllowedPeerToPeer { get; init; }
  public required bool CanSend { get; init; }
  public required bool CanReceive { get; init; }
}
  
public partial record Media  {
  public required string RawName { get; init; }
  public required string DisplayName { get; init; }
  public required double WidthMm { get; init; }
  public required double HeightMm { get; init; }
}
  
public partial record Printer  {
  public required string Id { get; init; }
  public required string DeviceId { get; init; }
  public required string Name { get; init; }
  public required string PrinterUri { get; init; }
  public required PrinterStatus Status { get; init; }
  public required List<Media>? SupportedMedia { get; init; }
  public required bool Shared { get; init; }
  public required bool IsDefault { get; init; }
}
  
public partial record Preferences  {
  public required bool AllowedPeerToPeer { get; init; }
  public required bool CanReceive { get; init; }
  public required bool CanSend { get; init; }
  public required bool AllowedToInternet { get; init; }
}
  
