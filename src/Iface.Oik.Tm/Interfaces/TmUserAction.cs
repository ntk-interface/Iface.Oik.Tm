using System;
using Iface.Oik.Tm.Dto;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmUserAction : TmNotifyPropertyChanged
  {
    public Guid Id { get; }

    public MqttKnownTopic       Action      { get; private set; }
    public TmUserActionCategory Category    { get; private set; }
    public string               TypeString  { get; private set; }
    public DateTime             Time        { get; private set; }
    public string               StateString { get; private set; }
    public int                  Importance  { get; private set; }
    public string               Text        { get; private set; }
    public string               Username    { get; private set; }

    public long?  FullTma   { get; private set; }
    public Guid?  ExtraId   { get; private set; }
    public int?   ExtraInt  { get; private set; }
    public string ExtraText { get; private set; }

    private int       _num;
    private DateTime? _ackTime;
    private string    _ackUser;

    public int Num
    {
      get => _num;
      set
      {
        _num = value;
        NotifyOfPropertyChange();
      }
    }

    public DateTime? AckTime
    {
      get => _ackTime;
      set
      {
        _ackTime = value;
        NotifyOfPropertyChange();
        NotifyOfPropertyChange(nameof(IsAcked));
      }
    }

    public string AckUser
    {
      get => _ackUser;
      set
      {
        _ackUser = value;
        NotifyOfPropertyChange();
      }
    }

    public bool IsAcked => AckTime != null; // TODO
    
    public TmType TmAddrType           { get; private set; }
    public uint   TmAddrComplexInteger { get; private set; } // sql: tma // one-based, 0=none
    public string TmAddrString         { get; private set; }
    
    public bool HasTmAddr   => TmAddrType != 0;
    public bool HasTmStatus => TmAddrType == TmType.Status;
    public bool HasTmAnalog => TmAddrType == TmType.Analog;


    protected TmUserAction(Guid id)
    {
      Id = id;
    }


    public static TmUserAction CreateFromDto(TmUserActionDto dto)
    {
      var action = new TmUserAction(dto.Id)
      {
        Action      = dto.Action,
        Category    = dto.Category,
        TypeString  = dto.Category.GetDescription(),
        Time        = dto.Time,
        StateString = dto.State,
        Importance  = dto.Importance,
        Text        = dto.Text,
        Username    = dto.Username,
        FullTma     = dto.Tma,
        ExtraId     = dto.ExtraId,
        ExtraInt    = dto.ExtraInt,
        ExtraText   = dto.ExtraText,
      };
      action.PrepareTmAddrValues();

      return action;
    }


    public static TmUserAction CreateFromMqttDto(MqttUserActionLogDto dto)
    {
      var action = new TmUserAction(dto.Id)
      {
        Action      = dto.Action,
        Category    = dto.Category,
        TypeString  = dto.Category.GetDescription(),
        Time        = dto.Time,
        StateString = dto.State,
        Importance  = dto.Importance,
        Text        = dto.Text,
        Username    = dto.Username,
        FullTma     = dto.Tma,
        ExtraId     = dto.ExtraId,
      };
      action.PrepareTmAddrValues();

      return action;
    }


    private void PrepareTmAddrValues()
    {
      if (FullTma == null)
      {
        TmAddrType           = TmType.Unknown;
        TmAddrComplexInteger = 0;
        TmAddrString         = null;
        return;
      }
      var tmAddr = TmAddr.CreateFromSqlFullTma(FullTma.Value);
      TmAddrType           = tmAddr.Type;
      TmAddrComplexInteger = tmAddr.ToComplexInteger();
      TmAddrString         = tmAddr.ToSqlTmaStr();
    }


    public override int GetHashCode()
    {
      return Id.GetHashCode();
    }
  }
}