﻿using System;
using System.Collections.Generic;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Native.Utils;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmStatus : TmTag
  {
    public static readonly string InvalidStatusCaption = "???";


    public enum ClassCaption
    {
      Off,
      On,
      Break,
      Malfunction,
      Intermediate,
      Flag1,
      Flag1Off,
      Flag1On,
      Flag2,
      Flag2Off,
      Flag2On,
      Flag3,
      Flag3Off,
      Flag3On,
      Flag4,
      Flag4Off,
      Flag4On,
      ClassName
    }


    private short     _status;
    private TmFlags   _flags = TmFlags.Invalid;
    private TmS2Flags _s2Flags;
    private short     _normalStatus = -1;
    private short     _importance;

    public short Status
    {
      get => _status;
      set => SetPropertyValueAndRefresh(ref _status, value);
    }

    public TmFlags Flags
    {
      get => _flags;
      set => SetPropertyValueAndRefresh(ref _flags, value);
    }

    public TmS2Flags S2Flags
    {
      get => _s2Flags;
      set => SetPropertyValueAndRefresh(ref _s2Flags, value);
    }

    public short NormalStatus
    {
      get => _normalStatus;
      set => SetPropertyValueAndRefresh(ref _normalStatus, value);
    }

    public short Importance
    {
      get => _importance;
      set => SetPropertyValueAndRefresh(ref _importance, value);
    }

    public bool IsNormalStatusOn      => NormalStatus == 1;
    public bool IsNormalStatusOff     => NormalStatus == 0;
    public bool IsNormalStatusUnknown => NormalStatus == -1;

    public bool IsAps             => Flags.HasFlag(TmFlags.StatusAps);
    public bool IsOn              => Status > 0;
    public bool IsOff             => Status == 0;
    public bool IsUnreliable      => Flags.HasFlag(TmFlags.Unreliable);
    public bool IsInvalid         => Flags.HasFlag(TmFlags.Invalid);
    public bool IsManuallyBlocked => Flags.HasFlag(TmFlags.ManuallyBlocked);
    public bool IsManuallySet     => Flags.HasFlag(TmFlags.ManuallySet);
    public bool IsIntermediate    => S2Flags.HasFlag(TmS2Flags.Intermediate);
    public bool IsBreak           => S2Flags.HasFlag(TmS2Flags.Break) && !IsIntermediate;
    public bool IsMalfunction     => S2Flags.HasFlag(TmS2Flags.Malfunction);
    public bool IsS2Failure       => IsBreak || IsMalfunction || IsIntermediate;
    public bool IsUnacked         => Flags.HasFlag(TmFlags.Unacked);
    public bool HasTelecontrol    => Flags.HasFlag(TmFlags.TmCtrlPresent);
    public bool IsRequested       => Flags.HasFlag(TmFlags.Requested);
    public bool IsResChannel      => Flags.HasFlag(TmFlags.ResChannel);
    public bool IsAbnormal        => Flags.HasFlag(TmFlags.Abnormal);
    public bool IsInverted        => Flags.HasFlag(TmFlags.Inverted);
    public bool IsFlag1           => Flags.HasFlag(TmFlags.LevelA);
    public bool IsFlag2           => Flags.HasFlag(TmFlags.LevelB);
    public bool IsFlag3           => Flags.HasFlag(TmFlags.LevelC);
    public bool IsFlag4           => Flags.HasFlag(TmFlags.LevelD);
    
    public string ClassName => GetClassCaption(ClassCaption.ClassName);

    public string CaptionOn          => GetClassCaption(ClassCaption.On);
    public string CaptionOff         => GetClassCaption(ClassCaption.Off);
    public string CaptionBreak       => GetClassCaption(ClassCaption.Break);
    public string CaptionMalfunction => GetClassCaption(ClassCaption.Malfunction);

    public string Flag1Name => GetClassCaption(ClassCaption.Flag1);
    public string Flag2Name => GetClassCaption(ClassCaption.Flag2);
    public string Flag3Name => GetClassCaption(ClassCaption.Flag3);
    public string Flag4Name => GetClassCaption(ClassCaption.Flag4);

    public string CaptionFlag1On => GetClassCaption(ClassCaption.Flag1On);
    public string CaptionFlag1Off => GetClassCaption(ClassCaption.Flag1Off);
    public string CaptionFlag2On  => GetClassCaption(ClassCaption.Flag2On);
    public string CaptionFlag2Off => GetClassCaption(ClassCaption.Flag2Off);
    public string CaptionFlag3On  => GetClassCaption(ClassCaption.Flag3On);
    public string CaptionFlag3Off => GetClassCaption(ClassCaption.Flag3Off);
    public string CaptionFlag4On  => GetClassCaption(ClassCaption.Flag4On);
    public string CaptionFlag4Off => GetClassCaption(ClassCaption.Flag4Off);

    public string Flag1Status =>
      IsFlag1 ? GetClassCaption(ClassCaption.Flag1On) : GetClassCaption(ClassCaption.Flag1Off);

    public string Flag2Status =>
      IsFlag2 ? GetClassCaption(ClassCaption.Flag2On) : GetClassCaption(ClassCaption.Flag2Off);

    public string Flag3Status =>
      IsFlag3 ? GetClassCaption(ClassCaption.Flag3On) : GetClassCaption(ClassCaption.Flag3Off);

    public string Flag4Status =>
      IsFlag4 ? GetClassCaption(ClassCaption.Flag4On) : GetClassCaption(ClassCaption.Flag4Off);

    public bool IsDrawoutUnderMaintenance => Flag1Name.ToLower()   == "положение_тележки" &&
                                             Flag1Status.ToLower() == "ремонтное";

    public bool IsDrawoutUnderCheckup => Flag2Name.ToLower()   == "положение_тележки" &&
                                         Flag2Status.ToLower() == "контрольное";


    public string StatusCaption
    {
      get
      {
        if (S2Flags.HasFlag(TmS2Flags.Intermediate))
        {
          return GetClassCaption(ClassCaption.Intermediate);
        }
        if (S2Flags.HasFlag(TmS2Flags.Break))
        {
          return GetClassCaption(ClassCaption.Break);
        }
        if (S2Flags.HasFlag(TmS2Flags.Malfunction))
        {
          return GetClassCaption(ClassCaption.Malfunction);
        }
        if (Status == 1)
        {
          return GetClassCaption(ClassCaption.On);
        }
        if (Status == 0)
        {
          return GetClassCaption(ClassCaption.Off);
        }
        return InvalidStatusCaption;
      }
    }


    public override string ValueToDisplay => StatusCaption;

    public override List<string> FlagsToDisplay
    {
      get
      {
        var flagsToDisplay = new List<string>();

        if (IsUnreliable)
        {
          flagsToDisplay.Add("Неактуальное значение (NT)");
        }
        if (IsInvalid)
        {
          flagsToDisplay.Add("Недействительное значение (IV)");
        }
        if (IsResChannel)
        {
          flagsToDisplay.Add("Взят с резерва");
        }
        if (IsRequested)
        {
          flagsToDisplay.Add("Идет опрос");
        }
        if (IsAbnormal)
        {
          flagsToDisplay.Add("Несоответствие нормальному режиму");
        }
        if (IsManuallyBlocked)
        {
          flagsToDisplay.Add("Заблокировано оператором");
        }
        if (IsManuallySet)
        {
          flagsToDisplay.Add("Установлено вручную");
        }
        if (IsFlag1)
        {
          flagsToDisplay.Add($"{Flag1Name}   {Flag1Status}");
        }
        if (IsFlag2)
        {
          flagsToDisplay.Add($"{Flag2Name}   {Flag2Status}");
        }
        if (IsFlag3)
        {
          flagsToDisplay.Add($"{Flag3Name}   {Flag3Status}");
        }
        if (IsFlag4)
        {
          flagsToDisplay.Add($"{Flag4Name}   {Flag4Status}");
        }

        return flagsToDisplay;
      }
    }


    public string FakeStateString(short fakeState)
    {
      return FakeStateString((int) fakeState);
    }


    public string FakeStateString(int fakeState)
    {
      return GetClassCaption(fakeState == 0 ? ClassCaption.Off : ClassCaption.On);
    }


    public TmStatus(int ch, int rtu, int point)
      : base(TmType.Status, ch, rtu, point)
    {
    }


    public TmStatus(TmAddr addr)
      : base(addr)
    {
    }


    public override int GetHashCode()
    {
      return base.GetHashCode();
    }


    public override bool Equals(object obj)
    {
      return Equals(obj as TmStatus);
    }


    public bool Equals(TmStatus comparison)
    {
      if (ReferenceEquals(comparison, null))
      {
        return false;
      }
      if (ReferenceEquals(this, comparison))
      {
        return true;
      }

      return TmAddr  == comparison.TmAddr &&
             Status  == comparison.Status &&
             Flags   == comparison.Flags  &&
             S2Flags == comparison.S2Flags;
    }


    public static bool operator ==(TmStatus left, TmStatus right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }
      return left.Equals(right);
    }


    public static bool operator !=(TmStatus left, TmStatus right)
    {
      return !(left == right);
    }


    public override string ToString()
    {
      return $"{Name} = {StatusCaption}";
    }


    public bool HasFlag(TmFlags flags)
    {
      return Flags.HasFlag(flags);
    }


    public string GetClassCaption(ClassCaption type)
    {
      switch (type)
      {
        case ClassCaption.Off:
          return GetClassDataValue("0Txt", (IsAps) ? "СНЯТ" : "ОТКЛ");

        case ClassCaption.On:
          return GetClassDataValue("1Txt", (IsAps) ? "ВЗВЕДЕН" : "ВКЛ");

        case ClassCaption.Break:
          return GetClassDataValue("BTxt", "00-Обрыв");

        case ClassCaption.Malfunction:
          return GetClassDataValue("MTxt", "11-Неисправность");

        case ClassCaption.Intermediate:
          return GetClassDataValue("ITxt", "Промежуточное");

        case ClassCaption.Flag1:
          return GetClassDataValue("F1Name");

        case ClassCaption.Flag2:
          return GetClassDataValue("F2Name");

        case ClassCaption.Flag3:
          return GetClassDataValue("F3Name");

        case ClassCaption.Flag4:
          return GetClassDataValue("F4Name");

        case ClassCaption.Flag1Off:
          return GetClassDataValue("F10Txt");

        case ClassCaption.Flag1On:
          return GetClassDataValue("F11Txt");

        case ClassCaption.Flag2Off:
          return GetClassDataValue("F20Txt");

        case ClassCaption.Flag2On:
          return GetClassDataValue("F21Txt");

        case ClassCaption.Flag3Off:
          return GetClassDataValue("F30Txt");

        case ClassCaption.Flag3On:
          return GetClassDataValue("F31Txt");

        case ClassCaption.Flag4Off:
          return GetClassDataValue("F40Txt");

        case ClassCaption.Flag4On:
          return GetClassDataValue("F41Txt");
        
        case ClassCaption.ClassName:
          return GetClassDataValue("ClassName");

        default:
          return "";
      }
    }


    private string GetClassDataValue(string key, string defaultValue = "")
    {
      if (ClassData != null && 
          ClassData.TryGetValue(key, out var caption))
      {
        return caption ?? defaultValue;
      }
      return defaultValue;
    }


    public override void SetTmcObjectProperties(string tmcObjectPropertiesString)
    {
      NormalStatus = -1;
      
      base.SetTmcObjectProperties(tmcObjectPropertiesString);
    }


    protected override void SetTmcObjectProperties(string key, string value)
    {
      base.SetTmcObjectProperties(key, value);
      
      switch (key)
      {
        case "Normal":
        {
          if (int.TryParse(value, out var normalStatus))
          {
            NormalStatus = (short) ((normalStatus == 0 || normalStatus == 1) ? normalStatus : -1);
          }

          break;
        }
        case "Importance":
        {
          if (int.TryParse(value, out var importance))
          {
            Importance = (short) importance;
          }

          break;
        }
        case "Class":
        {
          if (byte.TryParse(value, out var classId))
          {
            ClassId = classId;
          }

          break;
        }
      }
    }


    public void FromTmcCommonPoint(TmNativeDefs.TCommonPoint tmcCommonPoint)
    {
      TmNativeDefs.TStatusPoint tmcStatusPoint;
      try
      {
        tmcStatusPoint = TmNativeUtil.GetStatusPointFromCommonPoint(tmcCommonPoint);
      }
      catch (ArgumentException)
      {
        return;
      }

      IsInit  = (tmcCommonPoint.TM_Flags != 0xFFFF);
      Status  = tmcStatusPoint.Status;
      Flags   = (TmFlags) tmcStatusPoint.Flags;
      S2Flags = (TmS2Flags) tmcCommonPoint.tm_s2;
      ChangeTime = DateUtil.GetDateTimeFromTimestampWithEpochCheck(tmcCommonPoint.tm_local_ut,
                                                                   tmcCommonPoint.tm_local_ms);
    }


    public void FromTStatusPoint(TmNativeDefs.TStatusPoint tmcStatusPoint)
    {
      if (tmcStatusPoint.Flags == -1)
      {
        return;
      }
      
      IsInit  = true;
      Status  = (short) (tmcStatusPoint.Status & 0x0001); // на случай, если задана датаграмма ExtsShowS2
      S2Flags = (TmS2Flags) (tmcStatusPoint.Status >> 1); // на случай, если задана датаграмма ExtsShowS2
      Flags   = (TmFlags) tmcStatusPoint.Flags;
    }


    public void FromDatagram(byte[] buf)
    {
      IsInit  = true;
      Status  = (short)(buf[14] & 1);
      Flags   = (TmFlags)BitConverter.ToInt16(buf,   18);
      S2Flags = (TmS2Flags)BitConverter.ToUInt16(buf, 16);
    }


    public void UpdateWithDto(TmStatusDto dto)
    {
      if (dto == null) return;

      UpdateWithDto(dto.VCode,
                    dto.Flags,
                    dto.VS2,
                    dto.ChangeTime);
    }


    public void UpdateWithDto(short status, int flags, short s2Flags, DateTime? changeTime)
    {
      IsInit     = true;
      Status     = status;
      Flags      = (TmFlags) (flags & 0x0000_FFFF); // 3 и 4 байты флагов - служебные, не для клиента
      S2Flags    = (TmS2Flags) s2Flags;
      ChangeTime = changeTime.NullIfEpoch();
    }


    public void SetSqlPropertiesAndClassData(string name,
                                             short  importance,
                                             short  normalStatus,
                                             short  classId,
                                             string offCaption,
                                             string onCaption,
                                             string breakCaption,
                                             string malfunctionCaption)
    {
      Name         = name;
      Importance   = importance;
      NormalStatus = (short) ((normalStatus == 0 || normalStatus == 1) ? normalStatus : -1);

      if (classId < 0)
      {
        return;
      }
      ClassId = (byte) classId;
      ClassData = new Dictionary<string, string>
      {
        {"0Txt", offCaption},
        {"1Txt", onCaption},
        {"BTxt", breakCaption},
        {"MTxt", malfunctionCaption},
      };
    }


    public void UpdatePropertiesWithDto(TmStatusPropertiesDto dto)
    {
      if (dto?.Name == null) return;

      SetSqlPropertiesAndClassData(dto.Name,
                                   dto.VImportance,
                                   dto.VNormalState,
                                   dto.Provider,
                                   dto.ClassId,
                                   dto.ClText0,
                                   dto.ClText1,
                                   dto.ClBreakText,
                                   dto.ClMalfunText,
                                   dto.ClFlAName,
                                   dto.ClFlBName,
                                   dto.ClFlCName,
                                   dto.ClFlDName,
                                   dto.ClFlAText0,
                                   dto.ClFlAText1,
                                   dto.ClFlBText0,
                                   dto.ClFlBText1,
                                   dto.ClFlCText0,
                                   dto.ClFlCText1,
                                   dto.ClFlDText0,
                                   dto.ClFlDText1);
    }


    public void SetSqlPropertiesAndClassData(string name,
                                             short  importance,
                                             short  normalStatus,
                                             string provider,
                                             short  classId,
                                             string offCaption,
                                             string onCaption,
                                             string breakCaption,
                                             string malfunctionCaption,
                                             string flag1Name,
                                             string flag2Name,
                                             string flag3Name,
                                             string flag4Name,
                                             string flag1OffCaption,
                                             string flag1OnCaption,
                                             string flag2OffCaption,
                                             string flag2OnCaption,
                                             string flag3OffCaption,
                                             string flag3OnCaption,
                                             string flag4OffCaption,
                                             string flag4OnCaption)
    {
      Name         = name;
      Importance   = importance;
      NormalStatus = (short) ((normalStatus == 0 || normalStatus == 1) ? normalStatus : -1);

      HasTmProvider = !string.IsNullOrEmpty(provider);

      if (classId < 0)
      {
        return;
      }
      ClassId = (byte) classId;
      ClassData = new Dictionary<string, string>
      {
        {"0Txt", offCaption},
        {"1Txt", onCaption},
        {"BTxt", breakCaption},
        {"MTxt", malfunctionCaption},
        {"F1Name", flag1Name},
        {"F2Name", flag2Name},
        {"F3Name", flag3Name},
        {"F4Name", flag4Name},
        {"F10Txt", flag1OffCaption},
        {"F11Txt", flag1OnCaption},
        {"F20Txt", flag2OffCaption},
        {"F21Txt", flag2OnCaption},
        {"F30Txt", flag3OffCaption},
        {"F31Txt", flag3OnCaption},
        {"F40Txt", flag4OffCaption},
        {"F41Txt", flag4OnCaption},
      };
    }

    
    public string GetCustomFlagName(TmFlags flag)
    {
      switch (flag)
      {
        case TmFlags.LevelA: return Flag1Name;
        case TmFlags.LevelB: return Flag2Name;
        case TmFlags.LevelC: return Flag3Name;
        case TmFlags.LevelD: return Flag4Name;
        default:             return string.Empty;
      }
    }
    
    
    public string GetCustomFlagStatus(TmFlags flag)
    {
      switch (flag)
      {
        case TmFlags.LevelA: return Flag1Status;
        case TmFlags.LevelB: return Flag2Status;
        case TmFlags.LevelC: return Flag3Status;
        case TmFlags.LevelD: return Flag4Status;
        default:             return string.Empty;
      }
    }
    

    public static TmStatus CreateFromTmTreeDto(TmStatusTmTreeDto dto)
    {
      if (dto?.Name == null) return null;

      var status = new TmStatus(dto.Ch, dto.Rtu, dto.Point);
      status.UpdateWithTmTreeDto(dto);

      return status;
    }


    public void UpdateWithTmTreeDto(TmStatusTmTreeDto dto)
    {
      if (dto?.Name == null) return;

      UpdateWithDto(dto.MapToTmStatusDto());
      UpdatePropertiesWithDto(dto.MapToTmStatusPropertiesDto());
    }


    public static TmStatus CreateFromTmcCommonPointEx(TmNativeDefs.TCommonPoint tmcCommonPoint)
    {
      var tmStatus = new TmStatus(tmcCommonPoint.Ch, tmcCommonPoint.RTU, tmcCommonPoint.Point);
      
      tmStatus.FromTmcCommonPoint(tmcCommonPoint);
      tmStatus.Name = tmcCommonPoint.name;

      return tmStatus;
    }
  }
}