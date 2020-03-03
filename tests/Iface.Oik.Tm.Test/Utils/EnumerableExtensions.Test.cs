using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoFixture.Xunit2;
using FakeItEasy;
using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class EnumerableExtensionsTest
  {
    private class Dummy
    {
      public int X;
      public int Y;
    }


    private const int DummyCount = 3;


    public class ForEachMethod
    {
      [Theory, TmAutoData]
      public void SetsCorrectValues(int x, int y)
      {
        var dummies = A.CollectionOfDummy<Dummy>(DummyCount);
        var steps   = 0;

        dummies.ForEach(dummy =>
        {
          dummy.X = x;
          dummy.Y = y;
          steps++;
        });

        dummies.Should().AllBeEquivalentTo(new Dummy {X = x, Y = y});
        steps.Should().Be(DummyCount);
      }


      public class ForEachWithIndexMethod
      {
        [Theory, TmAutoData]
        public void SetsCorrectValues(int x, int y)
        {
          var dummies = A.CollectionOfDummy<Dummy>(DummyCount);
          var steps   = 0;

          dummies.ForEach((dummy, index) =>
          {
            dummy.X = x;
            dummy.Y = y;
            index.Should().Be(steps);
            steps++;
          });

          dummies.Should().AllBeEquivalentTo(new Dummy {X = x, Y = y});
          steps.Should().Be(DummyCount);
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

        array.IsNullOrEmpty().Should().BeTrue();
        collection.IsNullOrEmpty().Should().BeTrue();
        list.IsNullOrEmpty().Should().BeTrue();
        dictionary.IsNullOrEmpty().Should().BeTrue();
      }


      [Fact]
      public void ReturnsTrueForEmpty()
      {
        var array      = new int[] { };
        var collection = new Collection<int>();
        var list       = new List<int>();
        var dictionary = new Dictionary<int, int>();

        array.IsNullOrEmpty().Should().BeTrue();
        collection.IsNullOrEmpty().Should().BeTrue();
        list.IsNullOrEmpty().Should().BeTrue();
        dictionary.IsNullOrEmpty().Should().BeTrue();
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForNonEmpty(int dummy)
      {
        var array      = new int[] {dummy};
        var collection = new Collection<int> {dummy};
        var list       = new List<int> {dummy};
        var dictionary = new Dictionary<int, int> {{dummy, dummy}};

        array.IsNullOrEmpty().Should().BeFalse();
        collection.IsNullOrEmpty().Should().BeFalse();
        list.IsNullOrEmpty().Should().BeFalse();
        dictionary.IsNullOrEmpty().Should().BeFalse();
      }
    }
  }
}