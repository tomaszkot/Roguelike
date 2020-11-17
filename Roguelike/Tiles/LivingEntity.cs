using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Factors;
using Roguelike.Managers;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;
using Roguelike.Utils;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.Tiles
{
  public enum EntityState { Idle, Moving, Attacking, CastingSpell }
  public enum EntityMoveKind { Freestyle, FollowingHero, ReturningHome }

  public class LivingEntity : Tile, ILastingEffectOwner
  {
    static Dictionary<EntityStatKind, EntityStatKind> statsHitIncrease = new Dictionary<EntityStatKind, EntityStatKind> {
                { EntityStatKind.LifeStealing, EntityStatKind.Health },
                { EntityStatKind.ManaStealing, EntityStatKind.Mana }
    };

    public EntityMoveKind MoveKind { get; set; }
    public static Point DefaultInitialPoint = new Point(0, 0);
    public Point PrevPoint;
    public Point InitialPoint = DefaultInitialPoint;
    EntityStats stats = new EntityStats();
    Dictionary<EffectType, float> chanceToExperienceEffect = new Dictionary<EffectType, float>();
    public int ActiveScrollCoolDownCounter { get; set; }
    Dictionary<EntityStatKind, float> nonPhysicalDamageStats = new Dictionary<EntityStatKind, float>();
    public virtual Scroll ActiveScroll
    {
      get 
      {
        return null;
      }
    }

    public bool CanAttack { get; set; } = true;
    public EffectType DiedOfEffect;
    
    public EntityState state;
    public EntityState State
    {
      get { return state; }
      set{state = value;}
    }
    List<Algorithms.PathFinderNode> pathToTarget;
    private LastingEffectsSet lastingEffectsSet;
    protected List<EffectType> immunedEffects = new List<EffectType>();

    [JsonIgnore]
    public List<LivingEntity> EverHitBy { get; set; } = new List<LivingEntity>();
    //public static Func<SpellCastPolicy> spellCastPolicyProvider;

    bool alive = true;
    //[JsonIgnoreAttribute]
    public EntityStats Stats { get => stats; set => stats = value; }

    public int Level 
    { 
      get; 
      set; 
    } = 1;

    //Dictionary<EffectType, float> lastingEffSubtractions = new Dictionary<EffectType, float>();

    public bool IsWounded { get; protected set; }
    protected Dictionary<EffectType, int> effectsToUse = new Dictionary<EffectType, int>();
    public static readonly EffectType[] PossibleEffectsToUse = new EffectType[] {
    EffectType.Weaken, EffectType.Rage, EffectType.IronSkin, EffectType.ResistAll, EffectType.Inaccuracy
    };
    
    static LivingEntity()
    {
    }

    public LivingEntity():this(new Point(-1, -1), '\0')
    {
    }

    public LivingEntity(Point point, char symbol) : base(point, symbol)
    {
      lastingEffectsSet = new LastingEffectsSet(this, null);
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

      effectsToUse[EffectType.Weaken] = GenerationInfo.DefaultEnemyWeakenUsageCount;
      effectsToUse[EffectType.Rage] = GenerationInfo.DefaultEnemyRageUsageCount;
      effectsToUse[EffectType.IronSkin] = GenerationInfo.DefaultEnemyIronSkinUsageCount;
      effectsToUse[EffectType.ResistAll] = GenerationInfo.DefaultEnemyResistAllUsageCount;
      effectsToUse[EffectType.Inaccuracy] = GenerationInfo.DefaultEnemyResistAllUsageCount;
    }

    public bool WasEverHitBy(LivingEntity le) 
    {
      return EverHitBy.Contains(le);
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

    ILogger Logger
    {
      get { return Container.GetInstance<ILogger>(); }
    }

    public virtual bool Alive
    {
      get { return alive; }
      set
      {
        if (alive != value)
        {
            alive = value;
          
        }
      }
    }

    Container container;
    [JsonIgnore]
    public virtual Container Container
    {
      get { return container; }
      set
      {
        container = value;
        this.lastingEffectsSet.Container = value;
      }
    }

    [JsonIgnore]
    public EventsManager EventsManager
    {
      get { return Container.GetInstance<EventsManager>(); }
    }

    internal bool CalculateIfHitWillHappen(LivingEntity target)
    {
      //var randVal = RandHelper.Random.NextDouble();
      var hitWillHappen = CalculateIfStatChanceApplied(EntityStatKind.ChanceToHit, target);
      return hitWillHappen;
    }

    internal bool CalculateIfStatChanceApplied(EntityStatKind esk, LivingEntity target = null, FightItem fi = null)
    {
      //Container.GetInstance<ILogger>().LogInfo(this + " CalculateIfStatChanceApplied...");
      var randVal = (float)RandHelper.Random.NextDouble();
      var chance = GetEffectChance(esk);
      //if (fi != null && fi is ThrowingKnife)
      //{
      //  chance = fi.GetFactor(false);
      //}
      if (target != null)
      {
        if (esk == EntityStatKind.ChanceToHit)
        {
          if (ShouldEvade(target, EntityStatKind.ChanceToEvadeMeleeAttack, null))
          {
            EventsManager.AppendAction(new LivingEntityAction(LivingEntityActionKind.Missed) { InvolvedEntity = this , Info = Name+" missed "+ target.Name});
            return false;
          }

          Logger.LogInfo(this + " CalculateIfStatChanceApplied true");
          return true;
        }
      }
      return randVal > 0 && (randVal * 100 <= chance);
    }

    virtual protected bool ShouldEvade(LivingEntity target, EntityStatKind esk, Spell spell)
    {
      var avoidCh = target.GetCurrentValue(esk);
      var randValCh = (float)RandHelper.Random.NextDouble();
      if (randValCh * 100 <= avoidCh)
      {
        return true;
      }
      return false;

    }

    public virtual float GetHitAttackValue(bool withVariation)
    {
      return GetCurrentValue(EntityStatKind.Attack);
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

      lastHitBySpell = false;
      var attack = attacker.GetHitAttackValue(true);
      var inflicted = attack/defense;
      ReduceHealth(inflicted);

      var desc = Name.ToString() + " received damage: " + inflicted.Formatted();
      
      foreach (var stat in attacker.GetNonPhysicalDamages())
      {
        var npd = CalculateNonPhysicalDamage(stat.Key, stat.Value);
        if (npd != 0)
          desc += " " + GetAttackDesc(stat.Key) + ": " + npd.Formatted();
        inflicted += npd;
      }
            
      var ga = new LivingEntityAction(LivingEntityActionKind.GainedDamage) { InvolvedValue = inflicted, InvolvedEntity = this };
      ga.Info = desc;

      AppendAction(ga);

      PlaySound("punch");

      if (!this.EverHitBy.Contains(attacker))
        this.EverHitBy.Add(attacker);
      //if (this is Enemy || this is Hero)// || this is CrackedStone)
      //{
      //  PlayPunchSound();
      //}
      var dead = DieIfShould(EffectType.Unset);
      if(!dead && IsWounded)
      {
        if(attacker.CanCauseBleeding())
          lastingEffectsSet.EnsureEffect(EffectType.Bleeding, inflicted, attacker);
      }

      return inflicted;
    }

    public virtual bool CanCauseBleeding()
    {
      return true;
    }

    public virtual  float GetAttackVariation()
    {
      return 0;
    }
        
    string GetAttackDesc(EntityStatKind esk)
    {
      if (esk == EntityStatKind.FireAttack)
        return "fire";
      else if (esk == EntityStatKind.PoisonAttack)
        return "poison";
      else if (esk == EntityStatKind.ColdAttack)
        return "cold";
      else if (esk == EntityStatKind.LightingAttack)
        return "lighting";

      return esk.ToString();
    }

    public Dictionary<EntityStatKind, float> GetNonPhysicalDamages()
    {
      foreach (var stat in Loot.AttackingNonPhysicalStats)
      {
        nonPhysicalDamageStats[stat] = Stats.GetTotalValue(stat);
      }

      return nonPhysicalDamageStats;
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
          amount /= Stats.Defense;
        else
        {
          var magicAttackDamageReductionPerc = Stats.GetCurrentValue(EntityStatKind.MagicAttackDamageReduction);
          amount -= GetReducePercentage(amount, magicAttackDamageReductionPerc);
        }
        sound = spell.GetHitSound();

        lastHitBySpell = true;
      }
      ReduceHealth(amount);
      var ga = new LivingEntityAction(LivingEntityActionKind.GainedDamage) { InvolvedValue = amount, InvolvedEntity = this };
      var desc = damageDesc ?? "received damage: " + amount.Formatted();
      ga.Info = Name.ToString() + " " + desc;

      AppendAction(ga);
      PlaySound(sound);

      var frighten = this.GetFirstLastingEffect(EffectType.Frighten);
      if(frighten !=null)
        RemoveLastingEffect(this, frighten);

      if (attacker != null || (spell.Caller != null && spell.Caller.LastingEffects.Any(i => i.Type == EffectType.Transform)))
      {
        var removeTr = attacker ?? spell.Caller;
        var transf = this.GetFirstLastingEffect(EffectType.Transform);
        if (transf != null)
          RemoveLastingEffect(removeTr, transf);
      }
      if (attacker != null && spell == null && amount > 0)
        StealStatIfApplicable(amount, attacker);

      var effectInfo = lastingEffectsSet.TryAddLastingEffectOnHit(amount, attacker, spell);
      var attackedInst = attacker ?? spell.Caller;
      if (attackedInst != null)
      {
        if (!EverHitBy.Contains(attackedInst))
          EverHitBy.Add(attackedInst);
      }
    }

    static LastingEffectCalcInfo heBase = new LastingEffectCalcInfo(EffectType.Unset, 0, new EffectiveFactor(0), new PercentageFactor(0));
    protected virtual LastingEffectCalcInfo GetPhysicalHitEffect(LivingEntity victim, FightItem fi = null)
    {
      return heBase;
    }

    public virtual float GetChanceToExperienceEffect(EffectType et)
    {
      if (et == EffectType.Stunned && GetFirstLastingEffect(EffectType.Stunned) != null)
        return 0;//it was too easy
      return chanceToExperienceEffect[et];
    }

    public bool HasLastingEffect(EffectType le)
    {
      return GetFirstLastingEffect(le) != null;
    }

    public LastingEffect GetFirstLastingEffect(EffectType le)
    {
      return LastingEffects.Where(i => i.Type == le).FirstOrDefault();
    }
            
    public bool IsImmuned(EffectType effect)
    {
      if (this is CrackedStone)
        return true;
      return immunedEffects.Contains(effect) || chanceToExperienceEffect[effect] == 0;
    }

    public virtual void ApplyLastingEffects()
    {
      lastingEffectsSet.ApplyLastingEffects();
    }
        
    protected void DoConsume(EntityStatKind statFromConsumable, LastingEffectCalcInfo inc)
    {
      this.Stats.ChangeStatDynamicValue(statFromConsumable, (float)inc.EffectiveFactor.Value);
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
          attacker.Stats.ChangeStatDynamicValue(stat.Value, lsAmount);
        }
      }
    }

    [JsonIgnore]
    public List<LastingEffect> LastingEffects
    {
      get
      {
        return lastingEffectsSet.LastingEffects;
      }
    }

    public LastingEffectsSet LastingEffectsSet { get => lastingEffectsSet;  }

    public virtual void RemoveLastingEffect(LivingEntity livEnt, LastingEffect le)
    {
      lastingEffectsSet.RemoveLastingEffect(livEnt, le);
    }
        
    public static float GetReducePercentage(float orgAmount, float discPerc)
    {
      return orgAmount * discPerc / 100f;
    }

    public override string ToString()
    {
      var str = base.ToString();
      str += " "+this.State + ", Alive:"+Alive + ", H:"+Stats.Health + ", Lvl:" + this.Level;
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

    public bool DieIfShould(EffectType effect)
    {
      if (Alive && IsHealthZero())
      {
        Alive = false;
        DiedOfEffect = effect;
        //AppendDeadAction();
        return true;
      }
      return false;
    }

    public LivingEntityAction GetDeadAction()
    {
      var info = Name + " Died";
      if (DiedOfEffect != EffectType.Unset)
        info += ", killing effect: "+ DiedOfEffect.ToDescription();

      return new LivingEntityAction(LivingEntityActionKind.Died) { InvolvedEntity = this, Level = ActionLevel.Important, Info = info };
    }

    public bool IsHealthZero()
    {
      return Stats.GetCurrentValue(EntityStatKind.Health) <= 0;
    }

    public virtual void ReduceHealth(float amount)
    {
      Stats.GetStat(EntityStatKind.Health).Subtract(amount);
      DieIfShould(EffectType.Unset);
    }

    private float GetDefense()
    {
      return GetCurrentValue(EntityStatKind.Defense);
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
      var offSpell = spell as OffensiveSpell;
      var npd = CalculateNonPhysicalDamage(attackingStat, offSpell.Damage);
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
        OnHitBy(dmg /*, spell.FightItem*/, spell.Caller, spell);
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
      var def = Stats.GetStat(EntityStatKind.Defense);
      def.Subtract(def.Value.TotalValue/2);
      
      if (Wounded != null)
        Wounded(this, EventArgs.Empty);
    }

    public virtual float GetChanceToHit()
    {
      return GetEffectChance(EntityStatKind.ChanceToHit);
    }

    public virtual float GetEffectChance(EntityStatKind esk)
    {
      return GetCurrentValue(esk);
    }

    public int GetEffectUseCount(EffectType type)
    {
      return effectsToUse[type];
    }

    public bool HasAnyEffectToUse()
    {
      return effectsToUse.Any(i => i.Value > 0);
    }

    public bool HasEffectToUse(EffectType type)
    {
      return effectsToUse.ContainsKey(type);
    }

    public void ReduceEffectToUse(EffectType type)
    {
      if (HasEffectToUse(type))
        effectsToUse[type]--;
      else
      {
        Assert(false, "ReduceEffectToUse failed for " + type);
      }
    }

    bool lastHitBySpell;
    public EffectType GetRandomEffectToUse(bool canCastWeaken, bool canCastInaccuracy)
    {
      var effectsToUseFiltered = effectsToUse.ToList();
      if (!this.lastHitBySpell)
        effectsToUseFiltered.RemoveAll(i => i.Key == EffectType.ResistAll);
      if (!canCastWeaken)
        effectsToUseFiltered.RemoveAll(i => i.Key == EffectType.Weaken);
      if (!canCastInaccuracy)
        effectsToUseFiltered.RemoveAll(i => i.Key == EffectType.Inaccuracy);
      var effects = effectsToUseFiltered.Where(i => i.Value > 0).ToList();
      if (effects.Any())
      {
        var eff = RandHelper.GetRandomElem<KeyValuePair<EffectType, int>>(effects);
        //if (eff.Key == EffectType.Weaken)
        //{
        //  int k = 0;
        //}
        return eff.Key;
      }
      return EffectType.Unset;
    }

    public LastingEffect AddLastingEffectFromSpell(EffectType effectType)
    {
      SpellKind spellKind = SpellConverter.SpellKindFromEffectType(effectType);
      return AddLastingEffectFromSpell(spellKind, effectType);
    }

    public LastingEffect AddLastingEffectFromSpell(SpellKind spellKind, EffectType effectType)
    {
      return lastingEffectsSet.AddLastingEffectFromSpell(spellKind, effectType);
    }

    public EffectiveFactor CalcEffectiveFactor(EntityStatKind kind, float nominalValuePercInc)
    {
      var statValue = Stats.GetStat(kind).Value.TotalValue;
      var factor = CalcEffectValue(nominalValuePercInc, statValue);
      return new EffectiveFactor(factor);
    }

    private static float CalcEffectValue(float nominalValuePercInc, float statValue)
    {
      return statValue * nominalValuePercInc / 100f;
    }

    public void ApplyPassiveSpell(PassiveSpell spell)
    {
      AddLastingEffectFromSpell(spell.Kind, SpellConverter.EffectTypeFromSpellKind(spell.Kind));
    }

    public bool IsTransformed()
    {
      return this.LastingEffectsSet.HasEffect(EffectType.Transform);
    }
  }
}
