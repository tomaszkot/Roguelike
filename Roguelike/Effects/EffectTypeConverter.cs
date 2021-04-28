using Roguelike.Attributes;

namespace Roguelike.Effects
{
  class EffectTypeConverter
  {
    public static EffectType Convert(EntityStatKind esk)
    {
      switch (esk)
      {
        case EntityStatKind.Unset:
          break;
        case EntityStatKind.Strength:
          break;
        case EntityStatKind.Health:
          break;
        case EntityStatKind.Magic:
          break;
        case EntityStatKind.Defense:
          break;
        case EntityStatKind.Dexterity:
          break;
        case EntityStatKind.ResistFire:
          break;
        case EntityStatKind.ResistCold:
          break;
        case EntityStatKind.ResistPoison:
          break;
        case EntityStatKind.ChanceToHit:
          break;
        case EntityStatKind.ChanceToCastSpell:
          break;
        case EntityStatKind.Mana:
          break;
        case EntityStatKind.Attack:
          break;
        case EntityStatKind.FireAttack:
          return EffectType.Firing;
        case EntityStatKind.ColdAttack:
          return EffectType.Frozen;
        case EntityStatKind.PoisonAttack:
          return EffectType.Poisoned;
        case EntityStatKind.LightPower:
          break;
        case EntityStatKind.LifeStealing:
          break;
        case EntityStatKind.ManaStealing:
          break;
        case EntityStatKind.ChanceToCauseBleeding:
          break;
        case EntityStatKind.ChanceToCauseStunning:
          break;
        case EntityStatKind.ChanceToCauseTearApart:
          break;
        case EntityStatKind.ChanceToEvadeMeleeAttack:
          break;
        case EntityStatKind.ChanceToEvadeMagicAttack:
          break;
        case EntityStatKind.MeleeAttackDamageReduction:
          break;
        case EntityStatKind.MagicAttackDamageReduction:
          break;
        case EntityStatKind.AxeExtraDamage:
          break;
        case EntityStatKind.SwordExtraDamage:
          break;
        case EntityStatKind.BashingExtraDamage:
          break;
        case EntityStatKind.DaggerExtraDamage:
          break;
        case EntityStatKind.LightingAttack:
          break;
        case EntityStatKind.ResistLighting:
          break;
        case EntityStatKind.ChanceToStrikeBack:
          break;
        case EntityStatKind.ChanceToBulkAttack:
          break;
        default:
          break;
      }

      return EffectType.Unset;
    }

    public static EntityStatKind Convert(EffectType et)
    {
      EntityStatKind esk = EntityStatKind.Unset;
      if (et == EffectType.Bleeding)
        esk = EntityStatKind.Health;

      else if (et == EffectType.Inaccuracy)
        esk = EntityStatKind.ChanceToHit;
      else if (et == EffectType.Poisoned || et == EffectType.Frozen || et == EffectType.Firing)
        esk = EntityStatKind.Health;

      return esk;
    }
  }
}
