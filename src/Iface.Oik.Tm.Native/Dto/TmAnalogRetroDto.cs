using System;

namespace Iface.Oik.Tm.Native.Dto;

public record TmAnalogRetroDto
{
  public float Value { get; init; }
  public int?  Code  { get; init; }
  public float Load  { get; init; }
  public short Flags { get; init; }
  public long  Time  { get; init; }
}