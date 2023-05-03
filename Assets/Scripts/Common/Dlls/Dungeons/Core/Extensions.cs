using Dungeons.TileContainers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Dungeons.Core
{
  public static class Extensions
  {
    static Point InvalidPoint = GenerationConstraints.InvalidPoint;
    static Dictionary<double, double> distanceFrom = new Dictionary<double, double>();

    public static string GetCapitalized(this string val)
    {
      var result = "";
      if (val.Any())
      {
        var parts = val.Split(' ');
        foreach (var part in parts.Where(i => i.Any()))
        {
          if(result.Any())
            result += " ";
          var resPart = part.First().ToString().ToUpper() + part.Substring(1);
          result += resPart;
        }
      }

      return result;
    }

    public static double DistanceFrom(this Point point, Point other)
    {
      var dPowered = (Math.Pow(point.X - other.X, 2) + Math.Pow(point.Y - other.Y, 2));
      if (distanceFrom.ContainsKey(dPowered))
        return distanceFrom[dPowered];

      var res = Math.Sqrt(dPowered);
      distanceFrom[dPowered] = res;
      return res;
    }

    //public static double ToVector2D(this Vector3 point)
    //{
    //}

    public static double DistanceFrom(this Vector2D point, Vector2D other)
    {
      var dPowered = (Math.Pow(point.X - other.X, 2) + Math.Pow(point.Y - other.Y, 2));
      return Math.Sqrt(dPowered);
    }

    public static string ToUpperFirstLetter(this string source)
    {
      if (string.IsNullOrEmpty(source))
        return string.Empty;
      // convert to char array of the string
      char[] letters = source.ToCharArray();
      // upper case the first char
      letters[0] = char.ToUpper(letters[0]);
      // return the array made of the new char array
      return new string(letters);
    }

    public static bool IsValid(this Point pt)
    {
      return pt != InvalidPoint;
    }

    public static Point Invalid(this Point pt)
    {
      return InvalidPoint;
    }

    public static string FirstCharToUpper(string input)
    {
      switch (input)
      {
        case null: throw new ArgumentNullException(nameof(input));
        case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
        default: return input.First().ToString().ToUpper() + input.Substring(1);
      }
    }

    public static void Shuffle<T>(this List<T> list)
    {
      int n = list.Count;
      while (n > 1)
      {
        n--;
        int k = RandHelper.Random.Next(n + 1);
        T value = list[k];
        list[k] = list[n];
        list[n] = value;
      }
    }

    public static bool IsFromChildIsland(this Dungeons.Tiles.Tile tile)
    {
      return tile.DungeonNodeIndex <= DungeonNode.ChildIslandNodeIndex;
    }

    public static T GetRandomElem<T>(this List<T> list, T[] skip)
    {
      return RandHelper.GetRandomElem<T>(list, skip);
    }

    public static T GetRandomElem<T>(this List<T> list)
    {
      return RandHelper.GetRandomElem<T>(list, new T[] { });
    }

    public static T GetRandomElem<T>(this T[] arr)
    {
      return RandHelper.GetRandomElem<T>(arr);
    }

    public static void Raise<T>(this EventHandler<T> handler, object sender, T args)
    {
      if (handler != null)
        handler(sender, args);
    }

    public static void Raise(this EventHandler handler, object sender)
    {
      if (handler != null)
        handler(sender, EventArgs.Empty);
    }
        
  }
}
