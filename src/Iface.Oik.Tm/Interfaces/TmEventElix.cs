using System;

namespace Iface.Oik.Tm.Interfaces
{
  public class TmEventElix : IComparable<TmEventElix>
  {
    public ulong R { get; }
    public ulong M { get; }


    public TmEventElix(ulong r, ulong m)
    {
      R = r;
      M = m;
    }


    public static TmEventElix CreateFromByteArray(byte[] bytes)
    {
      return new TmEventElix(BitConverter.ToUInt64(bytes, 0),
                             BitConverter.ToUInt64(bytes, 8));
    }


    public byte[] ToByteArray()
    {
      var bytes = new byte[16];
      BitConverter.GetBytes(R).CopyTo(bytes, 0);
      BitConverter.GetBytes(M).CopyTo(bytes, 8);
      return bytes;
    }


    public string ToStringByteArray()
    {
      return BitConverter.ToString(ToByteArray());
    }


    public override int GetHashCode()
    {
      return (R, M).GetHashCode();
    }


    public override bool Equals(object obj)
    {
      return Equals(obj as TmEventElix);
    }


    public bool Equals(TmEventElix comparison)
    {
      if (ReferenceEquals(comparison, null))
      {
        return false;
      }
      if (ReferenceEquals(this, comparison))
      {
        return true;
      }

      return R == comparison.R &&
             M == comparison.M;
    }


    public static bool operator ==(TmEventElix left, TmEventElix right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }
      return left.Equals(right);
    }


    public static bool operator !=(TmEventElix left, TmEventElix right)
    {
      return !(left == right);
    }


    public int CompareTo(TmEventElix other)
    {
      if (R == other.R &&
          M == other.M)
      {
        return 0;
      }
      if (R > other.R)
      {
        return 1;
      }
      if (R == other.R &&
          M > other.M)
      {
        return 1;
      }
      return -1;
    }


    public override string ToString()
    {
      return $"{M}.{R}";
    }
  }
}