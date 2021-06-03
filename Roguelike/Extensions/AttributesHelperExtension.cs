using Roguelike.Discussions;
using System;

namespace Roguelike.Extensions
{
  public static class AttributesHelperExtension
  {
    public static string ToDescription(this Enum enumValue)
    {
      var valueString = enumValue.ToString();
      if (valueString == KnownSentenceKind.WhatsUp.ToString())
        return "What's up?";
      else if (valueString == KnownSentenceKind.SellHound.ToString())
        return "Sell me a hound";

      var res = "";

      int counter = 0;
      foreach (var ch in valueString)
      {
        if (char.IsUpper(ch) && counter > 0)
        {
          res += " ";
        }
        res += ch;

        counter++;
      }

      return res;
    }
  }
}
