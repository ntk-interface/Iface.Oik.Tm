using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iface.Oik.Tm.Interfaces;
using Iface.Oik.Tm.Native.Api;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Api;

public partial class TmsApi
{
  public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScript(TmStatus tmStatus)
  {
    if (tmStatus == null) return (false, null);

    await UpdateStatus(tmStatus).ConfigureAwait(false);
    var newStatus = tmStatus.Status ^ 1;

    return await CheckTelecontrolScriptExplicitly(tmStatus, newStatus).ConfigureAwait(false);
  }


  public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTelecontrolScriptExplicitly(
    TmStatus tmStatus,
    int      explicitNewStatus)
  {
    if (tmStatus == null) return (false, null);

    await UpdateStatus(tmStatus).ConfigureAwait(false);
    var (ch, rtu, point) = tmStatus.TmAddr.GetTupleShort();

    var scriptResult = await Task.Run(() => TmNative.tmcExecuteControlScript(_cid,
                                                                             ch,
                                                                             rtu,
                                                                             point,
                                                                             (short)explicitNewStatus))
                                 .ConfigureAwait(false);

    var conditions = new List<TmControlScriptCondition>();

    if (tmStatus.IsUnreliable ||
        tmStatus.IsInvalid    ||
        tmStatus.IsS2Failure  ||
        tmStatus.IsManuallyBlocked)
    {
      scriptResult = 0;
      conditions.Add(new TmControlScriptCondition(false, "Нет достоверной информации о состоянии"));
    }

    (await GetLastTmcErrorText().ConfigureAwait(false))
    ?.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
     .ForEach(condition =>
      {
        var isConditionMet = condition[0] == '1';
        var text           = condition.Substring(1);
        conditions.Add(new TmControlScriptCondition(isConditionMet, text));
      });

    return (scriptResult == 1, conditions);
  }


  public async Task<(bool, IReadOnlyCollection<TmControlScriptCondition>)> CheckTeleregulationScript(TmAnalog tmAnalog)
  {
    if (tmAnalog == null)
    {
      return (false, null);
    }

    await UpdateAnalog(tmAnalog).ConfigureAwait(false);

    TmNativeDefs.AnalogRegulationType command;
    if (tmAnalog.HasTeleregulationByCode)
    {
      command = TmNativeDefs.AnalogRegulationType.Code;
    }
    else if (tmAnalog.HasTeleregulationByValue)
    {
      command = TmNativeDefs.AnalogRegulationType.Value;
    }
    else if (tmAnalog.HasTeleregulationByStep)
    {
      command = TmNativeDefs.AnalogRegulationType.Step;
    }
    else
    {
      return (false, new List<TmControlScriptCondition> { new(false, "Не определено регулирование") });
    }

    var (ch, rtu, point) = tmAnalog.TmAddr.GetTupleShort();

    var scriptResult = await Task.Run(() => TmNativeApi.ExecuteTeleregulationScript(_cid,
                                        ch,
                                        rtu,
                                        point,
                                        command))
                                 .ConfigureAwait(false);

    var conditions = new List<TmControlScriptCondition>();

    if (tmAnalog.IsUnreliable ||
        tmAnalog.IsInvalid    ||
        tmAnalog.IsManuallyBlocked)
    {
      scriptResult = 0;
      conditions.Add(new TmControlScriptCondition(false, "Нет достоверной информации о состоянии"));
    }

    (await GetLastTmcErrorText().ConfigureAwait(false))
    ?.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
     .ForEach(condition =>
      {
        var isConditionMet = condition[0] == '1';
        var text           = condition.Substring(1);
        conditions.Add(new TmControlScriptCondition(isConditionMet, text));
      });

    return (scriptResult == 1, conditions);
  }


  public async Task OverrideTelecontrolScript()
  {
    await Task.Run(() => TmNative.tmcOverrideControlScript(_cid, true))
              .ConfigureAwait(false);
  }


