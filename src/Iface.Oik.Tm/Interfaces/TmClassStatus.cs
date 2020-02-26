using System;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmClassStatus : TmClass
  {
    public bool IsGlobalAckAllowed { get; }
    public bool IsUnackedWhen0     { get; }
    public bool IsUnackedWhen1     { get; }


    [Flags]
    private enum TmClassStatusFlags : uint
    {
      IsGlobalAckAllowedDisabled = 0x0000_0002,
      IsUnackedWhen0Disabled     = 0x0000_0004,
      IsUnackedWhen1Disabled     = 0x0000_0008,
    }


    public TmClassStatus(int    id,
                         string name,
                         int    flags)
      : base(id, name)
    {
      IsGlobalAckAllowed = (flags & (uint) TmClassStatusFlags.IsGlobalAckAllowedDisabled) == 0;
      IsUnackedWhen0     = (flags & (uint) TmClassStatusFlags.IsUnackedWhen0Disabled)     == 0;
      IsUnackedWhen1     = (flags & (uint) TmClassStatusFlags.IsUnackedWhen1Disabled)     == 0;
    }
  }
}