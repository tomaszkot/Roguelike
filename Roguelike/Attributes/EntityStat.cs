using Dungeons.Core;
using System;

namespace Roguelike
{
  public class EntityStat : ICloneable
  {
    EntityStatKind kind = EntityStatKind.Unknown;
    GenericEventArgs<EntityStatKind> args = new GenericEventArgs<EntityStatKind>();
    public event EventHandler<GenericEventArgs<EntityStatKind>> StatChanged;
    
    StatValue stat = new StatValue();
    public static readonly EntityStatKind[] BasicStats = { EntityStatKind.Health, EntityStatKind.Magic, EntityStatKind.Mana, EntityStatKind.Attack, EntityStatKind.Defence };
    public bool Hidden { get; set; }

    public EntityStat() : this(EntityStatKind.Unknown, 0)
    {
    }

    public EntityStat(EntityStatKind kind, float nominalValue)
    {
      this.Kind = kind;
      args.EventData = kind;
      Stat.NominalValue = nominalValue;
    }
    
    public EntityStatKind Kind {
      get { return kind; }
      set {
        SetKind(value);
      }
    }
    public void SetKind(EntityStatKind kind)
    {
      this.kind = kind;

      var sk = Kind;
      if (sk != EntityStatKind.Unknown
        && sk != EntityStatKind.Strength
        && sk != EntityStatKind.Health
        && sk != EntityStatKind.Magic
        && sk != EntityStatKind.Mana
        && sk != EntityStatKind.Attack
        && sk != EntityStatKind.Defence
        && sk != EntityStatKind.FireAttack
        && sk != EntityStatKind.ColdAttack
        && sk != EntityStatKind.PoisonAttack
        && sk != EntityStatKind.LightingAttack
        )
      {
        IsPercentage = true;
      }
    }

    public override string ToString()
    {
      return Kind + " " + TotalValue + " (" + Stat.ToString() + ")";
    }

    public void Divide(EntityStat other)
    {
      Stat.Divide(other.Stat);
    }

    public void MakeNegative()
    {
      Stat.MakeNegative();
    }

    public void Accumulate(EntityStat other)
    {
      Stat.Accumulate(other.Stat);
    }

    public void Divide(float value)
    {
      Stat.Divide(value);
    }

    public object Clone()
    {
      var clone = MemberwiseClone() as EntityStat;
      clone.Stat = this.Stat.Clone() as StatValue;
      return clone;

    }

    public float TotalValue
    {
      get { return stat.TotalValue; }
    }

    public float NominalValue
    {
      get { return stat.NominalValue; }
      set { stat.NominalValue = value; }
    }

    public float CurrentValue
    {
      get { return stat.CurrentValue; }
    }

    public void Subtract(float amount)
    {
      stat.Subtracted += amount;
      if (StatChanged != null)
        StatChanged(this, args);
    }

    public void SetSubtraction(float amount)
    {
      stat.Subtracted = amount;
      if (StatChanged != null)
        StatChanged(this, args);
    }

    public float Factor
    {
      set
      {
        stat.Factor = value;
        //   if (Kind == EntityStatKind.Health && stat.Factor > 0) WTF
        //stat.Factor = 0;//?TODO 
        if (StatChanged != null)
          StatChanged(this, args);
      }
      get { return stat.Factor; }
    }

    public StatValue Stat
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

    public bool IsPercentage { get; set; }

    public string GetFormattedCurrentValue()
    {
      var val = CurrentValue.ToString();
      if (IsPercentage)
        val +=  " " + "%";
      return val;
    }

  }
}
