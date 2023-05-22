using Dungeons.Core;
using Roguelike.Spells;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roguelike.Spells
{
  public class SpellStateSet
  {
    Dictionary<SpellKind, SpellState> spellStates = new Dictionary<SpellKind, SpellState>();

    public SpellStateSet()
    {
      var spells = EnumHelper.GetEnumValues<SpellKind>(true);
      foreach (var sp in spells)
      {
        spellStates[sp] = new SpellState()
        {
          Kind = sp,
          MaxLevel = GetMaxLevel(sp)
        };
      }
    }

    private int GetMaxLevel(SpellKind sp)
    {
      int max = 10;
      switch (sp)
      {
        case SpellKind.Unset:
          break;
        case SpellKind.FireBall:
          break;
        case SpellKind.CrackedStone:
          max = 5;
          break;
        case SpellKind.Skeleton:
          break;
        case SpellKind.Trap:
          break;
        case SpellKind.IceBall:
          break;
        case SpellKind.PoisonBall:
          break;
        case SpellKind.Transform:
          max = 5;
          break;
        case SpellKind.Frighten:
          max = 5;
          break;
        case SpellKind.Healing:
          break;
        case SpellKind.ManaShield:
          break;
        case SpellKind.Telekinesis:
          break;
        case SpellKind.StonedBall:
          break;
        case SpellKind.LightingBall:
          break;
        case SpellKind.Mana:
          break;
        case SpellKind.BushTrap:
          break;
        case SpellKind.Weaken:
          break;
        case SpellKind.NESWFireBall:
          break;
        case SpellKind.Teleport:
          max = 5;
          break;
        case SpellKind.IronSkin:
          break;
        case SpellKind.ResistAll:
          break;
        case SpellKind.Inaccuracy:
          break;
        case SpellKind.Identify:
          break;
        case SpellKind.Portal:
          break;
        case SpellKind.Dziewanna:
          break;
        case SpellKind.Swarog:
          break;
        case SpellKind.Swiatowit:
          break;
        case SpellKind.FireStone:
          break;
        case SpellKind.SwapPosition:
          max = 5;
          break;
        case SpellKind.Perun:
          break;
        default:
          break;
      }

      return max;
    }

    public SpellState GetState(SpellKind kind)
    {
      return spellStates[kind];
    }
  }
}
