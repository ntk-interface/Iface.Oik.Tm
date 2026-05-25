using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;

namespace Iface.Oik.Tm.Native.Dto;

public record struct TCommonPointDto
{
  public string? Name       { get; init; }
  public byte    CpFlags    { get; init; }
  public byte    Res1       { get; init; }
  public ushort  Type       { get; init; }
  public ushort  Ch         { get; init; }
  public ushort  Rtu        { get; init; }
  public ushort  Point      { get; init; }
  public uint    TmFlags    { get; init; }
  public ushort  TmS2       { get; init; }
  public ushort  TmFlags2   { get; init; }
  public uint    TmLocalUt  { get; init; }
  public uint    TmRemoteUt { get; init; }
  public ushort  TmLocalMs  { get; init; }
  public ushort  TmRemoteMs { get; init; }

  public TStatusPointDto? StatusPointDto { get; init; }
  public TAnalogPointDto? AnalogPointDto { get; init; }
  public TAccumPointDto?  AccumPointDto  { get; init; }


  internal static unsafe TCommonPointDto Create(TmNativeDefsUnsafe.TCommonPoint point,
                                                bool                            queryUnit = false,
                                                int?                            cid       = null)
  {
    return new TCommonPointDto
    {
      Name           = TmNativeUtil.GetCStringFromIntPtr(point.name),
      CpFlags        = point.cp_flags,
      Res1           = point.res1,
      Type           = point.Type,
      Ch             = point.Ch,
      Rtu            = point.RTU,
      Point          = point.Point,
      TmFlags        = point.TM_Flags,
      TmS2           = point.tm_s2,
      TmFlags2       = point.tm_flags2,
      TmLocalUt      = point.tm_local_ut,
      TmRemoteUt     = point.tm_remote_ut,
      TmLocalMs      = point.tm_local_ms,
      TmRemoteMs     = point.tm_remote_ms,
      StatusPointDto = point.Type == (ushort)TmNativeDefs.TmDataTypes.Status
                         ? TStatusPointDto.Create(TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.TStatusPoint>(point.Data))
                         : null,
      AnalogPointDto = point.Type == (ushort)TmNativeDefs.TmDataTypes.Analog
                         ? TAnalogPointDto.Create(TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.TAnalogPoint>(point.Data),
                                                  queryUnit,
                                                  cid)
                         : null,
      AccumPointDto = point.Type == (ushort)TmNativeDefs.TmDataTypes.Accum
                        ? TAccumPointDto.Create(TmNativeUtil.FromBytesPtr<TmNativeDefsUnsafe.TAccumPoint>(point.Data),
                                                queryUnit,
                                                cid)
                         : null,
    };
  }
}


public record struct TStatusPointDto
{
  public short Status { get; init; }
  public short Flags  { get; init; }


  internal static TStatusPointDto Create(TmNativeDefsUnsafe.TStatusPoint point)
  {
    return new TStatusPointDto
    {
      Status = point.Status,
      Flags  = point.Flags,
    };
  }
}


public record struct TAnalogPointDto
{
  public float   AsFloat { get; init; }
  public short   AsCode  { get; init; }
  public short   Flags   { get; init; }
  public string? Unit    { get; init; }
  public byte    Format  { get; init; }


  internal static unsafe TAnalogPointDto Create(TmNativeDefsUnsafe.TAnalogPoint point,
                                                bool                            queryUnit = false,
                                                int?                            cid       = null)
  {
    return new TAnalogPointDto
    {
      AsFloat = point.AsFloat,
      AsCode  = point.AsCode,
      Flags   = point.Flags,
      Unit    = queryUnit && cid.HasValue ? TmNativeApi.GetTextByRef(point.Unit, cid.Value) : string.Empty,
      Format  = point.Format,
    };
  }
}


public record struct TAccumPointDto
{
  public float   Value  { get; init; }
  public float   Load   { get; init; }
  public short   Flags  { get; init; }
  public string? Unit   { get; init; }
  public byte    Format { get; init; }


  internal static unsafe TAccumPointDto Create(TmNativeDefsUnsafe.TAccumPoint point,
                                               bool                           queryUnit = false,
                                               int?                           cid       = null)
  {
    return new TAccumPointDto
    {
      Value  = point.Value,
      Load   = point.Load,
      Flags  = point.Flags,
      Unit   = queryUnit && cid.HasValue ? TmNativeApi.GetTextByRef(point.Unit, cid.Value) : string.Empty,
      Format = point.Format,
    };
  }
}