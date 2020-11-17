using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike
{
  public enum GameContextSwitchKind { DungeonSwitched, NewGame, GameLoaded}
  public enum TurnOwner { Unset, Hero, Allies, Enemies }

  public class HeroPlacementResult
  {
    public AbstractGameLevel Node { get; set; }
    public Tile Tile { get; set; }
  }

  public class ContextSwitch
  {
    public GameContextSwitchKind Kind { get; set; }
    public AbstractGameLevel CurrentNode { get;  set; }
    public Hero Hero { get; set; }
  }

  public class GameContext
  {
    Hero hero;

    public virtual AbstractGameLevel CurrentNode { get; set; }
    public Hero Hero { get => hero; set => hero = value; }
    public event EventHandler<TurnOwner> TurnOwnerChanged;
    public event EventHandler<ContextSwitch> ContextSwitched;
    [JsonIgnore]
    public EventsManager EventsManager { get; set; }
    ILogger logger;

    [JsonIgnore]
    public Container Container { get; set; }
    TurnOwner turnOwner = TurnOwner.Hero;

    //total turn count in whole game
    Dictionary<TurnOwner, int> turnCounts = new Dictionary<TurnOwner, int>();

    //actions (move, attack) count in turn - typically 1
    Dictionary<TurnOwner, int> turnActionsCount = new Dictionary<TurnOwner, int>();
    public Action<Policy, LivingEntity , Tile > AttackPolicyInitializer;

    public GameContext(Container container)
    {
      this.logger = container.GetInstance<ILogger>();
      this.Container = container;

      turnActionsCount[TurnOwner.Hero] = 0;
      turnActionsCount[TurnOwner.Allies] = 0;
      turnActionsCount[TurnOwner.Enemies] = 0;

      turnCounts[TurnOwner.Hero] = 0;
      turnCounts[TurnOwner.Allies] = 0;
      turnCounts[TurnOwner.Enemies] = 0;
    }

    public void ApplyPhysicalAttackPolicy(LivingEntity attacker, Tile target, Action<Policy> AfterApply)
    {
      var attackPolicy = Container.GetInstance<AttackPolicy>();

      if (AttackPolicyInitializer != null)
        AttackPolicyInitializer(attackPolicy, attacker, target);
      attackPolicy.OnApplied += (s, e) => AfterApply(e);
      attackPolicy.Apply(attacker, target);
    }

    bool PrepareScroll(LivingEntity caster, Scroll scroll)
    {
      if (scroll.Count <= 0)
      {
        logger.LogError("scroll.Count <= 0");
        return false;
      }

      //scroll.Count--;
      if (caster is AdvancedLivingEntity advEnt)
        return advEnt.Inventory.Remove(scroll);

      return true;
    }

    public PassiveSpell ApplyPassiveSpell(LivingEntity caster, Scroll scroll)
    {
      if (!PrepareScroll(caster, scroll))
        return null;

      if (scroll.CreateSpell(caster) is PassiveSpell ps)
      {
        caster.ApplyPassiveSpell(ps);
        if(caster is Hero)
          MoveToNextTurnOwner();

        return ps;
      }
      else
        logger.LogError("!PassiveSpell " + scroll);

      return null;
    }

    public void ApplySpellAttackPolicy(LivingEntity caster, LivingEntity target, Scroll scroll, 
      Action<Policy> BeforeApply, 
      Action<Policy> AfterApply)
    {
      if (!PrepareScroll(caster, scroll))
        return;

      var policy = Container.GetInstance<SpellCastPolicy>();
      policy.Target = target;
      policy.ProjectilesFactory = Container.GetInstance<IProjectilesFactory>();
      policy.Scroll = scroll;
      if (BeforeApply!=null)
        BeforeApply(policy);

      policy.OnApplied += (s, e) =>
      {
        AfterApply(policy);
      };

      policy.Apply(caster);
    }

    public void ApplyMovePolicy(LivingEntity entity, Point newPos, Action<Policy> OnApplied)
    {
      var movePolicy = Container.GetInstance<MovePolicy>();
      //Logger.LogInfo("moving " + entity + " to " + newPos + " mp = " + movePolicy);

      movePolicy.OnApplied += (s, e) =>
      {
        if (OnApplied != null)
        {
          OnApplied(e);
        }
      };

      if (movePolicy.Apply(CurrentNode, entity, newPos))
      {
        EventsManager.AppendAction(new LivingEntityAction(kind: LivingEntityActionKind.Moved)
        {
          Info = entity.Name + " moved",
          InvolvedEntity = entity,
          MovePolicy = movePolicy
        });
      }
    }

    public virtual void SwitchTo(AbstractGameLevel node, Hero hero, GameState gs, GameContextSwitchKind context, Stairs stairs = null)
    {
      if (node == CurrentNode)
      {
        Debug.Assert(false);
        return;
      }
      if (CurrentNode != null)
        CurrentNode.ClearOldHeroPosition(context);

      Hero = hero;
      hero.OnContextSwitched(Container);
      var merchs = node.GetTiles<Merchant>();
      foreach(var merch in merchs)
        merch.OnContextSwitched(Container);
        
      var heroStartTile = PlaceHeroAtDungeon(node, gs, context, stairs);

      CurrentNode = node;

      CurrentNode.OnHeroPlaced(Hero);

      EmitContextSwitched(context);
    }

    public virtual HeroPlacementResult PlaceHeroAtDungeon(AbstractGameLevel node, GameState gs, GameContextSwitchKind context, Stairs stairs)
    {
      var res = new HeroPlacementResult();
      res.Node = node;

      Tile baseTile = null;
      if (context == GameContextSwitchKind.GameLoaded)
      {
        return PlaceLoadedHero(node, gs);
      }
      else if (stairs != null && context == GameContextSwitchKind.DungeonSwitched)
      {
        if (stairs.StairsKind == StairsKind.LevelUp)
        {
          var stairsDown = node.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelDown).FirstOrDefault();
          if (stairsDown != null)
            baseTile = stairsDown;
        }
        else if (stairs.StairsKind == StairsKind.LevelDown)
        {
          var stairsUp = node.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelUp).FirstOrDefault();
          if (stairsUp != null)
            baseTile = stairsUp;
        }
      }

      node.PlaceHeroNextToTile(context, Hero, res, baseTile);
      return res;
    }
        
    public virtual HeroPlacementResult PlaceLoadedHero(AbstractGameLevel node, GameState gs)
    {
      HeroPlacementResult res = null;
      var stairsUp = node.GetStairs(StairsKind.LevelUp);
      if (stairsUp != null)
        res = PlaceHeroAtDungeon(node, gs, GameContextSwitchKind.DungeonSwitched, stairsUp);
      else
      {
        var heroStartTile = node.GetHeroStartTile();
        node.PlaceHeroAtTile(GameContextSwitchKind.GameLoaded, Hero, heroStartTile);
      }

      return res;
    }
        
    public void EmitContextSwitched(GameContextSwitchKind context)
    {
      if (ContextSwitched != null)
        ContextSwitched(this, new ContextSwitch() { Kind = context, CurrentNode = this.CurrentNode, Hero = this.hero });
    }

    private bool pendingTurnOwnerApply;
    private bool autoTurnManagement = true;

    public void IncreaseActions(TurnOwner caller)
    {
      if (turnOwner == caller)
        TurnActionsCount[turnOwner]++;
      else
        Logger.LogError("TurnOwner mismatch! ", true);
    }

    public int GetActionsCount()
    {
      return TurnActionsCount[turnOwner];
    }

    public void MoveToNextTurnOwner()
    {
      if (!AutoTurnManagement)
        return;
      DoMoveToNextTurnOwner();
    }

    //TODO make priv, call in UT by refl.
    public void DoMoveToNextTurnOwner()
    {
      if (turnOwner == TurnOwner.Hero)
      {
        TurnActionsCount[TurnOwner.Hero] = 0;
        turnCounts[TurnOwner.Hero]++;
        TurnOwner = TurnOwner.Allies;

      }
      else if (turnOwner == TurnOwner.Allies)
      {
        TurnActionsCount[TurnOwner.Allies] = 0;
        turnCounts[TurnOwner.Allies]++;
        TurnOwner = TurnOwner.Enemies;
      }
      else
      {
        Debug.Assert(turnOwner == TurnOwner.Enemies);
        TurnActionsCount[TurnOwner.Enemies] = 0;
        turnCounts[TurnOwner.Enemies]++;
        TurnOwner = TurnOwner.Hero;
      }

      PendingTurnOwnerApply = true;

      if (TurnOwnerChanged != null)
        TurnOwnerChanged(this, turnOwner);

      
    }

    public bool HeroTurn
    {
      get { return turnOwner == TurnOwner.Hero; }
    }

    public void ReportHeroDeath()
    {
      EventsManager.AppendAction(Hero.GetDeadAction());
      HeroDeadReported = true;
    }

    public ILogger Logger { get => logger; set => logger = value; }
    public TurnOwner TurnOwner
    {
      get => turnOwner;
      set  
      {
        if (turnOwner != value)
        {
          turnOwner = value;
          if (!Hero.Alive)
          {
            ReportHeroDeath();
            return;
          }
          if (turnOwner == TurnOwner.Hero)
            Hero.ApplyLastingEffects();
        }
        //logger.LogInfo("to =>" + turnOwner);
      }
    }
    public bool PendingTurnOwnerApply { get => pendingTurnOwnerApply; set => pendingTurnOwnerApply = value; }
    public bool AutoTurnManagement { get => autoTurnManagement; set => autoTurnManagement = value; }
    public Dictionary<TurnOwner, int> TurnActionsCount { get => turnActionsCount; set => turnActionsCount = value; }
    public Dictionary<TurnOwner, int> TurnCounts { get => turnCounts; set => turnCounts = value; }
    public bool HeroDeadReported { get; internal set; }
    public bool HeroPlacedAfterLoad { get; set; }
    public AbstractGameLevel NodeHeroPlacedAfterLoad { get; set; }
  }
}
