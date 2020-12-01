using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Iface.Oik.Tm.Utils
{
  public static class EnumExtensions
  {
    public static string GetDescription(this Enum value)
    {
      return value.GetType()
                  .GetMember(value.ToString())
                  .FirstOrDefault()
                  ?.GetCustomAttribute<DescriptionAttribute>()
                  ?.Description
             ?? value.ToString();
    }
  }
}