using System;

namespace Iface.Oik.Tm.Interfaces
{
  public interface ITmAccumRetro
  {
    float    Value { get; }
    float    Load  { get; }
    TmFlags  Flags { get; }
    DateTime Time  { get; }
    int?     Code  { get; }
    
    bool IsUnreliable { get; }
  }
}