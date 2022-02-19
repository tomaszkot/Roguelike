using Roguelike.Attributes;
using Roguelike.Discussions;
using System;

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

      if (valueString == EntityStatKind.PhysicalProjectilesAttack.ToString())
      {
        return "Projectile Attack";//was too long on eq. desc
      }
      else if (valueString == EntityStatKind.ElementalSpellProjectilesAttack.ToString())
      {
        return "Elemental Attack";//was too long on eq. desc
      }

      if (valueString == KnownSentenceKind.WhatsUp.ToString())
        return "What's up?";
      else if (valueString == KnownSentenceKind.SellHound.ToString())
        return "Sell me a hound";
      else if (valueString == KnownSentenceKind.LetsTrade.ToString())
        return "Let's trade";
      //else if (valueString == EntityStatKind.ChanceToRepeatElementalProjectileAttack.ToString())
      //{
      //  return "Chance To Repeat Projectile Attack";
      //}
      //else if (valueString == EntityStatKind.ChanceToEvadeElementalProjectileAttack.ToString())
      //{
      //  return "Chance To Evade ElementalProjectileAttack";
      //}

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
