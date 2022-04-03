﻿using Roguelike.Abstract;
using Roguelike.Abstract.HotBar;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Tiles.Abstract;
using System;
using System.Collections.Generic;

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
    ExplosiveCocktail, ThrowingStone, ThrowingKnife, HunterTrap

    , StaffsMastering, SceptersMastering, WandsMastering, PoisonCocktail, Stride, OpenWound, Rage,
    WeightedNet,

    PiercingArrow, ArrowVolley, PerfectHit,

    FireBallMastering, IceBallMastering, PoisonBallMastering, SkeletonMastering
  }

  public abstract class Ability : IDescriptable, IHotbarItem
    {
    protected string primaryStatDescription;
    public int Level { get; set; } 
    public EntityStat PrimaryStat { get; set; }
    public EntityStat AuxStat { get; set; }
    public int MaxLevel = 5;
    protected Dictionary<int, int> abilityLevelToPlayerLevel = new Dictionary<int, int>();
    public int PageIndex { get; set; }
    public int PositionInPage { get; set; }
    public const string MessageMaxLevelReached = "Max level reached";
    protected List<string> customExtraStatDescription = new List<string>();
    public string LastIncError { get; set; }
    protected AbilityKind kind;
    public int CollDownCounter { get; set; }
    public int MaxCollDownCounter { get; set; } = 5;

    public Ability()
    {
      PrimaryStat = new EntityStat();
      AuxStat = new EntityStat();
      Revealed = true;
    }

    public abstract AbilityKind Kind { get; set; }

    protected virtual List<string> GetCustomExtraStatDescription(int level)
    {
      return customExtraStatDescription;
    }

    public bool IncreaseLevel(IAdvancedEntity entity)
    {
      LastIncError = "";
      if (Level == MaxLevel)
      {
        LastIncError = "Max level of ability reached";
        return false;
      }
      if (abilityLevelToPlayerLevel.ContainsKey(Level + 1))
      {
        var lev = abilityLevelToPlayerLevel[Level + 1];
        if (lev > entity.Level)
        {
          LastIncError = "Required character level for ability increase: " + lev;
          return false;
        }
      }
      Level++;
      SetStatsForLevel();
      return true;
    }

    public void SetFactor(bool primary, float factor)
    {
      if (primary)
        PrimaryStat.Factor = factor;
      else if(AuxStat.Kind != EntityStatKind.Unset)
        AuxStat.Factor = factor;
    }

    public virtual void SetStatsForLevel()
    {
      SetFactor(true, CalcFactor(true));
      SetFactor(false, CalcFactor(false));
    }

    public float CalcFactor(bool primary)
    {
      return CalcFactor(primary, Level);
    }

    public virtual string GetPrimaryStatDescription()
    {
      return primaryStatDescription;
    }

    public bool MaxLevelReached
    {
      get
      {
        return Level >= MaxLevel;
      }
    }

    protected static float CalcFightItemFactor(int level)
    {
      var fac = FactorCalculator.CalcFromLevel2(level + 1, 4) * 2.3f;
      return fac;
    }

    protected void SetName(string kindDesc)//Kind.ToDescription();
    {
      var nameToDisplay = kindDesc;
      nameToDisplay = nameToDisplay.Replace("Restore", "Restore ");
      nameToDisplay = nameToDisplay.Replace("Mastering", " Mastery");
      nameToDisplay = nameToDisplay.Replace("Defender", " Defender");
      Name = nameToDisplay;
    }
    
    public string GetFormattedCurrentValue(EntityStat es)
    {
      var fv = es.GetFormattedCurrentValue();
      //if (es.Unit != EntityStatUnit.Percentage && IsPercentageFromKind)//Health, Magic... ability is always in %
      //  fv += "%";
      return fv;
    }

    public abstract bool IsPercentageFromKind { get; }

    EntityStat CreateForLevel(EntityStat src, bool primary, int level)
    {
      var esN = new EntityStat(src.Kind, 0, src.Unit);
      var fac = CalcFactor(primary, level);
      esN.Factor = fac;
      return esN;
    }

    List<EntityStat> CreateForLevel(int level)
    {
      var res = new List<EntityStat>();
      
      res.Add(CreateForLevel(PrimaryStat, true, level));

      if (AuxStat.Kind != EntityStatKind.Unset)
        res.Add(CreateForLevel(AuxStat, false, level));

      return res;
    }

    public List<EntityStat> GetEntityStats(bool currentLevel)
    {
      var res = new List<EntityStat>();
      if (currentLevel)
      {
        res.Add(PrimaryStat);
        if (AuxStat.Kind != EntityStatKind.Unset)
          res.Add(AuxStat);
      }
      else 
      {
        if (Level < MaxLevel)
        {
          res = CreateForLevel(Level+1);
        }
      }
      return res;
    }

    public string[] GetExtraStatDescription(bool currentLevel)
    {
      var desc = new List<string>();

      bool usesCustomStatDescription = useCustomStatDescription();
      
      if (currentLevel)
      {
        if (usesCustomStatDescription)
                                      
        {
          desc.AddRange(this.GetCustomExtraStatDescription(Level));
        }
        else
        {
          desc.Add(PrimaryStat.Kind + ": " + GetFormattedCurrentValue(PrimaryStat));
          if (AuxStat.Kind != EntityStatKind.Unset)
            desc.Add(AuxStat.Kind + ": " + GetFormattedCurrentValue(AuxStat));
        }
      }
      else
      {
        if (Level < MaxLevel)
        {
          var esN = CreateForLevel(PrimaryStat, true, Level + 1);// new EntityStat(PrimaryStat.Kind, 0, PrimaryStat.Unit);
          desc.Add("Next Level: ");
          if (usesCustomStatDescription)
          {
            desc.AddRange(this.GetCustomExtraStatDescription(Level + 1));
          }
          else
          {
            desc.Add(PrimaryStat.Kind + ": " + GetFormattedCurrentValue(esN));
          }
          if (AuxStat.Kind != EntityStatKind.Unset)
          {
            esN = CreateForLevel(AuxStat, false, Level + 1);
            desc.Add(AuxStat.Kind + ": " + GetFormattedCurrentValue(esN));
          }
        }
        else
        {
          desc.Add(MessageMaxLevelReached);
        }
      }
      return desc.ToArray();
    }

    public abstract float CalcFactor(bool primary, int level);

    public abstract bool useCustomStatDescription();
    
    public bool Revealed
    {
      get;
      set;
    }

    public string Name
    {
      get;
      set;
    }

    public Tuple<EntityStat, EntityStat> GetNextLevelStats()
    {
      var primary = new EntityStat(PrimaryStat.Kind, 0);
      var secondary = new EntityStat(AuxStat.Kind, 0);
      if (Level < MaxLevel)
      {
        {
          var fac = CalcFactor(true, Level + 1);
          //if (Kind == PassiveAbilityKind.LootingMastering)
          //{
          //  //desc.AddRange(this.GetCustomExtraStatDescription(Level + 1));
          //}
          //else
          {
            primary.Factor = fac;
            //desc.Add(PrimaryStat.Kind + ": " + GetFormattedCurrentValue(esN));
          }
          if (AuxStat.Kind != EntityStatKind.Unset)
          {
            fac = CalcFactor(false, Level + 1);
            secondary = new EntityStat(AuxStat.Kind, 0);
            secondary.Factor = fac;
            //desc.Add(AuxStat.Kind + ": " + GetFormattedCurrentValue(esN));
          }
        }
      }
      return new Tuple<EntityStat, EntityStat>(primary, secondary);
    }

    //public EntityStatUnit EntityStatUnit
    //{
    //  get 
    //  { 
    //    if(IsPercentageFromKind)
    //      return EntityStatUnit.Percentage;
    //    return EntityStatUnit.Absolute;
    //  }
    //}
  }
}