  public async Task InputTelecontrolPassword(string password)
  {
    await Task.Run(() => TmNative.tmcSetTcPwd(_cid, EncodingUtil.StringToBytes(password))).ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> Telecontrol(TmStatus tmStatus)
  {
    if (tmStatus == null) return TmTelecontrolResult.CommandNotSentToServer;

    var (ch, rtu, point) = tmStatus.TmAddr.GetTuple();
    int currentStatus = await GetStatus(ch, rtu, point).ConfigureAwait(false);

    return await TelecontrolExplicitly(tmStatus, currentStatus ^ 1).ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> TelecontrolExplicitly(TmStatus tmStatus, int explicitNewStatus)
  {
    if (tmStatus == null) return TmTelecontrolResult.CommandNotSentToServer;

    var (ch, rtu, point) = tmStatus.TmAddr.GetTuple();

    // регистрируем событие переключения (в старом клиенте такой порядок - сначала событие, потом само переключение)
    var ev = new TmNativeDefs.TEvent
    {
      Ch    = ch,
      Rtu   = rtu,
      Point = point,
      Id    = (ushort)TmNativeDefs.EventTypes.Control,
      Imp   = 0,
      Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
      {
        Cmd = (byte)explicitNewStatus,
      }),
    };
    await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
              .ConfigureAwait(false);

    // телеуправление
    var result = await Task.Run(() => TmNative.tmcControlByStatus(_cid,
                                                                  (short)ch,
                                                                  (short)rtu,
                                                                  (short)point,
                                                                  (short)explicitNewStatus))
                           .ConfigureAwait(false);
    if (result <= 0) // если не прошло, регистрируем событие
    {
      ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
      {
        Result = (byte)result,
        Cmd    = (byte)explicitNewStatus,
      });
    }

    return (TmTelecontrolResult)result;
  }


