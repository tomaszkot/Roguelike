﻿using Roguelike.Attributes;
using Roguelike.Factors;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Roguelike.Spells
{
  public sealed class SpellStatsDescription
  {
    public int Level { get; set; }
    public int? ManaCost { get; set; }
    public int? MagicRequired { get; set; }

    public float? Damage
    {
      get => damage;
      set
      {
        damage = value;
      }
    }
    public int? Duration 
    { 
      get; 
      set; 
    }
    public EntityStatKind? StatKind { get; set; }
    public PercentageFactor StatKindPercentage { get; set; }
    List<string> extraStatDescription = new List<string>();
    private float? damage;

    SpellKind Kind { get; set; }
    public int Range { get; set; }
    public float? Durability { get; internal set; }

    List<EntityStat> entityStats;

    public SpellStatsDescription(int level, int? manaCost, int? magicRequired, SpellKind kind, int range)
    {
      Level = level;
      ManaCost = manaCost;
      MagicRequired = magicRequired;
      Kind = kind;
      Range = range;
           
    }

    public EntityStat[] GetEntityStats()
    {
      if (entityStats == null)
      {
        entityStats = new List<EntityStat>();
        if (ManaCost.HasValue)
        {
          entityStats.Add(new EntityStat() { Kind = EntityStatKind.Mana, Value = new StatValue() { Nominal = ManaCost.Value } });
        }

        if (Range > 0)
        {
          var esk = EntityStatKind.Unset;
          if (Kind == SpellKind.Teleport)
            esk = EntityStatKind.TeleportExtraRange;
          else if (Kind == SpellKind.FireBall)
            esk = EntityStatKind.FireBallExtraRange;
          else if (Kind == SpellKind.PoisonBall)
            esk = EntityStatKind.PoisonBallExtraRange;
          else if (Kind == SpellKind.IceBall)
            esk = EntityStatKind.IceBallExtraRange;
          else if (Kind == SpellKind.LightingBall)
            esk = EntityStatKind.LightingBallExtraRange;
          else if (Kind == SpellKind.SwapPosition)
            esk = EntityStatKind.SwapPositionExtraRange;
          else if (Kind == SpellKind.CrackedStone)
            esk = EntityStatKind.CrackedStoneExtraRange;
          else if (Kind == SpellKind.Frighten)
          {
            esk = EntityStatKind.FrightenExtraRange;
            
          }
          if (esk != EntityStatKind.Unset)
            entityStats.Add(new EntityStat() { Kind = esk, Value = new StatValue() { Nominal = Range } });
          else
          {
            throw new Exception("unsup Kind : "+ Kind + " ");
          }
        }


        if (Duration.HasValue)
        {

          if (Duration > 0)
          {
            var eskDur = EntityStatKind.Unset;
            if (Kind == SpellKind.Frighten)
              eskDur = EntityStatKind.FrightenDuration;
            else if (Kind == SpellKind.ManaShield)
              eskDur = EntityStatKind.ManaShieldDuration;
            else if (Kind == SpellKind.Transform)
              eskDur = EntityStatKind.TransformDuration;

            if (eskDur!= EntityStatKind.Unset)
              entityStats.Add(new EntityStat() { Kind = eskDur, Value = new StatValue() { Nominal = (float)Duration }, Unit = EntityStatUnit.Absolute });
            else
              throw new Exception("!Duration > 0 && !eskDur "+this);
          }
          else
            Debug.WriteLine("!Duration > 0");
        }
        if (Damage.HasValue)
        {
          var esk = EntityStatKind.ElementalSpellProjectilesAttack;
          if (Kind == SpellKind.Skeleton)
            esk = EntityStatKind.MeleeAttack;
          entityStats.Add(new EntityStat() { Kind = esk , Value = new StatValue() { Nominal = Damage.Value } });
        }
      }
      return entityStats.ToArray();
    }

    public void AddString(string str, bool addIndent = true)
    {
      if (addIndent)
        str = "  " + str;
      extraStatDescription.Add(str);
    }

    public string[] GetDescription(bool addIndent = true)
    {
      extraStatDescription.Clear();

      AddString("Level: " + Level, addIndent);

      if (ManaCost != null)
        AddString("Mana Cost: " + ManaCost, addIndent);
      if (Damage != null)
        AddString(Kind + " Damage: " + Damage, addIndent);
      if (Duration != null)
        AddString("Duration: " + Duration);
      if (StatKind != null)
        AddString(StatKind + " " + StatKindPercentage.ToString(), addIndent);
      if (Range > 0)
        AddString("Range: " + Range, addIndent);

      return extraStatDescription.ToArray();
    }
  }
}
