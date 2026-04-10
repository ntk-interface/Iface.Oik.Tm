using System;

namespace Iface.Oik.Tm.Native.Dto;

public record GetStatusRetroExArgs
{
  public int      TmCid             { get; init; }
  public short    Ch                { get; init; }
  public short    Rtu               { get; init; }
  public short    Point             { get; init; }
  public DateTime StartTime         { get; init; }
  public DateTime EndTime           { get; init; }
  public int      Step              { get; init; }
  public bool     UserRealTelemetry { get; init; }
}