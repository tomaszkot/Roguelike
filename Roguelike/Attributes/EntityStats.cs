using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Attributes
{
  public class EntityStats //: ICloneable
  {
    Dictionary<EntityStatKind, EntityStat> stats = new Dictionary<EntityStatKind, EntityStat>();
    int level = 1;
    //int experience;
    //int nextExperience;
    //int levelUpPoints;

    bool canAdvanceInExp = true;
    

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
      foreach (var myStat in this.Stats.Values)
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
      return this[esk].TotalValue;
    }

    public void Accumulate(EntityStats other)
    {
      foreach (var myStat in this.Stats.Values)
      {
        if (other.Stats.ContainsKey(myStat.Kind))//when upgrading to new game version thre can be new stats not available in old stuff 
          myStat.Value.Accumulate(other[myStat.Kind]);
      }

      //experience += other.experience;
    }

    public void Divide(EntityStats other)
    {
      foreach (var myStat in this.Stats.Values)
      {
        myStat.Value.Divide(other.Stats[myStat.Kind].Value);
      }
    }

    public void Divide(float value)
    {
      foreach (var myStat in this.Stats.Values)
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
      foreach (var myStat in this.Stats.Values)
      {
        if (active && myStat.Value.TotalValue == 0)
          continue;
        //sb.Append(myStat.Key + "=" + myStat.Value.NominalValue + "+" + myStat.Value.Factor + "(" + myStat.Value.TotalValue +  "); ");
        sb.Append(myStat.Kind + ":" + myStat.Value.ToString());
      }
      return sb.ToString();
    }

    public override string ToString()
    {
      return GetStatsDescription(false);
    }

    

    public void SetStat(EntityStatKind kind, float nominalValue)
    {
      this[kind].Nominal = nominalValue;
    }

    public float Attack
    {
      get
      {
        return this[EntityStatKind.Attack].TotalValue;
      }
    }

    public float Defense
    {
      get
      {
        return this[EntityStatKind.Defense].TotalValue;
      }
    }

    public float Health
    {
      get
      {
        return this[EntityStatKind.Health].CurrentValue;
      }
    }

    public float Strength
    {
      get
      {
        return this[EntityStatKind.Strength].CurrentValue;
      }
    }


    public bool HealthBelow(float factor)
    {
      //return Health < stats[EntityStatKind.Health].Value.Nominal * factor;
      return Health < this[EntityStatKind.Health].Nominal * factor;
    }

    public float ChanceToHit
    {
      get
      {
        return this[EntityStatKind.ChanceToHit].TotalValue;
      }
    }

    public float Mana
    {
      get
      {
        return this[EntityStatKind.Mana].CurrentValue;
      }
    }

    //public int Experience
    //{
    //  get
    //  {
    //    return experience;
    //  }

    //  set
    //  {
    //    experience = value;
    //  }
    //}

    //public int NextExperience
    //{
    //  get
    //  {
    //    return nextExperience;
    //  }

    //  set
    //  {
    //    nextExperience = value;
    //  }
    //}

    public int AbilityPoints { get; set; }

    public EntityStat GetStat(EntityStatKind esk)
    {
      return Stats[esk];
    }

    public void SetStat(EntityStatKind esk, EntityStat es)
    {
      Stats[esk] = es;
    }

    //TODO rename
    Dictionary<EntityStatKind, EntityStat> Stats
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

    public List<StatValue> Values()
    {
      return Stats.Values.Select(i=> i.Value).ToList();
    }

    public Dictionary<EntityStatKind, EntityStat> GetStats()
    {
      return Stats;
    }

    [JsonIgnore]
    public StatValue this[EntityStatKind kind]
    {
      get { return Stats[kind].Value; }
     // set { arr[i] = value; }
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
      this[kind].Nominal = value;
    }

    public void SetFactor(EntityStatKind kind, float value)
    {
      if (kind == EntityStatKind.Magic)
      {
        //int k = 0;
      }
      stats[kind].Factor = value;
    }

    //TOOD remove use indexer
    public float GetNominal(EntityStatKind kind)
    {
      return this[kind].Nominal;
    }

    public float GetFactor(EntityStatKind kind)
    {
      return this[kind].Factor;
    }

    public float GetCurrentValue(EntityStatKind kind)
    {
      return this[kind].CurrentValue;
    }

    //internal void SetArmor(Armor arm)
    //{
    //  stats[EntityStatKind.Defence].Factor = arm.Defence;
    //}

    //public EntityStats GetCopy()
    //{
    //  return (EntityStats)Clone();
    //}

    //public object Clone()
    //{
    //  EntityStats cloned = (EntityStats)MemberwiseClone();
    //  cloned.Stats = new Dictionary<EntityStatKind, EntityStat>();
    //  var eq = ReferenceEquals(cloned, this);
    //  foreach (var myStat in this.Stats)
    //  {
    //    cloned.Stats[myStat.Key] = (EntityStat)myStat.Value.Clone();
    //  }

    //  return cloned;
    //}

    //internal void IncreaseStatFactor(EntityStatKind sk)
    //{
    //  var inc = this[sk].TotalValue / 2;
    //  IncreaseStatDynamicValue(sk, inc);
    //  //IncreaseStatFactor(sk, loot.Amount);
    //}

    internal void IncreaseStatFactor(EntityStatKind sk, float percent)
    {
      var inc = this[sk].TotalValue * percent / 100;
      //if (inc < 1)
      //  inc = 1;
      ChangeStatDynamicValue(sk, inc);
    }

    //Stats.GetStat(EntityStatKind.Health).Subtract(amount);
    internal bool ChangeStatDynamicValue(EntityStatKind sk, float amount)
    {
      var he = Health;

      var cv = this[sk].CurrentValue;
      if (cv < this[sk].TotalValue && amount > 0 || cv > 0 && amount < 0)
      {
        var valMax = this[sk].TotalValue - cv;
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
        var otherStat = stats.Stats[myStat.Key];
        if ((otherStat.Factor > 0 && positive) ||
          (otherStat.Factor < 0 && !positive))
          AccumulateFactor(myStat.Key, otherStat.Factor);
      }
    }

    internal void AccumulateFactor(EntityStatKind primaryStat, float primaryStatValue)
    {
      if (this.Stats.ContainsKey(primaryStat))//old game save might not hve a stat
      {
        var stat = this.Stats[primaryStat];
        stat.Factor += primaryStatValue;
        if (stat.IsPercentage && stat.Value.TotalValue > 100)
        {
          var diff = stat.Value.TotalValue - 100;
          stat.Factor -= diff;
        }
      }
    }
  }

  public enum EntityStatKind
  {
    Unset,

    Strength, Health, Magic, Defense, Dexterity,

    ResistFire, ResistCold, ResistPoison, ChanceToHit, ChanceToCastSpell, Mana, Attack,
    FireAttack, ColdAttack, PoisonAttack, LightPower, LifeStealing, ManaStealing,

    //TODO generate dynamically this enum
    ChanceToCauseBleeding, ChanceToCauseStunning, ChanceToCauseTearApart, ChanceToEvadeMeleeAttack, ChanceToEvadeMagicAttack,
    MeleeAttackDamageReduction, MagicAttackDamageReduction, AxeExtraDamage, SwordExtraDamage, BashingExtraDamage, DaggerExtraDamage,
    LightingAttack, ResistLighting, ChanceToStrikeBack, ChanceToBulkAttack
  };

  public class EntityStatsTotal : EntityStats
  {
  }
}
