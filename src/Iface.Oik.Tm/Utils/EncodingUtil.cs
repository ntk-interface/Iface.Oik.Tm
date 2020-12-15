using System.Text;

namespace Iface.Oik.Tm.Utils
{
  public static class EncodingUtil
  {
    public static string Utf8ToWin1251(string src)
    {
      if (src == null) return null;

      var utf8    = Encoding.UTF8;
      var win1251 = Encoding.GetEncoding(1251);
      return win1251.GetString(Encoding.Convert(utf8, win1251, utf8.GetBytes(src)));
    }


    public static string Win1251ToUtf8(string src)
    {
      if (src == null) return null;
      
      var win1251 = Encoding.GetEncoding(1251);
      return Win1251BytesToUtf8(win1251.GetBytes(src));
    }
    

    public static string Win1251BytesToUtf8(byte[] src)
    {
      if (src == null) return null;
      
      var utf8   = Encoding.UTF8;
      var cp1251 = Encoding.GetEncoding(1251);
      return utf8.GetString(Encoding.Convert(cp1251, utf8, src)).Trim('\0');
    }
    
    
    public static string Cp866ToUtf8(string src)
    {
      if (src == null) return null;
      
      var cp866 = Encoding.GetEncoding(866);
      return Cp866BytesToUtf8String(cp866.GetBytes(src));
    }


    public static string Cp866BytesToUtf8String(byte[] src)
    {
      if (src == null) return null;

      var utf8  = Encoding.UTF8;
      var cp866 = Encoding.GetEncoding(866);
      return utf8.GetString(Encoding.Convert(cp866, utf8, src)).Trim('\0');
    }
    
  }
}