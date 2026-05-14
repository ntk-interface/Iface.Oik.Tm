using System.Text.RegularExpressions;

namespace Iface.Oik.Tm.Utils
{
  public static class StringExtensions
  {
    public static string RemoveMultipleWhitespaces(this string str)
    {
      return Regex.Replace(str, @"(\s)\s+", "$1");
    }
  }
}