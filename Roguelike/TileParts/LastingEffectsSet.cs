using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using Roguelike.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      bool appAction = true;
      if (LastingEffectStarted != null)
      {
        //GameManager.Instance.AppendRedLog("call LastingEffectStarted " + le.Type);
        LastingEffectStarted(this, le);
        if (le.Type == EffectType.Rage || le.Type == EffectType.Weaken || le.Type == EffectType.IronSkin || le.Type == EffectType.ResistAll)
        {
          var info = "";
          if (le.Type == EffectType.Rage || le.Type == EffectType.ResistAll)
          {
            info = livingEntity.Name + " used " + le.Type + " spell";
          }
          else
          {
            info = "Spell " + le.Type + " was casted on " + livingEntity.Name;
          }
          AppendAction(new LivingEntityAction(LivingEntityActionKind.UsedSpell) { Info = info, EffectType = le.Type, InvolvedEntity = this.livingEntity });
          appAction = false;
        }
      }

      if (le.Type == EffectType.Bleeding || 
          le.Type == EffectType.ConsumedRawFood ||
          le.Type == EffectType.ConsumedRoastedFood)
      {
        ApplyLastingEffect(le, true);
        RemoveFinishedLastingEffects();//food might be consumed at once
      }
      else if (appAction)
        AppendEffectAction(le.Type, true);
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

    private void ApplyLastingEffect(LastingEffect eff, bool newOne)
    {
      //var damage = CalcDamageAmount(eff);
      //if (damage > 0)
      //{
      //  ReduceHealth(damage);
      //}
      var value = eff.EffectAbsoluteValue.Factor.Value;
      if (eff.StatKind != EntityStatKind.Unset)
        this.livingEntity.Stats.ChangeStatDynamicValue(eff.StatKind, value);
      //if (eff.Type == EffectType.ConsumedRawFood)
      //{
      //  //eff.Subtraction = CalcLastingEffectFactor(EntityStatKind.Health, 0);
      //  Debug.Assert(eff.EffectAbsoluteValue.Factor.Value != 0);
      //  DoConsume(eff.StatKind, eff.EffectAbsoluteValue);
      //}

      eff.PendingTurns--;
      if (eff.StatKind != EntityStatKind.Unset)
        AppendEffectAction(eff.Type, newOne, value);

      if (LastingEffectApplied != null)
        LastingEffectApplied(this, eff);

      livingEntity.DieIfShould(eff.Type);
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
        le = new LastingEffect(eff, null);
        le.PendingTurns = calcEffectValue.Turns;
        le.StatKind = esk;
        le.EffectAbsoluteValue = calcEffectValue;

        if (eff == EffectType.TornApart)//this is basically a death
          le.EffectAbsoluteValue.Factor = new LastingEffectFactor() { Value = this.livingEntity.Stats.Health };
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

    private void AppendEffectAction(EffectType eff, bool newOne, float amount = 0, bool fromHit = true)
    {
      var lea = new LivingEntityAction(LivingEntityActionKind.ExperiencedEffect);
      lea.InvolvedEntity = this.livingEntity;
      lea.EffectType = eff;
      var targetName = livingEntity.Name.ToString();

      if (eff == EffectType.ConsumedRawFood)
      {
        lea.Info += targetName + " consured: ?";
      }
      else
      {
        if (newOne)
        {
          if (fromHit && eff != EffectType.Hooch)
            lea.Info = "Hitting " + targetName + " caused effect : " + eff;
          else
            lea.Info = targetName + " experienced " + eff + " effect";
          if (amount > 0)
          {
            lea.Info += ", received damage: " + amount.Formatted();
          }
        }
        else
        {
          if (amount == 0)
            return;
          lea.Info = "Effect " + eff + " applied, " + targetName + " received damage: " + amount.Formatted();
        }
      }
      lea.Level = ActionLevel.Important;
      AppendAction(lea);
    }

    //private void AddLastingEffect(LastingEffect le)
    //{
    //  LastingEffects.Add(le);
    //  bool appAction = true;
    //  if (LastingEffectStarted != null)
    //  {
    //    //GameManager.Instance.AppendRedLog("call LastingEffectStarted " + le.Type);
    //    LastingEffectStarted(this, le);
    //    if (le.Type == EffectType.Rage || le.Type == EffectType.Weaken || le.Type == EffectType.IronSkin || le.Type == EffectType.ResistAll)
    //    {
    //      var info = "";
    //      if (le.Type == EffectType.Rage || le.Type == EffectType.ResistAll)
    //      {
    //        info = livingEntity.Name + " used " + le.Type + " spell";
    //      }
    //      else
    //      {
    //        info = "Spell " + le.Type + " was casted on " + livingEntity.Name;
    //      }
    //      AppendAction(new LivingEntityAction(LivingEntityActionKind.UsedSpell) { Info = info, EffectType = le.Type, InvolvedEntity = this.livingEntity });
    //      appAction = false;
    //    }
    //  }

    //  if (le.Type == EffectType.Bleeding || le.Type == EffectType.ConsumedRawFood ||
    //      le.Type == EffectType.ConsumedRoastedFood)//trap must add damage at start
    //  {
    //    ApplyLastingEffect(le, true);
    //    RemoveFinishedLastingEffects();//food might be consumed at once
    //  }
    //  else if (appAction)
    //    AppendEffectAction(le.Type, true);
    //}

    public virtual void RemoveLastingEffect(LivingEntity livEnt, EffectType et)
    {
      var le = livEnt.LastingEffects.FirstOrDefault(i => i.Type == et);
      if (le != null)
      {
        livEnt.LastingEffects.RemoveAll(i => i.Type == et);
        le.Dispose();

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
      var subtr = le.EffectAbsoluteValue.Factor;
      if (et == EffectType.ResistAll)
      {
        var factor = add ? subtr.Value : -subtr.Value;

        foreach (var res in resists)
        {
          this.livingEntity.Stats.GetStat(res).Subtract(-factor);
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
      var spellLasting = spell as ILastingSpell;
      var tourLasting = spellLasting != null ? spellLasting.TourLasting : 0;

      return AddPercentageLastingEffect(effectType, tourLasting, spell.StatKind, spell.StatKindFactor);
    }

    protected LastingEffectCalcInfo CreateLastingEffectCalcInfo(EffectType eff, float absoluteFactor, int turns)
    {
      if (eff == EffectType.Bleeding || eff == EffectType.Inaccuracy || eff == EffectType.Weaken)
        absoluteFactor *= -1;
      var lef = new LastingEffectCalcInfo(eff, turns, new LastingEffectFactor(absoluteFactor));

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

    public void AddBleeding(float inflicted, LivingEntity attacker)
    {
      var effectInfo = CalcLastingEffDamage(EffectType.Bleeding, inflicted, attacker, null, null);
      var turns = effectInfo.Turns;
      if (turns <= 0)
        turns = 3;//TODO
      this.AddLastingEffect(effectInfo, EntityStatKind.Health, true);
    }

    LastingEffectCalcInfo CalcLastingEffDamage(EffectType et, float amount, LivingEntity attacker = null, Spell spell = null, FightItem fi = null)
    {
      return CreateLastingEffectCalcInfo(et, amount, 3);
    }

    public LastingEffectCalcInfo CalcLastingEffectFactor(EffectType eff, EntityStatKind kind, float nominalValuePercInc, int turns)
    {
      var statValue = this.livingEntity.Stats.GetStat(kind).Value.TotalValue;
      var factor = CalcEffectValue(nominalValuePercInc, statValue);
      return CreateLastingEffectCalcInfo(eff, factor, turns);
    }

    private static float CalcEffectValue(float nominalValuePercInc, float statValue)
    {
      return statValue * nominalValuePercInc / 100f;
    }

    public virtual LastingEffect AddPercentageLastingEffect(EffectType eff, int pendingTurns, EntityStatKind esk, float nominalValuePercInc)
    {
      bool onlyProlong = LastingEffects.Any(i => i.Type == eff);//TODO is onlyProlong done ?
      var calcEffectValue = CalcLastingEffectFactor(eff, esk, nominalValuePercInc, pendingTurns);

      //if(eff == EffectType.ConsumedRawFood)
      //  lastingEffSubtractions[eff] = calcEffectValue;//AddLastingEffect uses lastingEffSubtractions so it must be set

      var le = AddLastingEffect(calcEffectValue, esk, false);
      //le.Subtraction = calcEffectValue;
      le.StatKind = esk;

      bool handle = false;
      if (eff == EffectType.Rage || eff == EffectType.Weaken || eff == EffectType.IronSkin || eff == EffectType.Inaccuracy
          )
      {
        if (!onlyProlong)
        {
          le.EffectAbsoluteValue = calcEffectValue;
          handle = true;
        }
      }
      else if (eff == EffectType.ResistAll)
      {
        var effValue = nominalValuePercInc;
        foreach (var res in resists)
        {
          var statClone = this.livingEntity.Stats.GetStat(res).Clone() as EntityStat;
          statClone.Subtract(-effValue);
          var cv = statClone.Value.CurrentValue;
          // GameManager.Instance.AppendUnityLog("resist  st = " + res + " cv = " + cv);
          while (statClone.Value.CurrentValue > 100)
          {
            effValue -= 1;
            statClone = this.livingEntity.Stats.GetStat(res).Clone() as EntityStat;
            statClone.Subtract(-effValue);
          }
        }
        le.EffectAbsoluteValue.Factor.Value = effValue;
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
