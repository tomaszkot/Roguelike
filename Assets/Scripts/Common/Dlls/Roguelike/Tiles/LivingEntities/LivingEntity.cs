using Dungeons.Core;
using Dungeons.Core.Policy;
using Dungeons.Fight;
using Dungeons.Tiles;
using Dungeons.Tiles.Abstract;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Factors;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using Roguelike.Utils;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Roguelike.State
{
  public class CommandUseInfo
  {
    public EntityCommandKind Kind { get; set; }
    public int Cooldown { get; set; }
    public int UseCounter { get; set; }
    public string Sound { get; internal set; }
    public string Info { get; internal set; }
    public string MessageAboveHead { get; internal set; }

    public  void IncreaseUseCount()
    {
      UseCounter++;
    }

    public CommandUseInfo():this(EntityCommandKind.Unset)
    { 
    }
    public CommandUseInfo(EntityCommandKind cmd)
    {
      Kind = cmd;
    }
  }
}
namespace Roguelike.Tiles.LivingEntities
{
  public enum EntityState { Idle, Moving, Attacking, CastingProjectile, Sleeping }
  public enum EntityMoveKind { Freestyle, FollowingHero, ReturningHome }

  public enum DeathEffect { Unset, TearApart, BreakApart }

  /// <summary>
  /// Base type for everything that is alive and can be potentialy killed
  /// </summary>
  public class LivingEntity : Tile, ILastingEffectOwner, IDestroyable, ILivingEntity
  {
    //some attributes describing LivingEntity
    public EntityProffesionKind Proffesion 
    {
      get; 
      set; 
    }

    public bool IsMercenary => Proffesion == EntityProffesionKind.Mercenary;
    public bool HitRandomTarget { get; internal set; }
    public string Herd { get; set; } = "";
    [JsonIgnore]
    public bool LastMeleeAttackWasOK { get; set; }
    [JsonIgnore]
    public bool RewardGenerated { get; set; }
    public int level = 1;
    public int Level
    {
      get { return level; }
      set {
        level = value;
        if (value > 1 && Symbol == '@')
        {
          int k = 0;
          k++;
        }
      }
    }

    [JsonIgnore]
    public BattleOrder BattleOrder { get; set; }

    public bool LevelSet { get; set; }

    public Loot ForcedReward { get; set; }
    public EntityKind EntityKind { get; set; }

    List<EffectType> everCausedEffect = new List<EffectType>();

    [JsonIgnore]
    Dictionary<EffectType, int> lastingEffectCounter = new Dictionary<EffectType, int>();

    [JsonIgnore]
    public AbilityKind WorkingAbilityKind;

    //[JsonIgnore]
    //public AbilityKind ActivatedAbilityKind;

    public event EventHandler Wounded;

    [JsonIgnore]
    public DeathEffect DeathEffect { get; set; }
    public bool Immortal { get; set; } = false;
    protected const int StartStrength = 10;
    protected const int StartDefense = 6;
    public static readonly Dictionary<EntityStatKind, int> StartStatValues = new Dictionary<EntityStatKind, int>() {
      { EntityStatKind.Strength, StartStrength },
      { EntityStatKind.Health, 10 },
      { EntityStatKind.Defense, StartDefense },
      { EntityStatKind.Dexterity, 10 },
      { EntityStatKind.Mana, 10 },
      { EntityStatKind.Magic, 10 },
    };

    static Dictionary<EntityStatKind, EntityStatKind> statsHitIncrease = new Dictionary<EntityStatKind, EntityStatKind>
    {
      { EntityStatKind.LifeStealing, EntityStatKind.Health },
      { EntityStatKind.ManaStealing, EntityStatKind.Mana }
    };

    public int GetLastingEffectCounter(EffectType et)
    {
      if (!lastingEffectCounter.ContainsKey(et))
        return 0;
      return lastingEffectCounter[et];
    }

    public void IncreateLastingEffectCounter(EffectType et)
    {
      if (!lastingEffectCounter.ContainsKey(et))
        lastingEffectCounter[et] = 0;

      lastingEffectCounter[et]++;
    }

    public virtual float GetExtraDamage(SpellKind kind, float damage)
    {
      return 0;
    }

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
    public int SelectedScrollCoolDownCounter { get; set; }

    [JsonIgnore]
    public bool EverSensedWeakPoint = false;
    Dictionary<EntityStatKind, float> nonPhysicalDamageStats = new Dictionary<EntityStatKind, float>();
    public Tile FixedWalkTarget = null;
    public LivingEntity AllyModeTarget;
    public bool HasRelocateSkill { get; set; }
    //public static readonly EntityStats BaseStats;
    public string OriginMap { get; set; }
    SpellSource selectedManaPoweredSpellSource;

    [JsonIgnore]
    public int TrappedCounter { get; set; }
    public virtual SpellSource SelectedManaPoweredSpellSource
    {
      get
      {
        return selectedManaPoweredSpellSource;
      }
      set { selectedManaPoweredSpellSource = value; }
    }

    public FightItem GetActivatedFightItem()
    {
      var fightItem = SelectedFightItem;
      return fightItem;
    }

    public virtual FightItem GetFightItemFromActiveProjectileAbility()
    {
      return null;
    }

    public virtual Ability GetActivePhysicalProjectileAbility()
    {
      return null;
    }

    public virtual Ability SelectedActiveAbility
    {
      get;
      set;
    }

    public virtual FightItem SelectedFightItem
    {
      get => selectedFightItem;
      set
      {
        selectedFightItem = value;
      }
    }

    public Point Position
    {
      get { return point; }
    }

