﻿using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Factors;
using Roguelike.Spells;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace Roguelike.Effects
{
  public enum EffectType
  {
    Unset,

    //these are applied each turn:
    Bleeding, Poisoned, Frozen, Firing, ConsumedRawFood, ConsumedRoastedFood, BushTrap,

    //these are applied at start of effect, then removed at the end:
    Transform,
    TornApart,
    Frighten,
    Stunned,
    ManaShield, Rage, Weaken, IronSkin, ResistAll, Inaccuracy, Hooch,
    Ally
  }

  public interface ILastingEffectOwner
  {
    string Name { get; set; }

  }

  public enum EffectOrigin
  {
    Unset,
    SelfCasted,
    OtherCasted,
    External
  }
  public enum EffectApplication { Single, EachTurn }

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
        Origin = EffectOrigin.Unset;
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
    public const int DefaultPendingTurns = 3;

    public EffectApplication Application { get; set; }
    public EffectOrigin Origin { get; set; }
    public EffectType Type { get; set; }

    public EntityStatKind StatKind;
    public int PendingTurns = DefaultPendingTurns;

    //absolute value deducted/added to a stat
    public EffectiveFactor EffectiveFactor { get; set; }

    //percentage value deducted/added to a stat
    public PercentageFactor PercentageFactor { get; set; }

    //public bool FromTrapSpell { get; internal set; }
    ILastingEffectOwner owner;

    public Guid Id { get; set; }

    public Tile source;
    /// <summary>
    /// src of effect e.g. food
    /// </summary>
    public Tile Source
    {
      get { return source; }
      set
      {
        source = value;
        UniqueId = CalcUniqueId(Type, source);
      }
    }

    public static string CalcUniqueId(EffectType type, Tile source)
    {
      var id = type.ToString();
      if (source != null)
      {
        id += "_" + source.tag1;
      }

      return id;
    }

    public string UniqueId { get; private set; }

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
      }
    }

    public LastingEffect(EffectType type, ILastingEffectOwner owner, int turns, EffectOrigin origin, EffectiveFactor effectiveFactor,
                         PercentageFactor percentageFactor)
    {
      this.Id = Guid.NewGuid();
      this.Origin = origin;
      this.Type = type;
      this.Owner = owner;
      this.PendingTurns = turns;
      this.EffectiveFactor = effectiveFactor;
      this.PercentageFactor = percentageFactor;
      Application = EffectApplication.Single;

      if (type == EffectType.Bleeding ||
         type == EffectType.Poisoned ||
         type == EffectType.Frozen ||
         type == EffectType.Firing ||
         type == EffectType.ConsumedRawFood ||
         type == EffectType.ConsumedRoastedFood ||
         type == EffectType.BushTrap)
      {
        Application = EffectApplication.EachTurn;
      }
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

      if (Type == EffectType.Stunned)
        return res;

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
      else
      {
        if (StatKind != EntityStatKind.Unset)
          end = " to " + this.StatKind.ToDescription();
      }


      if (middle.Any())
        res += middle;

      //if(EffectiveFactor.EffectiveFactor.Value >= 0)
      //  res += sign;

      if (EffectiveFactor.Value != 0)
        res += EffectiveFactor;
      else
        res += PercentageFactor;

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

      if (Origin == EffectOrigin.SelfCasted)
      {
        expected = ownerName;
        expected += " casted:";
      }
      else
      {
      }

      if (Origin == EffectOrigin.OtherCasted)
      {
        //expected += Type.ToDescription();
        expected += "Spell was casted on " + ownerName;
      }
      var res = expected;
      if (res.Any())
        res += " ";
      res += le.Description;
      return res;
    }
  }
}
