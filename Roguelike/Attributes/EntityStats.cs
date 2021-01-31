using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Attributes
{
  public class EntityStats 
  {
    Dictionary<EntityStatKind, EntityStat> stats = new Dictionary<EntityStatKind, EntityStat>();
    //int level = 1;
    bool canAdvanceInExp = true;
    
    public EntityStats()
    {
      var statKinds = Enum.GetValues(typeof(EntityStatKind));
      foreach (EntityStatKind sk in statKinds)
      {
        //Ensure(sk);
      }
    }

    public void Ensure(EntityStatKind kind)
    {
      if(!stats.ContainsKey(kind))
        stats[kind] = new EntityStat(kind, 0);
    }

    //TODO rename, TODO public for serialization
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
        
    public void Divide(EntityStats otherStats)
    {
      foreach (var otherStat in otherStats.Stats.Values)
      {
        this[otherStat.Kind].Divide(otherStats.Stats[otherStat.Kind].Value);
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

    public EntityStat GetStat(EntityStatKind esk)
    {
      Ensure(esk);
      return Stats[esk];
    }

    public void SetStat(EntityStatKind esk, EntityStat es)
    {
      Ensure(esk);
      Stats[esk] = es;
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
      get 
      {
        Ensure(kind);
        return Stats[kind].Value; 
      }
    }

    //public int Level
    //{
    //  get
    //  {
    //    return level;
    //  }

    //  set
    //  {
    //    level = value;
    //  }
    //}
        
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
      this[kind].Factor = value;
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

    internal void IncreaseStatFactor(EntityStatKind sk, float percent)
    {
      var inc = this[sk].TotalValue * percent / 100;
      //if (inc < 1)
      //  inc = 1;
      ChangeStatDynamicValue(sk, inc);
    }

    internal bool ChangeStatDynamicValue(EntityStatKind sk, float amount)
    {
      Ensure(sk);
      var he = Health;

      var currentValue = this[sk].CurrentValue;
      //if (currentValue < this[sk].TotalValue && amount > 0 || currentValue > 0 && amount < 0)
      {
        //eating food can not make health bigger than TotalValue
        float val = amount;
        if (sk == EntityStatKind.Health || sk == EntityStatKind.Mana)
        {
          var valMax = this[sk].TotalValue - currentValue;
          if (val > valMax)
            val = valMax;
        }

        Stats[sk].Subtract(-val);

        return true;
      }

      //return false;
    }

    internal void ResetStatFactors()
    {
      foreach (var myStat in this.Stats)
      {
        myStat.Value.Factor = 0;
      }
    }

    public void Accumulate(EntityStats otherStats)
    {
      foreach (var otherStat in otherStats.Stats)
      {
        Ensure(otherStat.Key);
        AccumulateFactor(otherStat.Key, otherStat.Value.Factor);
      }

      //experience += other.experience;
    }

    public void Increase(float fixedFactorValue)
    {
      foreach (var stat in Stats)
      {
        AccumulateFactor(stat.Key, fixedFactorValue);
      }

      //experience += other.experience;
    }

    internal void AccumulateFactors(EntityStats otherStats, bool positive)
    {
      foreach (var otherStat in otherStats.Stats)
      {
        if ((otherStat.Value.Factor > 0 && positive) || (otherStat.Value.Factor < 0 && !positive))
        {
          Ensure(otherStat.Key);
          AccumulateFactor(otherStat.Key, otherStat.Value.Factor);
        }
      }
    }

    internal void AccumulateFactor(EntityStatKind kind, float value)
    {
      Ensure(kind);
      var stat = this.Stats[kind];
      stat.Factor += value;
      if (stat.IsPercentage && stat.Value.TotalValue > 100)
      {
        var diff = stat.Value.TotalValue - 100;
        stat.Factor -= diff;
      }
    }

    internal void PrepareForSave()
    {
      var toRemove = Stats.Where(i => GetTotalValue(i.Key) == 0).ToList();
      foreach (var rem in toRemove)
        Stats.Remove(rem.Key);
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
