using System;

namespace Iface.Oik.Tm.Interfaces
{
  public readonly struct TmClassStatus
  {
    public int    Id   { get; }
    public string Name { get; }

    public bool IsGlobalAckAllowed { get; }
    public bool IsUnackedWhen0     { get; }
    public bool IsUnackedWhen1     { get; }


    public TmClassStatus(int    id,
                         string name,
                         int    flags)
    {
      Id   = id;
      Name = name;

      IsGlobalAckAllowed = (flags & (uint) TmClassStatusFlags.IsGlobalAckAllowedDisabled) == 0;
      IsUnackedWhen0     = (flags & (uint) TmClassStatusFlags.IsUnackedWhen0Disabled)     == 0;
      IsUnackedWhen1     = (flags & (uint) TmClassStatusFlags.IsUnackedWhen1Disabled)     == 0;
    }


    [Flags]
    private enum TmClassStatusFlags : uint
    {
      IsGlobalAckAllowedDisabled = 0x0000_0002,
      IsUnackedWhen0Disabled     = 0x0000_0004,
      IsUnackedWhen1Disabled     = 0x0000_0008,
    }
  }
}