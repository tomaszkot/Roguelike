using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Extensions;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;

namespace Roguelike.Abilities
{
  public enum PassiveAbilityKind
  {
    Unset, RestoreHealth, RestoreMana, LootingMastering,

    AxesMastering, BashingMastering, DaggersMastering, SwordsMastering,
    StrikeBack, BulkAttack,
    BowsMastering, CrossBowsMastering

    //Traps, RemoveClaws, RemoveTusk, Skinning, , ,
    //HuntingMastering /*<-(to del)*/

    //,Scroll//user must invest in each scroll indywidually

  }

  /// <summary>
  /// PassiveAbility works automatically
  /// </summary>
  public class PassiveAbility : Ability
  {
    PassiveAbilityKind kind;
    public bool BeginTurnApply;
    
    public override string ToString()
    {
      return base.ToString() + Kind;
    }
        
    public PassiveAbilityKind Kind
    {
      get { return kind; }
      set
      {
        kind = value;
        SetName(Kind.ToDescription());
        SetPrimaryStatDescription();
        EntityStatKind psk = EntityStatKind.Unset;
        EntityStatKind ask = EntityStatKind.Unset;
        switch (kind)
        {
          case PassiveAbilityKind.RestoreHealth:
            psk = EntityStatKind.Health;
            this.BeginTurnApply = true;
            break;
          case PassiveAbilityKind.RestoreMana:
            this.BeginTurnApply = true;
            psk = EntityStatKind.Mana;
            break;
          case PassiveAbilityKind.AxesMastering:
            psk = EntityStatKind.ChanceToCauseTearApart;
            ask = EntityStatKind.AxeExtraDamage;
            break;
          case PassiveAbilityKind.BashingMastering:
            psk = EntityStatKind.ChanceToCauseStunning;
            ask = EntityStatKind.BashingExtraDamage;
            break;
          case PassiveAbilityKind.DaggersMastering:
            psk = EntityStatKind.ChanceToCauseBleeding;
            ask = EntityStatKind.DaggerExtraDamage;
            break;
          case PassiveAbilityKind.SwordsMastering:
            psk = EntityStatKind.ChanceToHit;
            ask = EntityStatKind.SwordExtraDamage;
            break;
                     
          case PassiveAbilityKind.LootingMastering:
          case PassiveAbilityKind.StrikeBack:
          case PassiveAbilityKind.BulkAttack:
            PageIndex = 1;
            if (kind == PassiveAbilityKind.StrikeBack)
            {
              psk = EntityStatKind.ChanceToStrikeBack;

            }
            else if (kind == PassiveAbilityKind.BulkAttack)
            {
              psk = EntityStatKind.ChanceToBulkAttack;

            }
            break;
          default:
            break;
        }
        PrimaryStat.SetKind(psk);
        AuxStat.SetKind(ask);
      }
    }

    public float GetFactor(bool primary)
    {
      return primary ? PrimaryStat.Factor : AuxStat.Factor;
    }
        
    EntityStatKind GetEffectiveStatKind(bool primary)
    {
      return primary ? PrimaryStat.Kind : AuxStat.Kind;
    }
        
    public override bool IsPercentageFromKind()
    {
      return IsPercentageFromKind(Kind);
    }

    public override float CalcFactor(bool primary, int level)
    {
      PassiveAbilityKind kind = Kind;

      float factor = 0;
      switch (kind)
      {
        case PassiveAbilityKind.RestoreHealth:
        case PassiveAbilityKind.RestoreMana:
          try
          {
            var mults = new float[] { 0, .3f, .75f, 1f, 1.5f, 2.5f };
            factor = mults[level];
          }
          catch (Exception)
          {
            throw;
          }
          break;
        case PassiveAbilityKind.AxesMastering:
        case PassiveAbilityKind.BashingMastering:
        case PassiveAbilityKind.DaggersMastering:
        case PassiveAbilityKind.SwordsMastering:

          if (primary)
            factor = level;
          else
            factor = level * 5;
          break;

        case PassiveAbilityKind.StrikeBack:
          var multsDefSB = new int[] { 0, 2, 4, 7, 10, 15 };
          factor = multsDefSB[level];
          break;
        case PassiveAbilityKind.BulkAttack:
          var multsDefSB1 = new int[] { 0, 4, 7, 10, 15, 20 };
          factor = multsDefSB1[level];
          break;
       
        default:
          break;
      }
      return factor;
    }
        
    private float GetFightItemFactor(bool primary, int level, FightItemKind kind)
    {
      //HACK GameManager.Instance.Hero.Abilities
      //return GameManager.Instance.Hero.Abilities.GetFightItem(kind).GetFactor(primary, level);
      return 0;
    }
                
    public void SetPrimaryStatDescription()
    {
      var desc = "";
      switch (kind)
      {
        case PassiveAbilityKind.RestoreHealth:
          desc = "Restores health at turn's begining";
          break;
        case PassiveAbilityKind.RestoreMana:
          desc = "Restores mana at turn's begining";
          break;
        case PassiveAbilityKind.AxesMastering:
        case PassiveAbilityKind.BashingMastering:
        case PassiveAbilityKind.DaggersMastering:
        case PassiveAbilityKind.SwordsMastering:
          desc = "Bonus when using ";
          var wpn = kind.ToString().Replace("Mastering", "");
          if (wpn.EndsWith("s"))
            wpn = wpn.TrimEnd("s".ToCharArray());
          desc += wpn + " weapon";
          break;
        case PassiveAbilityKind.LootingMastering:
          desc = "Bonus to loot experience";// frequency and quality";
          break;
        //case AbilityKind.HuntingMastering:
        //  desc = "Bonus to Hunting (Traps)";
        //  break;
        case PassiveAbilityKind.StrikeBack:
          desc = "Chance to strike back when being hit";
          break;
        case PassiveAbilityKind.BulkAttack:
          desc = "Chance to strike all sourronding enemies in one turn";
          break;
        default:
          break;
      }
      primaryStatDescription = desc;
    }
        
    public override bool useCustomStatDescription()
    {
      return Kind == PassiveAbilityKind.LootingMastering;
    }

    public static bool IsPercentageFromKind(PassiveAbilityKind kind)
    {
      if (kind == PassiveAbilityKind.RestoreHealth ||
          kind == PassiveAbilityKind.RestoreMana)
        return true;
      return false;
    }

    public bool IsPercentage(bool primary)
    {
      if (IsPercentageFromKind())
        return true;
      return primary ? PrimaryStat.IsPercentage : AuxStat.IsPercentage;
    }
       

    //FightItemKind GetFightItemKind(PassiveAbilityKind abilityKind)
    //{
    //  switch (abilityKind)

    //  {
    //    case PassiveAbilityKind.ExplosiveMastering:
    //      return FightItemKind.ExplosiveCocktail;
    //    case PassiveAbilityKind.LootingMastering:
    //      break;
    //    case PassiveAbilityKind.ThrowingWeaponsMastering:
    //      return FightItemKind.ThrowingKnife;

    //    //case AbilityKind.HuntingMastering:
    //    //  return FightItemKind.Trap;

    //    default:
    //      break;
    //  }

    //  return FightItemKind.Unset;
    //}

    
    
  }
}
