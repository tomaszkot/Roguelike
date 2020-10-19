using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Factors;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System.Linq;
using System.Xml.Serialization;

namespace Roguelike.Effects
{
  public enum EffectOrigin { SelfCasted, OtherCasted, Experieced}

  public class LastingEffectCalcInfo
  {
    public EffectOrigin Origin { get; set; }
    public EffectType Type { get; set; }
    public int Turns { get; }
    
    //absolute value deducted/added to a stat
    public EffectiveFactor EffectiveFactor { get; set; }

    //percentage value deducted/added to a stat
    public PercentageFactor PercentageFactor { get; set; } = new PercentageFactor(0);

    public LastingEffectCalcInfo(EffectType type, int turns, EffectiveFactor effective, PercentageFactor perc)
    {
      Type = type;
      Origin = EffectOrigin.SelfCasted;
      if (type == EffectType.Bleeding)
        Origin = EffectOrigin.Experieced;
      else if (type == EffectType.Weaken || type == EffectType.Inaccuracy)
        Origin = EffectOrigin.OtherCasted;

      Turns = turns;
      EffectiveFactor = effective;
      PercentageFactor = perc;
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
      get { return CalcInfo.Type; }
      set 
      { 
        CalcInfo.Type = value; 
      }
    }

    public EntityStatKind StatKind;
    public int PendingTurns = 3;
    public LastingEffectCalcInfo CalcInfo { get; set; } = new LastingEffectCalcInfo(EffectType.Unset, 0, new EffectiveFactor(0), new PercentageFactor(0));
    
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
        //if (owner != null)
        //  owner.OnEffectStarted(Type);
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

    //internal void Dispose()
    //{
    //  //if (Owner != null)
    //  //  Owner.OnEffectFinished(Type);
    //}

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
      if (CalcInfo.EffectiveFactor.Value != 0)
        res += CalcInfo.EffectiveFactor;
      else
        res += CalcInfo.PercentageFactor;

      if (end.Any())
        res += end;


      return res;
    }

    public override string ToString()
    {
      return Type + ", " + Description;
    }

    public LivingEntityAction CreateAction(LastingEffect le)
    {
      var target = (Owner as LivingEntity);
      var lea = new LivingEntityAction(LivingEntityActionKind.ExperiencedEffect);
      lea.InvolvedEntity = target;
      lea.EffectType = le.Type;
      var targetName = target.Name.ToString();

      lea.Info = CreateActionInfo(le, target);

      lea.Level = ActionLevel.Important;
      return lea;
    }

    private string CreateActionInfo(LastingEffect le, LivingEntity target)
    {
      var expected = "";
      var ownerName = target.Name;

      var origin = CalcInfo.Origin;

      if (origin == EffectOrigin.SelfCasted)
      {
        expected = ownerName;
        expected += " casted:";
      }
      else if (origin == EffectOrigin.Experieced)
      {
        expected = ownerName;
        expected += " experienced:";
      }
      else if (origin == EffectOrigin.OtherCasted)
      {
        //expected += " was casted on "+ownerName;
      }

      //

      if (origin == EffectOrigin.OtherCasted)
      {
        //expected += Type.ToDescription();
        expected += "Spell was casted on " + ownerName;
      }

      return expected + " " + le.Description;
    }
  }
}
