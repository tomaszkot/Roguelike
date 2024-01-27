using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Spells;

namespace Roguelike
{
  public static class Converters
  {
    public static EntityStatKind ToEntityStatKind(this SpellKind spellKind)
    {
      return EntityStatKindFromSpellKind(spellKind);
    }

    public static EntityStatKind EntityStatKindFromSpellKind(SpellKind spellKind)
    {
      EntityStatKind esk = EntityStatKind.Unset;
      switch (spellKind)
      {
        case SpellKind.FireBall:
        case SpellKind.NESWFireBall:
          esk = EntityStatKind.FireAttack;
          break;

        case SpellKind.IceBall:
          esk = EntityStatKind.ColdAttack;
          break;
        case SpellKind.PoisonBall:
          esk = EntityStatKind.PoisonAttack;
          break;
        case SpellKind.LightingBall:
          esk = EntityStatKind.LightingAttack;
          break;

        default:
          break;
      }

      return esk;
    }
  }
}


namespace Roguelike.Spells
{
  public class SpellConverter
  {
    
  

    public static EffectType EffectTypeFromSpellKind(SpellKind sk)
    {
      switch (sk)
      {
        case SpellKind.Unset:
          break;
        case SpellKind.FireBall:
          return EffectType.Firing;
          
        case SpellKind.CrackedStone:
          break;
        case SpellKind.Skeleton:
          break;
        //case SpellKind.Trap:
        //  break;
        case SpellKind.IceBall:
          return EffectType.Frozen;
          
        case SpellKind.PoisonBall:
          return EffectType.Poisoned;
        case SpellKind.Transform:
          return EffectType.Transform;
        case SpellKind.Frighten:
          return EffectType.Frighten;
        case SpellKind.Healing:
          break;
        case SpellKind.ManaShield:
          return EffectType.ManaShield;
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
        default:
          break;
      }

      return EffectType.Unset;
    }

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
          spellKind = SpellKind.FireBall;
          break;
        case EffectType.Transform:
          spellKind = SpellKind.Transform;
          break;
        case EffectType.TornApart:
          break;
        case EffectType.Frighten:
          spellKind = SpellKind.Frighten;
          break;
        case EffectType.Stunned:
          break;
        case EffectType.ManaShield:
          spellKind = SpellKind.ManaShield;
          break;
        case EffectType.BushTrap:
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
