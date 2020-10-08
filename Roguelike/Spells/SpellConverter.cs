using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Spells
{
  public class SpellConverter
  {
    public static SpellKind SpellKindFromEffectType(EffectType et)
    {
      SpellKind spellKind = SpellKind.Unset;
      switch (et)
      {
        case EffectType.Bleeding:
          break;
        case EffectType.Poisoned:
          break;
        case EffectType.Frozen:
          break;
        case EffectType.Firing:
          break;
        case EffectType.Transform:
          break;
        case EffectType.TornApart:
          break;
        case EffectType.Frighten:
          break;
        case EffectType.Stunned:
          break;
        case EffectType.ManaShield:
          break;
        case EffectType.BushTrap:
          break;
        case EffectType.Rage:
          spellKind = SpellKind.Rage;
          break;
        case EffectType.Weaken:
          spellKind = SpellKind.Weaken;
          break;
        case EffectType.IronSkin:
          spellKind = SpellKind.IronSkin;
          break;
        case EffectType.ResistAll:
          spellKind = SpellKind.ResistAll;
          break;
        case EffectType.Inaccuracy:
          spellKind = SpellKind.Inaccuracy;
          break;
        case EffectType.Hooch:
          break;
        case EffectType.ConsumedRawFood:
          break;
        case EffectType.ConsumedRoastedFood:
          break;
        default:
          break;
      }

      return spellKind;
    }
  }
}
