using System;
using System.Collections.Generic;
using System.Text;

namespace Roguelike.Core.Extensions
{
  internal static class EnumExtensions
  {
    public static bool IsGod(this Roguelike.Spells.SpellKind sk)
    {
      if (sk == Roguelike.Spells.SpellKind.Dziewanna ||
           sk == Roguelike.Spells.SpellKind.Swarog ||
           sk == Roguelike.Spells.SpellKind.Swiatowit ||
           sk == Roguelike.Spells.SpellKind.Perun
           )
        return true;
      return false;
    }
  }
}
