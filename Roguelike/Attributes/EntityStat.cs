using Dungeons.Core;
using System;

namespace Roguelike.Attributes
{
  public class EntityStat 
  {
    EntityStatKind kind = EntityStatKind.Unset;
    public event EventHandler<EntityStatKind> StatChanged;
    
    StatValue stat = new StatValue();
    public static readonly EntityStatKind[] BasicStats = { EntityStatKind.Health, EntityStatKind.Magic, EntityStatKind.Mana, EntityStatKind.Attack,
      EntityStatKind.Defence, EntityStatKind.Dexterity };
    public bool Hidden { get; set; }

    public EntityStat() : this(EntityStatKind.Unset, 0)
    {
    }

    public EntityStat(EntityStatKind kind, float nominalValue)
    {
      this.Kind = kind;
      Value.Nominal = nominalValue;
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
      if (sk != EntityStatKind.Unset
        && sk != EntityStatKind.Strength
        && sk != EntityStatKind.Health
        && sk != EntityStatKind.Magic
        && sk != EntityStatKind.Mana
        && sk != EntityStatKind.Attack
        && sk != EntityStatKind.Defence
        && sk != EntityStatKind.Dexterity
        && sk != EntityStatKind.FireAttack
        && sk != EntityStatKind.ColdAttack
        && sk != EntityStatKind.PoisonAttack
        //&& sk != EntityStatKind.LightingAttack //TODO
        )
      {
        IsPercentage = true;
      }
    }

    public override string ToString()
    {
      return Kind + " " + Value.TotalValue + " (" + Value.ToString() + ")";
    }

    //public void Divide(EntityStat other)
    //{
    //  Stat.Divide(other.Stat);
    //}

    //public void MakeNegative()
    //{
    //  Stat.MakeNegative();
    //}

    //public void Accumulate(EntityStat other)
    //{
    //  Stat.Accumulate(other.Stat);
    //}

    //public void Divide(float value)
    //{
    //  Stat.Divide(value);
    //}
        
    //public float TotalValue
    //{
    //  get { return stat.TotalValue; }
    //}

    //public float NominalValue
    //{
    //  get { return stat.Nominal; }
    //  set { stat.Nominal = value; }
    //}

    //public float CurrentValue
    //{
    //  get { return stat.CurrentValue; }
    //}

    public void Subtract(float amount)
    {
      stat.Subtracted += amount;
      if (StatChanged != null)
        StatChanged(this, Kind);
    }

    public void SetSubtraction(float amount)
    {
      stat.Subtracted = amount;
      if (StatChanged != null)
        StatChanged(this, Kind);
    }

    public float Factor
    {
      set
      {
        stat.Factor = value;
        //   if (Kind == EntityStatKind.Health && stat.Factor > 0) WTF
        //stat.Factor = 0;//?TODO 
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

    public bool IsPercentage { get; set; }
    //public StatValue Stat1 { get => stat; set => stat = value; }

    public string GetFormattedCurrentValue()
    {
      var val = Value.CurrentValue.ToString();
      if (IsPercentage)
        val +=  " " + "%";
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
