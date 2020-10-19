﻿using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Factors;
using Roguelike.Managers;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.TileParts
{
  /// <summary>
  /// TODO declare LivingEntity as partial and this class as inner of it
  /// </summary>
  public class LastingEffectsSet
  {
    List<LastingEffect> lastingEffects = new List<LastingEffect>();
    public List<LastingEffect> LastingEffects { get => lastingEffects; set => lastingEffects = value; }
    LivingEntity livingEntity;
    static EntityStatKind[] resists = new EntityStatKind[] { EntityStatKind.ResistCold, EntityStatKind.ResistFire, EntityStatKind.ResistPoison, EntityStatKind.ResistLighting };
    public event EventHandler<LastingEffect> LastingEffectStarted;
    public event EventHandler<LastingEffect> LastingEffectApplied;
    public event EventHandler<LastingEffect> LastingEffectDone;

    public LastingEffectsSet(LivingEntity le)
    {
      this.livingEntity = le;
    }

    EventsManager eventsManager;
    [JsonIgnore]
    public EventsManager EventsManager
    {
      get { return eventsManager; }
      set { eventsManager = value; }
    }

    public void AddLastingEffect(LastingEffect le)
    {
      LastingEffects.Add(le);
      LastingEffectStarted?.Invoke(this, le);

      if (le.AppliedEachTurn)
      {
        ApplyLastingEffect(le, true);
        RemoveFinishedLastingEffects();//food might be consumed at once
      }
      AppendEffectAction(le);
    }

    private void RemoveFinishedLastingEffects()
    {
      var done = LastingEffects.Where(i => i.PendingTurns <= 0).ToList();
      foreach (var doneItem in done)
      {
        RemoveLastingEffect(this.livingEntity, doneItem.Type);
      }
    }

    protected void AppendAction(GameAction ac)
    {
      if (EventsManager != null)
        EventsManager.AppendAction(ac);
    }

    public virtual void ApplyLastingEffects()
    {
      EffectType eff = EffectType.Unset;

      foreach (var i in LastingEffects)
      {
        ApplyLastingEffect(i, false);

        eff = i.Type;
        if (IsHealthZero())
          break;
      };

      RemoveFinishedLastingEffects();
      livingEntity.DieIfShould(eff);
    }

    public bool IsHealthZero()
    {
      return livingEntity.IsHealthZero();
    }

    private void ApplyLastingEffect(LastingEffect le, bool newOne)
    {
      le.PendingTurns--;
            
      if (newOne || le.AppliedEachTurn)
      {
        var value = le.CalcInfo.EffectiveFactor.Value;
        Assert(le.StatKind != EntityStatKind.Unset);
        this.livingEntity.Stats.ChangeStatDynamicValue(le.StatKind, value);
        AppendEffectAction(le);
      }

      LastingEffectApplied?.Invoke(this, le);

      livingEntity.DieIfShould(le.Type);
    }

    public virtual LastingEffect AddLastingEffect
    (
      LastingEffectCalcInfo calcEffectValue,
      EntityStatKind esk = EntityStatKind.Unset,
      bool fromHit = true

    )
    {
      var eff = calcEffectValue.Type;
      if (this.livingEntity.IsImmuned(eff))
        return null;
      var le = LastingEffects.Where(i => i.Type == eff).FirstOrDefault();
      if (le == null)
      {
        le = new LastingEffect(eff, this.livingEntity);
        le.PendingTurns = calcEffectValue.Turns;
        le.StatKind = esk;
        le.CalcInfo = calcEffectValue;

        if (eff == EffectType.TornApart)//this is basically a death
          le.CalcInfo.EffectiveFactor = new EffectiveFactor(this.livingEntity.Stats.Health);
        else if (eff == EffectType.Hooch)
        {
          //TODO merge from old
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
        }
        AddLastingEffect(le);
      }
      else
      {
        le.PendingTurns = calcEffectValue.Turns;

        if (LastingEffectStarted != null)
        {
          ////make sure gui shows it - after load ManaShield was not visible
          //if (eff == EffectType.ManaShield)
          //  LastingEffectStarted(this, new GenericEventArgs<LastingEffect>(le));
        }
      }

      return le;
    }

    private void AppendEffectAction(LastingEffect le)
    {
      LivingEntityAction lea = CreateAction(le);
      AppendAction(lea);
    }

    public LivingEntityAction CreateAction(LastingEffect le)
    {
      var lea = new LivingEntityAction(LivingEntityActionKind.ExperiencedEffect);
      lea.InvolvedEntity = this.livingEntity;
      lea.EffectType = le.Type;
      var targetName = livingEntity.Name.ToString();

      lea.Info = targetName + " " + le.Description;
      lea.Level = ActionLevel.Important;
      return lea;
    }

    public virtual void RemoveLastingEffect(LivingEntity livEnt, EffectType et)
    {
      var le = livEnt.LastingEffects.FirstOrDefault(i => i.Type == et);
      if (le != null)
      {
        livEnt.LastingEffects.RemoveAll(i => i.Type == et);
        //le.Dispose();

        HandleSpecialFightStat(le, false);
        //TODO
        //if (et == EffectType.Hooch)
        //{
        //  ApplyHoochEffects(false);
        //  lastingEffHooch = null;
        //}

        if (livEnt == livingEntity && LastingEffectDone != null)
          LastingEffectDone(this, le);
      }
    }

    //For the time of lasting effect some state is changed, then restored to the original value (flag add)
    public void HandleSpecialFightStat(LastingEffect le, bool add)
    {
      var et = le.Type;
      if (et == EffectType.ConsumedRawFood || et == EffectType.ConsumedRoastedFood)
        return;
      var subtr = le.CalcInfo.EffectiveFactor;
      if (et == EffectType.ResistAll)
      {
        var factor = add ? subtr.Value : -subtr.Value;

        foreach (var res in resists)
        {
          var stat = this.livingEntity.Stats.GetStat(res);
          stat.Subtract(-factor);
        }
        return;
      }

      EntityStatKind esk = le.StatKind;

      if (esk != EntityStatKind.Unset)
      {
        var factor = add ? subtr.Value : -subtr.Value;
        if (et == EffectType.Weaken || et == EffectType.Inaccuracy)
        {
          factor *= -1;
        }
        var st = this.livingEntity.Stats.GetStat(esk);
        st.Subtract(-factor);
        //st = this.Stats.Stats[esk];
        // //Debug.WriteLine(" st = "+ st);
      }
    }

    public LastingEffect AddLastingEffectFromSpell(SpellKind spellKind, EffectType effectType)
    {
      var spell = Scroll.CreateSpell(spellKind, this.livingEntity);
      var spellLasting = spell as ILastingEffectSrc;
      if (spellLasting != null)
      {
        return AddPercentageLastingEffect(effectType, spellLasting);//spellLasting.TourLasting, spellLasting.StatKind, spellLasting.StatKindPercentage.Value);
      }

      return null;
    }

    protected LastingEffectCalcInfo CreateLastingEffectCalcInfo(EffectType eff, float effectiveFactor, float percentageFactor, int turns)
    {
      if (eff == EffectType.Bleeding || eff == EffectType.Inaccuracy || eff == EffectType.Weaken)
        effectiveFactor *= -1;
      var lef = new LastingEffectCalcInfo(eff, turns, new EffectiveFactor(effectiveFactor), new PercentageFactor(percentageFactor));

      return lef;
    }

    public LastingEffectCalcInfo TryAddLastingEffect(float amount, LivingEntity attacker, Spell spell)
    {
      var effectInfo = CalcLastingEffDamage(EffectType.Unset, amount, attacker, spell, null);
      if (effectInfo.Type != EffectType.Unset && ! livingEntity.IsImmuned(effectInfo.Type))
      {
        var rand = RandHelper.Random.NextDouble();
        var chance = livingEntity.GetChanceToExperienceEffect(effectInfo.Type);
        if (spell != null)
        {
          if (spell.SendByGod && spell.Kind != SpellKind.LightingBall)
            chance *= 2;

        }
        //if (fightItem != null)
        //chance += fightItem.GetFactor(false);
        if (rand * 100 <= chance)
        {
          this.AddLastingEffect(effectInfo);
          //AppendEffectAction(effectInfo.Type, true); duplicated message
        }
      }

      return effectInfo;
    }

    public LastingEffect AddBleeding(float inflicted, LivingEntity attacker)
    {
      var effectInfo = CalcLastingEffDamage(EffectType.Bleeding, inflicted, attacker, null, null);
      var turns = effectInfo.Turns;
      if (turns <= 0)
        turns = 3;//TODO
      var le = this.AddLastingEffect(effectInfo, EntityStatKind.Health, true);
      return le;
    }

    LastingEffectCalcInfo CalcLastingEffDamage(EffectType et, float amount, LivingEntity attacker = null, Spell spell = null, FightItem fi = null)
    {
      return CreateLastingEffectCalcInfo(et, amount, 0, 3);
    }

    public LastingEffectCalcInfo CalcLastingEffectInfo(EffectType eff, ILastingEffectSrc src)
    {
      var factor = src.StatKindEffective.Value != 0 ? src.StatKindEffective : this.livingEntity.CalcEffectiveFactor(src.StatKind, src.StatKindPercentage.Value);
      return CreateLastingEffectCalcInfo(eff, factor.Value, src.StatKindPercentage.Value, src.TourLasting);
    }
        
    public virtual LastingEffect AddPercentageLastingEffect(EffectType eff, ILastingEffectSrc src)
    {
      bool onlyProlong = LastingEffects.Any(i => i.Type == eff);//TODO is onlyProlong done ?
      var calcEffectValue = CalcLastingEffectInfo(eff, src);
      var le = AddLastingEffect(calcEffectValue, src.StatKind, false);
      
      bool handle = false;
      if (eff == EffectType.Rage || eff == EffectType.Weaken || eff == EffectType.IronSkin || eff == EffectType.Inaccuracy)
      {
        if (!onlyProlong)
        {
          le.CalcInfo = calcEffectValue;
          handle = true;
        }
      }
      else if (eff == EffectType.ResistAll)
      {
        //effValue must be adjusted not to be over 100
        var effValue = src.StatKindEffective.Value;
        foreach (var res in resists)
        {
          var original = this.livingEntity.Stats.GetStat(res);
          var statClone = original.Clone() as EntityStat;
          statClone.Subtract(-effValue);
          var cv = statClone.Value.CurrentValue;
          // GameManager.Instance.AppendUnityLog("resist  st = " + res + " cv = " + cv);
          while (statClone.Value.CurrentValue > 100)
          {
            effValue -= 1;
            statClone = original.Clone() as EntityStat;
            statClone.Subtract(-effValue);
          }
        }
        //update it
        if(effValue!= le.CalcInfo.EffectiveFactor.Value)
          le.CalcInfo.EffectiveFactor = new EffectiveFactor(effValue);
        handle = true;
      }
      else if (eff == EffectType.ConsumedRoastedFood || eff == EffectType.ConsumedRawFood)
      {
      }
      else
        Assert(false, "AddLastingEffect - unhandeled eff = " + eff);

      if (handle)
      {
        HandleSpecialFightStat(le, true);
      }
      return le;
    }

    protected void Assert(bool check, string desc = "")
    {
      if (EventsManager != null)
        EventsManager.Assert(check, desc);
    }
  }
}
