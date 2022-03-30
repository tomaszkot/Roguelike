using Newtonsoft.Json;
using Roguelike.Calculated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Attributes
{
  public enum AttackKind
  {
    Unset,
    Melee,//sword, axe
    PhysicalProjectile,//from bow, hand
    WeaponElementalProjectile,//from wand, staff
    SpellElementalProjectile,//from scroll, book
  }

  public enum EntityStatUnit
  { 
    Unset,
    Absolute,
    Percentage
  }

  public enum EntityStatImportance
  {
    Unset,
    Normal,
    Important
  }

  //Would be nice to generate dynamically these enums
  public enum EntityStatKind
  {
    Unset = 0,

    Strength = 1, Health = 2, Magic = 3, Defense = 4, Dexterity = 5,

    Mana = 10,
    
    ResistFire = 30, ResistCold = 32, ResistPoison = 34, ResistLighting = 36,


    ChanceToMeleeHit = 50, 
    ChanceToCastSpell = 54,
    ChanceToPhysicalProjectileHit = 56,

    ChanceToEvadeMeleeAttack = 70,
    ChanceToEvadeElementalProjectileAttack = 74,
    ChanceToEvadePhysicalProjectileAttack = 76,
    
    ChanceToCauseBleeding = 80, ChanceToCauseStunning = 82, ChanceToCauseTearApart = 84, 

    ChanceToStrikeBack = 90, ChanceToBulkAttack = 92, ChanceToBurnNeighbour = 94, 
    ChanceToRepeatMelleeAttack = 100, ChanceToRepeatElementalProjectileAttack = 102, ChanceToRepeatPhysicalProjectileAttack = 104,
    ChanceToCauseElementalAilment,
    ChanceToElementalProjectileBulkAttack = 110,

    FireAttack = 120,
    ColdAttack = 124,
    PoisonAttack = 126,
    LightingAttack = 128,

    LightPower = 132, 
    LifeStealing = 134, 
    ManaStealing = 136,
    
    AxeExtraDamage = 140, SwordExtraDamage = 142, BashingExtraDamage = 144, DaggerExtraDamage = 146,
    ExlosiveCoctailExtraDamage = 148,
    PoisonCoctailExtraDamage = 149,
    ThrowingKnifeExtraDamage = 150, ThrowingStoneExtraDamage = 152,
    //    ExtraElementalDamage,
    StaffExtraElementalProjectileDamage = 160, ScepterExtraElementalProjectileDamage = 162, WandExtraElementalProjectileDamage = 164, 
    HunterTrapExtraDamage = 170, CrossbowExtraDamage = 172, BowExtraDamage = 174, 
    PhysicalProjectilesRange = 180, ElementalProjectilesRange = 182,

    //CalculatedEntityStatKind
    MeleeAttack = 1000,
    PhysicalProjectilesAttack = 1010,//from bow, stone
    ElementalSpellProjectilesAttack = 1020,//elementals from scroll, book
    ElementalWeaponProjectilesAttack = 1030,//elementals from wand, staff

    CausedBleedingDuration = 1100,

    NumberOfPiercedVictims = 1200,
    ChanceForPiercing = 1201,

    ArrowVolleyCount = 1210
  };

  public class EntityStats
  {
    Dictionary<EntityStatKind, EntityStat> stats = new Dictionary<EntityStatKind, EntityStat>();

    public EntityStats()
    {
      //var statKinds = Enum.GetValues(typeof(EntityStatKind));
      //foreach (EntityStatKind sk in statKinds)
      //{
      //  //Ensure(sk);
      //}
    }

    public void Ensure(EntityStatKind kind)
    {
      if (!stats.ContainsKey(kind))
        stats[kind] = new EntityStat(kind, 0);
    }

    [JsonProperty]
    public Dictionary<EntityStatKind, EntityStat> Stats
    {
      get
      {
        return stats;
      }
      private set
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

    public float MeleeAttack
    {
      get
      {
        return this[EntityStatKind.MeleeAttack].TotalValue;
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

    public float ChanceToMeleeHit
    {
      get
      {
        return this[EntityStatKind.ChanceToMeleeHit].TotalValue;
      }
    }

    public float Mana
    {
      get
      {
        return this[EntityStatKind.Mana].CurrentValue;
      }
    }

    public float Magic
    {
      get
      {
        return this[EntityStatKind.Magic].CurrentValue;
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
      return Stats.Values.Select(i => i.Value).ToList();
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

    public void SetNominal(EntityStatKind kind, float value)
    {
      if (kind == EntityStatKind.Health)
      {
        //int k = 0;
      }
      this[kind].Nominal = value;

      if (kind == EntityStatKind.ChanceToMeleeHit)
      {
        if (value > 100)
        {
          int k = 0;
          k++;
        }
      }
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

    internal void IncreaseDynamicStatValue(EntityStatKind sk, EntityStat percIncrease, bool useCurrentValue = true)
    {
      var inc = FactorCalculator.CalcPercentageValue(Stats[sk].GetValueToCalcPercentage(useCurrentValue), percIncrease.Factor);
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
        if (otherStat.Value.Factor != 0)
        {
          Ensure(otherStat.Key);
          AccumulateFactor(otherStat.Key, otherStat.Value.Factor);
        }
      }
    }

    public void Increase(float fixedFactorValue)
    {
      foreach (var stat in Stats)
      {
        AccumulateFactor(stat.Key, fixedFactorValue);
      }
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
      if (stat.Unit == EntityStatUnit.Percentage && stat.Value.TotalValue > 100)
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

  
}
