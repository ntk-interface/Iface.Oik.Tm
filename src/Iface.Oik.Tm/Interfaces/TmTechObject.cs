using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmTechObject : TmNotifyPropertyChanged
  {
    public static readonly string          DefaultName          = "Элемент";
    public static readonly TmTopologyState DefaultTopologyState = TmTopologyState.Unknown;

    public static readonly string PropertyPlacard           = "^pl1";
    public static readonly char   PropertyPlacardSplitter   = '|';
    public static readonly string PropertyGroundIsPermitted = "^plG";
    public static readonly string PropertyTmStatus          = "#TC";


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
      private set
      {
        _isInit = value;
        Refresh();
      }
    }

    public Dictionary<string, string> Properties
    {
      get => _properties;
      private set
      {
        _properties = value;
        Refresh();
      }
    }

    public string Name
    {
      get => _name;
      private set
      {
        _name = value;
        Refresh();
      }
    }

    public TmTopologyState TopologyState
    {
      get => _topologyState;
      private set
      {
        _topologyState = value;
        Refresh();
      }
    }

    public object Reference { get; set; } // ссылка на связанный объект, например для схемы - выключатель, прибор и т.п.


    public TmTechObject(uint scheme, ushort type, uint obj, string name = null)
    {
      Scheme        = scheme;
      Type          = type;
      Object        = obj;
      Properties    = new Dictionary<string, string>();
      Name          = name ?? DefaultName;
      TopologyState = DefaultTopologyState;
    }


    public TmTechObject(int scheme, int type, int obj, string name = null)
      : this((uint) scheme, (ushort) type, (uint) obj, name)
    {
    }


    public string GetPropertyOrDefault(string property)
    {
      if (property == null) return null;

      return Properties.TryGetValue(property, out var value) ? value : null;
    }


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
      Name          = GetNameFromProperties();
      TopologyState = GetTopologyStateFromProperties();

      return true;
    }


    private string GetNameFromProperties()
    {
      var name = GetPropertyOrDefault("n");

      return (!string.IsNullOrEmpty(name)) ? name : DefaultName;
    }


    private TmTopologyState GetTopologyStateFromProperties()
    {
      var voltagedProperty = GetPropertyOrDefault("$V");
      var groundedProperty = GetPropertyOrDefault("$G");

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


    public TmAddr GetTmStatusAddr()
    {
      return TmAddr.TryParse(GetPropertyOrDefault(PropertyTmStatus), out var tmAddr)
        ? tmAddr
        : null;
    }


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
      position = (TmPlacardPosition) positionInt;
      return true;
    }


    public TmNativeDefs.TTechObj ToNativeTechObj()
    {
      return new TmNativeDefs.TTechObj
      {
        Scheme = Scheme,
        Type   = Type,
        Object = Object,
      };
    }


    public static bool IsAddrEqual(TmTechObject left, TmTechObject right)
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


    public static TmTechObject Parse(string s)
    {
      if (!TryParse(s, out TmTechObject tmTob))
      {
        throw new ArgumentException("Недопустимая строка TmTechObject");
      }

      return tmTob;
    }


    public static bool TryParse(string s, out TmTechObject tmTob)
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
        tmTob = new TmTechObject(uint.Parse(scheme), ushort.Parse(type), uint.Parse(obj));
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
      return ((int) Scheme, Type, (int) Object);
    }


    public override string ToString()
    {
      return $"{Scheme}:{Type}:{Object} {string.Join(" | ", Properties.Select(p => $"{p.Key}={p.Value}"))}";
    }
  }
}