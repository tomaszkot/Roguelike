using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Factors;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System.Linq;
using System.Xml.Serialization;

namespace Roguelike.Effects
{
  public class LastingEffectCalcInfo
  {
    public EffectType Type { get; set; }
    public int Turns { get; }
    
    //absolute value deducted/added to a stat
    public EffectiveFactor EffectiveFactor { get; set; }
    
    public LastingEffectCalcInfo(EffectType type, int turns, EffectiveFactor factor)
    {
      Type = type;
      Turns = turns;
      EffectiveFactor = factor;
    }

    public override string ToString()
    {
      return Type + ", Turns:" + Turns + ", EffectiveFactor:" + EffectiveFactor;
    }
  }

  public class LastingEffect
  {
    public EffectType Type 
    { 
      get { return EffectiveFactor.Type; }
      set 
      { 
        EffectiveFactor.Type = value; 
      }
    }

    public EntityStatKind StatKind;
    public int PendingTurns = 3;
    public LastingEffectCalcInfo EffectiveFactor { get; set; } = new LastingEffectCalcInfo(EffectType.Unset, 0, new EffectiveFactor(0));
    public PercentageFactor PercentageFactor { get; set; } = new PercentageFactor(0);
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

    public bool ActivatedEachTurn 
    {
      get {
        return Type == EffectType.Bleeding || Type == EffectType.ConsumedRawFood || Type == EffectType.ConsumedRoastedFood;
      }
    }

    internal void Dispose()
    {
      if (Owner != null)
        Owner.OnEffectFinished(Type);
    }

    string description;
    public string Description 
    { 
      get 
      {
        if (description == null)
          description = GetDescription();

        return description;
      } 
    }
    
    string GetDescription()
    {
      string res = Type.ToDescription();

      var spellKind = SpellConverter.SpellKindFromEffectType(Type);
      var middle = "";
      var end = "";
      //var sign = EffectiveFactor.EffectiveFactor.Value >= 0 ? "+" : "-";

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
        middle = ", ";
        end = " to " + this.StatKind.ToDescription();
      }

      if (middle.Any())
        res += middle;

      //if(EffectiveFactor.EffectiveFactor.Value >= 0)
      //  res += sign;
      res += EffectiveFactor.EffectiveFactor;

      if (end.Any())
        res += end;


      return res;
    }

    public override string ToString()
    {
      return Type + ", " + Description;
    }

  }
}
