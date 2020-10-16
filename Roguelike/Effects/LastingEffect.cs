using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System.Linq;
using System.Xml.Serialization;

namespace Roguelike.Effects
{
  //public class LastingEffectFactor //Subtraction smount
  //{

  //}
  //public enum LastingEffectFactorKind { Unset, Damage,  }

  public struct LastingEffectFactor
  {
    public LastingEffectFactor(float val) { Value = val; }
    public float Value;//absolute value deducted/added to a stat

    public override string ToString()
    {
      return Value.ToString();
    }
  }

  public class LastingEffectCalcInfo
  {
    public EffectType Type;
    public int Turns;
    //public float Damage;
    //public float Subtraction//absolute value deducted/added to a stat
    //{
    //  get;
    //  set;
    //}
    public LastingEffectFactor Factor;//absolute value deducted/added to a stat

    public LastingEffectCalcInfo(EffectType type, int turns, LastingEffectFactor factor)
    {
      Type = type;
      Turns = turns;
      Factor = factor;
    }
  }

  public class LastingEffect
  {
    public EffectType Type 
    { 
      get { return EffectAbsoluteValue.Type; }
      set 
      { 
        EffectAbsoluteValue.Type = value; 
      }
    }

    public EntityStatKind StatKind;
    public int PendingTurns = 3;
    //public float DamageAmount = 0;//TODO move to EffectAbsoluteValue, add EffectAbsoluteValueKind 
    public LastingEffectCalcInfo EffectAbsoluteValue { get; set; } = new LastingEffectCalcInfo(EffectType.Unset, 0, new LastingEffectFactor(0));
    //public bool FromTrapSpell { get; internal set; }
    ILastingEffectOwner owner;

    [XmlIgnore]
    [JsonIgnore]
    public ILastingEffectOwner Owner
    {
      get
      {
        return owner;
      }

      set
      {
        owner = value;
        if (owner != null)
          owner.OnEffectStarted(Type);
      }
    }

    public LastingEffect() { }
    public LastingEffect(EffectType type, ILastingEffectOwner owner)
    {
      this.Type = type;
      this.Owner = owner;
    }

    internal void Dispose()
    {
      if (Owner != null)
        Owner.OnEffectFinished(Type);
    }

    public string GetDescription(LivingEntity owner)
    {
      string res = Type.ToDescription();
      //var damage = EffectAbsoluteValue.Factor.Value;//owner.CalcDamageAmount(this);// Owner.LivingEntityTile.CalcDamageAmount(le);

      var spellKind = SpellConverter.SpellKindFromEffectType(Type);
      var middle = "";
      var end = "";
      var sign = EffectAbsoluteValue.Factor.Value >= 0 ? "+" : "-";

      if (Type == EffectType.Bleeding)
      {
        end = " Health (per turn)";
        middle = ", ";
        //res += ", " + EffectAbsoluteValue.Factor + " Health (per turn)";
      }
      else if (Type == EffectType.ResistAll)
      {
        middle = " ";
        //res += " " + EffectAbsoluteValue.Factor;
      }
      else if (spellKind != SpellKind.Unset)
      {
        //Scroll.CreateSpell(spellKind, )
        //string preffix = "+";
        //if (Type == EffectType.Weaken || Type == EffectType.Inaccuracy)
        //  preffix = "-";
        //res += ", " + preffix + EffectAbsoluteValue.Factor + " to " + this.StatKind.ToDescription();
        middle = ", ";
        end = " to " + this.StatKind.ToDescription();
      }

      if (middle.Any())
        res += middle;

      if(EffectAbsoluteValue.Factor.Value >= 0)
        res += sign;
      res += EffectAbsoluteValue.Factor;

      if (end.Any())
        res += end;


      return res;
    }

  }
}
