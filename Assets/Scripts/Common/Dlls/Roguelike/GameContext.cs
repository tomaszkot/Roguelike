using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike
{
  public enum GameContextSwitchKind { DungeonSwitched, NewGame, GameLoaded, Teleported }
  public enum TurnOwner { Unset, Hero, Allies, Enemies, Animals}

  public class HeroPlacementResult
  {
    public AbstractGameLevel Node { get; set; }
    public Point Point { get; set; }
  }

  public class ContextSwitch
  {
    public GameContextSwitchKind Kind { get; set; }
    public AbstractGameLevel CurrentNode { get; set; }
    public Hero Hero { get; set; }

    public bool Result { get; set; }

    public Func<bool> Loader { get; set; }
  }

  public class GameContext
  {
    Hero hero;

    public virtual AbstractGameLevel CurrentNode { get; set; }
    public Hero Hero { get => hero; set => hero = value; }
    public event EventHandler<TurnOwner> TurnOwnerChanged;
    public event EventHandler<ContextSwitch> ContextSwitched;
    public event EventHandler<ContextSwitch> ContextSwitching;

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
      turnCounts[TurnOwner.Animals] = 0;
    }
        
    public bool CanUseScroll(LivingEntity caster, SpellSource spellSource, ISpell spell, ref string preventReason)
    {
      if (caster == null || (spellSource == null && spell == null))
      {
        logger.LogError("CanUseScroll null "+ caster + " " + spellSource + " " + spell);
        return false;
      }

      if (spellSource != null && spellSource.Count <= 0)
      {
        logger.LogError("SpellSource.Count <= 0");
        return false;
      }

      if (spell.ManaCost > caster.GetCurrentValue(EntityStatKind.Mana))
      {
        preventReason = "Not enough mana";
        return false;
      }

      return true;
    }

    bool UseAsyncContextSwitching = true;
    ////////////////////////////////////////////////////////////////////////////////////////////////
    public virtual bool SwitchTo
    (
      AbstractGameLevel node,
      Hero hero,
      GameState gs,
      GameContextSwitchKind context,
      AlliesManager am,
      Action after,
      Stairs stairsUsedByHero = null,
      Portal portal = null
    )
    {
      var tr = new TimeTracker();
      Logger.LogInfo("--SwitchTo starts: " + node);

      Func<bool> DoSwitch = () => {
        if (node == CurrentNode)
        {
          DebugHelper.Assert(false);
          return false;
        }
        if (CurrentNode != null)
          CurrentNode.ClearOldHeroPosition(context);

        Hero = hero;
        hero.OnContextSwitched(Container);
        var merchs = node.GetTiles<Merchant>();
        foreach (var merch in merchs)
          merch.OnContextSwitched(Container);

        //if (context != GameContextSwitchKind.Teleported)
        {
          PlaceLe(Hero, CurrentNode, node, hero, context, gs, stairsUsedByHero, null, portal);
          PlaceAllies(am, CurrentNode, node, context, gs, stairsUsedByHero, null, portal);
        }

        //swap active node
        CurrentNode = node;

        CurrentNode.OnHeroPlaced(Hero);
        
        EmitContextSwitched(context);
        Logger.LogInfo("--SwitchTo ends: " + node + " " + tr.TotalSeconds);
        if(after!=null)
          after();
        return true;
      };

      if (UseAsyncContextSwitching)
      {
        if (ContextSwitching != null && context != GameContextSwitchKind.NewGame && context != GameContextSwitchKind.GameLoaded)
        {
          var cs = new ContextSwitch() { Kind = context, CurrentNode = this.CurrentNode, Hero = this.hero, Loader = DoSwitch };
          ContextSwitching(this, cs);
          return cs.Result;
        }
      }
      
      return DoSwitch();
    }

    public void PlaceAllies(AlliesManager am, AbstractGameLevel srcLevel, AbstractGameLevel destLevel,
      Roguelike.GameContextSwitchKind gameContext,
      GameState gs,
      Stairs stairsUsedByHero = null,
      Action<LivingEntity> placed = null,
      Portal portal = null
      )
    {
      //var alliesByLevel = CurrentNode.GetActiveAllies();
      var allies = am.AllEntities;
      foreach (var ally in allies)
      {
        PlaceLe(ally, srcLevel, destLevel, Hero, gameContext, gs, stairsUsedByHero, placed);
      }
    }

    public void PlaceLe(LivingEntity ally, AbstractGameLevel srcLevel, 
      AbstractGameLevel destLevel, 
      Hero hero, 
      Roguelike.GameContextSwitchKind gameContext,
      GameState gs,
      Stairs stairsUsedByHero = null,
      Action<LivingEntity> placed = null,
      Portal portal = null
      )
    {
      if (srcLevel != destLevel && srcLevel!=null)
      {
        srcLevel.SetEmptyTile(ally.point);
      }
      if (ally is Hero)
      {
        if (portal != null)
        {
          PlaceHeroByPortal(destLevel, gameContext, portal);
          return;
        }
        PlaceHeroAtDungeon(destLevel, gs, gameContext, stairsUsedByHero);
      }
      else
      {
        //ally
        var pt = destLevel.GetClosestEmpty(hero).point;
        var empties = destLevel.GetClosestEmpties(pt);
        var notFar = empties.Where(i => i.DistanceFrom(hero) > 1).ToList();
        if (notFar.Any())
          pt = notFar.First().point;
        var set = destLevel.SetTile(ally, pt);
        if (!set)
          destLevel.Logger.LogError("!failed to set " + ally);
        else if(placed!=null)
          placed(ally);
        var dist = ally.DistanceFrom(hero);
        var info = "dist=" + dist + " ally: " + ally + " hero: " + hero.point + " set : " + set + " node: " + destLevel;
        //Debug.WriteLine(info);
        destLevel.Logger.LogInfo(info);
      }
    }

    protected virtual void PlaceHeroByPortal(AbstractGameLevel destLevel, GameContextSwitchKind gameContext, Portal portal)
    {
    }

    public virtual HeroPlacementResult PlaceHeroAtDungeon(AbstractGameLevel node, GameState gs, GameContextSwitchKind context, Stairs stairsUsedByHero)
    {
      Tile baseTile = null;
      if (context == GameContextSwitchKind.NewGame)
      {
        baseTile = node.GetClosestEmpty(node.Tiles[0, 0]);
      }
      else if (context == GameContextSwitchKind.GameLoaded)
      {
        return PlaceLoadedHero(node, gs);
      }
      else if (stairsUsedByHero != null && context == GameContextSwitchKind.DungeonSwitched)
      {
        var sk = StairsKind.Unset;
        if (stairsUsedByHero.StairsKind == StairsKind.LevelUp)
          sk = StairsKind.LevelDown;

        else if (stairsUsedByHero.StairsKind == StairsKind.LevelDown)
          sk = StairsKind.LevelUp;

        var stairsFound = node.GetTiles<Stairs>().Where(i => i.StairsKind == sk).FirstOrDefault();
        if (stairsFound != null)
        {
          baseTile = stairsFound;
          Logger.LogInfo("stairsFound: "+ stairsFound);
        }
        else
        {
          //maybe pit down ?
          //DebugHelper.Assert(false);
        }

      }

      return node.PlaceHeroNextToTile(context, Hero, baseTile);
    }

    public virtual HeroPlacementResult PlaceLoadedHero(AbstractGameLevel node, GameState gs)
    {
      HeroPlacementResult res = null;
      var stairsUsedToSwitch = node.GetStairs(StairsKind.LevelDown);
      if (stairsUsedToSwitch != null)
        res = PlaceHeroAtDungeon(node, gs, GameContextSwitchKind.DungeonSwitched, stairsUsedToSwitch);
      else
      {
        var heroStartTile = node.GetHeroStartTile();
        if (heroStartTile.point.X != 1 || heroStartTile.point.Y != 1)
        {
          //int k = 0;
          //k++;
        }
        if (node.PlaceHeroAtTile(GameContextSwitchKind.GameLoaded, Hero, heroStartTile))
          res = new HeroPlacementResult() { Node = node, Point = Hero.point };
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

    public static bool ShalLReportTurnOwner(Policy policy = null)
    {
      if (policy != null && policy is ProjectileCastPolicy pcp)
      {
        if (pcp.Projectile != null && pcp.Projectile.ActiveAbilitySrc != Abilities.AbilityKind.Unset)
          return false;//TODO
      }
      return true;
    }

    public void IncreaseActions(TurnOwner caller, Policy policy = null)
    {
      if (turnOwner != caller)
      {
        if (!ShalLReportTurnOwner(policy))
          return;//TODO
        
        Logger.LogError("TurnOwner mismatch! ", true);
        return;
      }
      
      TurnActionsCount[turnOwner]++;
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
        
    void DoMoveToNextTurnOwner()
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
      else if (turnOwner == TurnOwner.Enemies)
      {
        TurnActionsCount[TurnOwner.Enemies] = 0;
        turnCounts[TurnOwner.Enemies]++;
        TurnOwner = TurnOwner.Animals;
      }
      //else if (turnOwner == TurnOwner.Animals)
      //{
      //  TurnActionsCount[TurnOwner.Enemies] = 0;
      //  turnCounts[TurnOwner.Enemies]++;
      //  TurnOwner = TurnOwner.Animals;
      //}
      else
      {
        DebugHelper.Assert(turnOwner == TurnOwner.Animals);
        TurnActionsCount[TurnOwner.Animals] = 0;
        turnCounts[TurnOwner.Animals]++;
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
          {
            Hero.ApplyAbilities();
            Hero.ApplyLastingEffects();
            Hero.ReduceHealthDueToSurface(CurrentNode);

          }
          if (!Hero.Alive)
          {
            ReportHeroDeath();
            return;
          }
          
        }
        //logger.LogInfo("TurnOwner to =>" + turnOwner);
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
