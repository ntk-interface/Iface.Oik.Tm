using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Xunit;
using Xunit.Sdk;

namespace Iface.Oik.Tm.Test
{
  public class AutoNSubstituteDataAttribute : AutoDataAttribute
  {
    public AutoNSubstituteDataAttribute()
      : base(() => new Fixture().Customize(new AutoNSubstituteCustomization()))
    {
    }
  }


  public class InlineAutoNSubstituteDataAttribute : CompositeDataAttribute
  {
    public InlineAutoNSubstituteDataAttribute(params object[] values)
      : base(new InlineDataAttribute(values), new AutoNSubstituteDataAttribute())
    {
    }
  }


  public class UseCultureAttribute : BeforeAfterTestAttribute
  {
    private readonly Lazy<CultureInfo> _culture;
    private readonly Lazy<CultureInfo> _uiCulture;

    private CultureInfo _originalCulture;
    private CultureInfo _originalUiCulture;


    public UseCultureAttribute(string culture)
      : this(culture, culture)
    {
    }


    public UseCultureAttribute(string culture, string uiCulture)
    {
      this._culture   = new Lazy<CultureInfo>(() => new CultureInfo(culture,   false));
      this._uiCulture = new Lazy<CultureInfo>(() => new CultureInfo(uiCulture, false));
    }


    public CultureInfo Culture   => _culture.Value;
    public CultureInfo UICulture => _uiCulture.Value;


    public override void Before(MethodInfo methodUnderTest)
    {
      _originalCulture   = Thread.CurrentThread.CurrentCulture;
      _originalUiCulture = Thread.CurrentThread.CurrentUICulture;

      Thread.CurrentThread.CurrentCulture   = Culture;
      Thread.CurrentThread.CurrentUICulture = UICulture;

      CultureInfo.CurrentCulture.ClearCachedData();
      CultureInfo.CurrentUICulture.ClearCachedData();
    }


    public override void After(MethodInfo methodUnderTest)
    {
      Thread.CurrentThread.CurrentCulture   = _originalCulture;
      Thread.CurrentThread.CurrentUICulture = _originalUiCulture;

      CultureInfo.CurrentCulture.ClearCachedData();
      CultureInfo.CurrentUICulture.ClearCachedData();
    }
  }
}