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
        ExtraId     = dto.ExtraId,
        ExtraInt    = dto.ExtraInt,
        ExtraText   = dto.ExtraText,
      };

      return action;
    }


    public override int GetHashCode()
    {
      return Id.GetHashCode();
    }
  }
}