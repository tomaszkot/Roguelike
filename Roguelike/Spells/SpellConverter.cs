using Roguelike.Effects;

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
          break;
        case SpellKind.CrackedStone:
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
          return EffectType.Transform;
        case SpellKind.Frighten:
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
        case SpellKind.Rage:
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
