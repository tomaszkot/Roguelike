using Roguelike.Abstract;
using Roguelike.Abstract.HotBar;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Tiles.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

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

    FireBallMastering, IceBallMastering, PoisonBallMastering, 
    SkeletonMastering, //Deprecated, ca not easily remove - as old save game might be a problem
    ThrowingTorch,
    Cannon,

    //Hunter c.d.
    Smoke,

    //Warrior
    IronSkin, ElementalVengeance, ZealAttack
  }

  public abstract class Ability : IDescriptable, IHotbarItem
  {
    protected string primaryStatDescription;
    public int Level { get; set; }

    public List<EntityStat> Stats { get; set; } = new List<EntityStat>();

    public int MaxLevel = 5;
    protected Dictionary<int, int> abilityLevelToPlayerLevel = new Dictionary<int, int>();
    public int PageIndex { get; set; }
    public int PositionInPage { get; set; }
    public const string MessageMaxLevelReached = "Max level reached";
    protected List<string> customExtraStatDescription = new List<string>();
    public string LastIncError { get; set; }
    protected AbilityKind kind;
    public int CoolDownCounter { get; set; }
    public bool UsesCoolDownCounter { get; set; } = false;
    public int MaxCollDownCounter { get; set; } = 5;
    public bool AutoApply { get; set; }
    public bool TurnsIntoLastingEffect { get; set; }

    public Ability()
    {
      PrimaryStat = new EntityStat();
      PrimaryStat.UseSign = true;
      AuxStat = new EntityStat();
      AuxStat.UseSign = true;
      Revealed = true;
    }

    public abstract AbilityKind Kind { get; set; }

    protected virtual List<string> GetCustomExtraStatDescription(int level)
    {
      return customExtraStatDescription;
    }

    /// <summary>
    /// TODO remove it
    /// </summary>
    public EntityStat PrimaryStat
    {
      get
      {
        if (!Stats.Any())
          return null;
        return Stats[0];
      }
      set
      {
        if (Stats.Any())
          Stats[0] = value;
        else
          Stats.Add(value);
      }
    }

    /// <summary>
    /// TODO remove it
    /// </summary>
    public EntityStat AuxStat
    {
      get
      {
        return ReturnAt(1);
      }
      set
      {
        SetAt(1, value);
      }
    }

    protected void SetAt(int index, EntityStat value)
    {
      while (Stats.Count < index + 1)
        Stats.Add(null);

      Stats[index] = value;
    }

    private EntityStat ReturnAt(int index)
    {
      if (Stats.Count < index + 1)
        return null;
      return Stats[index];
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

    public void SetFactor(int index, float factor)
    {
      if (Stats.Count > index)
        Stats[index].Factor = factor;
      else
      {
        int k = 0;
        k++;
      }
    }

    public virtual void SetStatsForLevel()
    {
      for (int i = 0; i < Stats.Count; i++)
        SetFactor(i, CalcFactor(i, Level));
    }

    //public float CalcFactor(bool primary)
    //{
    //  return CalcFactor(primary ? 0 : 1, Level);
    //}

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

    EntityStat CreateForLevel(EntityStat src, int statIndex, int level)
    {
      var esN = new EntityStat(src.Kind, 0, src.Unit);
      esN.UseSign = src.UseSign;
      var fac = CalcFactor(statIndex, level);
      esN.Factor = fac;
      return esN;
    }

    List<EntityStat> CreateForLevel(int level)
    {
      var res = new List<EntityStat>();
      var max = Stats.Where(i => i.Kind != EntityStatKind.Unset).Count();

      for (int i = 0; i < max; i++)
        res.Add(CreateForLevel(ReturnAt(i), i, level));

      return res;
    }

    public int GetExtraRange()
    {
      var range = GetExtraRangeStat();
      if (range != null)
        return (int)range.Value.CurrentValue;

      return 0;
    }

    public EntityStat GetExtraRangeStat()
    {
      return Stats.Where(i => i.IsExtraRange).FirstOrDefault();
    }

    public List<EntityStat> GetEntityStats(bool currentLevel)
    {
      var res = new List<EntityStat>();
      if (currentLevel)
      {
        return Stats.Where(i => i.Kind != EntityStatKind.Unset).ToList();
        //res.Add(PrimaryStat);
        //if (AuxStat.Kind != EntityStatKind.Unset)
        //  res.Add(AuxStat);
      }
      else
      {
        if (Level < MaxLevel)
        {
          res = CreateForLevel(Level + 1);
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
          var esN = CreateForLevel(PrimaryStat, 0, Level + 1);
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
            esN = CreateForLevel(AuxStat, 1, Level + 1);
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

    public abstract float CalcFactor(int index, int level);

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
        var fac = CalcFactor(0, Level + 1);
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
          fac = CalcFactor(1, Level + 1);
          secondary = new EntityStat(AuxStat.Kind, 0);
          secondary.Factor = fac;
          //desc.Add(AuxStat.Kind + ": " + GetFormattedCurrentValue(esN));
        }
      }
      return new Tuple<EntityStat, EntityStat>(primary, secondary);
    }
  }
}
