using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.Core
{
  public class EnumHelper
  {
    
    public static List<T> GetEnumValues<T>(bool skipUnset) where T : IConvertible
    {
      var values = Enum.GetValues(typeof(T)).Cast<T>().ToList();
      if (skipUnset)
      {
        values.RemoveAll(i => i.ToString() == "Unset");
      }
      return values;
    }

    public static IEnumerable<TEnum> Values<TEnum>(bool skipUnset)
    where TEnum : struct, IComparable, IFormattable, IConvertible
    {
      return GetEnumValues<TEnum>(skipUnset);
    }
  }
}