    [JsonIgnore]
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
        if (oldState == EntityState.Sleeping)
        {
          Logger.LogInfo(this + " state=>" + state);
        }
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
      foreach (var basicStat in StartStatValues)
      {
        Stats.SetNominal(basicStat.Key, GetStartStat(basicStat.Key));
      }

      Stats.SetNominal(EntityStatKind.ThrowingTorchChanceToCauseFiring, 30);

      AlignMeleeAttack();

      Alive = true;
      //Name = "";?
      Stats.SetNominal(EntityStatKind.ChanceToMeleeHit, 75);
      Stats.SetNominal(EntityStatKind.ChanceToPhysicalProjectileHit, 75);
      Stats.SetNominal(EntityStatKind.ChanceToCastSpell, 75);

      Stats.SetNominal(EntityStatKind.ChanceForPiercing, 75);

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

    protected void AlignMeleeAttack()
    {
      Stats.SetNominal(EntityStatKind.MeleeAttack, this.Stats.Strength);//attack is same as str for a simple entity
    }

    public virtual float GetStartStat(EntityStatKind esk)
    {
      var value = StartStatValues[esk];
      return value;
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

        if (kind != IncreaseStatsKind.Ability)
        {
          if (kv.Value.Kind == EntityStatKind.Strength ||
              kv.Value.Kind == EntityStatKind.MeleeAttack
              //kv.Value.Kind == EntityStatKind.Defense
              )
          {
            incToUse *= 1.7f;
          }
          //enemies usually do not have that stat
          //if (
          //    kv.Value.Kind == EntityStatKind.FireAttack ||
          //    )
          //{
          //  incToUse *= 0.2f;
          //}
          if (kv.Value.Unit == EntityStatUnit.Percentage)
          {
            incToUse = 1.1f;
            if (kv.Value.Value.Nominal > 50)
              incToUse = 1.05f;
          }
        }
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
      var rf = resistBasePercentage;
      if (tag1.ToLower().Contains("lava") || Name.ToLower().Contains("lava"))
      {
        rf += rf * 2f;
        if (rf > 75)
          rf = 75;
      }
      this.Stats.SetNominal(EntityStatKind.ResistFire, rf);
      this.Stats.SetNominal(EntityStatKind.ResistPoison, resistBasePercentage);
      this.Stats.SetNominal(EntityStatKind.ResistCold, resistBasePercentage);
      var rli = resistBasePercentage * 2.5f / 3f;
      this.Stats.SetNominal(EntityStatKind.ResistLighting, rli);
    }

