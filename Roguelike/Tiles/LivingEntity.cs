using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;
using Roguelike.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Roguelike.Tiles
{
  public enum EntityState { Idle, Moving, Attacking, CastingSpell }
  public enum EffectType
  {
    None, Bleeding, Poisoned, Frozen, Firing, Transform, TornApart, Frighten, Stunned,
    ManaShield, BushTrap, Rage, Weaken, IronSkin, ResistAll, Inaccuracy, Hooch, ConsumedFood
  }
    
  public interface ILastingEffectOwner
  {
    void OnEffectFinished(EffectType type);
    void OnEffectStarted(EffectType type);
  }

  class LastingEffectCalcInfo
  {
    public EffectType Type;
    public int Turns;
    public float Damage;
  }

  public class LivingEntity : Tile
  {
    static Dictionary<EntityStatKind, EntityStatKind> statsHitIncrease = new Dictionary<EntityStatKind, EntityStatKind> {
                { EntityStatKind.LifeStealing, EntityStatKind.Health },
                { EntityStatKind.ManaStealing, EntityStatKind.Mana }
    };

    public static Point DefaultInitialPoint = new Point(0, 0);
    public Point PrevPoint;
    public Point InitialPoint = DefaultInitialPoint;
    EntityStats stats = new EntityStats();
    Dictionary<EffectType, float> chanceToExperienceEffect = new Dictionary<EffectType, float>();

    public EffectType DiedOfEffect;
    public EntityState state;
    public EntityState State
    {
      get { return state; }
      set
      {
        state = value;
        //AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.Assert, Info = Name + " state = "+ state });
      }
    }
    List<Algorithms.PathFinderNode> pathToTarget;
    List<LastingEffect> lastingEffects = new List<LastingEffect>();
    protected List<EffectType> immunedEffects = new List<EffectType>();

    [JsonIgnore]
    public List<LivingEntity> EverHitBy { get; set; } = new List<LivingEntity>();
    //public static Func<SpellCastPolicy> spellCastPolicyProvider;

    bool alive = true;
    //[JsonIgnoreAttribute]
    public EntityStats Stats { get => stats; set => stats = value; }

    Dictionary<EffectType, float> lastingEffSubtractions = new Dictionary<EffectType, float>();
    static EntityStatKind[] resists = new EntityStatKind[] { EntityStatKind.ResistCold, EntityStatKind.ResistFire, EntityStatKind.ResistPoison,EntityStatKind.ResistLighting};
    public event EventHandler<LastingEffect> LastingEffectStarted;
    public event EventHandler<LastingEffect> LastingEffectApplied;
    public event EventHandler<LastingEffect> LastingEffectDone;
    public bool IsWounded { get; private set; }

    static LivingEntity()
    {
      //spellCastPolicyProvider = () => { return new SpellCastPolicy(); };
    }

    public LivingEntity():this(new Point(-1, -1), '\0')
    {
    }

    public LivingEntity(Point point, char symbol) : base(point, symbol)
    {
      Alive = true;
      Name = "";
      Stats.SetNominal(EntityStatKind.ChanceToHit, 75);
      Stats.SetNominal(EntityStatKind.ChanceToCastSpell, 75);
      
      var effectTypes = Enum.GetValues(typeof(EffectType)).Cast<EffectType>().ToList();
      foreach (var et in effectTypes)
      {
        var chance = GetDefaultChanceToExperienceEffect();
        if (et == EffectType.Bleeding || et == EffectType.TornApart)
          chance = 100;
        if (et == EffectType.Stunned)
          chance = 90;
        SetChanceToExperienceEffect(et, chance);
      }

      lastingEffSubtractions[EffectType.Rage] = 0;
      lastingEffSubtractions[EffectType.Weaken] = 0;
      lastingEffSubtractions[EffectType.Inaccuracy] = 0;
      lastingEffSubtractions[EffectType.IronSkin] = 0;
      lastingEffSubtractions[EffectType.ResistAll] = 0;
      lastingEffSubtractions[EffectType.ConsumedFood] = 0;
    }

    public virtual void SetChanceToExperienceEffect(EffectType et, int chance)
    {
      chanceToExperienceEffect[et] = chance;
    }

    //public int ActionsDoneInTurn { get; set; }
    protected virtual int GetDefaultChanceToExperienceEffect() { return 10; }

    public void ReduceMana(float amount)
    {
      Stats.GetStat(EntityStatKind.Mana).Subtract(amount);
    }

    public static LivingEntity CreateDummy()
    {
      return new LivingEntity(new Point(0, 0), '\0');
    }
    
    [JsonIgnore]
    public List<Algorithms.PathFinderNode> PathToTarget
    {
      get
      {
        return pathToTarget;
      }

      set
      {
        pathToTarget = value;
      }
    }
    
    public virtual bool Alive
    {
      get { return alive; }
      set
      {
        if (alive != value)
        {
          alive = value;
          if (!alive)
          {
            if (eventsManager == null)
              throw new Exception("eventsManager == null "+this);
            AppendAction(new LivingEntityAction(LivingEntityActionKind.Died) { InvolvedEntity = this, Level = ActionLevel.Important, Info = Name +" Died" });
          }
        }
      }
    }

    EventsManager eventsManager;
    [JsonIgnore]
    public EventsManager EventsManager
    {
      get { return eventsManager; }
      set { eventsManager = value; }
    }

    internal bool CalculateIfHitWillHappen(LivingEntity target)
    {
      return true;
    }

    public float OnPhysicalHit(LivingEntity attacker)
    {
      float defense = GetDefense();
      if (defense == 0)
      {
        AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.Assert, Info = "Stats.Defence == 0!!!" });
        return 0;
      }
      if (!Alive)
      {
        AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.Assert, Info = "!Alive" });
        return 0;
      }

      var inflicted = attacker.GetCurrentValue(EntityStatKind.Attack) / defense;
      ReduceHealth(inflicted);

      var ga = new LivingEntityAction(LivingEntityActionKind.GainedDamage) { InvolvedValue = inflicted, InvolvedEntity = this };
      var desc = "received damage: " + inflicted.Formatted();
      ga.Info = Name.ToString() + " " + desc;
      AppendAction(ga);

      PlaySound("punch");

      if (!this.EverHitBy.Contains(attacker))
        this.EverHitBy.Add(attacker);
      //if (this is Enemy || this is Hero)// || this is CrackedStone)
      //{
      //  PlayPunchSound();
      //}
      var dead = DieIfShould(EffectType.None);
      if (!dead && IsWounded && !lastingEffects.Any(i => i.Type == EffectType.Bleeding))
      {
        var effectInfo = CalcLastingEffDamage(inflicted, attacker, null, null);
        if (effectInfo.Turns <= 0)
          effectInfo.Turns = 3;//TODO
        this.AddLastingEffect(EffectType.Bleeding, effectInfo.Turns, effectInfo.Damage);
      }

      return inflicted;
    }

    private void PlaySound(string sound)
    {
      if(sound.Any())
        AppendAction(new SoundRequestAction() { SoundName = sound });
    }

    protected virtual void OnHitBy
    (
      float amount, 
      //FightItem fightItem, 
      LivingEntity attacker = null, 
      Spell spell = null, 
      string damageDesc = null
    )
    {
      if (!Alive)
        return;
      //if (LevelGenerationInfo.HeroGodMode && this is Hero)
      //  return;
      //if (attacker is Hero && this is Enemy && (this as Enemy).HeroAlly)
      //{
      //  amount /= 10;
      //}
      var sound = "";
      if (spell != null)
      {
        if (spell.Kind == SpellKind.StonedBall)
          amount /= Stats.Defence;
        else
        {
          var magicAttackDamageReductionPerc = Stats.GetCurrentValue(EntityStatKind.MagicAttackDamageReduction);
          amount -= GetReducePercentage(amount, magicAttackDamageReductionPerc);
        }
        sound = spell.GetHitSound();
      }
      ReduceHealth(amount);
      var ga = new LivingEntityAction(LivingEntityActionKind.GainedDamage) { InvolvedValue = amount, InvolvedEntity = this };
      var desc = damageDesc ?? "received damage: " + amount.Formatted();
      ga.Info = Name.ToString() + " " + desc;

      AppendAction(ga);
      PlaySound(sound);

      RemoveLastingEffect(this, EffectType.Frighten);
      if (attacker != null || (spell.Caller != null && spell.Caller.LastingEffects.Any(i => i.Type == EffectType.Transform)))
      {
        var removeTr = attacker ?? spell.Caller;
        RemoveLastingEffect(removeTr, EffectType.Transform);
      }
      if (attacker != null && spell == null && amount > 0)
      {
        StealStatIfApplicable(amount, attacker);
      }

      var effectInfo = CalcLastingEffDamage(amount, attacker, spell, null);
      if (effectInfo.Type != EffectType.None && !IsImmuned(effectInfo.Type))
      {
        var rand = RandHelper.Random.NextDouble();
        var chance = GetChanceToExperienceEffect(effectInfo.Type);
        if (spell != null)
        {
          if (spell.SendByGod && spell.Kind != SpellKind.LightingBall)
            chance *= 2;

        }
        //if (fightItem != null)
          //chance += fightItem.GetFactor(false);
        if (rand * 100 <= chance)
        {
          this.AddLastingEffect(effectInfo.Type, effectInfo.Turns, effectInfo.Damage);
          //AppendEffectAction(effectInfo.Type, true); duplicated message
        }
      }
      var died = DieIfShould(effectInfo.Type);
      
      var attackedInst = attacker ?? spell.Caller;
      if (attackedInst != null)
      {
        if (!EverHitBy.Contains(attackedInst))
          EverHitBy.Add(attackedInst);
      }
    }

    static Tuple<EffectType, int> heBase = new Tuple<EffectType, int>(EffectType.None, 0);
    protected virtual Tuple<EffectType, int> GetPhysicalHitEffect(LivingEntity victim, FightItem fi = null)
    {
      return heBase;
    }

    public virtual float GetChanceToExperienceEffect(EffectType et)
    {
      if (et == EffectType.Stunned && GetLastingEffect(EffectType.Stunned) != null)
        return 0;//it was too easy
      return chanceToExperienceEffect[et];
    }

    public LastingEffect GetLastingEffect(EffectType le)
    {
      return lastingEffects.Where(i => i.Type == le).SingleOrDefault();
    }

    LastingEffectCalcInfo CalcLastingEffDamage(float amount, LivingEntity attacker = null, Spell spell = null, FightItem fi = null)
    {
      LastingEffectCalcInfo effectInfo = new LastingEffectCalcInfo();
      var et = new Tuple<EffectType, int>(EffectType.None, 3);
      float effectDamage = 0;
      if (attacker != null && spell == null && amount > 0)
      {
        et = attacker.GetPhysicalHitEffect(this, fi);//bleeding, or torn apart
        if (et.Item1 != EffectType.Stunned)
          effectDamage = amount * 30.0f / 100f;//TODO 30
        StealStatIfApplicable(amount, attacker);
      }
      else if (spell != null)
      {
        //TODO
        //et = spell.GetEffectType();
        //var spellAtt = spell as AttackingSpell;
        //if (spellAtt != null && !spellAtt.SourceOfDamage)
        //{
        //  effectDamage = amount;
        //}
        //else
        //{

        //  if (spell.Kind == SpellKind.PoisonBall || spell.Kind == SpellKind.IceBall
        //    || spell.Kind == SpellKind.FireBall || spell.Kind == SpellKind.NESWFireBall
        //    || spell.Kind == SpellKind.LightingBall)
        //    effectDamage = CalcNonPhysicalDamageFromSpell(spell);
        //  else
        //  {
        //    if (spell.Kind != SpellKind.StonedBall)
        //      Assert(false, "spell = " + spell.Kind);
        //    effectDamage = spell.Damage;
        //  }
        //}
      }
      effectInfo.Type = et.Item1;
      effectInfo.Turns = et.Item2;
      effectInfo.Damage = effectDamage;
      return effectInfo;
    }

    public virtual LastingEffect AddLastingEffect(EffectType eff, int pendingTurns, EntityStatKind kind, float nominalValuePercInc)
    {
      bool onlyProlong = LastingEffects.Any(i => i.Type == eff);//TODO is onlyProlong dne ?
      var statValue = this.Stats.GetStat(kind).Value.TotalValue;
      var calcEffectValue = CalcEffectValue(nominalValuePercInc, statValue);

      if(eff == EffectType.ConsumedFood)
        lastingEffSubtractions[eff] = calcEffectValue;//AddLastingEffect uses lastingEffSubtractions so it must be set

      var le = AddLastingEffect(eff, pendingTurns, 0, kind);
      le.StatKind = kind;
      
      bool handle = false;
      if (eff == EffectType.Rage || eff == EffectType.Weaken || eff == EffectType.IronSkin || eff == EffectType.Inaccuracy
          )
      {
        if (!onlyProlong)
        {
          lastingEffSubtractions[eff] = calcEffectValue;
          handle = true;
        }
      }
      else if (eff == EffectType.ResistAll)
      {
        var effValue = nominalValuePercInc;
        foreach (var res in resists)
        {
          var statClone = this.Stats.GetStat(res).Clone() as EntityStat;
          statClone.Subtract(-effValue);
          var cv = statClone.Value.CurrentValue;
          // GameManager.Instance.AppendUnityLog("resist  st = " + res + " cv = " + cv);
          while (statClone.Value.CurrentValue > 100)
          {
            effValue -= 1;
            statClone = this.Stats.GetStat(res).Clone() as EntityStat;
            statClone.Subtract(-effValue);
          }
        }
        lastingEffSubtractions[eff] = effValue;
        handle = true;
      }
      else if (eff == EffectType.ConsumedFood)
      {

      }
      else
        Assert(false, "AddLastingEffect - unhandeled eff = " + eff);

      if (handle)
      {
        HandleSpecialFightStat(eff, true);
      }
      return le;
    }

    private static float CalcEffectValue(float nominalValuePercInc, float statValue)
    {
      return statValue * nominalValuePercInc / 100f;
    }

    //For the time of lasting effect some state is changed, then restored to the original value (flag add)
    void HandleSpecialFightStat(EffectType et, bool add)
    {
      float factor = 0;
      Func<EffectType, float> getEffValue = (EffectType effType) => { return lastingEffSubtractions[effType]; };

      
      
      if (et == EffectType.ResistAll)
      {
        factor = add ? getEffValue(EffectType.ResistAll) : -getEffValue(EffectType.ResistAll);

        foreach (var res in resists)
        {
          this.Stats.GetStat(res).Subtract(-factor);
        }
        return;
      }

      EntityStatKind esk = EntityStatKind.Unset;
      EffectType effT = EffectType.None;
      if (et == EffectType.Rage)
      {
        effT = EffectType.Rage;
        esk = EntityStatKind.Attack;
      }
      else if (et == EffectType.Weaken)
      {
        effT = EffectType.Weaken;
        esk = EntityStatKind.Defence;
        //add = false;
      }
      else if (et == EffectType.Inaccuracy)
      {
        effT = EffectType.Inaccuracy;
        esk = EntityStatKind.ChanceToHit;
        //add = false;
      }
      else if (et == EffectType.IronSkin)
      {
        effT = EffectType.IronSkin;
        esk = EntityStatKind.Defence;
      }

      if (esk != EntityStatKind.Unset)
      {
        factor = add ? getEffValue(effT) : -getEffValue(effT);
        if (et == EffectType.Weaken || et == EffectType.Inaccuracy)
        {
          factor *= -1;
        }
        var st = this.Stats.GetStat(esk);
        st.Subtract(-factor);
        //st = this.Stats.Stats[esk];
        // //Debug.WriteLine(" st = "+ st);
      }
    }

    public virtual LastingEffect AddLastingEffect(EffectType eff, int pendingTurns, float damage, EntityStatKind esk = EntityStatKind.Unset, bool fromHit = true)
    {
      if (IsImmuned(eff))
        return null;
      var le = LastingEffects.Where(i => i.Type == eff).FirstOrDefault();
      if (le == null)
      {
        le = new LastingEffect() { Type = eff, PendingTurns = pendingTurns,StatKind = esk,  DamageAmount = damage };
        if (eff == EffectType.TornApart)//this is basically a death
          le.DamageAmount = this.Stats.Health;
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
        le.PendingTurns = pendingTurns;

        if (LastingEffectStarted != null)
        {
          ////make sure gui shows it - after load ManaShield was not visible
          //if (eff == EffectType.ManaShield)
          //  LastingEffectStarted(this, new GenericEventArgs<LastingEffect>(le));
        }
      }

      return le;
    }

    private void AddLastingEffect(LastingEffect le)
    {
      //if(le.Type == EffectType.ConsumedFood)
      //  lastingEffSubtractions[EffectType.ConsumedFood] = CalcEffectValue(nominalValuePercInc, statValue);

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
            info = Name + " used " + le.Type + " spell";
          }
          else
          {
            info = "Spell " + le.Type + " was casted on " + Name;
          }
          AppendAction(new LivingEntityAction(LivingEntityActionKind.UsedSpell) { Info = info, EffectType = le.Type });
          appAction = false;
        }
      }

      if (le.Type == EffectType.Bleeding || le.Type == EffectType.ConsumedFood)//trap must add damage at start
      {
        ApplyLastingEffect(le, true);
        RemoveFinishedLastingEffects();//food might be consumed at once
      }
      else if (appAction)
        AppendEffectAction(le.Type, true);
    }

    private void AppendEffectAction(EffectType eff, bool newOne, float amount = 0, bool fromHit = true)
    {
      var lea = new LivingEntityAction(LivingEntityActionKind.ExperiencedEffect);
      lea.InvolvedEntity = this;
      lea.EffectType = eff;
      var targetName = Name.ToString();

      if (eff == EffectType.ConsumedFood)
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

    bool IsImmuned(EffectType effect)
    {
      if (this is CrackedStone)
        return true;
      return immunedEffects.Contains(effect) || chanceToExperienceEffect[effect] == 0;
    }

    public virtual void ApplyLastingEffects()
    {
      EffectType eff = EffectType.None;

      foreach (var i in LastingEffects)
      {
        ApplyLastingEffect(i, false);

        eff = i.Type;
        if (HealthZero())
          break;
      };

      RemoveFinishedLastingEffects();
      DieIfShould(eff);
    }

    private void RemoveFinishedLastingEffects()
    {
      var done = LastingEffects.Where(i => i.PendingTurns <= 0).ToList();
      foreach (var doneItem in done)
      {
        RemoveLastingEffect(this, doneItem.Type);
      }
    }

    protected void DoConsume(EntityStatKind statFromConsumable, float inc)
    {
      this.Stats.IncreaseStatDynamicValue(statFromConsumable, inc);
    }

    private void ApplyLastingEffect(LastingEffect eff, bool newOne)
    {
      var damage = eff.DamageAmount;
      if (eff.DamageAmount > 0)
      {
        if (eff.Type == EffectType.Bleeding)// && eff.FromTrapSpell)
        {
          damage /= Stats.Defence;
        }
        ReduceHealth(damage);
      }

      if (eff.Type == EffectType.ConsumedFood)
      {
        var inc = lastingEffSubtractions[EffectType.ConsumedFood];
        DoConsume(eff.StatKind, inc);
      }

      eff.PendingTurns--;
      if (eff.Type != EffectType.ManaShield && eff.Type != EffectType.Transform && eff.Type != EffectType.Frighten && eff.Type != EffectType.Stunned
          && eff.Type != EffectType.Rage && eff.Type != EffectType.Weaken && eff.Type != EffectType.IronSkin && eff.Type != EffectType.ResistAll
          && eff.Type != EffectType.Inaccuracy)
        AppendEffectAction(eff.Type, newOne, damage);

      if (LastingEffectApplied != null)
        LastingEffectApplied(this, eff);

      DieIfShould(eff.Type);
    }
    /// <summary>
    /// Increases health or mana by stealing
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="attacker"></param>
    private static void StealStatIfApplicable(float damageAmount, LivingEntity attacker)
    {
      foreach (var stat in statsHitIncrease)
      {
        var stealStatValue = attacker.Stats.GetCurrentValue(stat.Key);
        if (stealStatValue > 0)
        {
          var lsAmount = stealStatValue / 100f * damageAmount;
          attacker.Stats.IncreaseStatDynamicValue(stat.Value, lsAmount);
        }
      }
    }

    [JsonIgnore]
    public List<LastingEffect> LastingEffects
    {
      get
      {
        return lastingEffects;
      }

      set
      {
        lastingEffects = value;
      }
    }

    public virtual void RemoveLastingEffect(LivingEntity livEnt, EffectType et)
    {
      var le = livEnt.LastingEffects.FirstOrDefault(i => i.Type == et);
      if (le != null)
      {
        livEnt.LastingEffects.RemoveAll(i => i.Type == et);
        le.Dispose();
        
        HandleSpecialFightStat(et, false);
        //TODO
        //if (et == EffectType.Hooch)
        //{
        //  ApplyHoochEffects(false);
        //  lastingEffHooch = null;
        //}

        if (livEnt == this && LastingEffectDone != null)
          LastingEffectDone(this, le);
      }
    }

    public static float GetReducePercentage(float orgAmount, float discPerc)
    {
      return orgAmount * discPerc / 100f;
    }

    public override string ToString()
    {
      var str = base.ToString();
      str += " "+this.State + ", Alive:"+Alive + ", H:"+Stats.Health;
      return str;
    }

    protected void AppendAction(GameAction ac)
    {
      if(EventsManager != null)
        EventsManager.AppendAction(ac);
    }

    protected void Assert(bool check, string desc = "")
    {
      if (EventsManager != null)
        EventsManager.Assert(check, desc);
    }

    private bool DieIfShould(EffectType effect)
    {
      if (Alive && HealthZero())
      {
        Alive = false;
        DiedOfEffect = effect;
        return true;
      }
      return false;
    }

    private bool HealthZero()
    {
      return Stats.GetCurrentValue(EntityStatKind.Health) <= 0;
    }

    public virtual void ReduceHealth(float amount)
    {
      Stats.GetStat(EntityStatKind.Health).Subtract(amount);
    }

    private float GetDefense()
    {
      return GetCurrentValue(EntityStatKind.Defence);
    }

    internal void ApplyPhysicalDamage(LivingEntity victim)
    {
      victim.OnPhysicalHit(this);
    }

    public float GetCurrentValue(EntityStatKind kind)
    {
      var stat = Stats.GetStat(kind);
      var cv = stat.Value.CurrentValue;
      if (stat.IsPercentage && cv > 100)
      {
        cv = 100;
      }
      return cv;
    }

    //public virtual float GetHitAttackValue()//bool withVariation)
    //{
    //  var str = Stats.GetCurrentValue(EntityStatKind.Strength);
    //  //var as1 = Stats.Stats[EntityStatKind.Attack];
    //  var att = Stats.GetCurrentValue(EntityStatKind.Attack);

    //  return str + att;
    //}

    public float GetTotalValue(EntityStatKind esk)
    {
      return Stats.GetTotalValue(esk);
    }

    public float CalcNonPhysicalDamageFromSpell(Spell spell)
    {
      EntityStatKind attackingStat = EntityStatKind.Unset;
      switch (spell.Kind)
      {
        case SpellKind.FireBall:
        case SpellKind.NESWFireBall:
          attackingStat = EntityStatKind.FireAttack;
          break;

        case SpellKind.IceBall:
          attackingStat = EntityStatKind.ColdAttack;
          break;
        case SpellKind.PoisonBall:
          attackingStat = EntityStatKind.PoisonAttack;
          break;
        case SpellKind.LightingBall:
          attackingStat = EntityStatKind.LightingAttack;
          break;

        default:
          break;
      }
      var npd = CalculateNonPhysicalDamage(attackingStat, spell.Damage);
      return npd;
    }

    float CalculateNonPhysicalDamage(EntityStatKind attackingStat, float damageAmount)
    {
      if (damageAmount == 0)
        return 0;
      float damage = damageAmount;
      var resist = GetResistValue(attackingStat);
      return damage - (damage * resist / 100);//resist is %
    }

    float GetResistValue(EntityStatKind attackingStat)
    {
      var resist = GetResist(attackingStat);
      if (resist == EntityStatKind.Unset)
        return 0;
      return GetCurrentValue(resist);
    }

    //public void UseScroll(SpellCastPolicy policy)
    //{
    //  policy.Apply(this);
    //}

    public static EntityStatKind GetResist(EntityStatKind attackingStat)
    {
      if (attackingStat == EntityStatKind.FireAttack)
        return EntityStatKind.ResistFire;
      else if (attackingStat == EntityStatKind.PoisonAttack)
        return EntityStatKind.ResistPoison;
      else if (attackingStat == EntityStatKind.ColdAttack)
        return EntityStatKind.ResistCold;
      else if (attackingStat == EntityStatKind.LightingAttack)
        return EntityStatKind.ResistLighting;

      return EntityStatKind.Unset;
    }

    public bool OnHitBy(Roguelike.Abstract.ISpell md)
    {
      if (md is Spell)
      {
        var spell = md as Spell;
        //if (ShouldEvade(this, EntityStatKind.ChanceToEvadeMagicAttack, spell))
        //{
        //  //GameManager.Instance.AppendDiagnosticsUnityLog("ChanceToEvadeMagicAttack worked!");
        //  AppendMissedAction(spell.Caller, GameManager.Instance, false);
        //  return false;
        //}
        //lastHitBySpell = true;
        var dmg = CalcNonPhysicalDamageFromSpell(spell);
        OnHitBy(dmg /*, spell.FightItem*/, null, spell);
      }

      //else if (md is FightItem)
      //{
      //  float damage = 0;
      //  FightItem fi = md as FightItem;
      //  if (md is ExplosiveCocktail)
      //  {
      //    lastHitBySpell = true;//to put on enemy resist
      //    var spell = new FireBallSpell(this);
      //    spell.Caller = md.Caller;
      //    spell.SourceOfDamage = false;//dmg is fixed !
      //    var expl = md as ExplosiveCocktail;
      //    spell.FightItem = fi;
      //    damage = CalculateNonPhysicalDamage(EntityStatKind.FireAttack, expl.GetDamage());
      //    OnHitBy(damage, fi, null, spell);
      //  }
      //  else if (md is ThrowingKnife)
      //  {
      //    damage = fi.GetDamage() / GetDefence();
      //    OnHitBy(damage, fi, fi.Caller, null);
      //  }
      //  else
      //  {
      //    GameManager.Instance.AppendDiagnosticsUnityLogError(new Exception("OnHitBy!"));
      //  }
      //}
      //else
      //{
      //  GameManager.Instance.AppendDiagnosticsUnityLogError(new Exception("OnHitBy - not supported"));
      //}
      return true;
    }

    public event EventHandler Wounded;

    public void SetIsWounded()
    {
      if (IsWounded)
        return;

      IsWounded = true;
      var def = Stats.GetStat(EntityStatKind.Defence);
      def.Subtract(def.Value.TotalValue/2);
      
      if (Wounded != null)
        Wounded(this, EventArgs.Empty);
    }
  }
}
