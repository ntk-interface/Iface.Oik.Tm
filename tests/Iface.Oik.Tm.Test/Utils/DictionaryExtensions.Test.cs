using System.Collections.Generic;
using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class DictionaryExtensionsTest
  {
    public class DictionaryEqualsMethod
    {
      [Fact]
      public void ReturnsTrue()
      {
        var dict1 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k3", "v3"},
        };
        var dict2 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k3", "v3"},
        };

        var result = dict1.DictionaryEquals(dict2);

        result.Should().BeTrue();
      }


      [Fact]
      public void ReturnsTrueWhenEqualReference()
      {
        var dict1 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k3", "v3"},
        };
        var dict2 = dict1;

        var result = dict1.DictionaryEquals(dict2);

        result.Should().BeTrue();
      }


      [Fact]
      public void ReturnsTrueWhenOrderDiffers()
      {
        var dict2 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k3", "v3"},
        };
        var dict1 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k3", "v3"}, // нарушен порядок
          {"k2", "v2"},
        };

        var result = dict1.DictionaryEquals(dict2);

        result.Should().BeTrue();
      }


      [Fact]
      public void ReturnsFalseWhenNull()
      {
        var dict1 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k3", "v3"},
        };
        Dictionary<string, string> dict2 = null;

        var result = dict1.DictionaryEquals(dict2);

        result.Should().BeFalse();
      }


      [Fact]
      public void ReturnsFalseWhenValueDiffers()
      {
        var dict1 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k3", "v3"},
        };
        var dict2 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k3", "v4"}, // другое значение
        };

        var result = dict1.DictionaryEquals(dict2);

        result.Should().BeFalse();
      }


      [Fact]
      public void ReturnsFalseWhenKeyDiffers()
      {
        var dict1 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k3", "v3"},
        };
        var dict2 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k4", "v3"}, // другой ключ
        };

        var result = dict1.DictionaryEquals(dict2);

        result.Should().BeFalse();
      }


      [Fact]
      public void ReturnsFalseWhenFirstIsBigger()
      {
        var dict1 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k3", "v3"},
        };
        var dict2 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
        };

        var result = dict1.DictionaryEquals(dict2);

        result.Should().BeFalse();
      }


      [Fact]
      public void ReturnsFalseWhenSecondIsBigger()
      {
        var dict1 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
        };
        var dict2 = new Dictionary<string, string>
        {
          {"k1", "v1"},
          {"k2", "v2"},
          {"k3", "v3"},
        };

        var result = dict1.DictionaryEquals(dict2);

        result.Should().BeFalse();
      }
    }


    public class AddWithUniquePostfixIfNeededMethod
    {
      [Fact]
      public void AddsCorrectKeyValuePairs()
      {
        var dict = new Dictionary<string, string>();

        dict.AddWithUniquePostfixIfNeeded("a", "value1");
        dict.AddWithUniquePostfixIfNeeded("b", "value2");
        dict.AddWithUniquePostfixIfNeeded("c", "value3");
        dict.AddWithUniquePostfixIfNeeded("c", "value4");
        dict.AddWithUniquePostfixIfNeeded("c", "value5");

        dict.Should().Equal(new Dictionary<string, string>
        {
          {"a", "value1"}, 
          {"b", "value2"},
          {"c", "value3"},
          {"c_1", "value4"},
          {"c_2", "value5"},
        });
      }
    }
  }
}