    protected float GetIncrease(int level, float factor = 1)
    {
      return 1 + (level * GenerationInfo.EnemyStatsIncreasePerLevel * factor);
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

    [JsonIgnore]
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

    internal bool CalculateIfHitWillHappen(IHitable target, AttackKind kind, ProjectileFightItem pfi)
    {
      if (IsAlwaysHitting(kind))
        return true;

      var esk = EntityStatKind.Unset;
      if (kind == AttackKind.Melee)
        esk = EntityStatKind.ChanceToMeleeHit;
      else if (kind == AttackKind.PhysicalProjectile)
        esk = EntityStatKind.ChanceToPhysicalProjectileHit;
      else if (kind == AttackKind.SpellElementalProjectile || kind == AttackKind.WeaponElementalProjectile)
        esk = EntityStatKind.ChanceToCastSpell;

      var hitWillHappen = CalculateIfStatChanceApplied(esk, target, pfi);
      return hitWillHappen;
    }

    public virtual bool ShallAvoidTrap()
    {
      return TrappedCounter > 0;
    }

    public bool IsAlwaysHitting(AttackKind kind)
    {
      return AlwaysHit.ContainsKey(kind) && AlwaysHit[kind];
    }

    internal bool CalculateIfStatChanceApplied(EntityStatKind esk, IHitable target, ProjectileFightItem pfi = null)
    {
      if (!(target is LivingEntity))
        return true;
      if (target != null)
      {
        bool evaded = false;
        if (esk == EntityStatKind.ChanceToMeleeHit)
        {
          if (ShouldEvade(target, EntityStatKind.ChanceToEvadeMeleeAttack, null))
            evaded = true;
        }
        else if (esk == EntityStatKind.ChanceToPhysicalProjectileHit)
        {
          if (ShouldEvade(target, EntityStatKind.ChanceToEvadePhysicalProjectileAttack, null))
            evaded = true;
          //Logger.LogInfo(this + " CalculateIfStatChanceApplied true");
        }
        if (evaded)
        {
          return false;
        }
      }
      return ShallApplyStatKind(esk, pfi);
    }

    public bool ShallApplyStatKind(EntityStatKind esk, ProjectileFightItem pfi)
    {
      var chance = GetEffectChance(esk, pfi);

      var randVal = (float)RandHelper.Random.NextDouble();
      return randVal > 0 && (randVal * 100 <= chance);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="esk">ChanceToPhysicalProjectileHit, ChanceToEvadeMeleeAttack</param>
    /// <param name="spell"></param>
    /// <returns></returns>
    virtual protected bool ShouldEvade(IHitable target, EntityStatKind esk, Spell spell)
    {
      if (spell != null && spell is OffensiveSpell os && os.AlwaysHit)
        return false;

      if (target is LivingEntity leTarget)
      {
        var avoidChance = leTarget.GetCurrentValue(esk);
        var randValCh = (float)RandHelper.Random.NextDouble();
        if (randValCh * 100 <= avoidChance)
          return true;
      }


      return false;
    }

    public virtual AttackDescription GetAttackValue(AttackKind attackKind)
    {
      return new AttackDescription(this, true, attackKind);
    }

    [JsonIgnore]
    public AttackKind LastReceivedAttackKind { get; set; }

    private void ReduceHealth(LivingEntity attacker, string sound, string damageDesc, 
      string damageSource, ref float inflicted, AttackKind ak)
    {
      LastReceivedAttackKind = ak;
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

      var ga = new LivingEntityAction(LivingEntityActionKind.GainedDamage)
      {
        InvolvedValue = inflicted, 
        InvolvedEntity = this, 
        AttackerEntity = attacker,
        Sound = sound 
      };
      var desc = damageDesc ?? Name + " received damage: " + inflicted.Formatted() + " " + damageSource;
      ga.Info = desc;

      AppendAction(ga);
      PlaySound(sound);

      var frighten = this.GetFirstLastingEffect(EffectType.Frighten);
      if (frighten != null)
        RemoveLastingEffect(frighten);

      attacker.HandleTransformOnAttack();

      if (!this.EverHitBy.Contains(attacker))
        this.EverHitBy.Add(attacker);

      if (State == EntityState.Sleeping)
        State = EntityState.Idle;
    }

    public void HandleTransformOnAttack()
    {
      if (IsTransformed())
      {
        var transf = GetFirstLastingEffect(EffectType.Transform);
        if (transf != null)
          RemoveLastingEffect(transf);
      }
    }

    [JsonIgnore]
    public int InflictedHitsCount { get; set; }
    protected virtual void OnDamageCaused(float inflicted, LivingEntity victim) 
    {
      InflictedHitsCount++;
    }

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

    public float CalcMeleeDamage(float attackerPower, ref string desc, ProjectileFightItem fi = null)
    {
      float defense = GetDefense();
      if (defense == 0)
      {
        AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.Assert, Info = "Stats.Defense == 0!!!" });
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
      var damageName = "melee";
      if (fi != null)
        damageName = "projectile";
      desc = Name.ToString() + " received " + damageName + " damage: " + inflicted.Formatted();
      return inflicted;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attackerItf"></param>
    /// <returns></returns>
    public HitResult OnHitBy(ILivingEntity attackerItf)
    {
      var res = HitResult.Hit;
      OnMeleeHitBy(attackerItf as LivingEntity);
      return res;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="attacker"></param>
    /// <returns></returns>
    public virtual float OnMeleeHitBy(LivingEntity attackerItf)
    {
      var attacker = attackerItf as LivingEntity;
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

      if (this is Enemy && GenerationInfo.DebugInfo.ForcedEffectType != EffectType.Unset)
      {
        var effectInfo = LastingEffectsSet.CalcLastingEffDamage(GenerationInfo.DebugInfo.ForcedEffectType, inflicted, null);
        LastingEffectsSet.AddLastingEffect(effectInfo, EffectOrigin.External, null, EffectTypeConverter.Convert(effectInfo.Type));
      }
      //this.LastingEffectsSet.AddPercentageLastingEffect(EffectType.Frozen, new Scroll(SpellKind.IceBall), attacker);

      return InflictMeleeDamage(attacker, true, ref inflicted, ref desc);
    }

    public float InflictMeleeDamage(LivingEntity attacker, bool normalAttack, ref float inflicted, ref string desc)
    {
      ReduceHealth(attacker, "punch", desc, "", ref inflicted, AttackKind.Melee);

      if (normalAttack && inflicted > 0)
        StealStatIfApplicable(inflicted, attacker);
      //if (this is Enemy || this is Hero)// || this is CrackedStone)
      //{
      //  PlayPunchSound();
      //}
      var dead = DieIfShould(EffectType.Unset);
      if (normalAttack && !dead)
      {
        if (IsWounded || attacker.AlwaysCausesLastingEffect(EffectType.Bleeding))
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

    public Dictionary<AttackKind, int> AttackKindHitsCounter { get; set; }

    public HitResult OnHitBy(IDamagingSpell ds, IPolicy policy)
    {
      OnHitBy(ds.Damage, ds as Spell);
      return HitResult.Hit;
    }

    public HitResult OnHitBy(IProjectile projectile, IPolicy policy)
    {

      if (projectile is Spell spell)
      {
        if (ShouldEvade(this, EntityStatKind.ChanceToEvadeElementalProjectileAttack, spell))
        {
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
          return OnHitBy(pfi);
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

    HitResult ReturnHitResult(EntityStatKind esk, ProjectileFightItem pfi)//, FightItemKind fik = FightItemKind.Unset)
    {
      LivingEntity attacker = pfi.Caller;
      return ReturnHitResult(attacker, esk, pfi.Damage, pfi.Duration, pfi.HitSound, pfi.FightItemKind);
    }
    HitResult ReturnHitResult(LivingEntity attacker, EntityStatKind esk, float damage, int duration, string snd,
      FightItemKind fik = FightItemKind.Unset)
    {
      float npd = 0;
      EffectType et = EffectType.Unset;
      if (fik != FightItemKind.WeightedNet)
      {
        npd = CalculateNonPhysicalDamage(esk, damage);
        et = EffectTypeConverter.Convert(esk);
      }
      else
        et = EffectType.WebTrap;

      lastingEffectsSet.EnsureEffect(et, npd, attacker, duration);
      PlaySound(snd);//TODO
      LastReceivedAttackKind = AttackKind.PhysicalProjectile;
      return HitResult.Hit;
    }

    protected virtual HitResult OnHitBy(ProjectileFightItem pfi)
    {
      var ad = pfi.AttackDescription;
      if (ad == null)
      {
        Logger.LogError("OnHitBy ProjectileFightItem GetActivatedFightItem == null");
        return HitResult.Unset;
      }
      //Logger.LogInfo(this+" OnHitBy ProjectileFightItem ");
      var damageDesc = "";
      var inflicted = CalcMeleeDamage(ad.CurrentPhysicalVariated, ref damageDesc, pfi);//TODO what about NonPhysical?
      var sound = pfi.HitTargetSound;// "punch";
      var srcName = pfi.FightItemKind.ToDescription();
      var attacker = pfi.Caller;
      fightItemHitsCounter.Increment(pfi.FightItemKind);
      pfi.Caller.EverUsedFightItem = true;

      if (pfi.FightItemKind == FightItemKind.ExplosiveCocktail)
      {
        return ReturnHitResult(EntityStatKind.FireAttack, pfi);
      }
      if (pfi.FightItemKind == FightItemKind.ThrowingTorch)
      {
        if (attacker.IsStatRandomlyTrue(EntityStatKind.ThrowingTorchChanceToCauseFiring, EffectType.Firing))
        {
          return ReturnHitResult(EntityStatKind.FireAttack, pfi);
        }
      }
      else if (pfi.FightItemKind == FightItemKind.PoisonCocktail)
      {
        return ReturnHitResult(EntityStatKind.PoisonAttack, pfi);
      }
      else if (pfi.FightItemKind == FightItemKind.WeightedNet)
      {
        return ReturnHitResult(EntityStatKind.Unset, pfi);
      }
      else if (pfi.FightItemKind == FightItemKind.ThrowingKnife || pfi.FightItemKind.IsBowLikeAmmunition())
      {
        if (RandHelper.GetRandomDouble() > 0.75)
          StartBleeding(inflicted / 3, attacker, 3);
      }
      else if (pfi.FightItemKind == FightItemKind.HunterTrap)
      {
        TrappedCounter++;
        inflicted = pfi.Damage;
        var bleed = StartBleeding(inflicted, null, pfi.Duration);
        if (bleed != null)
          bleed.Source = pfi;

        pfi.SetState(FightItemState.Busy);
        sound = pfi.HitSound;
      }
      //else
      //Logger.LogError("OnHitBy unhandled FightItemKind!");

      foreach (var kv in ad.NonPhysical)
      {
        inflicted += CalculateNonPhysicalDamage(kv.Key, kv.Value);
      }

      ReduceHealth(attacker, sound, damageDesc, srcName, ref inflicted, AttackKind.PhysicalProjectile);

      if (pfi.FightItemKind.IsBowLikeAmmunition())
      {
        if (pfi.FightItemKind.IsCausingElementalVengeance())
        {
          if (RandHelper.GetRandomDouble() > 0.25)//TODO ability
          {
            lastingEffectsSet.EnsureEffect(pfi.GetEffectType(), inflicted, attacker, 3);
          }
        }
      }

      return HitResult.Hit;
    }

    protected virtual float GetDamageAddition(ProjectileFightItem pfi)
    {
      return 0;
    }

    public LastingEffect StartBleeding(float damageEachTurn, LivingEntity attacker, int turnLasting)
    {
      if (IsImmuned(EffectType.Bleeding))
        return null;
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


      var inflicated = amount;
      ReduceHealth(attacker, sound, damageDesc, srcName, ref inflicated, AttackKind.SpellElementalProjectile);

      LastingEffectsSet.TryAddLastingEffectOnHit(inflicated, attacker, spell);
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

    bool IsNegative(EffectType effect)
    {
      return effect == EffectType.Bleeding ||
             effect == EffectType.Poisoned ||
             effect == EffectType.Firing ||
             effect == EffectType.Frozen ||
             effect == EffectType.BushTrap ||
             effect == EffectType.TornApart ||
             effect == EffectType.Frighten ||
             effect == EffectType.Stunned ||
             effect == EffectType.Weaken ||
             effect == EffectType.Inaccuracy ||
             effect == EffectType.WebTrap;
    }

    public bool IsImmuned(EffectType effect)
    {
      if (ImmuneOnEffects && IsNegative(effect))
        return true;
      return  immunedEffects.Contains(effect) || chanceToExperienceEffect[effect] == 0;
    }

    public void RemoveImmunity(EffectType effect)
    {
      if (immunedEffects.Contains(effect))
        immunedEffects.Remove(effect);
    }

    public void AddImmunity(EffectType effect)
    {
      immunedEffects.Add(effect);
    }

    public virtual void ApplyLastingEffects()
    {
      lastingEffectsSet.ApplyLastingEffects();
    }

    protected bool DoConsume(EntityStatKind statFromConsumable, LastingEffectCalcInfo inc)
    {
      return this.Stats.ChangeStatDynamicValue(statFromConsumable, (float)inc.EffectiveFactor.Value);
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

    [JsonIgnore]//LastingEffectsSet is saved
    public List<LastingEffect> LastingEffects
    {
      get
      {
        return lastingEffectsSet.LastingEffects;
      }
    
    }

    public LastingEffectsSet LastingEffectsSet 
    { 
      get => lastingEffectsSet;
      set 
      {
        lastingEffectsSet = value;
        lastingEffectsSet.LivingEntity = this;
      }
    }

    //TODO ! use IsInProjectileReach
    public float MaxMagicAttackDistance { get; internal set; } = GenerationInfo.MaxMagicAttackDistance;

    public virtual void RemoveLastingEffect(LastingEffect le)
    {
      lastingEffectsSet.RemoveLastingEffect(le);
    }

    public bool IsBadlyWounded()
    {
      var he = GetCurrentValue(EntityStatKind.Health) / GetTotalValue(EntityStatKind.Health);
      return he <= .35f;
    }

    //public static float GetReducePercentage(float orgAmount, float discPerc)
    //{
    //  return orgAmount * discPerc / 100f;
    //}

    public override string ToString()
    {
      var str = " L:" + Level + " " + base.ToString();
      str += " "+ this.State + ", Alive:" + Alive + ", H:" + Stats.Health + ", Lvl:" + Level;
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
      if (Alive)
      {
        if (IsHealthGone())
        {
          if (Immortal)
          {
            return false;
          }
          Alive = false;
          DiedOfEffect = effect;
          //AppendDeadAction();
          return true;
        }
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
          //if (DiedOfEffect == EffectType.Bleeding && LastingEffectsSet.LastingEffects.Any())
          //{
          //  var trap = LastingEffectsSet.LastingEffects
          //    .Where(i => i.Source is ProjectileFightItem pfi && pfi.FightItemKind == FightItemKind.HunterTrap)
          //    .Select(i => i.Source)
          //    .Cast<ProjectileFightItem>()
          //    .SingleOrDefault();
          //  if (trap != null)
          //  {
          //    trap.SetState(FightItemState.Deactivated);
          //  }
          //}
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
      //Debug.WriteLine(Name+ " ReduceHealth: "+ amount);

      Stats.GetStat(EntityStatKind.Health).Subtract(amount);
      DieIfShould(EffectType.Unset);
    }

    private float GetDefense()
    {
      return GetCurrentValue(EntityStatKind.Defense);
    }

    public virtual bool HasEnoughMana(float manaCost)
    {
      return GetCurrentValue(EntityStatKind.Mana) >= manaCost;
    }

    public float GetCurrentValue(EntityStatKind kind)
    {
      var stat = Stats.GetStat(kind);
      var cv = stat.Value.CurrentValue;
      if (stat.Unit == EntityStatUnit.Percentage && cv > 100)
      {
        Logger.LogError(stat.Kind+ " stat.Unit == EntityStatUnit.Percentage && cv > 100! " + this);
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

    public virtual float GetEffectChance(EntityStatKind esk, ProjectileFightItem pfi)
    {

      var ch = GetCurrentValue(esk);
      if (esk == EntityStatKind.ChanceToPhysicalProjectileHit)
      {
        if (pfi != null && pfi.AbilityKind == AbilityKind.Cannon)
        {
          ch += GetActiveAbility(AbilityKind.Cannon).AuxStat.Factor;
        }
        else if (SelectedActiveAbility != null && SelectedActiveAbility.Kind == AbilityKind.PiercingArrow)
        {
          ch += GetActiveAbility(AbilityKind.PiercingArrow).PrimaryStat.Factor;
        }
      }
      return ch;
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
      var factor = statValue * nominalValuePercInc / 100f;
      return new EffectiveFactor(factor);
    }

    public void ApplyPassiveSpell(PassiveSpell spell)
    {
      var et = SpellConverter.EffectTypeFromSpellKind(spell.Kind);
      if (et != EffectType.Unset)
        AddLastingEffectFromSpell(et);
      else
        Assert(false);
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
    private FightItem selectedFightItem;
    public static Func<float, float> LevelStatIncreaseCalculated;
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
      SetLevel(level);
      InitStatsFromName();
      var hard = diff == Difficulty.Hard;
      float inc = 1;
      if (CanIncreaseStatsDueToDifficulty())
      {
        if (diff == Difficulty.Normal)
          inc = 1.1f;
        else if (diff == Difficulty.Hard)
          inc = 1.25f;
      }
      if (inc > 1)
        IncreaseStats(inc, IncreaseStatsKind.Difficulty);

      if (level > 1)
      {
        inc = GetIncrease(hard ? level + 1 : level);
        if(LevelStatIncreaseCalculated!=null)
          inc = LevelStatIncreaseCalculated(inc);
        IncreaseStats(inc, IncreaseStatsKind.Level);
      }
      InitResistance();
      InitActiveScroll();
      LevelSet = true;
      return true;
    }

    public void SetFakeLevel(int level)
    {
      SetLevel(level);
    }

    private void SetLevel(int level)
    {
      this.Level = level;
    }

    protected virtual bool CanIncreaseStatsDueToDifficulty()
    {
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
      return SelectedManaPoweredSpellSource;
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

    public virtual bool RemoveFightItem(FightItem fi)
    {
      return false;
    }

    public virtual bool HasFightItem(FightItemKind fik)
    {
      return false;
    }

    public virtual FightItem GetFightItem(FightItemKind kind)
    {
      return null;
    }


    public virtual bool IsInProjectileReach(Roguelike.Abstract.Projectiles.IProjectile fi, Point target)
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

    public bool IsStatRandomlyTrue(EntityStatKind kind, EffectType et = EffectType.Unset)
    {
      if (AlwaysCausesLastingEffect(et))
        return true;

      if (GenerationInfo.DebugInfo.HackedStats.Contains(kind))
      {
        if (RandHelper.GetRandomDouble() > .5f)
          return true;
      }
      if (kind == EntityStatKind.Unset)
        return false;
      var statValue = GetCurrentValue(kind);
      if (statValue <= 0)
        return false;
      var isTrue = statValue / 100f > RandHelper.GetRandomDouble();

      Logger.LogInfo(this.Name + " IsStatRandomlyTrue " + statValue + " " + kind + " " + isTrue);
      return isTrue;
    }

    public float CalcNextLevelExperience(float currentNextLevelExperience)
    {
      return Calculated.FactorCalculator.AddFactor((int)currentNextLevelExperience, 110);
    }

    public virtual bool CanUseAbility(AbilityKind kind, AbstractGameLevel node, out string reason, Dungeons.Tiles.Abstract.IHitable victim = null)
    {
      reason = "";
      bool activeUsed = false;
      var ab = GetActiveAbility(kind);
      if (ab != null)
        activeUsed = true;
      var esk = EntityStatKind.Unset;

      if (kind == AbilityKind.StrikeBack)
        esk = EntityStatKind.ChanceToStrikeBack;
      //else if (kind == AbilityKind.ThrowingTorch)
      //  esk = EntityStatKind.ThrowingTorchChanceToCauseFiring;
      else if (kind == AbilityKind.Stride || kind == AbilityKind.OpenWound)
      {
        if (!LastMeleeAttackWasOK)
          return false;
      }
      if (!activeUsed)
        return IsStatRandomlyTrue(esk, EffectType.Unset);

      if (kind == AbilityKind.Cannon)
      {
        if (victim == null)
          return false;
        if (victim.Position.DistanceFrom(this.point) <= 1.51)
        {
          reason = "victim is too close";
          return false;
        }
      }
      bool can = CanUseAbilityDueToItsState(ab, ref reason);
      return can;
    }

    protected bool CanUseAbilityDueToItsState(ActiveAbility ab, ref string reason)
    {
      var state = GetAbilityState(ab);
      var can = state == AbilityState.Activated;
      if (!can)
        reason = "!CanUseAbilityDueToItsState "+ state;
      return can;
    }

    public virtual Weapon GetActiveWeapon()
    {
      return null;
    }

    [JsonIgnore]
    public bool MoveDueToAbilityVictim { get; internal set; }

    public virtual bool Consume(IConsumable consumable)
    {
      if (consumable.StatKind == EntityStatKind.Unset)
      {
        var pot = consumable.Loot as Potion;
        Dungeons.DebugHelper.Assert(pot != null && pot.Kind == PotionKind.Antidote);
      }
      bool consumed = false;
      if (consumable is SpecialPotion)
      {
        var sp = consumable as SpecialPotion;
        this.Stats[sp.StatKind].Nominal += (float)consumable.StatKindEffective.Value;
        consumed = true;
      }
      else
      {
        if (consumable is Potion potion)
        {
          Dungeons.DebugHelper.Assert(consumable.ConsumptionSteps == 1);
          consumed = ConsumePotion(potion);
        }
        else
        {
          if (consumable is Hooch hooch)
          {
            LastingEffectsSet.AddPercentageLastingEffect(EffectType.Hooch, consumable, consumable.Loot);
            consumed = true;
          }
          else if (consumable is Food food && food.EffectType == EffectType.Poisoned)
          {
            var npd = CalculateNonPhysicalDamage(EntityStatKind.PoisonAttack, food.StatKindEffective.Value);
            int tours = food.Duration;
            LastingEffectsSet.EnsureEffect(EffectType.Poisoned, npd, null, tours);
            consumed = true;
          }
          else
          {
            EffectType et = EffectType.ConsumedRawFood;
            if (consumable.Roasted)
              et = EffectType.ConsumedRoastedFood;
            var le = LastingEffectsSet.AddPercentageLastingEffect(et, consumable, consumable.Loot);
            consumed = le !=null;
          }
        }
      }
      if (consumed)
      {
        var info = Name + " consumed " + (consumable as Dungeons.Tiles.Tile).Name + ", Health: " + this.GetCurrentValue(EntityStatKind.Health);
        AppendAction(new LootAction(consumable.Loot, this) { Kind = LootActionKind.Consumed, Info = info });
      }
      return consumed;
    }

    public bool ConsumePotion(Potion potion)
    {
      bool consumed;
      if (potion.Kind == PotionKind.Antidote)
      {
        var le = GetFirstLastingEffect(EffectType.Poisoned);
        if (le != null)
          RemoveLastingEffect(le);

        consumed = true;
      }
      else
      {
        var factor = LastingEffectsSet.CalcLastingEffectInfo(EffectType.Unset, potion);
        consumed = DoConsume(potion.StatKind, factor);
      }

      return consumed;
    }

    [JsonIgnore]
    public bool EverUsedFightItem { get; set; }

    [JsonIgnore]
    public bool LastAttackWasProjectile { get; set; }

    public virtual ActiveAbility GetActiveAbility(AbilityKind kind)
    {
      return null;
    }

    public virtual PassiveAbility GetPassiveAbility(AbilityKind kind)
    {
      return null;
    }

    public virtual AbilityKind SelectedActiveAbilityKind
    {
      get { return AbilityKind.Unset; }
    }

    [JsonIgnore]
    public int ChaseCounter { get; internal set; }
    public bool IsLooted { get; set; }

    [JsonIgnore]
    public bool LastMoveOnPathResult { get; internal set; }
    public int MovesCounter { get; internal set; }

    Dictionary<EntityCommandKind, CommandUseInfo> advEnemySkillUseCount = new Dictionary<EntityCommandKind, CommandUseInfo>();
    public int GetAdvEnemySkillUseCount(EntityCommandKind skill) 
    {
      EnsureCmd(skill);
      return advEnemySkillUseCount.ContainsKey(skill) ? advEnemySkillUseCount[skill].UseCounter : 0;
    }

    public int GetAdvEnemySkillCooldown(EntityCommandKind skill)
    {
      EnsureCmd(skill);
      return advEnemySkillUseCount.ContainsKey(skill) ? advEnemySkillUseCount[skill].Cooldown : 0;
    }

    CommandUseInfo EnsureCmd(EntityCommandKind skill)
    {
      if (!advEnemySkillUseCount.ContainsKey(skill))
      {
        advEnemySkillUseCount[skill] = new CommandUseInfo(skill);
        if (skill == EntityCommandKind.Resurrect)
        {
          advEnemySkillUseCount[skill].Sound = "raise_my_friends";
          advEnemySkillUseCount[skill].MessageAboveHead = "Raise my friends!";
        }
        else if (skill == EntityCommandKind.SenseVictimWeakResist)
        {
          advEnemySkillUseCount[skill].Sound = "FallenOneSense";
          advEnemySkillUseCount[skill].MessageAboveHead = "Let me sense your weaknesses...";
        }
        else if (skill == EntityCommandKind.MakeFakeClones)
        {
          advEnemySkillUseCount[skill].Sound = "FallenOneSurround";
          advEnemySkillUseCount[skill].MessageAboveHead = "Let me surround you...";
        }

        if (string.IsNullOrEmpty(advEnemySkillUseCount[skill].Info))
         advEnemySkillUseCount[skill].Info = Name + " used " + skill.ToDescription() + " skill";
      }
      return advEnemySkillUseCount[skill];
    }

    public int SetAdvEnemySkillCooldown(EntityCommandKind skill, int val)
    {
      EnsureCmd(skill);
      return advEnemySkillUseCount[skill].Cooldown = val;
    }

    public void IncreaseAdvEnemySkillUseCount(EntityCommandKind skill)
    {
      EnsureCmd(skill);
      GetCommand(skill).UseCounter = GetAdvEnemySkillUseCount(skill) + 1;
    }

    public CommandUseInfo GetCommand(EntityCommandKind skill)
    {
      EnsureCmd(skill);

      return advEnemySkillUseCount[skill];
    }

    internal void DescreseAdvEnemySkillUseCount(EntityCommandKind cmd)
    {
      EnsureCmd(cmd);
      int count = GetAdvEnemySkillUseCount(cmd);
      if (count > 0)
      {
        advEnemySkillUseCount[cmd].UseCounter--;
      }
    }
    public virtual bool HasSpecialSkill(EntityCommandKind skill)
    {
      var fo = tag1.StartsWith("fallen_one");
      if (skill == EntityCommandKind.Resurrect)
        return Name.Contains("Skeleton");
      return fo;
    }

    public bool HasSpecialSkillUnused(EntityCommandKind cmd)
    {
      return HasSpecialSkill(cmd) && GetAdvEnemySkillUseCount(cmd) == 0;
    }

    bool lastTimeWasLava = false;
    public void ReduceHealthDueToSurface(AbstractGameLevel CurrentNode)
    {
      var sks = CurrentNode.GetSurfaceKindsUnderLivingEntity(this);
      if (sks.Contains(Tiles.SurfaceKind.Lava) || sks.Contains(Tiles.SurfaceKind.Oil))
      {
        var surs = CurrentNode.GetSurfacesUnderPoint(point);
        if (surs.Any(i => i.IsBurning))
        {
          var isLava = surs.Any(i => i.Kind == SurfaceKind.Lava);
          if (isLava)
          {
            var reduce = lastTimeWasLava ? Stats.Health : Stats.Health / 2;
            if (reduce < 10)
              reduce = 10;
            ReduceHealth(reduce);
            LastingEffectsSet.EnsureEffect(Effects.EffectType.Firing, Stats.Health / 5);
            lastTimeWasLava = true;
          }
          else
            AddFiringFromOil();
        }
      }
      else
        lastTimeWasLava = false;
    }

    public void AddFiringFromOil()
    {
      var le = this;
      //var ci = le.LastingEffectsSet.CalcLastingEffDamage(EffectType.Firing, amount: 2);
      //le.LastingEffectsSet.AddLastingEffect(ci, EffectOrigin.External, hs);
      LastingEffectsSet.EnsureEffect(Effects.EffectType.Firing, Stats.Health / 5);
    }

    internal void Ressurect(float health)
    {
      Alive = true;
      var sv = new StatValue();
      sv.Nominal = health;
      this.LastingEffectsSet.LastingEffects.Clear();
      this.Stats.Stats[EntityStatKind.Health].Value = sv;
    }

    AttackPolicy nonMeleeAttackPolicy;
    List<Dungeons.Tiles.Abstract.IHitable> targets;
    /// <summary>
    /// 
    /// </summary>
    public bool AttackTargets(AttackPolicy attackPolicy)
    {
      if (targets == null)
      {
        Logger.LogError("AttackTargets targets == null");
        return false;
      }
      //var targetsToRemove = targets.ToList();
      foreach (var nextTarget in targets.ToArray())
      {
        attackPolicy.AttackNextTarget(this, nextTarget);
      }
      return true;
    }

    public bool AttackTargets()
    {
      return AttackTargets(nonMeleeAttackPolicy);
    }

    void Log(string log)
    {
      // Logger.LogInfo(log);
    }
    public bool ApplyProjectileCastPolicy
    (
      ProjectileCastPolicy projectileCastPolicy,
      List<Dungeons.Tiles.Abstract.IHitable> targets
    )
    {
      Log("le ApplyProjectileCastPolicy start");
      this.nonMeleeAttackPolicy = projectileCastPolicy;
      this.targets = targets;

      //this causes a bug that sometimes torch is thrown sometimes not, but always count is  reduced!
      //if (MakeReadyForProjectileAttack != null)
      //{
      //  if (projectileCastPolicy.Projectile is ProjectileFightItem pfi && pfi.FightItemKind == FightItemKind.ThrowingTorch)
      //  {
      //    MakeReadyForProjectileAttack(this, EventArgs.Empty);
      //    Logger.LogInfo("ApplyProjectileCastPolicy false!");
      //    return false;
      //  }
      //}

      return AttackTargets(nonMeleeAttackPolicy);

    }

    public bool ApplyStaticSpellCastPolicy(StaticSpellCastPolicy animatedStaticSpellCastPolicy, List<IHitable> targets)
    {
      Log("le ApplyStaticSpellCastPolicy start");
      this.nonMeleeAttackPolicy = animatedStaticSpellCastPolicy;
      this.targets = targets;
      return AttackTargets(nonMeleeAttackPolicy);
    }

    [JsonIgnore]
    public Func<FightItem, Vector2D> ProjectileFightItemStartPos;
    [JsonIgnore]
    public bool d_canMove = true;

    public Vector2D GetProjectileFightItemStartPos(FightItem fi)
    {
      if (ProjectileFightItemStartPos != null)
        return ProjectileFightItemStartPos(fi);
      return GetStdProjectileFightItemStartPos();
    }

    public Vector2D GetStdProjectileFightItemStartPos()
    {
      return new Vector2D() { X = point.X, Y = point.Y };
    }

    public virtual bool AlwaysCausesLastingEffect(EffectType type)
    {
      return false;
    }

    public virtual bool CalcShallMoveFaster(AbstractGameLevel node)
    {
      return false;
    }

    public virtual bool HandleOrder(BattleOrder order, Hero hero, Dungeons.TileContainers.DungeonNode node)
    {
      return false;
    }

    public void PlayHitSound(Dungeons.Tiles.Abstract.IProjectile proj)
    {
      PlaySound(proj.HitSound);
    }

    public void PlayHitSound(IDamagingSpell spell)
    {
      PlaySound(spell.HitSound);
    }

    public virtual bool CanHighlightAbility(AbilityKind kind)
    {
      return false;
    }

    public AbilityState GetAbilityState(Ability ab)
    {
      if (ab.CoolDownCounter > 0)
        return AbilityState.CoolingDown;
      if (WorkingAbilityKind == ab.Kind || LastingEffectsSet.HasEffect(ab.Kind))
        return AbilityState.Working;
      if (SelectedActiveAbilityKind == ab.Kind)
      {
        var canActivate = CanHighlightAbility(ab.Kind);
        var newState = canActivate ? AbilityState.Activated : AbilityState.Unusable;
        return newState;
      }
      return AbilityState.Unset;
      
    }

    public void AppendUsedAbilityAction(Abilities.AbilityKind abilityKind)
    {
      AppendAction(new AbilityStateChangedEvent()
      {
        Info = Name + " used ability " + abilityKind.ToDescription(),
        Level = ActionLevel.Important,
        AbilityUser = this,
        AbilityKind = abilityKind,
        AbilityState = AbilityState.Working
      }); ;

      if (abilityKind == AbilityKind.Smoke)
        PlaySound("cloth");
    }

    public void HandleActiveAbilityUsed(AbilityKind abilityKind)
    {
      var ab = GetActiveAbility(abilityKind);

      AppendUsedAbilityAction(abilityKind);

      if (ab.UsesCoolDownCounter)
      {
        //if (ab.TurnsIntoLastingEffect)
        //{
        //  if (!LastingEffectsSet.HasEffect(abilityKind) && abilityKind != AbilityKind.Smoke)
        //  {
        //    //WorkingAbilityKind = AbilityKind.Unset;
        //    HandleActiveAbilityEffectDone(abilityKind);
        //  }
        //}
        //else
        if (abilityKind != AbilityKind.Smoke)
        {
          var lastsManyTurns = LastingEffectsSet.HasEffect(abilityKind);

          if (!lastsManyTurns)
          {
            StartAbilityCooling(abilityKind, ab);
          }
        }
      }
    }

    public void StartAbilityCooling(AbilityKind abilityKind)
    {
      var ab = GetActiveAbility(abilityKind);
      StartAbilityCooling(abilityKind, ab);
    }

    public void StartAbilityCooling(AbilityKind abilityKind, ActiveAbility ab)
    {
      //start cool down
      ab.CoolDownCounter = ab.MaxCollDownCounter;
      WorkingAbilityKind = AbilityKind.Unset;
      AppendAction(new AbilityStateChangedEvent()
      { AbilityKind = abilityKind, AbilityState = AbilityState.CoolingDown, AbilityUser = this });
    }

    public float GetHealthRatio()
    {
      return GetCurrentValue(EntityStatKind.Health) / GetTotalValue(EntityStatKind.Health);
    }

    public float GetManaRatio()
    {
      return GetCurrentValue(EntityStatKind.Mana) / GetTotalValue(EntityStatKind.Mana);
    }

    public bool CanBeBlessed()
    {
      if (HasLastingEffect(EffectType.Poisoned))
        return true;
      if (HasLastingEffect(EffectType.Frozen))
        return true;
      if (HasLastingEffect(EffectType.Bleeding))
        return true;

      if (GetHealthRatio() < 1)
        return true;

      if (GetManaRatio() < 0.5f)
        return true;

      return false;
    }

    public virtual bool CanUseEquipment(IEquipment eq, bool autoPutoOn)
    {
      return false;

    }

    public bool CanUseEquipment(IEquipment eq, EntityStat eqStat)
    {
      var ceiled = EntityStat.GetRoundedStat(Stats.GetNominal(eqStat.Kind));//skeleton had dex 24.9 on level 6! , was hown as 25 , but could not use eq
      return ceiled >= eq.GetReqStatValue(eqStat);
    }

    internal bool CanUseCommand(EntityCommandKind cmd)
    {
      if (!HasSpecialSkill(cmd))
        return false;

      var skill = GetCommand(cmd);

      if (cmd == EntityCommandKind.MakeFakeClones)
      {
        return skill.UseCounter == 0 && GetHealthRatio() < 0.5;
      }
      if (cmd == EntityCommandKind.SenseVictimWeakResist)
        return skill.UseCounter == 0 && GetHealthRatio() < 0.75;

      return true;
    }
  }
}

