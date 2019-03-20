using Dungeons.Core;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike
{
  public enum EntityStatKind
  {
    Unknown,

    Strength, Health, Magic, Attack, Defence,

    ResistFire, ResistCold, ResistPoison, ChanceToHit, ChanceToCastSpell, Mana,
    FireAttack, ColdAttack, PoisonAttack, LightPower, LifeStealing, ManaStealing,

    ChanceToCauseBleeding, ChanceToCauseStunning, ChanceToCauseTearApart, ChanceToEvadeMeleeAttack, ChanceToEvadeMagicAttack,
    MeleeAttackDamageReduction, MagicAttackDamageReduction, AxeExtraDamage, SwordExtraDamage, BashingExtraDamage, DaggerExtraDamage,
    LightingAttack, ResistLighting, ChanceToStrikeBack, ChanceToBulkAttack
  };

  public class StatValue
  {
    /// <summary>
    /// Original, native Attribute Value, in case of living entity can be increased by experience points
    /// </summary>
    private float nominalValue = 0;

    /// <summary>
    /// Amount of values from equipment or abilities
    /// </summary>
    float factor = 0;

    /// <summary>
    /// Amount of values subtracted, e.g. gained damage or used mana
    /// </summary>
    float subtracted = 0;


    public float NominalValue
    {
      get
      {
        return nominalValue;
      }

      set
      {
        nominalValue = value;
      }
    }

    public float Factor
    {
      get
      {
        return factor;
      }

      set
      {
        factor = value;
      }
    }

    public override string ToString()
    {
      return "NV: " + NominalValue + ", F: " + Factor + ", Sub:" + Subtracted;
    }

    public float TotalValue
    {
      get { return NominalValue + Factor; }
    }

    /// <summary>
    /// Whne damage was gained or spell used total is reduced by subtraction
    /// </summary>
    public float CurrentValue
    {
      get { return TotalValue - Subtracted; }
    }

    public float Subtracted
    {
      get
      {
        return subtracted;
      }

      set
      {
        subtracted = value;
      }
    }

    public void Divide(float value)
    {
      if (value != 0)
      {
        NominalValue /= value;

        Factor /= value;

        Subtracted /= value;

      }
    }

    public void MakeNegative()
    {
      NominalValue *= -1;
      Factor *= -1;
      Subtracted *= -1;

    }

    public void Accumulate(StatValue other)
    {
      NominalValue += other.NominalValue;
      Factor += other.Factor;
      Subtracted += other.Subtracted;

    }

    public void Divide(StatValue other)
    {
      NominalValue /= other.NominalValue;
      if (other.Factor != 0)
        Factor /= other.Factor;
      if (other.Subtracted != 0)
        Subtracted /= other.Subtracted;
    }
    public object Clone()
    {
      var clone = MemberwiseClone();
      return clone;
    }
  }



  public class EntityStats : ICloneable
  {
    Dictionary<EntityStatKind, EntityStat> stats = new Dictionary<EntityStatKind, EntityStat>();
    int level = 1;
    int experience;
    int nextExperience;
    int levelUpPoints;

    bool canAdvanceInExp = true;
    public event EventHandler<GenericEventArgs<EntityStatKind>> StatLeveledUp;

    public EntityStats()
    {
      var statKinds = Enum.GetValues(typeof(EntityStatKind));
      foreach (EntityStatKind sk in statKinds)
      {
        stats[sk] = new EntityStat(sk, 0);

      }
    }

    public List<EntityStatKind> GetNonPhysicalAttacks()
    {
      List<EntityStatKind> at = new List<EntityStatKind>();
      //EntityStatKind esk = EntityStatKind.Unknown;
      if (GetCurrentValue(EntityStatKind.FireAttack) > 0)
        at.Add(EntityStatKind.FireAttack);
      if (GetCurrentValue(EntityStatKind.PoisonAttack) > 0)
        at.Add(EntityStatKind.PoisonAttack);
      if (GetCurrentValue(EntityStatKind.ColdAttack) > 0)
        at.Add(EntityStatKind.ColdAttack);
      return at;
    }

    public void MakeNegative()
    {
      foreach (var myStat in this.Stats)
      {
        myStat.Value.MakeNegative();
      }
    }
    public bool CanAdvanceInExp
    {
      get
      {
        return canAdvanceInExp;
      }

      set
      {
        canAdvanceInExp = value;
      }
    }

    public List<EntityStat> GetBasicStats()
    {
      return stats.Where(i => EntityStat.BasicStats.Contains(i.Key)).ToList().Select(i => i.Value).ToList();
    }

    public float GetTotalValue(EntityStatKind esk)
    {
      return Stats[esk].TotalValue;
    }

    public void Accumulate(EntityStats other)
    {
      foreach (var myStat in this.Stats)
      {
        if (other.Stats.ContainsKey(myStat.Key))//when upgrading to new game version thre can be new stats not available in old stuff 
          myStat.Value.Accumulate(other.Stats[myStat.Key]);
      }

      experience += other.experience;
    }

    public void Divide(EntityStats other)
    {
      foreach (var myStat in this.Stats)
      {
        myStat.Value.Divide(other.Stats[myStat.Key]);
      }
    }

    public void Divide(float value)
    {
      foreach (var myStat in this.Stats)
      {
        myStat.Value.Divide(value);
      }
    }

    public string GetActiveStatsDescription()
    {
      return GetStatsDescription(true);
    }

    string GetStatsDescription(bool active)
    {
      var sb = new StringBuilder();
      foreach (var myStat in this.Stats)
      {
        if (active && myStat.Value.TotalValue == 0)
          continue;
        //sb.Append(myStat.Key + "=" + myStat.Value.NominalValue + "+" + myStat.Value.Factor + "(" + myStat.Value.TotalValue +  "); ");
        sb.Append(myStat.Value.ToString());
      }
      return sb.ToString();
    }

    public override string ToString()
    {
      return GetStatsDescription(false);
    }

    public void IncreaseStatByLevelUpPoint(EntityStatKind stat)
    {
      if (LevelUpPoints == 0)
        return;
      Stats[stat].NominalValue += 1;
      LevelUpPoints--;
      EmitStatsLeveledUp(stat);
    }

    public void EmitStatsLeveledUp(EntityStatKind stat)
    {
      if (StatLeveledUp != null)
        StatLeveledUp(this, new GenericEventArgs<EntityStatKind>(stat));
    }

    public float Attack
    {
      get
      {
        return Stats[EntityStatKind.Attack].TotalValue;
      }
    }

    public float Defence
    {
      get
      {
        return Stats[EntityStatKind.Defence].TotalValue;
      }
    }

    public float Health
    {
      get
      {
        return Stats[EntityStatKind.Health].CurrentValue;
      }
    }

    public float Strength
    {
      get
      {
        return Stats[EntityStatKind.Strength].CurrentValue;
      }
    }


    public bool HealthBelow(float factor)
    {
      return Health < Stats[EntityStatKind.Health].NominalValue * factor;
    }

    public float ChanceToHit
    {
      get
      {
        return stats[EntityStatKind.ChanceToHit].TotalValue;
      }
    }

    public float Mana
    {
      get
      {
        return Stats[EntityStatKind.Mana].CurrentValue;
      }
    }

    public int PrevLevelExperience { get; set; }

    public int Experience
    {
      get
      {
        return experience;
      }

      set
      {
        experience = value;
      }
    }

    //public bool IncreaseExp(int factor)
    //{
    //  experience += factor;
    //  bool lu = experience >= NextExperience;
    //  if (lu && canAdvanceInExp)
    //  {
    //    PrevLevelExperience = NextExperience;
    //    Level++;
    //    //TODO move to hero class
    //    LevelUpPoints += LevelGenerationInfo.Instance.LevelUpPoints;
    //    AbilityPoints += 2;
    //    nextExperience = (int)(nextExperience + (nextExperience * LevelGenerationInfo.Instance.NextExperienceIncrease));
    //    if (Level == 2)
    //      nextExperience += Hero.BaseExperience;//TODO

    //    return true;
    //  }

    //  return false;
    //}

    public int NextExperience
    {
      get
      {
        return nextExperience;
      }

      set
      {
        nextExperience = value;
      }
    }

    public int LevelUpPoints
    {
      get
      {
        return levelUpPoints;
      }

      set
      {
        levelUpPoints = value;
      }
    }

    public int AbilityPoints { get; set; }

    //TODO rename
    public Dictionary<EntityStatKind, EntityStat> Stats
    {
      get
      {
        return stats;
      }
      set
      {
        stats = value;
      }
    }

    public int Level
    {
      get
      {
        return level;
      }

      set
      {
        level = value;
      }
    }



    public void SetNominal(EntityStatKind kind, float value)
    {
      if (kind == EntityStatKind.Health)
      {
        //int k = 0;
      }
      stats[kind].NominalValue = value;
    }

    public void SetFactor(EntityStatKind kind, float value)
    {
      if (kind == EntityStatKind.Magic)
      {
        //int k = 0;
      }
      stats[kind].Factor = value;
    }

    public float GetNominal(EntityStatKind kind)
    {
      return stats[kind].NominalValue;
    }

    public float GetFactor(EntityStatKind kind)
    {
      return stats[kind].Factor;
    }

    public float GetCurrentValue(EntityStatKind kind)
    {
      return stats[kind].CurrentValue;
    }

    //internal void SetArmor(Armor arm)
    //{
    //  stats[EntityStatKind.Defence].Factor = arm.Defence;
    //}

    public EntityStats GetCopy()
    {
      return (EntityStats)Clone();
    }

    public object Clone()
    {
      EntityStats cloned = (EntityStats)MemberwiseClone();
      cloned.Stats = new Dictionary<EntityStatKind, EntityStat>();
      var eq = ReferenceEquals(cloned, this);
      foreach (var myStat in this.Stats)
      {
        cloned.Stats[myStat.Key] = (EntityStat)myStat.Value.Clone();
      }

      return cloned;
    }

    internal void IncreaseStatFactor(EntityStatKind sk)
    {
      var inc = Stats[sk].TotalValue / 2;
      IncreaseStatDynamicValue(sk, inc);
      //IncreaseStatFactor(sk, loot.Amount);
    }

    internal void IncreaseStatFactor(EntityStatKind sk, float percent)
    {
      var inc = Stats[sk].TotalValue * percent / 100;
      //if (inc < 1)
      //  inc = 1;
      IncreaseStatDynamicValue(sk, inc);
    }

    internal bool IncreaseStatDynamicValue(EntityStatKind sk, float amount)
    {
      var cv = Stats[sk].CurrentValue;
      if (cv < Stats[sk].TotalValue && amount > 0 || cv > 0 && amount < 0)
      {
        var valMax = Stats[sk].TotalValue - cv;
        float val = amount;
        if (val > valMax)
          val = valMax;

        Stats[sk].Subtract(-val);

        return true;
      }

      return false;
    }

    internal void ResetStatFactors()
    {
      foreach (var myStat in this.Stats)
      {
        myStat.Value.Factor = 0;
      }
    }

    internal void AccumulateFactors(EntityStats stats, bool positive)
    {
      foreach (var myStat in this.Stats)
      {
        if ((stats.Stats[myStat.Key].Factor > 0 && positive) ||
          (stats.Stats[myStat.Key].Factor < 0 && !positive))
          AccumulateFactor(myStat.Key, stats.Stats[myStat.Key].Factor);
      }
    }

    internal void AccumulateFactor(EntityStatKind primaryStat, float primaryStatValue)
    {
      if (this.Stats.ContainsKey(primaryStat))//old game save might not hve a stat
      {
        var stat = this.Stats[primaryStat];
        stat.Factor += primaryStatValue;
        if (stat.IsPercentage && stat.TotalValue > 100)
        {
          var diff = stat.TotalValue - 100;
          stat.Factor -= diff;
        }
      }
    }
  }

  public class EntityStatsTotal : EntityStats
  {
  }
}
