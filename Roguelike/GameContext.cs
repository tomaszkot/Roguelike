using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Policies;
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

    public void ApplySpellPolicy()
    {
    }

    public void ApplyPhysicalAttackPolicy(LivingEntity attacker, Tile target, Action<Policy> AfterApply)
    {
      var attackPolicy = Container.GetInstance<AttackPolicy>();

      if (AttackPolicyInitializer != null)
        AttackPolicyInitializer(attackPolicy, attacker, target);
      attackPolicy.OnApplied += (s, e) => AfterApply(e);
      attackPolicy.Apply(attacker, target);
    }

    public void ApplySpellAttackPolicy(LivingEntity caster, LivingEntity target, Scroll scroll, 
      Action<Policy> BeforeApply, 
      Action<Policy> AfterApply)
    {
      if (scroll.Count <= 0)
      {
        logger.LogError("scroll.Count <= 0");
        return;
      }

      scroll.Count--;

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
      {
        var heros = CurrentNode.GetTiles<Hero>();
        var heroOnPrev = heros.SingleOrDefault();
        var set = CurrentNode.SetEmptyTile(heroOnPrev.Point);
        if (!set)
          Logger.LogError("failed to reset hero pos!");
      }


      this.Hero = hero;
      hero.OnContextSwitched(Container);
      var merchs = node.GetTiles<Merchant>();
      foreach(var merch in merchs)
        merch.OnContextSwitched(Container);

      if (!Hero.Point.IsValid() || context == GameContextSwitchKind.DungeonSwitched)
      {
        if (context == GameContextSwitchKind.DungeonSwitched)
        {
          ClearOldHeroPosition(CurrentNode, context);
        }
        var heroStartTile = PlaceHeroAtDungeon(node, context, stairs);
        PlaceHeroAtTile(node, Hero, heroStartTile);
      }
      else
      {
        AbstractGameLevel destNode = null;
        if (context == GameContextSwitchKind.GameLoaded)
        {
          destNode = PlaceLoadedHero(node, gs, context, stairs);
        }

        if (destNode == null && !node.SetTile(Hero, Hero.Point))
        {
          logger.LogError("!node.SetTile " + Hero);
        }
      }

      CurrentNode = node;

      if(Hero.DungeonNodeIndex < CurrentNode.Nodes.Count)
        CurrentNode.Nodes[Hero.DungeonNodeIndex].Reveal(true);
      //EventsManager.AppendAction(new GameStateAction() { InvolvedNode = node, Type = GameStateAction.ActionType.ContextSwitched });
      EmitContextSwitched(context);
    }

    public virtual AbstractGameLevel PlaceLoadedHero(AbstractGameLevel node, GameState gs, GameContextSwitchKind context, Stairs stairs)
    {
      AbstractGameLevel level = null;
      //if (gameState.Settings.CoreInfo.RestoreHeroToSafePointAfterLoad)
      {
        Tile heroStartTile = null;
        if (node.Index > 0)
        {
          var stairsUp = node.GetStairs(StairsKind.LevelUp);
          if (stairsUp != null)
            heroStartTile = PlaceHeroAtDungeon(node, context, stairs);
        }
        else
          heroStartTile = GetHeroStartTile(node);

        if (heroStartTile != null)
        {
          ClearOldHeroPosition(node, context);
          PlaceHeroAtTile(node, Hero, heroStartTile);
          level = node;
        }
      }

      return level;
    }

    protected void PlaceHeroAtTile(AbstractGameLevel node, Hero hero, Tile tile)
    {
      if (node.SetTile(hero, tile.Point, false))
        hero.DungeonNodeIndex = tile.DungeonNodeIndex;
    }

    private void ClearOldHeroPosition(AbstractGameLevel node, GameContextSwitchKind context)
    {
      var heros = node.GetTiles<Hero>();
      var heroInNode = heros.SingleOrDefault();
      //Debug.Assert(heroInNode != null);
      if (heroInNode == null && context == GameContextSwitchKind.DungeonSwitched)
        logger.LogError("SwitchTo heros.Count = " + heros.Count);

      if (heroInNode != null)
        node.SetEmptyTile(heroInNode.Point);//Hero is going to be placed in the node, remove it from the old one (CurrentNode)
    }

    protected virtual Tile PlaceHeroAtDungeon(AbstractGameLevel node, GameContextSwitchKind context, Stairs stairs)
    {
      Tile heroStartTile = null;
      Tile baseTile = null;
      if (stairs == null && node.Index > 0 && context == GameContextSwitchKind.GameLoaded)
      {
        baseTile = node.GetStairs(StairsKind.LevelUp);
      }
      else if (stairs != null && stairs.StairsKind == StairsKind.LevelUp && context == GameContextSwitchKind.DungeonSwitched)
      {
        var stairsDown = node.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelDown).FirstOrDefault();
        if (stairsDown != null)
          baseTile = stairsDown;
      }
      if (baseTile != null)
      {
        heroStartTile = node.GetNeighborTiles<Tile>(baseTile).FirstOrDefault();
        heroStartTile.DungeonNodeIndex = baseTile.DungeonNodeIndex;
      }

      if (heroStartTile == null)
      {
        heroStartTile = GetHeroStartTile(node);
      }

      return heroStartTile;
    }

    private Tile GetHeroStartTile(AbstractGameLevel node)
    {
      Tile heroStartTile;
      var emp = node.GetEmptyTiles(levelIndexMustMatch: false)//merged level migth have index  999 and none tile has such
               .FirstOrDefault();
      if (emp == null)
      {
        Logger.LogError("GetHeroStartTile failed!");
      }
      heroStartTile = emp;
      return heroStartTile;
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

    public ILogger Logger { get => logger; set => logger = value; }
    public TurnOwner TurnOwner
    {
      get => turnOwner;
      set  
      {
        if (turnOwner != value)
        {
          turnOwner = value;
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
