namespace Iface.Oik.Tm.Utils
{
  public static class NumericUtil
  {
    public static float? NullIfMaxValue(float value)
    {
      if (value.Equals(float.MaxValue))
      {
        return null;
      }
      return value;
    }
  }
}