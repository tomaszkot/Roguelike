using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;

namespace Roguelike.Effects
{
  public static class EffectTypeConverter
  {
    public static EffectType GetEffectType(this ProjectileFightItem pfi)
    {
      switch (pfi.FightItemKind)
      {
        case FightItemKind.Unset:
          break;
        case FightItemKind.ExplosiveCocktail:
        case FightItemKind.ThrowingTorch:
          return EffectType.Firing;
          
        case FightItemKind.ThrowingKnife:
        case FightItemKind.HunterTrap:
          return EffectType.Bleeding;
        case FightItemKind.Stone:
          break;
        
        case FightItemKind.PlainArrow:
          break;
        case FightItemKind.IronArrow:
          break;
        case FightItemKind.SteelArrow:
          break;
        case FightItemKind.PlainBolt:
          break;
        case FightItemKind.IronBolt:
          break;
        case FightItemKind.SteelBolt:
          break;
        case FightItemKind.PoisonArrow:
        case FightItemKind.PoisonBolt:
        case FightItemKind.PoisonCocktail:
          return EffectType.Poisoned;
        case FightItemKind.IceArrow:
        case FightItemKind.IceBolt:
          return EffectType.Frozen;
        case FightItemKind.FireArrow:
        case FightItemKind.FireBolt:
          return EffectType.Firing;
        case FightItemKind.WeightedNet:
          break;
        case FightItemKind.CannonBall:
          break;
        case FightItemKind.Smoke:
          break;
        default:
          break;
      }

      return EffectType.Unset;
    }

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
        case EntityStatKind.ChanceToMeleeHit:
          break;
        case EntityStatKind.ChanceToCastSpell:
          break;
        case EntityStatKind.Mana:
          break;
        case EntityStatKind.MeleeAttack:
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
        case EntityStatKind.ChanceToEvadeElementalProjectileAttack:
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
        esk = EntityStatKind.ChanceToMeleeHit;//TODO Crossbow, Bow?
      else if (et == EffectType.Poisoned || et == EffectType.Frozen || et == EffectType.Firing)
        esk = EntityStatKind.Health;

      return esk;
    }
  }
}
