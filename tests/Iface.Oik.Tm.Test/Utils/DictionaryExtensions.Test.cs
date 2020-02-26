using System.Collections.Generic;
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
        
        Assert.True(result);
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
        
        Assert.True(result);
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
        
        Assert.True(result);
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
        
        Assert.False(result);
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
        
        Assert.False(result);
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
        
        Assert.False(result);
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
        
        Assert.False(result);
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
        
        Assert.False(result);
      }
    }
  }
}