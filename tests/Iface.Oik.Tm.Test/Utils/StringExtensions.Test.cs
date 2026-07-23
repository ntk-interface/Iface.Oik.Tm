using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class StringExtensionsTest
  {
    public class RemoveMultipleWhitespacesMethod
    {
      [Fact]
      public void ReturnsNull_WhenInputIsNull()
      {
        string input = null;

        var result = input.RemoveMultipleWhitespaces();

        result.Should().BeNull();
      }


      [Fact]
      public void ReturnsEmpty_WhenInputIsEmpty()
      {
        var result = "".RemoveMultipleWhitespaces();

        result.Should().BeEmpty();
      }


      [Fact]
      public void ReturnsSameString_WhenNoMultipleSpaces()
      {
        var result = "abc def".RemoveMultipleWhitespaces();

        result.Should().Be("abc def");
      }


      [Fact]
      public void CollapsesMultipleSpaces()
      {
        var result = "abc   def   ghi".RemoveMultipleWhitespaces();

        result.Should().Be("abc def ghi");
      }


      [Fact]
      public void CollapsesMultipleTabs()
      {
        var result = "a\t\tb".RemoveMultipleWhitespaces();

        result.Should().Be("a\tb");
      }
    }
  }
}
