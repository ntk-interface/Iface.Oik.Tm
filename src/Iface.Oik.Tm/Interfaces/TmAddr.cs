using System;
using Iface.Oik.Tm.Native.Interfaces;
using Iface.Oik.Tm.Utils;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmAddr : IComparable<TmAddr>
  {
    private uint   _addr; // normalized value (all parts is zero-based)
    private TmType _type;

    public ushort Ch
    {
      get => (ushort) ((_addr & 0xFF_00_00_00) >> 24);
      set
      {
        if (value > 254)
        {
          throw new ArgumentException("Недопустимый номер канала в TmAddr");
        }

        _addr = (uint) ((_addr & 0x00_FF_FF_FF) | (value << 24));
      }
    }

    public ushort Rtu
    {
      get => (ushort) (1 + (_addr >> 16) & 0xFF);
      set
      {
        if (value > 255)
        {
          throw new ArgumentException("Недопустимый номер КП в TmAddr");
        }

        _addr = (uint) ((_addr & 0xFF_00_FF_FF) | ((value - 1) << 16));
      }
    }

    public ushort Point
    {
      get => (ushort) (1 + _addr & 0xFFFF);
      set
      {
        if (value > 65535)
        {
          throw new ArgumentException("Недопустимый номер объекта в TmAddr");
        }

        _addr = (uint) ((_addr & 0xFF_FF_00_00) | (value - 1));
      }
    }

    public TmType Type
    {
      get => _type;
      set => _type = value;
    }

    // constructor for json
    public TmAddr()
    {
    }


    public TmAddr(TmType type, uint addr)
    {
      _type = type;
      _addr = addr;
    }


    public TmAddr(uint addr)
      : this(TmType.Unknown, addr)
    {
    }


    public TmAddr(TmType type, ushort ch, ushort rtu, ushort point)
    {
      if (ch > 254 || rtu > 255 || point > 65535 || rtu < 1 || point < 1)
      {
        throw new ArgumentException("Недопустимые составляющие адреса TmAddr");
      }

      _type = type;
      _addr = (uint) (ch        << 24 |
                      (rtu - 1) << 16 |
                      point - 1);
    }


    public TmAddr(TmType type, int ch, int rtu, int point)
      : this(type, (ushort) ch, (ushort) rtu, (ushort) point)
    {
    }


    public TmAddr(ushort ch, ushort rtu, ushort point)
      : this(TmType.Unknown, ch, rtu, point)
    {
    }


    public TmAddr(int ch, int rtu, int point)
      : this(TmType.Unknown, (ushort) ch, (ushort) rtu, (ushort) point)
    {
    }


    public static TmAddr Create(TmType type, ushort ch, ushort rtu, ushort point)
    {
      return new TmAddr(type, ch, rtu, point);
    }


    public static TmAddr Create(ushort ch, ushort rtu, ushort point)
    {
      return new TmAddr(ch, rtu, point);
    }


    public static TmAddr Create(TmType type, uint value)
    {
      return new TmAddr(type, value);
    }


    public static TmAddr Create(uint value)
    {
      return new TmAddr(value);
    }


    public static TmAddr Create(TmType type, string ch, string rtu, string point)
    {
      if (!ushort.TryParse(ch,    out var intCh)  ||
          !ushort.TryParse(rtu,   out var intRtu) ||
          !ushort.TryParse(point, out var intPoint))
      {
        return null;
      }
      try
      {
        return new TmAddr(type, intCh, intRtu, intPoint);
      }
      catch (Exception)
      {
        return null;
      }
    }


    public static TmAddr Create(string ch, string rtu, string point)
    {
      if (!ushort.TryParse(ch,    out var intCh)  ||
          !ushort.TryParse(rtu,   out var intRtu) ||
          !ushort.TryParse(point, out var intPoint))
      {
        return null;
      }
      try
      {
        return new TmAddr(intCh, intRtu, intPoint);
      }
      catch (Exception)
      {
        return null;
      }
    }


    public static TmAddr CreateFromNormalized(TmType type, uint value)
    {
      return new TmAddr(type,
        (ushort)((value & 0xFF_00_00_00) >> 24),
        (ushort)((value & 0x00_FF_00_00) >> 16),
        (ushort)(value & 0x00_00_FF_FF));
    }


    public static TmAddr CreateFromSqlTmaAndTmaType(ushort tmaType, int value)
    {
      return CreateFromSqlTma(((TmNativeDefs.TmDataTypes) tmaType).ToTmType(), value);
    }

    
    public static TmAddr CreateFromSqlTma(TmType type, int value)
    {
      return CreateFromNormalized(type, (uint) value);
    }


    public static TmAddr CreateFromSqlTma(int value)
    {
      return CreateFromNormalized(TmType.Unknown, (uint) value);
    }


    public static TmAddr CreateFromNoPadding(TmType type, uint value)
    {
      return CreateFromNormalized(type, value);
    }


    public static TmAddr CreateFromNoPadding(uint value)
    {
      return CreateFromNormalized(TmType.Unknown, value);
    }



    public bool Equals(int ch, int rtu, int point)
    {
      return (Ch, Rtu, Point) == (ch, rtu, point);
    }


    public override bool Equals(object obj)
    {
      return Equals(obj as TmAddr);
    }


    public bool Equals(TmAddr comparison)
    {
      if (ReferenceEquals(comparison, null))
      {
        return false;
      }
      if (ReferenceEquals(this, comparison))
      {
        return true;
      }

      return (_addr, _type) == (comparison._addr, comparison._type);
    }


    public static bool operator ==(TmAddr left, TmAddr right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }
      return left.Equals(right);
    }


    public static bool operator !=(TmAddr left, TmAddr right)
    {
      return !(left == right);
    }


    public override int GetHashCode()
    {
      return (Ch, Rtu, Point, Type).GetHashCode();
    }


    public int CompareTo(TmAddr other)
    {
      if (_addr > other._addr)
      {
        return 1;
      }
      if (_addr < other._addr)
      {
        return -1;
      }
      return 0;
    }


    public override string ToString()
    {
      string type;
      switch (_type)
      {
        case TmType.Status:
          type = "#TC";
          break;

        case TmType.Analog:
          type = "#TT";
          break;

        case TmType.Accum:
          type = "#TI";
          break;

        default:
          type = "#??";
          break;
      }

      return $"{type}{Ch}:{Rtu}:{Point}";
    }


    public static TmAddr Parse(string s, TmType type = TmType.Unknown)
    {
      if (!TryParse(s, out TmAddr tmAddr, type))
      {
        throw new ArgumentException("Недопустимая строка TmAddr");
      }

      return tmAddr;
    }


    public static bool TryParse(string s, out TmAddr tmAddr, TmType type = TmType.Unknown)
    {
      if (string.IsNullOrWhiteSpace(s))
      {
        tmAddr = null;
        return false;
      }

      var addrParts = s.Trim().Split(':', ',', ' ');
      if (addrParts.Length == 4) // в SQL возвращается в формате #TC:20:1:1
      {
        addrParts = new[] {addrParts[0] + addrParts[1], addrParts[2], addrParts[3]};
      }
      if (addrParts.Length != 3)
      {
        tmAddr = null;
        return false;
      }

      try
      {
        var (ch, rtu, point) = (addrParts[0], addrParts[1], addrParts[2]);
        if (ch.StartsWith("#TC"))
        {
          type = TmType.Status; // TODO подумать стоит ли заменять тип, если он уже указан в параметре
          ch   = ch.Remove(0, 3);
        }
        else if (ch.StartsWith("#TT"))
        {
          type = TmType.Analog;
          ch   = ch.Remove(0, 3);
        }
        else if (ch.StartsWith("#TI"))
        {
          type = TmType.Accum;
          ch   = ch.Remove(0, 3);
        }

        tmAddr = new TmAddr(type, ushort.Parse(ch), ushort.Parse(rtu), ushort.Parse(point));
        return true;
      }
      catch (Exception)
      {
        tmAddr = null;
        return false;
      }
    }

    public static bool TryParseType(string s, out TmType type, TmType defaultType = TmType.Unknown)
    {
      type = defaultType;
      if (string.IsNullOrWhiteSpace(s))
      {
        return false;
      }
      s = s.Trim();
      try
      {
        if (s.StartsWith("#TC"))
        {
          type = TmType.Status;
          return true;
        }
        if (s.StartsWith("#TT"))
        {
          type = TmType.Analog;
          return true;
        }
        if (s.StartsWith("#TI"))
        {
          type = TmType.Accum;
          return true;
        }
        return false;
      }
      catch (Exception)
      {
        return false;
      }
    }


    public uint ToInteger()
    {
      return _addr;
    }

    public uint ToComplexInteger()
    {
      return EncodeComplexInteger(Ch, Rtu, Point);
    }

    public uint ToIntegerWithoutPadding()
    {
      return EncodeComplexInteger(Ch, Rtu, Point);
    }


    public TmNativeDefs.TAdrTm ToAdrTm()
    {
      return new TmNativeDefs.TAdrTm
      {
        Ch    = (short) Ch,
        RTU   = (short) Rtu,
        Point = (short) Point,
      };
    }


    public short GetSqlTmaType()
    {
      return (short) Type.ToNativeType();
    }


    public int ToSqlTma()
    {
      return (int) ToIntegerWithoutPadding();
    }


    public long ToSqlFullTma()
    {
      return ((long) Type.ToNativeType() << 32) + ToSqlTma();
    }


    public string ToSqlTmaStr()
    {
      var str = ToString();
      return str.Insert(3, ":"); // в SQL вместо #TC0:1:1 нужно значение #TC:0:1:1, вставляем двоеточие
    }


    public (ushort, ushort, ushort) GetTuple()
    {
      return (Ch, Rtu, Point);
    }


    public (short, short, short) GetTupleShort()
    {
      return ((short) Ch, (short) Rtu, (short) Point);
    }


    public static uint EncodeComplexInteger(int ch, int rtu, int point)
    {
      return (uint) (point + (rtu << 16) + (ch << 24));
    }


    public static uint EncodeNormalizedInteger(int ch, int rtu, int point)
    {
      return (uint) ((point - 1) + ((rtu - 1) << 16) + (ch << 24));
    }
    

    public static bool DecodeComplexInteger(uint addr, out ushort ch, out ushort rtu, out ushort point)
    {
      if (addr == 0)
      {
        ch = rtu = point = 0;
        return false;
      }
      ch    = (ushort)((addr & 0xFF_00_00_00) >> 24);
      rtu   = (ushort)((addr >> 16) & 0xFF);
      point = (ushort)(addr         & 0xFFFF);
      return ch < 255 && rtu <= 255 && point <= 65535;
    }

    
    public static void DecodeNormalizedInteger(uint addr, out ushort ch, out ushort rtu, out ushort point)
    {
      ch    = (ushort)((addr & 0xFF_00_00_00) >> 24);
      rtu   = (ushort)(1 + (addr >> 16) & 0xFF);
      point = (ushort)(1 + addr         & 0xFFFF);
    }
  }
}