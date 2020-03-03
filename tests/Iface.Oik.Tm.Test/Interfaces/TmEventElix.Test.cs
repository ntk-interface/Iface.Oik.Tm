using FluentAssertions;
using Iface.Oik.Tm.Interfaces;
using Xunit;

namespace Iface.Oik.Tm.Test.Interfaces
{
  public class TmEventElixTest
  {
    public class Constructor
    {
      [Theory, TmAutoData]
      public void SetsCorrectValues(uint r, uint m)
      {
        var elix = new TmEventElix(r, m);

        elix.R.Should().Be(r);
        elix.M.Should().Be(m);
      }
    }


    public class CreateFromByteArrayMethod
    {
      [Theory]
      [InlineData(new byte[]
                  {
                    9, 0, 0, 0, 0, 0, 0, 0,
                    1, 1, 0, 0, 0, 0, 0, 0,
                  },
                  9, 257)]
      public void SetsCorrectValues(byte[] bytes, uint expectedR, uint expectedM)
      {
        var elix = TmEventElix.CreateFromByteArray(bytes);

        elix.R.Should().Be(expectedR);
        elix.M.Should().Be(expectedM);
      }
    }


    public class ToByteArrayMethod
    {
      [Theory]
      [InlineData(9, 257, new byte[]
      {
        9, 0, 0, 0, 0, 0, 0, 0,
        1, 1, 0, 0, 0, 0, 0, 0,
      })]
      public void ReturnsCorrectValues(uint r, uint m, byte[] expected)
      {
        var elix = new TmEventElix(r, m);

        var result = elix.ToByteArray();

        result.Should().Equal(expected);
      }
    }


    public class ToStringByteArrayMethod
    {
      [Theory]
      [InlineData(9, 257, "09-00-00-00-00-00-00-00-01-01-00-00-00-00-00-00")]
      public void ReturnsCorrectValues(uint r, uint m, string expected)
      {
        var elix = new TmEventElix(r, m);

        var result = elix.ToStringByteArray();

        result.Should().Be(expected);
      }
    }


    public class EqualsMethod
    {
      [Theory, TmAutoData]
      public void ReturnsTrue(uint r, uint m)
      {
        var elix1 = new TmEventElix(r, m);
        var elix2 = new TmEventElix(r, m);

        Assert.True(elix1.Equals(elix2));
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForDifferentR(uint r, uint m)
      {
        var elix1 = new TmEventElix(r,     m);
        var elix2 = new TmEventElix(r + 1, m);

        Assert.False(elix1.Equals(elix2));
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForDifferentM(uint r, uint m)
      {
        var elix1 = new TmEventElix(r, m);
        var elix2 = new TmEventElix(r, m + 1);

        Assert.False(elix1.Equals(elix2));
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForNull(uint r, uint m)
      {
        var elix1 = new TmEventElix(r, m);

        Assert.False(elix1.Equals(null));
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForWrongObject(uint r, uint m)
      {
        var elix1 = new TmEventElix(r, m);

        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.False(elix1.Equals("string, will not work"));
      }
    }


    public class EqualityOperator
    {
      [Theory, TmAutoData]
      public void ReturnsTrue(uint r, uint m)
      {
        var elix1 = new TmEventElix(r, m);
        var elix2 = new TmEventElix(r, m);

        Assert.True(elix1 == elix2);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForDifferentR(uint r, uint m)
      {
        var elix1 = new TmEventElix(r,     m);
        var elix2 = new TmEventElix(r + 1, m);

        Assert.False(elix1 == elix2);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForDifferentM(uint r, uint m)
      {
        var elix1 = new TmEventElix(r, m);
        var elix2 = new TmEventElix(r, m + 1);

        Assert.False(elix1 == elix2);
      }


      [Theory, TmAutoData]
      public void ReturnsFalseForNull(uint r, uint m)
      {
        var elix1 = new TmEventElix(r, m);

        Assert.False(elix1 == null);
      }


      [Theory, TmAutoData]
      public void ReturnsTrueForNullWhenNull(uint r, uint m)
      {
        TmEventElix elix1 = null;

        Assert.True(elix1 == null);
      }
    }


    public class InequalityOperator
    {
      [Theory, TmAutoData]
      public void ReturnsTrueForDifferentR(uint r, uint m)
      {
        var elix1 = new TmEventElix(r,     m);
        var elix2 = new TmEventElix(r + 1, m);

        Assert.True(elix1 != elix2);
      }


      [Theory, TmAutoData]
      public void ReturnsTrueForDifferentM(uint r, uint m)
      {
        var elix1 = new TmEventElix(r, m);
        var elix2 = new TmEventElix(r, m + 1);

        Assert.True(elix1 != elix2);
      }


      [Theory, TmAutoData]
      public void ReturnsFalse(uint r, uint m)
      {
        var elix1 = new TmEventElix(r, m);
        var elix2 = new TmEventElix(r, m);

        Assert.False(elix1 != elix2);
      }


      [Theory, TmAutoData]
      public void ReturnsTrueForNull(uint r, uint m)
      {
        var elix1 = new TmEventElix(r, m);

        Assert.True(elix1 != null);
      }
    }


    public class ToStringMethod
    {
      [Theory, TmAutoData]
      public void ReturnsCorrectValues(uint r, uint m)
      {
        var elix = new TmEventElix(r, m);

        var result = elix.ToString();

        result.Should().Be($"{m}.{r}");
      }
    }
  }
}