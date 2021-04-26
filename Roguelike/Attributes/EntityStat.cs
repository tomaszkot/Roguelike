using Dungeons.Core;
using System;

namespace Roguelike.Attributes
{
  public class EntityStat 
  {
    EntityStatKind kind = EntityStatKind.Unset;
    public event EventHandler<EntityStatKind> StatChanged;
    
    StatValue stat = new StatValue();
    //public static readonly EntityStatKind[] BasicStats = { EntityStatKind.Health, EntityStatKind.Magic, EntityStatKind.Mana, EntityStatKind.Attack,
    //  EntityStatKind.Defense, EntityStatKind.Dexterity };
    public bool Hidden { get; set; }
    public bool CanBeBelowZero { get; set; } = false;

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
        && sk != EntityStatKind.Defense
        && sk != EntityStatKind.Dexterity
        && sk != EntityStatKind.FireAttack
        && sk != EntityStatKind.ColdAttack
        && sk != EntityStatKind.PoisonAttack
        //&& sk != EntityStatKind.LightingAttack //TODO
        )
      {
        IsPercentage = true;
      }

      if (kind == EntityStatKind.Health)
        CanBeBelowZero = true;//death
    }

    public override string ToString()
    {
      return Kind + " " + Value.TotalValue + " (" + Value.ToString() + ")";
    }

    //public void Divide(EntityStat other)
    //{
    //  Stat.Divide(other.Stat);
    //}

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

    public static float Round(float value)
    {
      //if (value % 0.5f >= 0)
      //  return (float)Math.Ceiling(value);
      //else
      //  return (float)Math.Floor(value);
      return (float)Math.Round(value);
    }

    public string GetFormattedCurrentValue(float cv)
    {
      var val = cv.ToString();
      if (IsPercentage)
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
