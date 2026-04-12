using System;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class Tob : TmNotifyPropertyChanged
  {
    public static readonly string          DefaultName          = "Элемент";
    public static readonly TmTopologyState DefaultTopologyState = TmTopologyState.Unknown;
    
    public uint   Scheme { get; }
    public ushort Type   { get; }
    public uint   Object { get; }

    private bool                       _isInit;
    private string                     _name;
    private TmTopologyState            _topologyState;

    public bool IsInit
    {
      get => _isInit;
      private set => SetPropertyValueAndRefresh(ref _isInit, value);
    }

    public string Name
    {
      get => _name;
      private set => SetPropertyValueAndRefresh(ref _name, value);
    }

    public TmTopologyState TopologyState
    {
      get => _topologyState;
      private set => SetPropertyValueAndRefresh(ref _topologyState, value);
    }

    public string NameOrDefault => !string.IsNullOrEmpty(Name)
      ? Name
      : Enum.IsDefined(typeof(ModusElement), Type)
        ? ((ModusElement)(Type)).GetDescription()
        : DefaultName;
    
    
    public Guid CimGuid => GuidUtil.EncodeCimEquipment((int)Scheme, (int)Object);


    public Tob(uint scheme, ushort type, uint obj, string name = null)
    {
      Scheme        = scheme;
      Type          = type;
      Object        = obj;
      Name          = name;
      TopologyState = DefaultTopologyState;
    }


    public Tob(int scheme, int type, int obj, string name = null)
      : this((uint)scheme, (ushort)type, (uint)obj, name)
    {
    }


    public void UpdateTopologyStatus(CimTopologyStatus status)
    {
      IsInit = true;
      if (status.HasFlag(CimTopologyStatus.IsGrounded))
      {
        TopologyState = TmTopologyState.IsGrounded;
      }
      else if (status.HasFlag(CimTopologyStatus.IsVoltaged))
      {
        TopologyState = TmTopologyState.IsVoltaged;
      }
      else
      {
        TopologyState = TmTopologyState.IsNotVoltaged;
      }
    }


    public void UpdateName(string name)
    {
      Name = !string.IsNullOrEmpty(name) ? name : null;
    }


    public static bool IsAddrEqual(Tob left, Tob right)
    {
      if (left == null || right == null) return false;

      return left.Scheme == right.Scheme &&
             left.Type   == right.Type   &&
             left.Object == right.Object;
    }


    public bool IsAddrEqual(int scheme, int type, int obj)
    {
      return Scheme == scheme &&
             Type   == type   &&
             Object == obj;
    }


    public static Tob Parse(string s)
    {
      if (!TryParse(s, out Tob tmTob))
      {
        throw new ArgumentException("Недопустимая строка TmTechObject");
      }

      return tmTob;
    }


    public static bool TryParse(string s, out Tob tmTob)
    {
      if (string.IsNullOrWhiteSpace(s))
      {
        tmTob = null;
        return false;
      }

      var addrParts = s.Trim().Split(':', ',', ' ');
      if (addrParts.Length != 3)
      {
        tmTob = null;
        return false;
      }
      try
      {
        var (scheme, type, obj) = (addrParts[0], addrParts[1], addrParts[2]);
        if (scheme.StartsWith("#TO"))
        {
          scheme = scheme.Remove(0, 3);
        }
        tmTob = new Tob(uint.Parse(scheme), ushort.Parse(type), uint.Parse(obj));
        return true;
      }
      catch (Exception)
      {
        tmTob = null;
        return false;
      }
    }


    public (uint, ushort, uint) GetTuple()
    {
      return (Scheme, Type, Object);
    }


    public (int, int, int) GetTupleInt()
    {
      return ((int)Scheme, Type, (int)Object);
    }


    public string ToAddrString()
    {
      return $"#TO{Scheme}:{Type}:{Object}";
    }


    public override string ToString()
    {
      return $"{ToAddrString()}, N={NameOrDefault}, Topo={TopologyState}, Cim={CimGuid}";
    }
  }
}