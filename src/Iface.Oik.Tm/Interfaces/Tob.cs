using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class Tob : TmNotifyPropertyChanged
  {
    public static readonly string          DefaultName          = "Элемент";
    public static readonly TmTopologyState DefaultTopologyState = TmTopologyState.Unknown;


    [Obsolete] public static readonly string PropertyName              = "n";
    [Obsolete] public static readonly string PropertyIsVoltaged        = "$V";
    [Obsolete] public static readonly string PropertyIsGrounded        = "$G";
    [Obsolete] public static readonly string PropertyPlacard           = "^pl1";
    [Obsolete] public static readonly char   PropertyPlacardSplitter   = '|';
    [Obsolete] public static readonly string PropertyGroundIsPermitted = "^plG";
    [Obsolete] public static readonly string PropertyTmStatus          = "#TC";


    public uint   Scheme { get; }
    public ushort Type   { get; }
    public uint   Object { get; }

    private bool                       _isInit;
    private Dictionary<string, string> _properties;
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

    public object Reference { get; set; } // ссылка на связанный объект, например для схемы - выключатель, прибор и т.п.

    public string NameOrDefault => !string.IsNullOrEmpty(Name)
      ? Name
      : Enum.IsDefined(typeof(ModusElement), Type)
        ? ((ModusElement)(Type)).GetDescription()
        : DefaultName;

    [Obsolete]
    public Dictionary<string, string> Properties
    {
      get => _properties;
      private set => SetPropertyValueAndRefresh(ref _properties, value);
    }


    [Obsolete]
    public bool? IsVoltaged
    {
      get
      {
        var voltagedProperty = GetPropertyOrDefault(PropertyIsVoltaged);
        if (voltagedProperty == null)
        {
          return null;
        }
        return (voltagedProperty == "1");
      }
    }


    [Obsolete]
    public bool? IsGrounded
    {
      get
      {
        var groundedProperty = GetPropertyOrDefault(PropertyIsGrounded);
        if (groundedProperty == null)
        {
          return null;
        }
        return (groundedProperty == "1");
      }
    }


    public Guid CimGuid => GuidUtil.EncodeCimEquipment((int)Scheme, (int)Object);


    public Tob(uint scheme, ushort type, uint obj, string name = null)
    {
      Scheme        = scheme;
      Type          = type;
      Object        = obj;
      Properties    = new Dictionary<string, string>();
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


    [Obsolete]
    public string GetPropertyOrDefault(string property)
    {
      if (property == null) return null;

      return Properties.TryGetValue(property, out var value) ? value : null;
    }


    [Obsolete]
    public bool SetPropertiesFromTmc(IEnumerable<string> tmcProperties)
    {
      if (tmcProperties == null) return false;

      var newProperties = tmcProperties.Select(prop => prop.Split('='))
                                       .Where(kvp => kvp.Length == 2)
                                       .ToDictionary(kvp => kvp[0],
                                                     kvp => kvp[1]);
      if (Properties.DictionaryEquals(newProperties))
      {
        return false;
      }

      IsInit        = true;
      Properties    = newProperties;
      Name          = GetPropertyOrDefault(PropertyName);
      TopologyState = GetTopologyStateFromProperties();

      return true;
    }


    [Obsolete]
    private TmTopologyState GetTopologyStateFromProperties()
    {
      var voltagedProperty = GetPropertyOrDefault(PropertyIsVoltaged);
      var groundedProperty = GetPropertyOrDefault(PropertyIsGrounded);

      if (voltagedProperty == null ||
          groundedProperty == null)
      {
        return DefaultTopologyState;
      }
      if (groundedProperty == "1")
      {
        return TmTopologyState.IsGrounded;
      }
      if (voltagedProperty == "1")
      {
        return TmTopologyState.IsVoltaged;
      }
      return TmTopologyState.IsNotVoltaged;
    }


    [Obsolete]
    public TmAddr GetTmStatusAddr()
    {
      return TmAddr.TryParse(GetPropertyOrDefault(PropertyTmStatus), out var tmAddr)
        ? tmAddr
        : null;
    }


    [Obsolete]
    public bool TryParsePermittedGround(out TmPlacardPosition position, out float scale)
    {
      position = TmPlacardPosition.Center;
      scale    = 0;

      var permittedGroundInfo = GetPropertyOrDefault(PropertyGroundIsPermitted);
      if (string.IsNullOrWhiteSpace(permittedGroundInfo))
      {
        return false;
      }
      var permittedGroundParts = permittedGroundInfo.Split(',');
      if (permittedGroundParts.Length < 3                             ||
          !int.TryParse(permittedGroundParts[1], out var positionInt) ||
          !float.TryParse(permittedGroundParts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out scale))
      {
        return false;
      }
      position = (TmPlacardPosition)positionInt;
      return true;
    }


    [Obsolete]
    public TmNativeDefs.TTechObj ToNativeTechObj()
    {
      return new TmNativeDefs.TTechObj
      {
        Scheme = Scheme,
        Type   = Type,
        Object = Object,
      };
    }
  }
}