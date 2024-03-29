﻿using Dungeons.Core;
using Dungeons.Tiles;
using Dungeons.Tiles.Abstract;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.Effects;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Factors;
using Roguelike.Managers;
using Roguelike.Spells;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Effects
{
  public class LastingEffectsSet
  {
    List<LastingEffect> lastingEffects = new List<LastingEffect>();
    public List<LastingEffect> LastingEffects { get => lastingEffects; set => lastingEffects = value; }

    LivingEntity livingEntity;
    [JsonIgnore]
    public LivingEntity LivingEntity 
    {
      get { return livingEntity; }
      set {
        livingEntity = value;
      }
    }

    static EntityStatKind[] resists = new EntityStatKind[] { EntityStatKind.ResistCold, EntityStatKind.ResistFire, EntityStatKind.ResistPoison, EntityStatKind.ResistLighting };
    public event EventHandler<LastingEffect> LastingEffectStarted;
    public event EventHandler<LastingEffect> LastingEffectApplied;
    public event EventHandler<LastingEffect> LastingEffectDone;

    public LastingEffectsSet(LivingEntity le, Container container)
    {
      this.Container = container;
      this.livingEntity = le;
    }

    [JsonIgnore]
    public EventsManager EventsManager
    {
      get
      {
        return Container.GetInstance<EventsManager>();
        //return livingEntity.EventsManager;
      }
    }

    bool AddLastingEffect(LastingEffect le)
    {
      if (livingEntity.IsImmuned(le.Type))
        return false;

      LastingEffects.Add(le);
      this.livingEntity.IncreateLastingEffectCounter(le.Type);

      if (le.Type != EffectType.Stunned)
      {
        if (!ApplyLastingEffect(le, true))
        {
          LastingEffects.Remove(le);
          return false;
        }
        if (le.Application != EffectApplication.EachTurn)
        {
          HandleStatSubtraction(le, true);
          if(le.Sibling!=null && le.Sibling.Application != EffectApplication.EachTurn)
            HandleStatSubtraction(le.Sibling, true);
        }
      }

      //let observers know it happened
      LastingEffectStarted?.Invoke(this, le);
      //AppendEffectAction(le);
      return true;
    }

    bool CanBeProlonged(LastingEffect le)
    {
      return le.Type != EffectType.ConsumedRawFood && le.Type != EffectType.ConsumedRoastedFood;
    }

    public LastingEffect GetByType(EffectType type)
    {
      return LastingEffects.Where(i => i.Type == type).FirstOrDefault();
    }

    public LastingEffect GetByAbilityKind(AbilityKind ak)
    {
      return LastingEffects.Where(i => i.AbilityKind == ak).FirstOrDefault();//not Single as ElementalVengeance adds 3
    }

    public bool HasEffect(AbilityKind ak)
    {
      return GetByAbilityKind(ak) != null;
    }

    public bool HasEffect(EffectType type)
    {
      return GetByType(type) != null;
    }

    public virtual LastingEffect AddLastingEffect
    (
      float percentageValue,
      EntityStatKind esk,
      int duration
    )
    {
      var effectiveFactor = livingEntity.CalcEffectiveFactor(esk, percentageValue);
      var leci = CreateLastingEffectCalcInfo(EffectType.WildRage, effectiveFactor.Value, percentageValue, duration);
      return AddLastingEffect(leci, EffectOrigin.OtherCasted, livingEntity, EntityStatKind.Strength);
    }

    public virtual LastingEffect AddLastingEffect
    (
      LastingEffectCalcInfo calcEffectValue,
      EffectOrigin origin,
      Tile source,
      EntityStatKind esk = EntityStatKind.Unset,
      bool fromHit = true,
      ILastingEffectSrc src = null
    )
    {
      var eff = calcEffectValue.Type;
      if (this.livingEntity.IsImmuned(eff))
        return null;
      var le = GetByType(eff);
      if (le == null || !CanBeProlonged(le))
      {
        le = CreateLE(calcEffectValue, origin, source, esk, eff);
        le.LastingEffectSrc = src;
        if (eff == EffectType.Hooch)
        {
          var hooch = source as Hooch;
          var percentValue = hooch.GetSecondPercentageStatIncrease().Value;
          var effectiveFactor = this.livingEntity.CalcEffectiveFactor(hooch.SecondStatKind, percentValue);
          var leci = CreateLastingEffectCalcInfo(eff, effectiveFactor.Value, percentValue, hooch.Duration);
          var leSibling = CreateLE(leci, origin, source, hooch.SecondStatKind, eff);
          le.Sibling = leSibling;
        }

        if (!AddLastingEffect(le))
          return null;
      }
      else
      {
        ProlongEffect(Math.Abs(calcEffectValue.EffectiveFactor.Value), le, calcEffectValue.Turns);
      }

      return le;
    }

    private LastingEffect CreateLE(LastingEffectCalcInfo calcEffectValue, EffectOrigin origin, Tile source, EntityStatKind esk, EffectType eff)
    {
      LastingEffect le = new LastingEffect(eff, livingEntity, calcEffectValue.Turns, origin, calcEffectValue.EffectiveFactor, calcEffectValue.PercentageFactor);
      le.Source = source;
      le.PendingTurns = calcEffectValue.Turns;
      le.StatKind = esk;

      if (eff == EffectType.TornApart)//this is basically a death
        le.EffectiveFactor = new EffectiveFactor(this.livingEntity.Stats.Health);
      return le;
    }

    private void RemoveFinishedLastingEffects()
    {
      var done = LastingEffects.Where(i => i.PendingTurns <= 0).ToList();
      foreach (var doneItem in done)
      {
        RemoveLastingEffect(doneItem);
      }
    }

    protected void AppendAction(GameEvent ac)
    {
      if (EventsManager != null)
        EventsManager.AppendAction(ac);
    }

    public virtual void ApplyLastingEffects()
    {
      //if(Container !=null)
      //  Container.GetInstance<ILogger>().LogInfo(livingEntity + " ApplyLastingEffects... "+ LastingEffects.Count);
      if (this.livingEntity is Hero)
      {
        //int k = 0;
        //k++;
      }
      EffectType eff = EffectType.Unset;

      foreach (var i in LastingEffects)
      {
        ApplyLastingEffect(i, false);

        eff = i.Type;
        if (IsHealthGone())
          break;
      };

      RemoveFinishedLastingEffects();
      livingEntity.DieIfShould(eff);
    }

    public bool IsHealthGone()
    {
      return livingEntity.IsHealthGone();
    }

    private bool ApplyLastingEffect(LastingEffect le, bool newOne)
    {
      //if(Container != null)
      //Container.GetInstance<ILogger>().LogInfo(livingEntity + " ApplyLastingEffect: " + le);

      le.PendingTurns--;

      if (le.Application == EffectApplication.EachTurn)
      {
        if (!HandleStatSubtraction(le, true))
          return false;
      }

      LastingEffectApplied?.Invoke(this, le);

      livingEntity.DieIfShould(le.Type);
      if (this.livingEntity.Alive)
      {
        if (le.Sibling != null)
          ApplyLastingEffect(le.Sibling, newOne);
      }
      return true;

    }

    private bool HandleStatSubtraction(LastingEffect le, bool add)
    {
      var value = le.EffectiveFactor.Value;

      if (!le.ChangesStats(le.Type))
        return false;

      if (le.Type == EffectType.TornApart)
        value = livingEntity.Stats.Health;

      if (le.StatKind != EntityStatKind.Unset)
      {
        var applied = livingEntity.Stats.ChangeStatDynamicValue(le.StatKind, add ? value : -value);
        if (!applied)
          return false;
        if(
          le.Type != EffectType.ManaShield &&
          (
            le.AbilityKind != Abilities.AbilityKind.ElementalVengeance
          //|| le.Type == EffectType.FireAttack //display once, TODO
          )
          )
          AppendEffectAction(le, !add);
      }

      return true;
    }

    private void AppendEffectAction(LastingEffect le, bool removed)
    {
      LivingEntityAction lea = le.CreateAction(le, removed, this.LivingEntity);
      AppendAction(lea);
    }

    

    public virtual void RemoveLastingEffect(LastingEffect le)
    {
      if (le != null)
      {
        bool removed = livingEntity.LastingEffects.Remove(le);
        Assert(removed);

        //HandleStatSubtraction(le, true);
        if (le.Application != EffectApplication.EachTurn)
        {
          try
          {
            HandleStatSubtraction(le, false);
            if (le.Sibling != null && le.Sibling.Application != EffectApplication.EachTurn)
              HandleStatSubtraction(le.Sibling, false);
          }
          catch (Exception ex)
          {
            Assert(false, ex.Message);
          }
        }

        if (le.Source is FightItem fi)
        {
          fi.SetState(FightItemState.Deactivated);
          AppendAction(new LootAction(fi, null) { Kind = LootActionKind.Deactivated });
        }

        if (le.AbilityKind != AbilityKind.Unset)
        {
          livingEntity.StartAbilityCooling(le.AbilityKind);
        }

        if (LastingEffectDone != null)
          LastingEffectDone(this, le);

      }
    }

    [JsonIgnore]
    public Container Container { get; internal set; }

    public LastingEffect AddLastingEffectFromSpell(SpellKind spellKind, EffectType effectType)
    {
      var scroll = new Scroll(spellKind);
      var spell = scroll.CreateSpell(this.livingEntity);
      return AddLastingEffectFromSpell(effectType, spell);
    }

    public LastingEffect AddLastingEffectFromSpell(EffectType effectType, Abstract.Spells.ISpell spell)
    {
      var spellLasting = spell as ILastingEffectSrc;
      if (spellLasting != null)
      {
        return AddPercentageLastingEffect(effectType, spellLasting, spell.Caller);
      }
      return null;
    }

    //protected 
    public LastingEffectCalcInfo CreateLastingEffectCalcInfo(EffectType eff, float effectiveFactor, float percentageFactor, int turns)
    {
      if (eff == EffectType.Bleeding || eff == EffectType.Inaccuracy || eff == EffectType.Weaken ||
          eff == EffectType.Poisoned || eff == EffectType.Firing || eff == EffectType.Frozen)
        effectiveFactor *= -1;
      var lef = new LastingEffectCalcInfo(eff, turns, new EffectiveFactor(effectiveFactor), new PercentageFactor(percentageFactor));

      return lef;
    }

    public LastingEffect TryAddLastingEffectOnHit(float hitAmount, LivingEntity attacker, Spell spell)
    {
      var et = SpellConverter.EffectTypeFromSpellKind(spell.Kind);

      var effectInfo = CalcLastingEffDamage(et, hitAmount, null);
      return TryAddLastingEffect(effectInfo, spell.Caller);
    }

    private LastingEffect TryAddLastingEffect(LastingEffectCalcInfo effectInfo, LivingEntity attacker = null)
    {
      LastingEffect le = null;
      if (effectInfo != null && effectInfo.Type != EffectType.Unset && !livingEntity.IsImmuned(effectInfo.Type))
      {
        var attackerChance = attacker.Stats.GetCurrentValue(EntityStatKind.ChanceToCauseElementalAilment);
        var rand = RandHelper.Random.NextDouble();
        var chanceOfVictim  = livingEntity.GetChanceToExperienceEffect(effectInfo.Type);
        chanceOfVictim += attackerChance;
        var add = rand * 100 <= chanceOfVictim;

        //TODO
        if (!add && attacker is Enemy enemy)
        {
          if (enemy.PowerKind != EnemyPowerKind.Plain)
          {
            float threshold = 0.65f;
            if (attacker.EverCausedEffect(effectInfo.Type))
              threshold = 0.85f;
            add = rand > threshold && GetByType(effectInfo.Type) == null;
            if (add)
              attacker.SetEverCaused(effectInfo.Type);
          }
        }
        //if (fightItem != null)
        //chance += fightItem.GetFactor(false);
        if (add)
        {
          le = AddLastingEffect(effectInfo, EffectOrigin.External, null, EffectTypeConverter.Convert(effectInfo.Type));
        }
      }

      return le;
    }

    public LastingEffect TryAddLastingEffectOnHit(float hitAmount, LivingEntity attacker, EntityStatKind esk)
    {
      EffectType et = EffectTypeConverter.Convert(esk);
      if (et == EffectType.Unset)
        return null;

      var effectInfo = CalcLastingEffDamage(et, hitAmount, null);
      return TryAddLastingEffect(effectInfo, attacker);
    }

    public int GetPendingTurns(EffectType et, LivingEntity attacker = null)
    {
      if (attacker is Hero hero)
      {
        if (et == EffectType.WebTrap)
        {
          var ab = attacker.GetActiveAbility(AbilityKind.WeightedNet);
          var stats = ab.GetEntityStats(true);
          var dur = stats.Where(i => i.Kind == EntityStatKind.WeightedNetDuration).FirstOrDefault();
          if (dur != null)
          {
            var val = LastingEffect.DefaultPendingTurns + (int)dur.Factor; ;
            return val; 
          }
          int k = 0;
          k++;
        }
      }
      return LastingEffect.DefaultPendingTurns;
    }

    public LastingEffect EnsureEffect(EffectType et, float inflictedDamage, LivingEntity attacker = null, int turnLasting = -1, Tile source = null)
    {
      var effectInfo = CalcLastingEffDamage(et, inflictedDamage, null, attacker);
      if(turnLasting > 0 && turnLasting > effectInfo.Turns)//HACK!
        effectInfo.Turns = turnLasting;

      if(attacker!=null)
        attacker.HandleTransformOnAttack();
      var currentEffect = this.AddLastingEffect(effectInfo, EffectOrigin.External, source, EffectTypeConverter.Convert(et), true);
      return currentEffect;
    }

    private void ProlongEffect(float inflictedDamage, LastingEffect le, int turns = 0)
    {
      var effectInfo = CalcLastingEffDamage(le.Type, inflictedDamage, null);
      le.PendingTurns = turns > 0 ? turns : GetPendingTurns(le.Type);
      le.PercentageFactor = effectInfo.PercentageFactor;
      le.EffectiveFactor = effectInfo.EffectiveFactor;
    }

    public LastingEffectCalcInfo CalcLastingEffDamage(EffectType et, float amount, FightItem fi = null, LivingEntity attacker = null)
    {
      return CreateLastingEffectCalcInfo(et, amount, 0, GetPendingTurns(et, attacker));
    }

    public LastingEffectCalcInfo CalcLastingEffectInfo(EffectType eff, ILastingEffectSrc src)
    {
      Assert(src.StatKindEffective.Value != 0 || src.StatKindPercentage.Value != 0);
      var factor = src.StatKindEffective.Value != 0 ? src.StatKindEffective : this.livingEntity.CalcEffectiveFactor(src.StatKind, src.StatKindPercentage.Value);
      return CreateLastingEffectCalcInfo(eff, factor.Value, src.StatKindPercentage.Value, src.Duration);
    }

    public virtual LastingEffect AddLastingEffect(EffectType eff, ILastingEffectSrc src, Tile effectSrc)
    {
      var calcEffectValue = CalcLastingEffectInfo(eff, src);
      var le = AddLastingEffect(calcEffectValue, EffectOrigin.SelfCasted, effectSrc, src.StatKind, false, src);
      return le;
    }

    public virtual LastingEffect AddPercentageLastingEffect(EffectType eff, ILastingEffectSrc src, Tile effectSrc)
    {
      var calcEffectValue = CalcLastingEffectInfo(eff, src);

      var origin = EffectOrigin.Unset;
      if (eff == EffectType.IronSkin || eff == EffectType.Hooch)
      {
        origin = EffectOrigin.SelfCasted;
      }
      else if (eff == EffectType.Weaken || eff == EffectType.Inaccuracy)
      {
        origin = EffectOrigin.OtherCasted;
      }
      var le = AddLastingEffect(calcEffectValue, origin, effectSrc, src.StatKind, false);

      //else if (eff == EffectType.ResistAll)
      //{
      //  ////effValue must be adjusted not to be over 100
      //  //var effValue = src.StatKindEffective.Value;
      //  //foreach (var res in resists)
      //  //{
      //  //  var original = this.livingEntity.Stats.GetStat(res);
      //  //  var statClone = original.Clone() as EntityStat;
      //  //  statClone.Subtract(-effValue);
      //  //  var cv = statClone.Value.CurrentValue;
      //  //  // GameManager.Instance.AppendUnityLog("resist  st = " + res + " cv = " + cv);
      //  //  while (statClone.Value.CurrentValue > 100)
      //  //  {
      //  //    effValue -= 1;
      //  //    statClone = original.Clone() as EntityStat;
      //  //    statClone.Subtract(-effValue);
      //  //  }
      //  //}
      //  ////update it
      //  //if (effValue != le.EffectiveFactor.Value)
      //  //  le.EffectiveFactor = new EffectiveFactor(effValue);
      //  //handle = true;
      //}
      //else if (eff == EffectType.ConsumedRoastedFood || eff == EffectType.ConsumedRawFood || eff == EffectType.Transform ||
      //  eff == EffectType.ManaShield)
      //{
      //}
      return le;
    }

    protected void Assert(bool check, string desc = "")
    {
      if (EventsManager != null)
        EventsManager.Assert(check, desc);
    }
  }
}

//Assert(lastingEffHooch == null);
//if (lastingEffHooch == null)
//{
//  lastingEffHooch = new HoochEffect();
//  lastingEffHooch.Strength = Hooch.Strength;
//  var str = GetCurrentValue(GetHoochStat());
//  lastingEffHooch.StrengthAbsoluteValue = str * Hooch.Strength / 100f;
//  lastingEffHooch.ChanceToHit -= Hooch.ChanceToHit;

//  ApplyHoochEffects(true);
//  if (HoochApplied != null)
//    HoochApplied(this, EventArgs.Empty);
//}


//if (et == EffectType.Hooch)
//{
//  ApplyHoochEffects(false);
//  lastingEffHooch = null;
//}