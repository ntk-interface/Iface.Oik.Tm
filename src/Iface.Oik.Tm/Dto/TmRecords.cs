using System;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Dto;

public record struct TmStatusRecord(int       Tma,
                                    short     Status,
                                    TmFlags   Flags,
                                    TmS2Flags S2Flags,
                                    DateTime? ChangeTime)
{
  public static TmStatusRecord CreateFromCommonPointDto(TCommonPointDto dto)
  {
    if (dto.StatusPointDto == null)
    {
      throw new TmNativeException("Ошибка разбора TStatusPointDto");
    }
    return new TmStatusRecord(TmAddr.EncodeTma(dto.Ch, dto.Rtu, dto.Point),
                              dto.StatusPointDto.Value.Status,
                              (TmFlags)dto.StatusPointDto.Value.Flags,
                              (TmS2Flags)dto.TmS2,
                              DateUtil.GetDateTimeFromTimestampWithEpochCheck(dto.TmLocalUt,
                                                                              dto.TmLocalMs));
  }
}


public record struct TmAnalogRecord(int       Tma,
                                    float     Value,
                                    TmFlags   Flags,
                                    DateTime? ChangeTime)
{
  public static TmAnalogRecord CreateFromCommonPointDto(TCommonPointDto dto)
  {
    if (dto.AnalogPointDto == null)
    {
      throw new TmNativeException("Ошибка разбора TAnalogPointDto");
    }
    return new TmAnalogRecord(TmAddr.EncodeTma(dto.Ch, dto.Rtu, dto.Point),
                              dto.AnalogPointDto.Value.AsFloat,
                              (TmFlags)dto.AnalogPointDto.Value.Flags,
                              DateUtil.GetDateTimeFromTimestampWithEpochCheck(dto.TmLocalUt,
                                                                              dto.TmLocalMs));
  }
}


public record struct TmAccumRecord(int       Tma,
                                   float     Value,
                                   float     Load,
                                   TmFlags   Flags,
                                   DateTime? ChangeTime)
{
  public static TmAccumRecord CreateFromCommonPointDto(TCommonPointDto dto)
  {
    if (dto.AccumPointDto == null)
    {
      throw new TmNativeException("Ошибка разбора TAccumPointDto");
    }
    return new TmAccumRecord(TmAddr.EncodeTma(dto.Ch, dto.Rtu, dto.Point),
                             dto.AccumPointDto.Value.Value,
                             dto.AccumPointDto.Value.Load,
                             (TmFlags)dto.AccumPointDto.Value.Flags,
                             DateUtil.GetDateTimeFromTimestampWithEpochCheck(dto.TmLocalUt,
                                                                             dto.TmLocalMs));
  }
}