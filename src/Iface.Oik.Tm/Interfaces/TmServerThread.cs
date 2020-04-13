using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmServerThread : TmNotifyPropertyChanged
  {
    private readonly int _hashCode;

    private int _upTime;
    private float _workTime;
    
    public int    Id       { get; private set; }
    public string Name     { get; private set; }
    public int    UpTime
    {
      get => _upTime;
      set
      {
        _upTime = value;
        NotifyOfPropertyChange();
        NotifyOfPropertyChange($"UpTimeString");
      }
    }

    public float WorkTime
    {
      get => _workTime;
      set
      {
        _workTime = value;
        NotifyOfPropertyChange();
        NotifyOfPropertyChange($"WorkTimeString");
      }
    }

    public string UpTimeString => $"{UpTime} s";
    public string WorkTimeString => $"{WorkTime:N3} s";

    public TmServerThread(int id, string name, int upTime, float workTime)
    {
      _hashCode = (id, name).ToTuple().GetHashCode();
      
      Id = id;
      Name = name;
      UpTime = upTime;
      WorkTime = workTime;
    }
    
    public override bool Equals(object obj)
    {
      return Equals(obj as TmServerThread);
    }


    public bool Equals(TmServerThread comparison)
    {
      const float epsilon = 0.0001f;
      
      if (ReferenceEquals(comparison, null))
      {
        return false;
      }

      if (ReferenceEquals(this, comparison))
      {
        return true;
      }

      return Name                                     == comparison.Name
          && Id                                       == comparison.Id
          && UpTime                                   == comparison.UpTime
          && Math.Abs(WorkTime - comparison.WorkTime) < epsilon;
    }


    public static bool operator ==(TmServerThread left, TmServerThread right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }

      return left.Equals(right);
    }

    public static bool operator !=(TmServerThread left, TmServerThread right)
    {
      return !(left == right);
    }

    public override int GetHashCode()
    {
      return _hashCode;
    }
  }
}