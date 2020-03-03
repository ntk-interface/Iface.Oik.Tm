using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoFixture.Xunit2;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class EnumerableExtensions
  {
    private struct Dummy
    {
      public int X;
      public int Y;
    }


    private const int DummyCount = 3;


    private static IEnumerable<Dummy> GetDummyList()
    {
      for (var i = 0; i < DummyCount; i++)
      {
        yield return new Dummy();
      }
    }


    public class ForEachMethod
    {
      [Theory, TmAutoData]
      public void SetsCorrectValues(int expectedX, int expectedY)
      {
        var steps = 0;

        GetDummyList().ForEach(dummy =>
        {
          dummy.X = expectedX;
          dummy.Y = expectedY;
          Assert.Equal(expectedX, dummy.X);
          Assert.Equal(expectedY, dummy.Y);
          steps++;
        });

        Assert.Equal(DummyCount, steps);
      }


      public class ForEachWithIndexMethod
      {
        [Theory, TmAutoData]
        public void SetsCorrectValues(int expectedX, int expectedY)
        {
          var steps = 0;

          GetDummyList().ForEach((dummy, index) =>
          {
            dummy.X = expectedX;
            dummy.Y = expectedY;
            Assert.Equal(expectedX, dummy.X);
            Assert.Equal(expectedY, dummy.Y);
            Assert.Equal(index,     steps);
            steps++;
          });

          Assert.Equal(DummyCount, steps);
        }
      }
    }


    public class IsNullOrEmptyMethod
    {
      [Fact]
      public void ReturnsTrueForNull()
      {
        int[]                array      = null;
        Collection<int>      collection = null;
        List<int>            list       = null;
        Dictionary<int, int> dictionary = null;

        Assert.True(array.IsNullOrEmpty());
        Assert.True(collection.IsNullOrEmpty());
        Assert.True(list.IsNullOrEmpty());
        Assert.True(dictionary.IsNullOrEmpty());
      }


      [Fact]
      public void ReturnsTrueForEmpty()
      {
        var array      = new int[] { };
        var collection = new Collection<int>();
        var list       = new List<int>();
        var dictionary = new Dictionary<int, int>();

        Assert.True(array.IsNullOrEmpty());
        Assert.True(collection.IsNullOrEmpty());
        Assert.True(list.IsNullOrEmpty());
        Assert.True(dictionary.IsNullOrEmpty());
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForNonEmpty(int dummy)
      {
        var array      = new int[] { dummy };
        var collection = new Collection<int> {dummy};
        var list       = new List<int> {dummy};
        var dictionary = new Dictionary<int, int> {{dummy, dummy}};

        Assert.False(array.IsNullOrEmpty());
        Assert.False(collection.IsNullOrEmpty());
        Assert.False(list.IsNullOrEmpty());
        Assert.False(dictionary.IsNullOrEmpty());
      }
    }
  }
}