using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
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
        return livingEntity.EventsManager;
      }
    }

    void AddLastingEffect(LastingEffect le)
    {
      //le.PendingTurns = 50;
      LastingEffects.Add(le);

      if (le.Type != EffectType.Stunned)
      {
        ApplyLastingEffect(le, true);
        if (le.Application != EffectApplication.EachTurn)
          HandleStatSubtraction(le, true);
      }

      //let observers know it happened
      LastingEffectStarted?.Invoke(this, le);
      //AppendEffectAction(le);
    }

    bool CanBeProlonged(LastingEffect le)
    {
      return le.Type != EffectType.ConsumedRawFood && le.Type != EffectType.ConsumedRoastedFood;
    }

    public LastingEffect GetByType(EffectType type)
    {
      return LastingEffects.Where(i => i.Type == type).FirstOrDefault();
    }

    public bool HasEffect(EffectType type)
    {
      return GetByType(type) != null;
    }

    public virtual LastingEffect AddLastingEffect
    (
      LastingEffectCalcInfo calcEffectValue,
      EffectOrigin origin,
      Tile source,
      EntityStatKind esk = EntityStatKind.Unset,
      bool fromHit = true
    )
    {
      var eff = calcEffectValue.Type;
      if (this.livingEntity.IsImmuned(eff))
        return null;
      var le = GetByType(eff);
      if (le == null || !CanBeProlonged(le))
      {
        le = new LastingEffect(eff, livingEntity, calcEffectValue.Turns, origin, calcEffectValue.EffectiveFactor, calcEffectValue.PercentageFactor);
        le.Source = source;
        le.PendingTurns = calcEffectValue.Turns;
        le.StatKind = esk;

        if (eff == EffectType.TornApart)//this is basically a death
          le.EffectiveFactor = new EffectiveFactor(this.livingEntity.Stats.Health);
        AddLastingEffect(le);
      }
      else
      {
        ProlongEffect(Math.Abs(calcEffectValue.EffectiveFactor.Value), le, calcEffectValue.Turns);
      }

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
        int k = 0;
        k++;
      }
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
      //if(Container != null)
      //Container.GetInstance<ILogger>().LogInfo(livingEntity + " ApplyLastingEffect: " + le);

      le.PendingTurns--;

      if (le.Application == EffectApplication.EachTurn)
      {
        HandleStatSubtraction(le, true);
      }

      LastingEffectApplied?.Invoke(this, le);

      livingEntity.DieIfShould(le.Type);
    }

    private void HandleStatSubtraction(LastingEffect le, bool add)
    {
      var value = le.EffectiveFactor.Value;

      if (le.Type == EffectType.Stunned)
        return;

      var esk = le.StatKind != EntityStatKind.Unset || le.Type == EffectType.ResistAll
        || le.Type == EffectType.Transform;
      Assert(esk);
      if (le.Type == EffectType.TornApart)
        value = livingEntity.Stats.Health;

      if (le.StatKind != EntityStatKind.Unset)
      {
        livingEntity.Stats.ChangeStatDynamicValue(le.StatKind, add ? value : -value);
        if(le.Type != EffectType.ManaShield)
          AppendEffectAction(le, !add);
      }
    }

    private void AppendEffectAction(LastingEffect le, bool removed)
    {
      LivingEntityAction lea = CreateAction(le, removed);
      AppendAction(lea);
    }

    public LivingEntityAction CreateAction(LastingEffect le, bool removed)
    {
      var lea = new LivingEntityAction(removed ? LivingEntityActionKind.EffectFinished : LivingEntityActionKind.ExperiencedEffect);
      lea.InvolvedEntity = this.livingEntity;
      lea.EffectType = le.Type;
      var targetName = livingEntity.Name.ToString();

      lea.Info = targetName + " " + le.Description;
      lea.Level = ActionLevel.Important;
      return lea;
    }

    public virtual void RemoveLastingEffect(LastingEffect le)
    {
      if (le != null)
      {
        bool removed = livingEntity.LastingEffects.Remove(le);
        Assert(removed);

        //HandleStatSubtraction(le, true);
        if (le.Application != EffectApplication.EachTurn)
          HandleStatSubtraction(le, false);

        if (LastingEffectDone != null)
          LastingEffectDone(this, le);
      }
    }

    [JsonIgnore]
    public Container Container { get; internal set; }

    public LastingEffect AddLastingEffectFromSpell(SpellKind spellKind, EffectType effectType)
    {
      var spell = Scroll.CreateSpell(spellKind, this.livingEntity);
      var spellLasting = spell as ILastingEffectSrc;
      if (spellLasting != null)
      {
        return AddPercentageLastingEffect(effectType, spellLasting, spell.Caller);
      }
      //else if (spellKind == SpellKind.Teleport)
      //{
      //  var effectInfo = CalcLastingEffDamage(EffectType.Unset, 0, null, null);
      //  return AddLastingEffect(effectInfo, EffectOrigin.SelfCasted, livingEntity);
      //}
      return null;
    }

    protected LastingEffectCalcInfo CreateLastingEffectCalcInfo(EffectType eff, float effectiveFactor, float percentageFactor, int turns)
    {
      if (eff == EffectType.Bleeding || eff == EffectType.Inaccuracy || eff == EffectType.Weaken ||
          eff == EffectType.Poisoned || eff == EffectType.Firing || eff == EffectType.Frozen)
        effectiveFactor *= -1;
      var lef = new LastingEffectCalcInfo(eff, turns, new EffectiveFactor(effectiveFactor), new PercentageFactor(percentageFactor));

      return lef;
    }

    public LastingEffect TryAddLastingEffectOnHit(float hitAmount, LivingEntity attacker, Spell spell)
    {
      var effectInfo = CalcLastingEffDamage(EffectType.Unset, hitAmount, spell, null);
      return TryAddLastingEffect(effectInfo);
    }

    private LastingEffect TryAddLastingEffect(LastingEffectCalcInfo effectInfo, LivingEntity attacker = null)
    {
      LastingEffect le = null;
      if (effectInfo != null && effectInfo.Type != EffectType.Unset && !livingEntity.IsImmuned(effectInfo.Type))
      {
        var rand = RandHelper.Random.NextDouble();
        var chance = livingEntity.GetChanceToExperienceEffect(effectInfo.Type);
        var add = rand * 100 <= chance;
        if (!add && attacker is Enemy enemy)
        {
          if (enemy.PowerKind != EnemyPowerKind.Plain)
          {
            float threshold = 0.65f;
            if (attacker.EverCausedHero(effectInfo.Type))
              threshold = 0.85f;
            add = rand > threshold && GetByType(effectInfo.Type) == null;
            if (add)
              attacker.SetEverCausedHero(effectInfo.Type);
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

      var effectInfo = CalcLastingEffDamage(et, hitAmount, null, null);
      return TryAddLastingEffect(effectInfo, attacker);
    }

    public static int GetPendingTurns(EffectType et)
    {
      return LastingEffect.DefaultPendingTurns;
    }

    public LastingEffect EnsureEffect(EffectType et, float inflictedDamage, LivingEntity attacker)
    {
      var effectInfo = CalcLastingEffDamage(et, inflictedDamage, null, null);

      var currentEffect = this.AddLastingEffect(effectInfo, EffectOrigin.External, null, EffectTypeConverter.Convert(et), true);
      return currentEffect;
    }

    private void ProlongEffect(float inflictedDamage, LastingEffect le, int turns = 0)
    {
      var effectInfo = CalcLastingEffDamage(le.Type, inflictedDamage, null, null);
      le.PendingTurns = turns > 0 ? turns : LastingEffectsSet.GetPendingTurns(le.Type);
      le.PercentageFactor = effectInfo.PercentageFactor;
      le.EffectiveFactor = effectInfo.EffectiveFactor;
    }

    LastingEffectCalcInfo CalcLastingEffDamage(EffectType et, float amount, Spell spell = null, FightItem fi = null)
    {
      return CreateLastingEffectCalcInfo(et, amount, 0, GetPendingTurns(et));
    }

    public LastingEffectCalcInfo CalcLastingEffectInfo(EffectType eff, ILastingEffectSrc src)
    {
      Assert(src.StatKindEffective.Value != 0 || src.StatKindPercentage.Value != 0);
      var factor = src.StatKindEffective.Value != 0 ? src.StatKindEffective : this.livingEntity.CalcEffectiveFactor(src.StatKind, src.StatKindPercentage.Value);
      return CreateLastingEffectCalcInfo(eff, factor.Value, src.StatKindPercentage.Value, src.TourLasting);
    }

    public virtual LastingEffect AddPercentageLastingEffect(EffectType eff, ILastingEffectSrc src, Tile effectSrc)
    {
      var calcEffectValue = CalcLastingEffectInfo(eff, src);

      var origin = EffectOrigin.Unset;
      if (eff == EffectType.Rage || eff == EffectType.IronSkin)
      {
        origin = EffectOrigin.SelfCasted;
      }
      else if (eff == EffectType.Weaken || eff == EffectType.Inaccuracy)
      {
        origin = EffectOrigin.OtherCasted;
      }
      var le = AddLastingEffect(calcEffectValue, origin, effectSrc, src.StatKind, false);

      //bool handle = false;
      //if (eff == EffectType.Rage || eff == EffectType.Weaken || eff == EffectType.IronSkin || eff == EffectType.Inaccuracy)
      //{
      //  //if (!onlyProlong)
      //  //{
      //  //  //le.CalcInfo = calcEffectValue;
      //  //  handle = true;
      //  //}
      //}
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

      //else
      //  Assert(false, "AddLastingEffect - unhandeled eff = " + eff);

      //if (handle)
      //{
      //  HandleSpecialFightStat(le, true);
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