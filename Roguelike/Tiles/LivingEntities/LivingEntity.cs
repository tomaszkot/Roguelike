using Dungeons.Core;
using Dungeons.Fight;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Factors;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Spells;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using Roguelike.Utils;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.Tiles.LivingEntities
{
  public enum EntityState { Idle, Moving, Attacking, CastingProjectile, Sleeping }
  public enum EntityMoveKind { Freestyle, FollowingHero, ReturningHome }

  /// <summary>
  /// Base type for everything that is alive and can be potentialy killed
  /// </summary>
  public class LivingEntity : Tile, ILastingEffectOwner, IDestroyable
  {
    //some attributes describing LivingEntity
    public string Herd { get; set; } = "";
    public int Level
    {
      get;
      set;
    } = 1;
    public EntityKind EntityKind { get; set; }

    List<EffectType> everCausedEffect = new List<EffectType>();
    public event EventHandler Wounded;

    public bool d_immortal = false;
    protected int StartStrength = 10;
    public static readonly EntityStat BaseStrength = new EntityStat(EntityStatKind.Strength, 10);
    public static readonly EntityStat BaseHealth = new EntityStat(EntityStatKind.Health, 10);
    public static readonly EntityStat BaseDefence = new EntityStat(EntityStatKind.Defense, 7);
    public static readonly EntityStat BaseDexterity = new EntityStat(EntityStatKind.Dexterity, 10);
    public static readonly EntityStat BaseMana = new EntityStat(EntityStatKind.Mana, 10);
    public static readonly EntityStat BaseMagic = new EntityStat(EntityStatKind.Magic, 10);

    static Dictionary<EntityStatKind, EntityStatKind> statsHitIncrease = new Dictionary<EntityStatKind, EntityStatKind>
    {
      { EntityStatKind.LifeStealing, EntityStatKind.Health },
      { EntityStatKind.ManaStealing, EntityStatKind.Mana }
    };

    List<Algorithms.PathFinderNode> pathToTarget;
    protected LastingEffectsSet lastingEffectsSet;
    protected List<EffectType> immunedEffects = new List<EffectType>();
    Dictionary<FightItemKind, int> fightItemHitsCounter = new Dictionary<FightItemKind, int>();

    [JsonIgnore]
    public bool Destroyed
    {
      get { return !Alive; }
      set
      {
        throw new Exception("LivingEntity.Destroyed - use Alive!");
      }
    }
    public EntityMoveKind MoveKind { get; set; }
    public static Point DefaultInitialPoint = new Point(0, 0);
    public Point PrevPoint;
    public Point InitialPoint = DefaultInitialPoint;
    EntityStats stats = new EntityStats();
    Dictionary<EffectType, float> chanceToExperienceEffect = new Dictionary<EffectType, float>();
    public int ActiveScrollCoolDownCounter { get; set; }
    Dictionary<EntityStatKind, float> nonPhysicalDamageStats = new Dictionary<EntityStatKind, float>();
    public Tile FixedWalkTarget = null;
    public LivingEntity AllyModeTarget;
    public bool HasRelocateSkill { get; set; }
    public static readonly EntityStats BaseStats;
    public string OriginMap { get; set; }
    SpellSource activeManaPoweredSpellSource;

    public virtual SpellSource ActiveManaPoweredSpellSource
    {
      get
      {
        return activeManaPoweredSpellSource;
      }
      set { activeManaPoweredSpellSource = value; }
    }

    //HACK, jest to handle AttackDescription with 0 amount of ammo
    public FightItem RecentlyActivatedFightItem { get; set; }

    public virtual FightItem ActiveFightItem
    {
      get => activeFightItem;
      set
      {
        activeFightItem = value;
        RecentlyActivatedFightItem = value;
      }
    }
    public Point Position
    {
      get { return point; }
    }

    public bool CanAttack { get; set; } = true;
    public EffectType DiedOfEffect;

    public EntityState state;
    public EntityState State
    {
      get { return state; }
      set
      {
        //Logger.LogInfo(this+" state=>"+state);
        var oldState = state;
        state = value;
        if (oldState != state)
          AppendAction(new LivingEntityStateChangedEvent(oldState, state, this));
      }
    }
    
    [JsonIgnore]
    public List<LivingEntity> EverHitBy { get; set; } = new List<LivingEntity>();

    bool alive = true;
    //[JsonIgnoreAttribute]
    public EntityStats Stats { get => stats; set => stats = value; }

    public bool IsWounded { get; protected set; }
    protected Dictionary<EffectType, int> effectsToUse = new Dictionary<EffectType, int>();
    public static readonly EffectType[] PossibleEffectsToUse = new EffectType[] {
    EffectType.Weaken, EffectType.IronSkin, EffectType.ResistAll, EffectType.Inaccuracy
    };

    [JsonIgnore]
    public Dictionary<AttackKind, bool> AlwaysHit = new Dictionary<AttackKind, bool>();

    static LivingEntity()
    {
      BaseStats = new EntityStats();

      BaseStats.SetStat(EntityStatKind.Strength, BaseStrength);
      BaseStats.SetStat(EntityStatKind.Defense, BaseDefence);
      BaseStats.SetStat(EntityStatKind.Health, BaseHealth);
      BaseStats.SetStat(EntityStatKind.Mana, BaseMana);
      BaseStats.SetStat(EntityStatKind.Magic, BaseMagic);
      BaseStats.SetStat(EntityStatKind.Dexterity, BaseDexterity);
    }

    public LivingEntity() : this(new Point(-1, -1), '\0', null)
    {

    }

    public LivingEntity(Container cont) : this(new Point(-1, -1), '\0', cont)
    {
    }

    public LivingEntity(Point point, char symbol, Container cont) : base(point, symbol)
    {
      lastingEffectsSet = new LastingEffectsSet(this, cont);
      this.Container = cont;
      foreach (var basicStat in BaseStats.GetStats())
      {
        var nv = basicStat.Value.Value.Nominal;
        if (nv > 0)
          Stats.SetNominal(basicStat.Key, nv);
      }

      Stats.SetNominal(EntityStatKind.MeleeAttack, BaseStrength.Value.Nominal);//attack is same as str for a simple entity

      Alive = true;
      Name = "";
      Stats.SetNominal(EntityStatKind.ChanceToMeleeHit, 75);
      Stats.SetNominal(EntityStatKind.ChanceToPhysicalProjectileHit, 75);
      Stats.SetNominal(EntityStatKind.ChanceToCastSpell, 75);

      var effectTypes = Enum.GetValues(typeof(EffectType)).Cast<EffectType>().ToList();
      foreach (var et in effectTypes)
      {
        var chance = GetDefaultChanceToExperienceEffect();
        //if (et == EffectType.Bleeding || et == EffectType.TornApart)
        //  chance = 100;
        //if (et == EffectType.Stunned)
        //  chance = 90;
        SetChanceToExperienceEffect(et, chance);
      }

      //var resists = new[] { EntityStatKind.ResistCold, EntityStatKind.ResistFire, EntityStatKind.ResistLighting, EntityStatKind.ResistPoison};
      //foreach(var res in resists)
      //  Stats.SetNominal(res, 10);

      effectsToUse[EffectType.Weaken] = GenerationInfo.DefaultEnemyWeakenUsageCount;
      //effectsToUse[EffectType.Rage] = GenerationInfo.DefaultEnemyRageUsageCount;
      effectsToUse[EffectType.IronSkin] = GenerationInfo.DefaultEnemyIronSkinUsageCount;
      effectsToUse[EffectType.ResistAll] = GenerationInfo.DefaultEnemyResistAllUsageCount;
      effectsToUse[EffectType.Inaccuracy] = GenerationInfo.DefaultEnemyResistAllUsageCount;
    }

    protected void AssertFalse(string info)
    {
      AppendAction(new Roguelike.Events.GameStateAction()
      {
        Type = Roguelike.Events.GameStateAction.ActionType.Assert,
        Info = info
      });
    }

    protected virtual void IncreaseStats(float inc, IncreaseStatsKind kind)
    {
      foreach (var kv in Stats.GetStats())
      {
        var incToUse = inc;
        if (kv.Value.Kind == EntityStatKind.Strength ||
            kv.Value.Kind == EntityStatKind.MeleeAttack
            //kv.Value.Kind == EntityStatKind.Defense
            )
        {
          incToUse *= 1.5f;
        }
        //enemies usually do not have that stat
        //if (
        //    kv.Value.Kind == EntityStatKind.FireAttack ||
        //    )
        //{
        //  incToUse *= 0.2f;
        //}
        var val = kv.Value.Value.Nominal * incToUse;
        Stats.SetNominal(kv.Key, val);
      }
    }

    public static float GetResistanceLevelFactor(int level)
    {
      //TODO !
      return (level + 1) * 10;
      //if (!ResistanceFactors.Any())
      //{
      //  for (int i = 0; i <= GameManager.MaxLevelIndex; i++)
      //  {
      //    double inp = ((double)i) / GameManager.MaxLevelIndex;
      //    float incPerc = (float)Sigmoid(inp);
      //    ////Debug.WriteLine(i.ToString() + ") ResistanceLevelFactor = " + fac);
      //    ResistanceFactors.Add(incPerc);
      //  }
      //}
      //if (level >= ResistanceFactors.Count)
      //  return 0;
    }

    protected virtual void InitResistance()
    {
      float resistBasePercentage = 5 * GetIncrease(this.Level, 3f);
      var incPerc = GetResistanceLevelFactor(this.Level);
      resistBasePercentage += resistBasePercentage * incPerc / 100;

      //if (PlainSymbol == GolemSymbol || PlainSymbol == VampireSymbol || PlainSymbol == WizardSymbol
      //  || kind != PowerKind.Plain)
      //{
      //  resistBasePercentage += 14;
      //}
      this.Stats.SetNominal(EntityStatKind.ResistFire, resistBasePercentage);
      this.Stats.SetNominal(EntityStatKind.ResistPoison, resistBasePercentage);
      this.Stats.SetNominal(EntityStatKind.ResistCold, resistBasePercentage);
      var rli = resistBasePercentage * 2.5f / 3f;
      this.Stats.SetNominal(EntityStatKind.ResistLighting, rli);
    }

    public float StatsIncreasePerLevel = .14f;

    protected float GetIncrease(int level, float factor = 1)
    {
      return 1 + (level * StatsIncreasePerLevel * factor);
    }

    public bool WasEverHitBy(LivingEntity le)
    {
      return EverHitBy.Contains(le);
    }

    public virtual void SetChanceToExperienceEffect(EffectType et, int chance)
    {
      chanceToExperienceEffect[et] = chance;
    }

    protected virtual int GetDefaultChanceToExperienceEffect() { return 10; }

    public void ReduceMana(float amount)
    {
      var stat = Stats.GetStat(EntityStatKind.Mana);
      stat.Subtract(amount);
    }

    [JsonIgnore]
    public virtual List<Algorithms.PathFinderNode> PathToTarget
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

    internal bool CalculateIfHitWillHappen(LivingEntity target, AttackKind kind)
    {
      if (IsAlwaysHitting(kind))
        return true;

      var esk = EntityStatKind.Unset;
      if (kind == AttackKind.Melee)
        esk = EntityStatKind.ChanceToMeleeHit;
      else if (kind == AttackKind.PhysicalProjectile)
        esk = EntityStatKind.ChanceToPhysicalProjectileHit;
      else if (kind == AttackKind.SpellElementalProjectile)
        esk = EntityStatKind.ChanceToCastSpell;

      var hitWillHappen = CalculateIfStatChanceApplied(esk, target);
      return hitWillHappen;
    }

    public bool IsAlwaysHitting(AttackKind kind)
    {
      return AlwaysHit.ContainsKey(kind) && AlwaysHit[kind];
    }

    internal bool CalculateIfStatChanceApplied(EntityStatKind esk, LivingEntity target = null)
    {
      var randVal = (float)RandHelper.Random.NextDouble();
      var chance = GetEffectChance(esk);
      if (target != null)
      {
        if (esk == EntityStatKind.ChanceToMeleeHit)
        {
          if (ShouldEvade(target, EntityStatKind.ChanceToEvadeMeleeAttack, null))
          {
            EventsManager.AppendAction(new LivingEntityAction(LivingEntityActionKind.Missed) { InvolvedEntity = this, Info = Name + " missed " + target.Name });
            return false;
          }
          //Logger.LogInfo(this + " CalculateIfStatChanceApplied true");
        }
        if (esk == EntityStatKind.ChanceToPhysicalProjectileHit)
        {
          if (ShouldEvade(target, EntityStatKind.ChanceToEvadePhysicalProjectileAttack, null))
          {
            EventsManager.AppendAction(new LivingEntityAction(LivingEntityActionKind.Missed) { InvolvedEntity = this, Info = Name + " missed " + target.Name });
            return false;
          }
          //Logger.LogInfo(this + " CalculateIfStatChanceApplied true");

        }
      }
      return randVal > 0 && (randVal * 100 <= chance);
    }

    virtual protected bool ShouldEvade(LivingEntity target, EntityStatKind esk, Spell spell)
    {
      if (spell != null && spell is OffensiveSpell os && os.AlwaysHit)
        return false;

      var avoidCh = target.GetCurrentValue(esk);
      var randValCh = (float)RandHelper.Random.NextDouble();
      if (randValCh * 100 <= avoidCh)
        return true;

      return false;
    }

    public virtual AttackDescription GetAttackValue(AttackKind attackKind)
    {
      return new AttackDescription(this, true, attackKind);
    }

    private void ReduceHealth(LivingEntity attacker, string sound, string damageDesc, string damageSource, ref float inflicted)
    {
      if (d_immortal)
        return;
      var manaShieldEffect = LastingEffectsSet.GetByType(EffectType.ManaShield);
      var manaReduce = inflicted;
      if (manaShieldEffect != null && this.Stats.Mana > 0)
      {
        if (inflicted > this.Stats.Mana)
          manaReduce = inflicted - this.Stats.Mana;

        ReduceMana(manaReduce);
        inflicted -= manaReduce;
        damageDesc = null;//TODO
      }

      ReduceHealth(inflicted);
      attacker.OnDamageCaused(inflicted, this);

      var ga = new LivingEntityAction(LivingEntityActionKind.GainedDamage) { InvolvedValue = inflicted, InvolvedEntity = this };
      var desc = damageDesc ?? Name + " received damage: " + inflicted.Formatted() + " " + damageSource;
      ga.Info = desc;

      AppendAction(ga);
      PlaySound(sound);

      var frighten = this.GetFirstLastingEffect(EffectType.Frighten);
      if (frighten != null)
        RemoveLastingEffect(frighten);

      if (attacker.IsTransformed())
      {
        var transf = attacker.GetFirstLastingEffect(EffectType.Transform);
        if (transf != null)
          attacker.RemoveLastingEffect(transf);
      }

      if (!this.EverHitBy.Contains(attacker))
        this.EverHitBy.Add(attacker);

      if (State == EntityState.Sleeping)
        State = EntityState.Idle;
    }

    protected virtual void OnDamageCaused(float inflicted, LivingEntity victim) { }
        
    internal bool EverCausedEffect(EffectType type)
    {
      return everCausedEffect.Contains(type);
    }

    internal void SetEverCaused(EffectType type)
    {
      everCausedEffect.Add(type);
    }

    public virtual bool CanCauseBleeding()
    {
      return true;
    }

    public static readonly List<int> AttackValueDecrease = Enumerable.Range(0, 20).ToList();
    public bool UseAttackVariation { get; set; } = true;

    public virtual float GetAttackVariation(AttackKind kind, float currentAttackValue, bool signed)
    {
      return FactorCalculator.GetRandAttackVariation(currentAttackValue, AttackValueDecrease, signed);
    }

    string GetAttackDesc(EntityStatKind esk)
    {
      if (esk == EntityStatKind.FireAttack ||
         esk == EntityStatKind.PoisonAttack ||
         esk == EntityStatKind.ColdAttack ||
         esk == EntityStatKind.LightingAttack)
        return esk.ToString().Replace("Attack", "").ToLower();

      return esk.ToString();
    }

    public Dictionary<EntityStatKind, float> GetNonPhysicalDamages()
    {
      nonPhysicalDamageStats.Clear();
      foreach (var stat in Loot.AttackingNonPhysicalStats)
      {
        var cv = Stats.GetCurrentValue(stat);
        if (cv > 0)
          nonPhysicalDamageStats[stat] = cv;
      }

      return nonPhysicalDamageStats;
    }

    protected void PlaySound(string sound)
    {
      if (sound.Any())
        AppendAction(new SoundRequestAction() { SoundName = sound });
    }

    float AppendNonPhysicalDamage(EntityStatKind esk, float npdAttack, ref float inflicted, ref string inflictedDesc)
    {
      var npd = CalculateNonPhysicalDamage(esk, npdAttack);
      if (npd != 0)
        inflictedDesc += " " + GetAttackDesc(esk) + ": " + npd.Formatted();
      inflicted += npd;

      return npd;
    }

    public float CalcMeleeDamage(float attackerPower, ref string desc)
    {
      float defense = GetDefense();
      if (defense == 0)
      {
        AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.Assert, Info = "Stats.Defence == 0!!!" });
        return 0;
      }
      if (!Alive)
      {
        //hitting a dead man ?
        AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.Assert, Info = "!Alive" });
        return 0;
      }

      lastHitBySpell = false;
      if (defense <= 0)
        defense = 1;//HACK, TODO
      var inflicted = attackerPower / defense;
      desc = Name.ToString() + " received melee damage: " + inflicted.Formatted();
      return inflicted;
    }
    public virtual float OnMelleeHitBy(LivingEntity attacker)
    {
      string desc = "";
      var av = attacker.GetAttackValue(AttackKind.Melee);
      var currentPhysicalVariated = av.CurrentPhysicalVariated;
      var inflicted = CalcMeleeDamage(currentPhysicalVariated, ref desc);
      var npds = attacker.GetNonPhysicalDamages();
      foreach (var stat in npds)
      {
        var npd = AppendNonPhysicalDamage(stat.Key, stat.Value, ref inflicted, ref desc);

        var manaShieldEffect = LastingEffectsSet.GetByType(EffectType.ManaShield);//TODO, shall Mana Shield be that powerful ?
        if (manaShieldEffect == null)
          LastingEffectsSet.TryAddLastingEffectOnHit(npd, attacker, stat.Key);
      }

      return InflictDamage(attacker, true, ref inflicted, ref desc);
    }

    public float InflictDamage(LivingEntity attacker, bool normalAttack, ref float inflicted, ref string desc)
    {
      ReduceHealth(attacker, "punch", desc, "", ref inflicted);

      if (normalAttack && inflicted > 0)
        StealStatIfApplicable(inflicted, attacker);
      //if (this is Enemy || this is Hero)// || this is CrackedStone)
      //{
      //  PlayPunchSound();
      //}
      var dead = DieIfShould(EffectType.Unset);
      if (normalAttack && !dead)
      {
        if (IsWounded)
        {
          if (attacker.CanCauseBleeding())
            StartBleeding(inflicted / 3, attacker, -1);
        }
        attacker.EnsurePhysicalHitEffect(inflicted, this);
      }

      return inflicted;
    }
        
    public int GetFightItemKindHitCounter(FightItemKind kind)
    {
      return fightItemHitsCounter.ContainsKey(kind) ? fightItemHitsCounter[kind] : 0;
    }

    public HitResult OnHitBy(Dungeons.Tiles.Abstract.IProjectile projectile)
    {
      if (projectile is Spell spell)
      {
        if (ShouldEvade(this, EntityStatKind.ChanceToEvadeElementalProjectileAttack, spell))
        {
          //GameManager.Instance.AppendDiagnosticsUnityLog("ChanceToEvadeMagicAttack worked!");
          var ga = new GameEvent() { Info = Name + " evaded " + spell.Kind, Level = ActionLevel.Important };
          AppendAction(ga);
          return HitResult.Evaded;
        }
        var attackingStat = spell.Kind.ToEntityStatKind();
        var ad = new AttackDescription(spell.Caller,
          true,
          spell.IsFromMagicalWeapon ? AttackKind.WeaponElementalProjectile : AttackKind.SpellElementalProjectile,
          spell as OffensiveSpell);
        var dmg = CalculateNonPhysicalDamage(attackingStat, ad.CurrentTotal);
        OnHitBy(dmg, spell);
        return HitResult.Hit;
      }

      else if (projectile is FightItem fi)
      {
        if (fi is ProjectileFightItem pfi)
        {
          var damageDesc = "";

          var ad = new AttackDescription(fi.Caller, true, AttackKind.PhysicalProjectile);
          var inflicted = CalcMeleeDamage(ad.CurrentPhysicalVariated, ref damageDesc);//TODO what about NonPhysical?
          var sound = pfi.HitTargetSound;// "punch";
          var srcName = fi.FightItemKind.ToDescription();
          var attacker = pfi.Caller;
          fightItemHitsCounter.Increment(fi.FightItemKind);

          if (fi.FightItemKind == FightItemKind.ExplosiveCocktail)
          {
            var npd = CalculateNonPhysicalDamage(EntityStatKind.FireAttack, fi.Damage);
            lastingEffectsSet.EnsureEffect(EffectType.Firing, npd, attacker, fi.TurnLasting);
            PlaySound(sound);//TODO
            return HitResult.Hit;
          }
          else if (fi.FightItemKind == FightItemKind.PoisonCocktail)
          {
            var npd = CalculateNonPhysicalDamage(EntityStatKind.PoisonAttack, fi.Damage);
            lastingEffectsSet.EnsureEffect(EffectType.Poisoned, npd, attacker, fi.TurnLasting);
            PlaySound(sound);//TODO
            return HitResult.Hit;
          }
          else if (fi.FightItemKind == FightItemKind.ThrowingKnife ||
            fi.FightItemKind == FightItemKind.PlainArrow || fi.FightItemKind == FightItemKind.PlainBolt)
          {
            if (RandHelper.GetRandomDouble() > 0.75)
              StartBleeding(inflicted / 3, attacker, 3);
          }
          else if (fi.FightItemKind == FightItemKind.HunterTrap)
          {
            inflicted = fi.Damage;
            var bleed = StartBleeding(inflicted, null, fi.TurnLasting);
            bleed.Source = fi;

            fi.SetState(FightItemState.Busy);
            sound = "trap";
          }

          foreach (var kv in ad.NonPhysical)
          {
            inflicted += CalculateNonPhysicalDamage(kv.Key, kv.Value);
          }

          ReduceHealth(attacker, sound, damageDesc, srcName, ref inflicted);
          return HitResult.Hit;
        }
        else
          Assert(false, "OnHitBy!" + projectile);
      }
      else
      {
        Assert(false, "OnHitBy - not supported" + projectile);
      }

      return HitResult.Unset;
    }

    protected virtual float GetDamageAddition(ProjectileFightItem pfi)
    {
      return 0;
    }

    public LastingEffect StartBleeding(float damageEachTurn, LivingEntity attacker, int turnLasting)
    {
      return lastingEffectsSet.EnsureEffect(EffectType.Bleeding, damageEachTurn, attacker, turnLasting);
    }

    public bool ImmuneOnEffects { get; set; }

    protected virtual void OnHitBy(float amount, Spell spell, string damageDesc = null)
    {
      if (!Alive)
        return;

      //if (attacker is Hero && this is Enemy && (this as Enemy).HeroAlly)
      //  amount /= 10;
      var sound = "";
      var srcName = "";
      if (spell != null)
      {
        srcName = " from " + spell.Kind.ToDescription();
        if (spell.Kind == SpellKind.StonedBall)
          amount /= Stats.Defense;

        sound = spell.GetHitSound();

        lastHitBySpell = true;
      }
      var attacker = spell.Caller;
      //Debug
      //if(damageDesc == null)
      //  damageDesc = "";
      //damageDesc += "_D " + spell.GetHashCode();

      //Debug
      ReduceHealth(attacker, sound, damageDesc, srcName, ref amount);

      if(!ImmuneOnEffects)
        LastingEffectsSet.TryAddLastingEffectOnHit(amount, attacker, spell);
    }

    protected virtual LastingEffect EnsurePhysicalHitEffect(float inflicted, LivingEntity victim)
    {
      return null;
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

    public void AddImmunity(EffectType effect)
    {
      immunedEffects.Add(effect);
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

    public LastingEffectsSet LastingEffectsSet { get => lastingEffectsSet; }

    //TODO ! use IsInProjectileReach
    public float MaxMagicAttackDistance { get; internal set; } = GenerationInfo.MaxMagicAttackDistance;

    public virtual void RemoveLastingEffect(LastingEffect le)
    {
      lastingEffectsSet.RemoveLastingEffect(le);
    }

    public static float GetReducePercentage(float orgAmount, float discPerc)
    {
      return orgAmount * discPerc / 100f;
    }

    public override string ToString()
    {
      var str = base.ToString();
      str += " " + this.State + ", Alive:" + Alive + ", H:" + Stats.Health + ", Lvl:" + this.Level;
      return str;
    }

    protected void AppendAction(GameEvent ac)
    {
      if (EventsManager != null)
        EventsManager.AppendAction(ac);
    }

    protected void Assert(bool check, string desc = "")
    {
      if (!check && EventsManager != null)
        EventsManager.Assert(check, desc);
    }

    public bool DieIfShould(EffectType effect)
    {
      if (Alive && IsHealthGone())
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
      {
        info += ", killing effect: " + DiedOfEffect.ToDescription();
        try
        {
          if (DiedOfEffect == EffectType.Bleeding && LastingEffectsSet.LastingEffects.Any())
          {
            var trap = LastingEffectsSet.LastingEffects
              .Where(i => i.Source is ProjectileFightItem pfi && pfi.FightItemKind == FightItemKind.HunterTrap)
              .Select(i => i.Source)
              .Cast<ProjectileFightItem>()
              .SingleOrDefault();
            if (trap != null)
            {
              trap.SetState(FightItemState.Deactivated);
            }
          }
        }
        catch (Exception ex)
        {
          Logger.LogError(ex);
        }
      }
      return new LivingEntityAction(LivingEntityActionKind.Died) { InvolvedEntity = this, Level = ActionLevel.Important, Info = info };
    }

    public bool IsHealthGone()
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
      victim.OnMelleeHitBy(this);
    }

    public float GetCurrentValue(EntityStatKind kind)
    {
      var stat = Stats.GetStat(kind);
      var cv = stat.Value.CurrentValue;
      if (stat.Unit == EntityStatUnit.Percentage && cv > 100)
      {
        Logger.LogError("stat.Unit == EntityStatUnit.Percentage && cv > 100! "+this);
        cv = 100;
      }

      return cv;
    }

    public float GetTotalValue(EntityStatKind esk)
    {
      return Stats.GetTotalValue(esk);
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
        
    public void SetIsWounded(bool wounded)
    {
      if (wounded && IsWounded)
        return;

      IsWounded = wounded;
      var def = Stats.GetStat(EntityStatKind.Defense);
      if (wounded)
      {
        def.Subtract(def.Value.TotalValue / 2);

        if (Wounded != null)
          Wounded(this, EventArgs.Empty);
      }
      else
        def.Subtract(-def.Value.TotalValue / 2);
    }

    public virtual float GetChanceToHit(bool melee)
    {
      return melee ? GetEffectChance(EntityStatKind.ChanceToMeleeHit) : GetEffectChance(EntityStatKind.ChanceToPhysicalProjectileHit);
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
      var factor = statValue * nominalValuePercInc / 100f;// CalcEffectValue(nominalValuePercInc, statValue);
      return new EffectiveFactor(factor);
    }

    public void ApplyPassiveSpell(PassiveSpell spell)
    {
      AddLastingEffectFromSpell(spell.Kind, SpellConverter.EffectTypeFromSpellKind(spell.Kind));
    }

    public bool IsTransformed()
    {
      return this.LastingEffectsSet.HasEffect(EffectType.Transform);
    }

    Dictionary<SurfaceKind, int> surfaceSkillLevel = new Dictionary<SurfaceKind, int>();
    public void SetSurfaceSkillLevel(SurfaceKind kind, int level)
    {
      surfaceSkillLevel[kind] = level;
    }

    public int GetSurfaceSkillLevel(SurfaceKind kind)
    {
      if (surfaceSkillLevel.ContainsKey(kind))
        return surfaceSkillLevel[kind];

      return 0;
    }

    public virtual void PlayAllySpawnedSound() { }

    Difficulty difficulty;
    private FightItem activeFightItem;

    public virtual bool SetLevel(int level, Difficulty? diff = null)
    {
      difficulty = Difficulty.Normal;
      if (diff == null)
        diff = GenerationInfo.Difficulty;
      if (diff != null)
        difficulty = diff.Value;
      Assert(level >= 1);
      if (!CanIncreaseLevel())
      {
        return false;
      }
      this.Level = level;
      InitStatsFromName();
      var hard = diff == Difficulty.Hard;
      float inc = 1;
      if (diff == Difficulty.Normal)
        inc = 1.1f;
      else if (diff == Difficulty.Hard)
        inc = 1.25f;

      if (inc > 1)
        IncreaseStats(inc, IncreaseStatsKind.Difficulty);

      if (level > 1)
      {
        inc = GetIncrease(hard ? level + 1 : level);
        IncreaseStats(inc, IncreaseStatsKind.Level);
      }
      InitResistance();
      InitActiveScroll();
      return true;
    }

    protected virtual bool CanIncreaseLevel()
    {
      return true;
    }

    protected virtual void InitActiveScroll()
    {
    }

    protected virtual void InitStatsFromName() { }

    public Point GetPoint()
    {
      return point;
    }

    public virtual SpellSource GetAttackingScroll()
    {
      return ActiveManaPoweredSpellSource;
    }

    public bool CanBeHitBySpell()
    {
      return true;
    }

    internal virtual bool CanMakeRandomMove()
    {
      return true;
    }

    public bool IsSleeping
    {
      get { return state == EntityState.Sleeping; }
    }

    public virtual void RemoveFightItem(FightItem fi)
    {

    }

    public bool IsInProjectileReach(Roguelike.Abstract.Projectiles.IProjectile fi, Point target)
    {
      return DistanceFrom(target) < fi.Range;
    }

    //static EffectType[] BlockingLEs = new[] { EffectType.Stunned, Effects.EffectType.Bleeding };

    public bool IsMoveBlockedDueToLastingEffect(out string reason)
    {
      reason = "";
      var le = LastingEffects.Where(i => i.PreventMove).FirstOrDefault();
      if (le != null)
      {
        reason = le.Type.ToDescription();
      }

      return le != null;
    }

    public bool IsStatRandomlyTrue(EntityStatKind kind)
    {
      var statValue = GetCurrentValue(kind);
      return statValue / 100f > RandHelper.GetRandomDouble();
    }

    public float CalcNextLevelExperience(float currentNextLevelExperience)
    {
       return Calculated.FactorCalculator.AddFactor((int)currentNextLevelExperience, 110);
    }

    public virtual bool CanUseAbility(AbilityKind kind, out bool activeUsed)
    {
      activeUsed = false;
      var esk = EntityStatKind.Unset;
      if (kind == AbilityKind.StrikeBack)
          esk = EntityStatKind.ChanceToStrikeBack;
      return IsStatRandomlyTrue(esk);
    }

    public virtual Weapon GetActiveWeapon()
    {
        return null;
    }

    [JsonIgnore]
    public bool MoveDueToAbilityVictim { get; internal set; }

    public virtual void Consume(IConsumable consumable)
    {
      if (consumable.StatKind == EntityStatKind.Unset)
      {
        var pot = consumable.Loot as Potion;
        Debug.Assert(pot != null && pot.Kind == PotionKind.Antidote);
      }

      if (consumable is SpecialPotion)
      {
        var sp = consumable as SpecialPotion;
        this.Stats[sp.StatKind].Nominal += (float)consumable.StatKindEffective.Value;
      }
      else
      {
        if (consumable is Potion potion)
        {
          Debug.Assert(consumable.ConsumptionSteps == 1);
          if (potion.Kind == PotionKind.Antidote)
          {
            var le = GetFirstLastingEffect(EffectType.Poisoned);
            if (le != null)
              RemoveLastingEffect(le);
          }
          else
          {
            var factor = LastingEffectsSet.CalcLastingEffectInfo(EffectType.Unset, consumable);
            DoConsume(consumable.StatKind, factor);
          }
        }
        else
        {
          if (consumable is Hooch hooch)
          {
            LastingEffectsSet.AddPercentageLastingEffect(EffectType.Hooch, consumable, consumable.Loot);
          }
          else if(consumable is Food food && food.EffectType == EffectType.Poisoned)
          {
            var npd = CalculateNonPhysicalDamage(EntityStatKind.PoisonAttack, food.StatKindEffective.Value);
            int tours = food.Duration;
            LastingEffectsSet.EnsureEffect(EffectType.Poisoned, npd, null, tours);
          }
          else
          {
            EffectType et = EffectType.ConsumedRawFood;
            if (consumable.Roasted)
              et = EffectType.ConsumedRoastedFood;
            LastingEffectsSet.AddPercentageLastingEffect(et, consumable, consumable.Loot);
          }
        }
      }

      var info = Name + " consumed " + (consumable as Dungeons.Tiles.Tile).Name + ", Health: " + this.GetCurrentValue(EntityStatKind.Health);
      AppendAction(new LootAction(consumable.Loot, this) { Kind = LootActionKind.Consumed, Info = info });
    }
  }
}
