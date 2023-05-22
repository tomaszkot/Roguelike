using Roguelike.Attributes;
using Roguelike.Discussions;
using Roguelike.Tiles.Looting;
using System;
using System.Linq;

namespace Roguelike.Extensions
{
  public static class AttributesHelperExtension
  {
    public static string ToDescription(this Enum enumValue)
    {
      var valueString = enumValue.ToString();

      //if (valueString == EntityStatKind.MeleeAttack.ToString())
      //{
      //  return "Attack";
      //}
      var res = "";
      if (valueString == EntityStatKind.PhysicalProjectilesAttack.ToString())
      {
        res = "Projectile Attack";//was too long on eq. desc
      }
      else if (valueString == EntityStatKind.ElementalSpellProjectilesAttack.ToString())
      {
        res = "Elemental Attack";//was too long on eq. desc
      }
      else if (valueString == EntityStatKind.ThrowingTorchChanceToCauseFiring.ToString())
      {
        res = "Extra Chance To Cause Firing";//was too long on eq. desc
      }
      else if (valueString == KnownSentenceKind.WhatsUp.ToString())
        res = "What's up?";
      else if (valueString == KnownSentenceKind.SellHound.ToString())
        res = "Sell me a hound";
      else if (valueString == KnownSentenceKind.LetsTrade.ToString())
        res = "Let's trade";
      else if (valueString == FoodKind.NiesiolowskiSoup.ToString())
        res = "Niesiolowski Soup";

      else if (valueString.EndsWith("ExtraRange"))
        res = "Extra Range";
      else if (valueString.EndsWith("Duration"))
        res = "Duration";
      //else if (valueString == EntityStatKind.ChanceToRepeatElementalProjectileAttack.ToString())
      //{
      //  return "Chance To Repeat Projectile Attack";
      //}
      //else if (valueString == EntityStatKind.ChanceToEvadeElementalProjectileAttack.ToString())
      //{
      //  return "Chance To Evade ElementalProjectileAttack";
      //}
      if (res.Any())
      {
        res = res.Replace("Extra ", "");
        return res;
      }

      res = "";

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
      res = res.Replace("Extra", "");
      return res;
    }
  }
}
