using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public interface ITmAnalogRetro
  {
    float    Value { get; }
    TmFlags  Flags { get; }
    DateTime Time  { get; }
  }
}