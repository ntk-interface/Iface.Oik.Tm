using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using AutoFixture.Xunit2;
using Xunit;
using Xunit.Sdk;

namespace Iface.Oik.Tm.Test
{
  // Обычный генератор AutoFixture подставляет значения int от 0 до 255
  // Канал ТМ-адреса ограничен 254, что может вызвать ошибку
  // Поэтому переопределяем генератор
  public class TmAutoDataAttribute : AutoDataAttribute
  {
    public static Fixture TmRandomNumberFixture
    {
      get
      {
        var f = new Fixture();
        f.Customizations.Add(new RandomNumericSequenceGenerator(1, 254));
        return f;
      }
    }
    
    
    public TmAutoDataAttribute()
      : base(() => TmRandomNumberFixture)
    {
    }
  }


  public class TmInlineAutoDataAttribute : CompositeDataAttribute
  {
    public TmInlineAutoDataAttribute(params object[] values)
    : base(new InlineDataAttribute(values), new TmAutoDataAttribute())
    {
      
    }
  }
  
  
  public class TmAutoFakeItEasyDataAttribute : AutoDataAttribute
  {
    public TmAutoFakeItEasyDataAttribute()
      : base(() => TmAutoDataAttribute.TmRandomNumberFixture.Customize(new AutoFakeItEasyCustomization()))
    {
    }
  }


  public class TmInlineAutoFakeItEasyDataAttribute : CompositeDataAttribute
  {
    public TmInlineAutoFakeItEasyDataAttribute(params object[] values)
      : base(new InlineDataAttribute(values), new TmAutoFakeItEasyDataAttribute())
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