  public async Task<TmTelecontrolResult> TeleregulateByStepUp(TmAnalog analog)
  {
    return await TeleregulateByStepUpOrDown(analog, true).ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> TeleregulateByStepDown(TmAnalog analog)
  {
    return await TeleregulateByStepUpOrDown(analog, false).ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> TeleregulateByCode(TmAnalog analog, int code)
  {
    return await TeleregulateByValueOrCode(analog, null, code).ConfigureAwait(false);
  }


  public async Task<TmTelecontrolResult> TeleregulateByValue(TmAnalog analog, float value)
  {
    return await TeleregulateByValueOrCode(analog, value, null).ConfigureAwait(false);
  }


  private async Task<TmTelecontrolResult> TeleregulateByValueOrCode(TmAnalog analog, float? value, int? code)
  {
    if (analog == null)
    {
      return TmTelecontrolResult.CommandNotSentToServer;
    }

    var (ch, rtu, point) = analog.TmAddr.GetTupleShort();

    if (value.HasValue)
    {
      var result = await Task.Run(() => TmNativeApi.TeleregulateByValue(_cid,
                                                                        ch,
                                                                        rtu,
                                                                        point,
                                                                        value.Value))
                             .ConfigureAwait(false);

      return (TmTelecontrolResult)result;
    }

    if (code.HasValue)
    {
      var result = await Task.Run(() => TmNativeApi.TeleregulateByCode(_cid,
                                                                       ch,
                                                                       rtu,
                                                                       point,
                                                                       (short)code.Value))
                             .ConfigureAwait(false);

      return (TmTelecontrolResult)result;
    }

    return TmTelecontrolResult.CommandNotSentToServer;
  }


  private async Task<TmTelecontrolResult> TeleregulateByStepUpOrDown(TmAnalog analog, bool isStepUp)
  {
    if (analog == null)
    {
      return TmTelecontrolResult.CommandNotSentToServer;
    }

    var (ch, rtu, point) = analog.TmAddr.GetTupleShort();

    var result = await Task.Run(() => TmNativeApi.TeleregulateByStepUpOrDown(_cid,
                                                                             ch,
                                                                             rtu,
                                                                             point,
                                                                             isStepUp))
                           .ConfigureAwait(false);

    return (TmTelecontrolResult)result;
  }


  public async Task SetStatus(int ch, int rtu, int point, int status)
  {
    if (status != 0 && status != 1) return;

    await Task.Run(() => TmNative.tmcSetStatus(_cid, (short)ch, (short)rtu, (short)point, (byte)status, Array.Empty<byte>(), 0))
              .ConfigureAwait(false);
  }


  public async Task SetStatusNormalOn(TmStatus status)
  {
    await SetStatusNormal(status, 1).ConfigureAwait(false);
  }


  public async Task SetStatusNormalOff(TmStatus status)
  {
    await SetStatusNormal(status, 0).ConfigureAwait(false);
  }


  public async Task ClearStatusNormal(TmStatus status)
  {
    await SetStatusNormal(status, -1).ConfigureAwait(false);
  }


  private async Task SetStatusNormal(TmStatus status, int normalValue)
  {
    if (status == null) return;

    var (ch, rtu, point) = status.TmAddr.GetTupleShort();

    await Task.Run(() => TmNative.tmcSetStatusNormal(_cid, ch, rtu, point, (ushort)normalValue))
              .ConfigureAwait(false);

    // переключаем также нормальное состояние резерва, если есть
    var resStatus = await FindTmTagReserveTag(status).ConfigureAwait(false);
    if (resStatus != null)
    {
      var (resCh, resRtu, resPoint) = resStatus.TmAddr.GetTupleShort();
      await Task.Run(() => TmNative.tmcSetStatusNormal(_cid, resCh, resRtu, resPoint, (ushort)normalValue))
                .ConfigureAwait(false);
    }
  }


  public async Task<bool> SwitchStatusManually(TmStatus tmStatus,
                                               bool     alsoBlockManually = false)
  {
    if (tmStatus == null) return false;

    var (ch, rtu, point) = tmStatus.TmAddr.GetTuple();

    int currentStatus = await GetStatus(ch, rtu, point).ConfigureAwait(false);
    int newStatus     = currentStatus ^ 1;

    // регистрируем событие переключения (в старом клиенте такой порядок - сначала событие, потом само переключение)
    var ev = new TmNativeDefs.TEvent
    {
      Ch    = ch,
      Rtu   = rtu,
      Point = point,
      Id    = (ushort)TmNativeDefs.EventTypes.ManualStatusSet,
      Imp   = 0,
      Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
      {
        Cmd = (byte)newStatus,
      }),
    };
    await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
              .ConfigureAwait(false);

    // выставляем новое состояние с флагом ручной установки
    if (tmStatus.IsInverted) // при инверсии нужно инвертировать команду
    {
      newStatus ^= 1;
    }

    byte flags = (byte)TmNativeDefs.Flags.ManuallySet;
    if (alsoBlockManually)
    {
      flags += (byte)TmNativeDefs.Flags.UnreliableManu;
    }

    var tvf = new TmNativeDefs.TTimedValueAndFlags
    {
      Vf =
      {
        Adr = tmStatus.TmAddr.ToAdrTm(),
        Type = (byte)TmNativeDefs.VfType.Status  +
               (byte)TmNativeDefs.VfType.FlagSet +
               (byte)TmNativeDefs.VfType.AlwaysSetValue,
        Flags = flags,
        Bits  = 1,
        Value = (uint)newStatus,
      },
      Xt =
      {
        Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
      }
    };
    await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
              .ConfigureAwait(false);

    // переключаем также резерв, если есть
    var resStatus = await FindTmTagReserveTag(tmStatus).ConfigureAwait(false);
    if (resStatus != null)
    {
      tvf.Vf.Adr = resStatus.TmAddr.ToAdrTm();
      await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
    }

    return true;
  }


  public async Task<bool> SetStatusBackdateManually(TmStatus tmStatus,
                                                    int      status,
                                                    DateTime time)
  {
    if (tmStatus == null) return false;

    var (ch, rtu, point) = tmStatus.TmAddr.GetTuple();

    var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
    var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

    // регистрируем событие переключения (в старом клиенте такой порядок - сначала событие, потом само переключение)
    var ev = new TmNativeDefs.TEvent
    {
      Ch       = ch,
      Rtu      = rtu,
      Point    = point,
      Id       = (ushort)TmNativeDefs.EventTypes.ManualStatusSet,
      Imp      = 0,
      DateTime = time.ToTmByteArray(),
      Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
      {
        Cmd = (byte)status,
      }),
    };
    await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
              .ConfigureAwait(false);

    // выставляем новое состояние с флагом ручной установки
    if (tmStatus.IsInverted) // при инверсии нужно инвертировать команду
    {
      status ^= 1;
    }

    var tvf = new TmNativeDefs.TTimedValueAndFlags
    {
      Vf =
      {
        Adr = tmStatus.TmAddr.ToAdrTm(),
        Type = (byte)TmNativeDefs.VfType.Status  +
               (byte)TmNativeDefs.VfType.FlagSet +
               (byte)TmNativeDefs.VfType.AlwaysSetValue,
        Flags = (byte)TmNativeDefs.Flags.ManuallySet,
        Bits  = 1,
        Value = (uint)status,
      },
      Xt =
      {
        Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
        Sec   = (uint)serverUtcTime,
      }
    };
    await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
              .ConfigureAwait(false);

    // переключаем также резерв, если есть
    var resStatus = await FindTmTagReserveTag(tmStatus).ConfigureAwait(false);
    if (resStatus != null)
    {
      tvf.Vf.Adr = resStatus.TmAddr.ToAdrTm();
      await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
    }

    return true;
  }


