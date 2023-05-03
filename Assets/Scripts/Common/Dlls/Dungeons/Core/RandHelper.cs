using System;
using System.Collections.Generic;
using System.Linq;

namespace Dungeons.Core
{
  public class RandHelper
  {
    private static Random random = new Random();

    public static int GetRandomInt(int max)
    {
      return random.Next(max);
    }

    public static double GetRandomDouble()
    {
      return random.NextDouble();
    }

    public static float GetRandomFloat()
    {
      return (float)random.NextDouble();
    }
    public static float GetRandomFloatInRange(float min, float max)
    {
      return RandHelper.GetRandomFloat() * max + min;
    }



    public static Random Random
    {
      get
      {
        return random;
      }
    }

    public static T GetRandomElem<T>(List<T> list)
    {
      if (!list.Any())
        return default(T);

      int val = random.Next(list.Count);
      return list[val];
    }

    public static T GetRandomElem<T>(T[] arr)
    {
      int val = random.Next(arr.Length);
      return arr[val];
    }

    public static T GetRandomElem<T>(List<T> list, T[] skip)
    {
      var set = list.Where(i => !skip.Contains(i)).ToList();
      if (set.Any())
      {
        int val = random.Next(set.Count);
        return set[val];
      }
      return default(T);
    }

    public static T GetRandomElem<T>(List<T> list, Func<T, bool> filter) where T : class
    {
      var set = list.Where(i => filter(i)).ToList();
      if (set.Any())
      {
        int val = random.Next(set.Count);
        return set[val];
      }
      return null;
    }

    public static T GetRandomEnumValue<T>(T[] skip)
    {
      var values = Enum.GetValues(typeof(T)).Cast<T>().Where(i => !skip.Contains(i)).ToList();

      int index = random.Next(values.Count());

      return values[index];
    }


    public static T GetRandomEnumValue<T>() where T :
      struct, IComparable, IFormattable, IConvertible
    {
      return GetRandomEnumValue<T>(true);
    }

    
    public static T GetRandomEnumValue<T>(bool skipUnset) where T :
      struct, IComparable, IFormattable, IConvertible
    {
      var values = EnumHelper.Values<T>(skipUnset);
      int index = random.Next(values.Count());

      return values.ElementAt(index);
    }

    public static T GetRandomEnumValueFromList<T>(T[] possibleOnes) where T : IConvertible
    {
      var values = Enum.GetValues(typeof(T)).Cast<T>().Where(i => possibleOnes.Contains(i)).ToList();
      int index = random.Next(values.Count());

      return values[index];
    }


    public static void DoThresholdAction(float minThreshold, Action ac)
    {
      var rand = Random.NextDouble();
      if (rand > minThreshold)
      {
        ac();
      }
    }
  }
}
