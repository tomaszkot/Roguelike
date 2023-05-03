using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Abilities
{
  
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
        var esks = new List<EntityStatKind>();
        //EntityStatKind third = EntityStatKind.Unset;
        switch (kind)
        {
          case AbilityKind.RestoreHealth:
            esks.Add(EntityStatKind.Health);
            this.BeginTurnApply = true;
            break;
          case AbilityKind.RestoreMana:
            this.BeginTurnApply = true;
            esks.Add(EntityStatKind.Mana);
            break;
          case AbilityKind.AxesMastering:
            esks.Add(EntityStatKind.ChanceToCauseTearApart);
            esks.Add(EntityStatKind.AxeExtraDamage);
            MaxLevel = 10;
            break;
          case AbilityKind.BashingMastering:
            esks.Add(EntityStatKind.ChanceToCauseStunning);
            esks.Add(EntityStatKind.BashingExtraDamage);
            MaxLevel = 10;
            break;
          case AbilityKind.DaggersMastering:
            esks.Add(EntityStatKind.ChanceToCauseBleeding);
            esks.Add(EntityStatKind.DaggerExtraDamage);
            MaxLevel = 10;
            break;
          case AbilityKind.SwordsMastering:
            esks.Add(EntityStatKind.ChanceToMeleeHit);
            esks.Add(EntityStatKind.SwordExtraDamage);
            MaxLevel = 10;
            break;
          
          case AbilityKind.StaffsMastering:
            esks.Add(EntityStatKind.ChanceToRepeatElementalProjectileAttack);
            esks.Add(EntityStatKind.StaffExtraElementalProjectileDamage);
            esks.Add(EntityStatKind.StaffExtraRange);//TODO
            MaxLevel = 10;
            break;
          case AbilityKind.SceptersMastering:
            esks.Add(EntityStatKind.ChanceToCauseElementalAilment);
            esks.Add(EntityStatKind.ScepterExtraElementalProjectileDamage);
            esks.Add(EntityStatKind.ScepterExtraRange);//TODO
            MaxLevel = 10;
            break;
          case AbilityKind.WandsMastering:
            esks.Add(EntityStatKind.ChanceToElementalProjectileBulkAttack);
            esks.Add(EntityStatKind.WandExtraElementalProjectileDamage);
            esks.Add(EntityStatKind.WandExtraRange);//TODO
            MaxLevel = 10;
            break;
          case AbilityKind.CrossBowsMastering:
            esks.Add(EntityStatKind.ChanceToCauseBleeding);
            esks.Add(EntityStatKind.CrossbowExtraDamage);
            esks.Add(EntityStatKind.CroobowsExtraRange);//TODO
            MaxLevel = 10;
            break;
          case AbilityKind.BowsMastering:
            esks.Add(EntityStatKind.ChanceToPhysicalProjectileHit);
            esks.Add(EntityStatKind.BowExtraDamage);
            esks.Add(EntityStatKind.BowsExtraRange);
            MaxLevel = 10;
            break;
          case AbilityKind.FireBallMastering:
            esks.Add(EntityStatKind.FireBallExtraDamage);
           // ask = EntityStatKind.ChanceToCauseFiring;//TODO
            break;
          case AbilityKind.IceBallMastering:
            esks.Add(EntityStatKind.IceBallExtraDamage);
           // ask = EntityStatKind.ChanceToCauseFreezing;//TODO
            break;
          case AbilityKind.PoisonBallMastering:
            esks.Add(EntityStatKind.PoisonBallExtraDamage);
           // ask = EntityStatKind.ChanceToCausePoisoning;//TODO
            break;
          //case AbilityKind.SkeletonMastering:
          //  esks.Add(EntityStatKind.PrimaryAttributes);
          //  esks.Add(EntityStatKind.MaxSkeletonsCount);
          //  break;

          case AbilityKind.LootingMastering:
          case AbilityKind.StrikeBack:
          case AbilityKind.BulkAttack:
            PageIndex = 1;
            if (kind == AbilityKind.StrikeBack)
            {
              esks.Add(EntityStatKind.ChanceToStrikeBack);

            }
            else if (kind == AbilityKind.BulkAttack)
            {
              esks.Add(EntityStatKind.ChanceToBulkAttack);

            }
            break;
          default:
            break;
        }
        for (int i = 0; i < esks.Count; i++)
        {
          if (Stats.Count == i)
          {
            var es = new EntityStat();
            es.UseSign = true;
            Stats.Add(es);
          }
          Stats[i].SetKind(esks[i]);
          if(Stats[i].IsExtraRange)
            Stats[i].Unit = EntityStatUnit.Absolute;
          else
            Stats[i].Unit = EntityStatUnit.Percentage;
        }

        //if (kind == AbilityKind.SkeletonMastering)
        //{
        //  Stats[1].Unit = EntityStatUnit.Absolute;
        //}
      }
    }

    public float GetFactor(bool primary)
    {
      return primary ? PrimaryStat.Factor : AuxStat.Factor;
    }
    
    public override bool IsPercentageFromKind => IsPercFromKind(Kind);

    public override float CalcFactor(int index, int level)
    {
      AbilityKind kind = Kind;
      bool primary = index == 0;
      float factor = 0;

      if (Stats[index].IsExtraRange)
      {
        return (level+1)/2;
      }
      
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
        case AbilityKind.FireBallMastering:
        case AbilityKind.IceBallMastering:
        case AbilityKind.PoisonBallMastering:


          if (primary)
          {
            factor = level * 2;

            if (kind == AbilityKind.WandsMastering //ChanceToElementalProjectileBulkAttack
              || kind == AbilityKind.SceptersMastering//ChanceToCauseElementalAilment
              || kind == AbilityKind.StaffsMastering//ChanceToRepeatElementalAttack
              )
            {
              var multsDef = new int[] { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
              factor = multsDef[level];
            }

            if (kind == AbilityKind.FireBallMastering
              || kind == AbilityKind.IceBallMastering
              || kind == AbilityKind.PoisonBallMastering
              )
            {
              //damage in %
              var multsDef = new int[] { 0, 10, 20, 30, 40, 50 };
              factor = multsDef[level];
            }
          }
          else if (index == 1)
          {
            factor = level * 5;

            if (kind == AbilityKind.WandsMastering)
            {
              var multsDef = new int[] { 0, 2, 4, 6, 8, 11, 15, 18, 25, 30, 35 };
              factor = multsDef[level];
            }
          }
          else if (index == 2)
          {
            if (kind == AbilityKind.BowsMastering || kind == AbilityKind.CrossBowsMastering)
            {
              factor = level;
            }
            else
            {
              throw new Exception("index == 2 ns!");
            }
          }
          break;
        //case AbilityKind.SkeletonMastering:
        //  if (primary)
        //  {
        //    //PrimaryAttributes %
        //    var multsDef = new int[] { 0, 10, 15, 20, 25, 30 };
        //    factor = multsDef[level];
        //  }
        //  else 
        //  {
        //    //MaxSkeletonsCount
        //    var maxSkeletonsCount = new int[] { 0, 1, 1, 2, 2, 3 };
        //    factor = maxSkeletonsCount[level];
        //  }
          //break;
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
        case AbilityKind.FireBallMastering:
        case AbilityKind.IceBallMastering:
        case AbilityKind.PoisonBallMastering:
          desc = "Bonus when using mana-powered "+ kind.ToString().Replace("Mastering", "");
          break;
        //case AbilityKind.SkeletonMastering:
        //  desc = "Bonus when using " + kind.ToString().Replace("Mastering", "");
        //  break;
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