  public async Task SetAnalog(int ch, int rtu, int point, float value)
  {
    await Task.Run(() => TmNative.tmcSetAnalog(_cid, (short)ch, (short)rtu, (short)point, value, Array.Empty<byte>()))
              .ConfigureAwait(false);
  }


  public async Task SetAnalogByCode(int ch, int rtu, int point, int code)
  {
    await Task.Run(() => TmNative.tmcSetAnalogByCode(_cid, (short)ch, (short)rtu, (short)point, (short)code))
              .ConfigureAwait(false);
  }


  public async Task<bool> SetAnalogBackdateManually(TmAnalog tmAnalog, float value, DateTime time)
  {
    if (tmAnalog == null) return false;

    var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
    var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

    // установка нового значения
    var uintValue = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0); // функция требует значение DWORD

    var tvf = new TmNativeDefs.TTimedValueAndFlags
    {
      Vf =
      {
        Adr = tmAnalog.TmAddr.ToAdrTm(),
        Type = (byte)TmNativeDefs.VfType.AnalogFloat +
               (byte)TmNativeDefs.VfType.FlagSet     +
               (byte)TmNativeDefs.VfType.AlwaysSetValue,
        Flags = (byte)TmNativeDefs.Flags.ManuallySet,
        Bits  = 32,
        Value = uintValue,
      },
      Xt =
      {
        Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
        Sec   = (uint)serverUtcTime,
      }
    };
    await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf })).ConfigureAwait(false);

    // выставляем также значение резерву, если есть
    var resAnalogs = await FindTmTagReserveTag(tmAnalog).ConfigureAwait(false);
    if (resAnalogs != null)
    {
      tvf.Vf.Adr = resAnalogs.TmAddr.ToAdrTm();
      await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
    }

    // регистрируем событие
    var ev = new TmNativeDefs.TEvent
    {
      Id       = (ushort)TmNativeDefs.EventTypes.ManualAnalogSet,
      Imp      = 0,
      DateTime = time.ToTmByteArray(),
      Data = TmNativeUtil.GetBytes(new TmNativeDefs.AnalogSetData
      {
        Cmd   = 1, // флаг ручной установки
        Value = value,
      }),
    };
    (ev.Ch, ev.Rtu, ev.Point) = tmAnalog.TmAddr.GetTuple();

    await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
              .ConfigureAwait(false);

    return true;
  }


  public async Task<bool> BackdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs,
                                          IReadOnlyList<float>    values,
                                          DateTime                time)
  {
    if (tmAnalogs.IsNullOrEmpty() || values.IsNullOrEmpty())
    {
      return false;
    }

    if (tmAnalogs.Count != values.Count)
    {
      return false;
    }

    var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
    var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

    var tvf = new TmNativeDefs.TTimedValueAndFlags[tmAnalogs.Count];
    for (var i = 0; i < tmAnalogs.Count; i++)
    {
      tvf[i] = new TmNativeDefs.TTimedValueAndFlags
      {
        Vf =
        {
          Adr = tmAnalogs[i].TmAddr.ToAdrTm(),
          Type = (byte)TmNativeDefs.VfType.AnalogFloat +
                 (byte)TmNativeDefs.VfType.AlwaysSetValue,
          Bits  = 32,
          Value = BitConverter.ToUInt32(BitConverter.GetBytes(values[i]), 0), // функция требует значение DWORD
        },
        Xt =
        {
          Sec = (uint)serverUtcTime,
        }
      };
    }

    var result = await Task.Run(() => TmNative.tmcSetTimedValues(_cid, (uint)tvf.Length, tvf))
                           .ConfigureAwait(false);

    return result > 0;
  }


  public async Task<bool> PostdateAnalogs(IReadOnlyList<TmAnalog> tmAnalogs,
                                          IReadOnlyList<float>    values,
                                          DateTime                time)
  {
    if (tmAnalogs.IsNullOrEmpty() || values.IsNullOrEmpty())
    {
      return false;
    }

    if (tmAnalogs.Count != values.Count)
    {
      return false;
    }

    var utcTime       = DateUtil.GetUtcTimestampFromDateTime(time);
    var serverUtcTime = TmNative.uxgmtime2uxtime(utcTime);

    return await Task.Run(() => TmNative.tmcPerspPutAnalogs(_cid,
                                                            (uint)serverUtcTime,
                                                            (uint)tmAnalogs.Count,
                                                            tmAnalogs.Select(a => a.TmAddr.ToAdrTm()).ToArray(),
                                                            values.ToArray()))
                     .ConfigureAwait(false);
  }


  public async Task<bool> SetAnalogManually(TmAnalog tmAnalog, float value, bool alsoBlockManually = false)
  {
    if (tmAnalog == null) return false;

    // установка нового значения
    var uintValue = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0); // функция требует значение DWORD

    byte flags = (byte)TmNativeDefs.Flags.ManuallySet;
    if (alsoBlockManually)
    {
      flags += (byte)TmNativeDefs.Flags.UnreliableManu;
    }

    var tvf = new TmNativeDefs.TTimedValueAndFlags
    {
      Vf =
      {
        Adr = tmAnalog.TmAddr.ToAdrTm(),
        Type = (byte)TmNativeDefs.VfType.AnalogFloat +
               (byte)TmNativeDefs.VfType.FlagSet     +
               (byte)TmNativeDefs.VfType.AlwaysSetValue,
        Flags = flags,
        Bits  = 32,
        Value = uintValue,
      },
      Xt =
      {
        Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
      }
    };
    await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf })).ConfigureAwait(false);

    // выставляем также значение резерву, если есть
    var resAnalogs = await FindTmTagReserveTag(tmAnalog).ConfigureAwait(false);
    if (resAnalogs != null)
    {
      tvf.Vf.Adr = resAnalogs.TmAddr.ToAdrTm();
      await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
    }

    // регистрируем событие
    var ev = new TmNativeDefs.TEvent
    {
      Id  = (ushort)TmNativeDefs.EventTypes.ManualAnalogSet,
      Imp = 0,
      Data = TmNativeUtil.GetBytes(new TmNativeDefs.AnalogSetData
      {
        Cmd   = 1, // флаг ручной установки
        Value = value,
      }),
    };
    (ev.Ch, ev.Rtu, ev.Point) = tmAnalog.TmAddr.GetTuple();

    await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
              .ConfigureAwait(false);

    return true;
  }


  public async Task<bool> SetAnalogTechParameters(TmAnalog analog, TmAnalogTechParameters parameters)
  {
    if (analog == null || parameters == null)
    {
      return false;
    }

    var tmcAddr = analog.TmAddr.ToAdrTm();
    var techParams = new TmNativeDefs.TAnalogTechParms
    {
      ZoneLim  = new float[TmNativeDefs.TAnalogTechParmsAlarmSize],
      Reserved = new uint[TmNativeDefs.TAnalogTechParamsReservedSize],
    };
    if (!await Task.Run(() => TmNative.tmcGetAnalogTechParms(_cid, ref tmcAddr, ref techParams))
                   .ConfigureAwait(false))
    {
      return false;
    }

    techParams.MinVal  = parameters.Min;
    techParams.MaxVal  = parameters.Max;
    techParams.Nominal = parameters.Nominal;
    if (techParams.AlrPresent > 0)
    {
      techParams.ZoneLim[0] = parameters.MinAlarmOrInvalid;
      techParams.ZoneLim[1] = parameters.MinWarningOrInvalid;
      techParams.ZoneLim[2] = parameters.MaxWarningOrInvalid;
      techParams.ZoneLim[3] = parameters.MaxAlarmOrInvalid;
    }

    return await Task.Run(() => TmNative.tmcSetAnalogTechParms(_cid, ref tmcAddr, ref techParams))
                     .ConfigureAwait(false);
  }


  public async Task<bool> SetAlarmValue(TmAlarm tmAlarm, float value)
  {
    if (tmAlarm.TmAnalog == null) return false;

    await Task.Run(() =>
    {
      // получение структуры уставки
      var (ch, rtu, point) = tmAlarm.TmAnalog.TmAddr.GetTupleShort();
      var nativeAlarm = new TmNativeDefs.TAlarm();
      TmNative.tmcPeekAlarm(_cid, ch, rtu, point, (short)tmAlarm.Id, ref nativeAlarm);

      // установка нового значения
      nativeAlarm.Value = value;
      TmNative.tmcPokeAlarm(_cid, ch, rtu, point, (short)tmAlarm.Id, ref nativeAlarm);
    }).ConfigureAwait(false);

    // регистрируем событие
    var message = $"Изменена уставка \"{tmAlarm.Name}\" на \"{tmAlarm.TmAnalog.Name}\"" +
                  $", новое значение {tmAlarm.TmAnalog.FakeValueWithUnitString(value)}";
    await AddStringToEventLog(message, tmAlarm.TmAnalog.TmAddr).ConfigureAwait(false);

    return true;
  }


  public async Task SetAccum(int ch, int rtu, int point, float value)
  {
    await Task.Run(() => TmNative.tmcSetAccumValue(_cid, (short)ch, (short)rtu, (short)point, value, Array.Empty<byte>()))
              .ConfigureAwait(false);
  }


  public async Task SetTagFlags(TmTag   tag,
                                TmFlags flags)
  {
    await ToggleTagFlags(tag, flags, true).ConfigureAwait(false);
  }


  public async Task ClearTagFlags(TmTag   tag,
                                  TmFlags flags)
  {
    await ToggleTagFlags(tag, flags, false).ConfigureAwait(false);
  }


  private async Task ToggleTagFlags(TmTag   tmTag,
                                    TmFlags flags,
                                    bool    isSet)
  {
    byte timedValueType;

    switch (tmTag)
    {
      case TmStatus _:
        timedValueType = (byte)TmNativeDefs.VfType.Status;
        break;
      case TmAnalog _:
        timedValueType = (byte)TmNativeDefs.VfType.AnalogFloat;
        break;
      case TmAccum _:
        timedValueType = (byte)TmNativeDefs.VfType.AccumFloat;
        break;
      default:
        return;
    }

    if (isSet)
    {
      timedValueType += (byte)TmNativeDefs.VfType.FlagSet;
    }
    else
    {
      timedValueType += (byte)TmNativeDefs.VfType.FlagClear;
    }

    var tvf = new TmNativeDefs.TTimedValueAndFlags
    {
      Vf =
      {
        Adr   = tmTag.TmAddr.ToAdrTm(),
        Type  = timedValueType,
        Flags = (byte)flags,
        Bits  = 0,
      },
      Xt =
      {
        Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
      }
    };
    await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
              .ConfigureAwait(false);

    // переключаем также флаги на резерве, если есть
    var resTmTag = await FindTmTagReserveTag(tmTag).ConfigureAwait(false);
    if (resTmTag != null)
    {
      tvf.Vf.Adr = resTmTag.TmAddr.ToAdrTm();
      await Task.Run(() => TmNative.tmcSetTimedValues(_cid, 1, new[] { tvf }))
                .ConfigureAwait(false);
    }


    // изменения флагов 1-4 для ТС записываются в журнал событий
    if (tmTag is TmStatus &&
        (flags.HasFlag(TmFlags.LevelA) ||
         flags.HasFlag(TmFlags.LevelB) ||
         flags.HasFlag(TmFlags.LevelC) ||
         flags.HasFlag(TmFlags.LevelD)))
    {
      var (ch, rtu, point) = tmTag.TmAddr.GetTuple();
      var evCh      = (byte)0xFF;
      var evCommand = (byte)(isSet ? 1 : 0);
      var ev = new TmNativeDefs.TEvent
      {
        Ch    = ch,
        Rtu   = rtu,
        Point = point,
        Id    = (ushort)TmNativeDefs.EventTypes.ManualStatusSet,
        Imp   = 0,
      };
      if (flags.HasFlag(TmFlags.LevelA))
      {
        ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Ch  = evCh,
          Rtu = 1,
          Cmd = evCommand,
        });
        await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
                  .ConfigureAwait(false);
      }

      if (flags.HasFlag(TmFlags.LevelB))
      {
        ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Ch  = evCh,
          Rtu = 2,
          Cmd = evCommand,
        });
        await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
                  .ConfigureAwait(false);
      }

      if (flags.HasFlag(TmFlags.LevelC))
      {
        ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Ch  = evCh,
          Rtu = 3,
          Cmd = evCommand,
        });
        await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
                  .ConfigureAwait(false);
      }

      if (flags.HasFlag(TmFlags.LevelD))
      {
        ev.Data = TmNativeUtil.GetBytes(new TmNativeDefs.ControlData
        {
          Ch  = evCh,
          Rtu = 4,
          Cmd = evCommand,
        });
        await Task.Run(() => TmNative.tmcRegEvent(_cid, ev))
                  .ConfigureAwait(false);
      }
    }
  }


  public async Task SetTagsFlags(IEnumerable<TmTag> tmTags,
                                 TmFlags            flags)
  {
    await ToggleTagsFlags(tmTags, flags, isSet: true).ConfigureAwait(false);
  }


  public async Task ClearTagsFlags(IEnumerable<TmTag> tmTags,
                                   TmFlags            flags)
  {
    await ToggleTagsFlags(tmTags, flags, isSet: false).ConfigureAwait(false);
  }


  private async Task ToggleTagsFlags(IEnumerable<TmTag> tmTags,
                                     TmFlags            flags,
                                     bool               isSet)
  {
    var timedValuesAndFlags = new List<TmNativeDefs.TTimedValueAndFlags>();

    foreach (var tmTag in tmTags)
    {
      byte timedValueType;
      switch (tmTag)
      {
        case TmStatus _:
          timedValueType = (byte)TmNativeDefs.VfType.Status;
          break;
        case TmAnalog _:
          timedValueType = (byte)TmNativeDefs.VfType.AnalogFloat;
          break;
        default:
          continue;
      }

      if (isSet)
      {
        timedValueType += (byte)TmNativeDefs.VfType.FlagSet;
      }
      else
      {
        timedValueType += (byte)TmNativeDefs.VfType.FlagClear;
      }

      timedValuesAndFlags.Add(new TmNativeDefs.TTimedValueAndFlags
      {
        Vf =
        {
          Adr   = tmTag.TmAddr.ToAdrTm(),
          Type  = timedValueType,
          Flags = (byte)flags,
          Bits  = 0,
        },
        Xt =
        {
          Flags = (ushort)TmNativeDefs.TMXTimeFlags.User,
        }
      });
    }

    await Task.Run(() => TmNative.tmcSetTimedValues(_cid,
                                                    (uint)timedValuesAndFlags.Count,
                                                    timedValuesAndFlags.ToArray()))
              .ConfigureAwait(false);
  }


  public async Task SetTagFlagsExplicitly(TmTag tag, TmFlags flags)
  {
    var (ch, rtu, point) = tag.TmAddr.GetTupleShort();

    switch (tag)
    {
      case TmStatus _:
        await Task.Run(() => TmNative.tmcSetStatusFlags(_cid, ch, rtu, point, (short)flags))
                  .ConfigureAwait(false);
        return;

      case TmAnalog _:
        await Task.Run(() => TmNative.tmcSetAnalogFlags(_cid, ch, rtu, point, (short)flags))
                  .ConfigureAwait(false);
        return;
    }
  }


  public async Task ClearTagFlagsExplicitly(TmTag tag, TmFlags flags)
  {
    var (ch, rtu, point) = tag.TmAddr.GetTupleShort();

    switch (tag)
    {
      case TmStatus _:
        await Task.Run(() => TmNative.tmcClrStatusFlags(_cid, ch, rtu, point, (short)flags))
                  .ConfigureAwait(false);
        return;

      case TmAnalog _:
        await Task.Run(() => TmNative.tmcClrAnalogFlags(_cid, ch, rtu, point, (short)flags))
                  .ConfigureAwait(false);
        return;
    }
  }


  public async Task<bool> AckTag(TmAddr addr)
  {
    switch (addr.Type)
    {
      case TmType.Status:
        return await AckStatus(new TmStatus(addr)).ConfigureAwait(false);
      case TmType.Analog:
        return await AckAnalog(new TmAnalog(addr)).ConfigureAwait(false);
      default:
        return false;
    }
  }


  public async Task AckAllStatuses()
  {
    await Task.Run(() => TmNative.tmcDriverCall(_cid,
                                                0,
                                                (short)TmNativeDefs.DriverCall.Acknowledge,
                                                0))
              .ConfigureAwait(false);
  }


  public async Task AckAllAnalogs()
  {
    await Task.Run(() => TmNative.tmcDriverCall(_cid,
                                                0,
                                                (short)TmNativeDefs.DriverCall.AckAnalog,
                                                0))
              .ConfigureAwait(false);
  }


  public async Task<bool> AckStatus(TmStatus status)
  {
    if (status == null) return false;

    var result = await Task.Run(() => TmNative.tmcDriverCall(_cid,
                                                             status.TmAddr.ToInteger(),
                                                             (short)TmNativeDefs.DriverCall.Acknowledge,
                                                             1))
                           .ConfigureAwait(false);
    return result == TmNativeDefs.Success;
  }


  public async Task<bool> AckAnalog(TmAnalog analog)
  {
    if (analog == null) return false;

    var result = await Task.Run(() => TmNative.tmcDriverCall(_cid,
                                                             analog.TmAddr.ToInteger(),
                                                             (short)TmNativeDefs.DriverCall.AckAnalog,
                                                             1))
                           .ConfigureAwait(false);
    return result == TmNativeDefs.Success;
  }
}