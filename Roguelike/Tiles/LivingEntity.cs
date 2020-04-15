using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Spells;
using Roguelike.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public enum EntityState { Idle, Moving, Attacking }
  public enum EffectType
  {
    None, Bleeding, Poisoned, Frozen, Firing, Transform, TornApart, Frighten, Stunned,
    ManaShield, BushTrap, Rage, Weaken, IronSkin, ResistAll, Inaccuracy, Hooch
  }
    
  public interface ILastingEffectOwner
  {
    void OnEffectFinished(EffectType type);
    void OnEffectStarted(EffectType type);
  }

  public class LivingEntity : Tile
  {
    static Dictionary<EntityStatKind, EntityStatKind> statsHitIncrease = new Dictionary<EntityStatKind, EntityStatKind> {
                { EntityStatKind.LifeStealing, EntityStatKind.Health },
                { EntityStatKind.ManaStealing, EntityStatKind.Mana }
    };

    public static Point DefaultInitialPoint = new Point(0, 0);
    //public event EventHandler<GenericEventArgs<LivingEntity>> Died;
    public Point PrevPoint;
    public Point InitialPoint = DefaultInitialPoint;
    EntityStats stats = new EntityStats();
    public EntityState State { get; set; }
    List<Algorithms.PathFinderNode> pathToTarget;
    List<LastingEffect> lastingEffects = new List<LastingEffect>();
    protected List<EffectType> immunedEffects = new List<EffectType>();

    [JsonIgnore]
    public List<LivingEntity> EverHitBy { get; set; } = new List<LivingEntity>();

    bool alive = true;
    //[JsonIgnoreAttribute]
    public EntityStats Stats { get => stats; set => stats = value; }

    public LivingEntity():this(new Point(-1, -1), '\0')
    {
    }

    public LivingEntity(Point point, char symbol) : base(point, symbol)
    {
      //this.EventsManager = eventsManager;
    }

    public void ReduceMana(float amount)
    {
      Stats.Stats[EntityStatKind.Mana].Subtract(amount);
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
        //gm.Assert(false, "Stats.Defence == 0");
        AppendAction(new GameStateAction() {Type = GameStateAction.ActionType.Assert, Info = "Stats.Defence == 0!!!" });
        return 0;
      }
      var inflicted = attacker.GetCurrentValue(EntityStatKind.Attack) / defense;
      ReduceHealth(inflicted);
      var ga = new LivingEntityAction(LivingEntityActionKind.GainedDamage) { InvolvedValue = inflicted, InvolvedEntity = this };
      var desc = "received damage: " + inflicted.Formatted();
      ga.Info = Name.ToString() + " " + desc;
#if UNITY_EDITOR
      ga.Info += "UE , Health = " + Stats.Health.Formatted();
#endif
      AppendAction(ga);
      if(!this.EverHitBy.Contains(attacker))
        this.EverHitBy.Add(attacker);
      //if (this is Enemy || this is Hero)// || this is CrackedStone)
      //{
      //  PlayPunchSound();
      //}
      DieIfShould();
      return inflicted;
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
      if (spell != null)
      {
        if (spell.Kind == SpellKind.StonedBall)
          amount /= Stats.Defence;
        else
        {
          var magicAttackDamageReductionPerc = Stats.GetCurrentValue(EntityStatKind.MagicAttackDamageReduction);
          amount -= GetReducePercentage(amount, magicAttackDamageReductionPerc);
        }
      }
      ReduceHealth(amount);
      var ga = new LivingEntityAction(LivingEntityActionKind.GainedDamage) { InvolvedValue = amount, InvolvedEntity = this };
      var desc = damageDesc ?? "received damage: " + amount.Formatted();
      ga.Info = Name.ToString() + " " + desc;
#if UNITY_EDITOR
      ga.Info += "UE , Health = " + Stats.Health.Formatted();
#endif
      AppendAction(ga);

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

      //TODO
      //var effectInfo = CalcLastingEffDamage(amount, attacker, spell, fightItem);
      //if (effectInfo.Type != EffectType.None && !IsImmuned(effectInfo.Type))
      //{
      //  var rand = CommonRandHelper.Random.NextDouble();
      //  var chance = GetChanceToExperienceEffect(effectInfo.Type);
      //  if (spell != null)
      //  {
      //    if (spell.SendByGod && spell.Kind != SpellKind.LightingBall)
      //      chance *= 2;

      //  }
      //  if (fightItem != null)
      //    chance += fightItem.GetFactor(false);
      //  if (rand * 100 <= chance)
      //  {
      //    this.AddLastingEffect(effectInfo.Type, effectInfo.Turns, effectInfo.Damage);
      //    //AppendEffectAction(effectInfo.Type, true); duplicated message
      //  }
      //}
      //var died = DieIfShould(effectInfo.Type);
      
      var attackedInst = attacker ?? spell.Caller;
      if (attackedInst != null)
      {
        if (!EverHitBy.Contains(attackedInst))
          EverHitBy.Add(attackedInst);
      }
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
        //TODO
        //HandleSpecialFightStat(et, false);
        //if (et == EffectType.Hooch)
        //{
        //  ApplyHoochEffects(false);
        //  lastingEffHooch = null;
        //}

        //if (livEnt == this && LastingEffectDone != null)
        //  LastingEffectDone(this, new GenericEventArgs<LastingEffect>(le));
      }
    }

    public static float GetReducePercentage(float orgAmount, float discPerc)
    {
      return orgAmount * discPerc / 100f;
    }

    public override string ToString()
    {
      var str = base.ToString();
      str += " "+this.State;
      return str;
    }

    protected void AppendAction(GameAction ac)
    {
      if(EventsManager != null)
        EventsManager.AppendAction(ac);
    }

    protected void Assert(bool check, string desc)
    {
      if (EventsManager != null)
        EventsManager.Assert(check, desc);
    }

    private bool DieIfShould()
    {
      if (Alive && HealthZero())
      {
        Alive = false;
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
      Stats.Stats[EntityStatKind.Health].Subtract(amount);
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
      var stat = Stats.Stats[kind];
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

    public bool OnHitBy(IMovingDamager md)
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
  }
}
