using System.Collections.ObjectModel;
using FluentAssertions;
using Iface.Oik.Tm.Utils;
using Xunit;

namespace Iface.Oik.Tm.Test.Utils
{
  public class ObservableCollectionExtensionsTest
  {
    public class AddRangeMethod
    {
      [Fact]
      public void AddsItems_ToEmptyCollection()
      {
        var collection = new ObservableCollection<int>();

        collection.AddRange(new[] { 1, 2, 3 });

        collection.Should().Equal(1, 2, 3);
      }


      [Fact]
      public void AppendsItems_ToNonEmptyCollection()
      {
        var collection = new ObservableCollection<int> { 0 };

        collection.AddRange(new[] { 1, 2 });

        collection.Should().Equal(0, 1, 2);
      }
    }
  }
}
