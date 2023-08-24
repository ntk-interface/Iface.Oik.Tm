using System;

namespace Iface.Oik.Tm.Interfaces
{
  public interface ITmAnalogRetro
  {
    float    Value { get; }
    TmFlags  Flags { get; }
    DateTime Time  { get; }
    int?     Code  { get; }
    
    bool IsUnreliable { get; }
  }
}