using System;

namespace Iface.Oik.Tm.Native.Dto;

public record TmNativeEventFilter
{
  public ushort                   Types       { get; init; }
  public long                     StartTime   { get; init; }
  public long                     EndTime     { get; init; }
  public int                     OutputLimit { get; init; }
  public TmNativeEventImportances Importances { get; init; }
}

[Flags]
public enum TmNativeEventImportances
{
  None = 0,
  Imp0 = 1,
  Imp1 = 2,
  Imp2 = 4,
  Imp3 = 8,
  Any  = 0xFF,
}