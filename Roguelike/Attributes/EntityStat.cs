using Roguelike.Calculated;
using Roguelike.Extensions;
using System;

namespace Roguelike.Attributes
{
  public class EntityStat
  {
    EntityStatKind kind = EntityStatKind.Unset;
    public event EventHandler<EntityStatKind> StatChanged;

    StatValue stat = new StatValue();
    public bool Hidden { get; set; }
    public bool CanBeBelowZero { get; set; } = false;

    public EntityStat() : this(EntityStatKind.Unset, 0)
    {
    }

    public EntityStat(EntityStatKind kind, float nominalValue)
    {
      this.Kind = kind;
      Value.Nominal = nominalValue;
      if (kind == EntityStatKind.ChanceToMeleeHit)
      {
        if (nominalValue > 100)
        {
          int k = 0;
          k++;
        }
      }
    }

    public EntityStatKind Kind
    {
      get { return kind; }
      set
      {
        SetKind(value);
      }
    }
    public void SetKind(EntityStatKind kind)
    {
      this.kind = kind;

      var sk = Kind;
      if (sk != EntityStatKind.Unset
        && sk != EntityStatKind.Strength
        && sk != EntityStatKind.Health
        && sk != EntityStatKind.Magic
        && sk != EntityStatKind.Mana
        && sk != EntityStatKind.MeleeAttack
        && sk != EntityStatKind.PhysicalProjectilesAttack
        && sk != EntityStatKind.ElementalSpellProjectilesAttack
        && sk != EntityStatKind.ElementalWeaponProjectilesAttack
        && sk != EntityStatKind.Defense
        && sk != EntityStatKind.Dexterity
        && sk != EntityStatKind.FireAttack
        && sk != EntityStatKind.ColdAttack
        && sk != EntityStatKind.PoisonAttack
        && sk != EntityStatKind.PhysicalProjectilesRange
        && sk != EntityStatKind.ElementalProjectilesRange
        )
      {
        Unit = EntityStatUnit.Percentage;
      }
      else
        Unit = EntityStatUnit.Absolute;

      if (kind == EntityStatKind.Health)
        CanBeBelowZero = true;//death
    }

    public override string ToString()
    {
      return Kind + " " + Value.TotalValue + " (" + Value.ToString() + ")";
    }

    public float GetValueToCalcPercentage(bool useCurrentValue)
    {
      if (this.Unit != EntityStatUnit.Absolute)
        throw new Exception("this.Unit != EntityStatUnit.Absolute "+this);
      return useCurrentValue ? Value.CurrentValue : Value.Nominal;
    }

    public float SumValueAndPercentageFactor(EntityStat factorPercentage, bool useCurrentValue)
    {
      if (factorPercentage.Unit != EntityStatUnit.Percentage)
        throw new Exception("factorPercentage.Unit != EntityStatUnit.Percentage" + factorPercentage);
      return SumValueAndPercentageFactor(factorPercentage.Value.Factor, useCurrentValue);
    }

    public float SumValueAndPercentageFactor(float factorPercentage, bool useCurrentValue)
    {
      if (Unit != EntityStatUnit.Absolute)
        throw new Exception("Unit != EntityStatUnit.Absolute " + this);

      float val = GetValueToCalcPercentage(useCurrentValue);
      return FactorCalculator.AddFactor(val, factorPercentage);
    }
    public float SumPercentageFactorAndValue(float value)
    {
      if (Unit != EntityStatUnit.Percentage)
        throw new Exception("Unit != EntityStatUnit.Percentage " + this);

      //float val = GetValueToCalcPercentage(useCurrentValue);
      return FactorCalculator.AddFactor(value, this.Factor);
    }

    public void Subtract(float amount)
    {
      var finalSubtr = stat.Subtracted + amount;
      CheckValue(finalSubtr);
      stat.Subtracted += amount;
      if (StatChanged != null)
        StatChanged(this, Kind);
    }

    private void CheckValue(float finalSubtr)
    {
      var cv = stat.CalculateCurrentValue(finalSubtr);
      if (cv < 0 && !CanBeBelowZero)
      {
        throw new Exception("Subtract, cv < 0 " + this.Kind);//thow to find bugs in the game
      }
    }

    public void SetSubtraction(float amount)
    {
      CheckValue(amount);
      stat.Subtracted = amount;
      if (StatChanged != null)
        StatChanged(this, Kind);
    }

    public float Factor
    {
      set
      {
        stat.Factor = value;
        if (Kind == EntityStatKind.Unset && value > 0)
        {
          if (value == 5)
          {
            int k = 0;
            k++;
          }
        }
        if (StatChanged != null)
          StatChanged(this, Kind);
      }
      get { return stat.Factor; }
    }

    public StatValue Value
    {
      get
      {
        return stat;
      }

      set
      {
        stat = value;
      }
    }

    public EntityStatUnit Unit { get; set; } = EntityStatUnit.Unset;
        
    public string GetFormattedCurrentValue()
    {
      return GetFormattedCurrentValue(Value.CurrentValue);
    }

    public static float Round(float value)
    {
      return value.Rounded();
    }

    public string GetFormattedCurrentValue(float cv)
    {
      var val = cv.ToString();
      if (Unit == EntityStatUnit.Percentage)
        val += " " + "%";
      else
      {
        cv = Round(cv);
        int intVal = (int)(Math.Ceiling(cv));
        val = intVal.ToString();
      }
      return val;
    }

    public object Clone()
    {
      var clone = MemberwiseClone() as EntityStat;
      clone.Value = this.Value.Clone() as StatValue;
      return clone;
    }
  }
}
