using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Tiles.Looting;
using System;

namespace Roguelike.Abilities
{
  public enum AbilityKind
  {
    Unset, RestoreHealth, RestoreMana, LootingMastering,

    AxesMastering, BashingMastering, DaggersMastering, SwordsMastering,
    StrikeBack, BulkAttack,
    BowsMastering, CrossBowsMastering,

    //Traps, RemoveClaws, RemoveTusk, Skinning, , ,
    //HuntingMastering /*<-(to del)*/
    ExplosiveMastering, ThrowingStoneMastering, ThrowingKnifeMastering, HunterTrapMastering

    ,StaffsMastering, SceptersMastering, WandsMastering, PoisonMastering, Stride
  }

  /// <summary>
  /// PassiveAbility works automatically
  /// </summary>
  public class PassiveAbility : Ability
  {
    public bool BeginTurnApply;
    
    public override string ToString()
    {
      return base.ToString() + Kind;
    }
        
    public override AbilityKind Kind
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
          case AbilityKind.RestoreHealth:
            psk = EntityStatKind.Health;
            this.BeginTurnApply = true;
            break;
          case AbilityKind.RestoreMana:
            this.BeginTurnApply = true;
            psk = EntityStatKind.Mana;
            break;
          case AbilityKind.AxesMastering:
            psk = EntityStatKind.ChanceToCauseTearApart;
            ask = EntityStatKind.AxeExtraDamage;
            break;
          case AbilityKind.BashingMastering:
            psk = EntityStatKind.ChanceToCauseStunning;
            ask = EntityStatKind.BashingExtraDamage;
            break;
          case AbilityKind.DaggersMastering:
            psk = EntityStatKind.ChanceToCauseBleeding;
            ask = EntityStatKind.DaggerExtraDamage;
            break;
          case AbilityKind.SwordsMastering:
            psk = EntityStatKind.ChanceToMeleeHit;
            ask = EntityStatKind.SwordExtraDamage;
            break;
          
          case AbilityKind.StaffsMastering:
            psk = EntityStatKind.ChanceToRepeatElementalProjectileAttack;
            ask = EntityStatKind.StaffExtraElementalProjectileDamage;
            break;
          case AbilityKind.SceptersMastering:
            psk = EntityStatKind.ChanceToCauseElementalAilment;
            ask = EntityStatKind.ScepterExtraElementalProjectileDamage;
            break;
          case AbilityKind.WandsMastering:
            psk = EntityStatKind.ChanceToElementalProjectileBulkAttack;
            ask = EntityStatKind.WandExtraElementalProjectileDamage;
            break;
          case AbilityKind.CrossBowsMastering:
            psk = EntityStatKind.ChanceToCauseBleeding;
            ask = EntityStatKind.CrossbowExtraDamage;
            break;
          case AbilityKind.BowsMastering:
            psk = EntityStatKind.ChanceToPhysicalProjectileHit;
            ask = EntityStatKind.BowExtraDamage;
            break;
          case AbilityKind.LootingMastering:
          case AbilityKind.StrikeBack:
          case AbilityKind.BulkAttack:
            PageIndex = 1;
            if (kind == AbilityKind.StrikeBack)
            {
              psk = EntityStatKind.ChanceToStrikeBack;

            }
            else if (kind == AbilityKind.BulkAttack)
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
    
    public override bool IsPercentageFromKind => IsPercFromKind(Kind);

    public override float CalcFactor(bool primary, int level)
    {
      AbilityKind kind = Kind;

      float factor = 0;
      switch (kind)
      {
        case AbilityKind.RestoreHealth:
        case AbilityKind.RestoreMana:
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
        case AbilityKind.AxesMastering:
        case AbilityKind.BashingMastering:
        case AbilityKind.DaggersMastering:
        case AbilityKind.SwordsMastering:
        case AbilityKind.WandsMastering:
        case AbilityKind.SceptersMastering:
        case AbilityKind.StaffsMastering:
        case AbilityKind.BowsMastering:
        case AbilityKind.CrossBowsMastering:

          if (primary)
          {
            factor = level;
            
            if (kind == AbilityKind.WandsMastering //ChanceToElementalBulkAttack
              || kind == AbilityKind.SceptersMastering//ChanceToCauseElementalAilment
              || kind == AbilityKind.StaffsMastering//ChanceToRepeatElementalAttack
              )
            {
              var multsDef = new int[] { 0, 4, 7, 10, 15, 20 };
              factor = multsDef[level];
            }
          }
          else
          {
            factor = level * 5;

            if (kind == AbilityKind.WandsMastering)
            {
              var multsDef = new int[] { 0, 4, 7, 10, 15, 20 };
              factor = multsDef[level];
            }
          }
          break;
        
        case AbilityKind.StrikeBack:
          var multsDefSB = new int[] { 0, 2, 4, 7, 10, 15 };
          factor = multsDefSB[level];
          break;
        case AbilityKind.BulkAttack:
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
        case AbilityKind.RestoreHealth:
          desc = "Restores health at turn's begining";
          break;
        case AbilityKind.RestoreMana:
          desc = "Restores mana at turn's begining";
          break;
        case AbilityKind.AxesMastering:
        case AbilityKind.BashingMastering:
        case AbilityKind.DaggersMastering:
        case AbilityKind.SwordsMastering:
        case AbilityKind.StaffsMastering:
        case AbilityKind.SceptersMastering:
        case AbilityKind.WandsMastering:
        case AbilityKind.BowsMastering:
        case AbilityKind.CrossBowsMastering:
          desc = "Bonus when using ";
          var wpn = kind.ToString().Replace("Mastering", "");
          if (wpn.EndsWith("s"))
            wpn = wpn.TrimEnd("s".ToCharArray());
          desc += wpn + " weapon";
          break;
        case AbilityKind.LootingMastering:
          desc = "Bonus to loot experience";// frequency and quality";
          break;
        //case AbilityKind.HuntingMastering:
        //  desc = "Bonus to Hunting (Traps)";
        //  break;
        case AbilityKind.StrikeBack:
          desc = "Chance to strike back when being hit";
          break;
        case AbilityKind.BulkAttack:
          desc = "Chance to strike all sourronding enemies in one turn";
          break;
       
        default:
          break;
      }
      primaryStatDescription = desc;
    }
        
    public override bool useCustomStatDescription()
    {
      return Kind == AbilityKind.LootingMastering;
    }

    public static bool IsPercFromKind(AbilityKind kind)
    {
      if (kind == AbilityKind.RestoreHealth ||
          kind == AbilityKind.RestoreMana)
        return true;
      return false;
    }

    public bool IsPercentage(bool primary)
    {
      if (IsPercFromKind(Kind))
        return true;
      return primary ? PrimaryStat.Unit == EntityStatUnit.Percentage : AuxStat.Unit == EntityStatUnit.Percentage;
    }
  }
}
