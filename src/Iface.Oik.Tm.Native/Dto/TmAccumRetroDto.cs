namespace Iface.Oik.Tm.Native.Dto;

public record TmAccumRetroDto
{
  public float Value { get; init; }
  public float Load  { get; init; }
  public short Flags { get; init; }
  public long  Time  { get; init; }
}