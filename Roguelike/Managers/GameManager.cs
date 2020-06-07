﻿using Dungeons.Core;
using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System.Drawing;
using Roguelike.Generators;
using System.Diagnostics;
using Roguelike.Serialization;
using SimpleInjector;
using Roguelike.Events;
using Newtonsoft.Json;
using Roguelike.Tiles.Interactive;
using Roguelike.Policies;
using Dungeons;
using Dungeons.TileContainers;

namespace Roguelike.Managers
{
  public enum InteractionResult { None, Handled, ContextSwitched, Attacked, Blocked};
  public struct MoveResult
  {
    public bool Possible { get; set; }
    public Point Point { get; set; }

    public MoveResult(bool Possible, Point Point)
    {
      this.Possible = Possible;
      this.Point = Point;
    }
  }

  public class GameManager
  {
    protected GameContext context;
    LootGenerator lootGenerator;
    EventsManager eventsManager;
    EnemiesManager enemiesManager;
    AlliesManager alliesManager;

    private IPersister persister;
    private ILogger logger;

    public EnemiesManager EnemiesManager { get => enemiesManager; set => enemiesManager = value; }
    public Hero Hero { get => Context.Hero; }
    protected GameState gameState = new GameState();
    public bool HeroTurn { get => Context.HeroTurn; }

    //public EventsManager ActionsManager { get => EventsManager; set => EventsManager = value; }
    [JsonIgnore]
    public EventsManager EventsManager { get => eventsManager; set => eventsManager = value; }
    public GameContext Context { get => context; set => context = value; }
    public AbstractGameLevel CurrentNode { get => context.CurrentNode; }
    public AlliesManager AlliesManager { get => alliesManager; set => alliesManager = value; }
    public LootGenerator LootGenerator { get => lootGenerator; set => lootGenerator = value; }
    public IPersister Persister
    {
      get => persister;
      set => persister = value;
    }
    public ILogger Logger { get => logger; set => logger = value; }
    public Func<Tile, InteractionResult> Interact;
    public Func<int, Stairs, InteractionResult> DungeonLevelStairsHandler;
    public LevelGenerator levelGenerator;
    public Container Container { get; set; }
    public Func<Hero, GameState, AbstractGameLevel> WorldLoader { get => worldLoader; set => worldLoader = value; }
    public Action WorldSaver { get; set; }
    public GameState GameState { get => gameState; }

    public SoundManager soundManager;

    public GameManager(Container container)
    {
      Container = container;

      LootGenerator = container.GetInstance<LootGenerator>();
      Logger = container.GetInstance<ILogger>();
      levelGenerator = container.GetInstance<LevelGenerator>();
      EventsManager = container.GetInstance<EventsManager>();
      EventsManager.ActionAppended += EventsManager_ActionAppended;

      Context = container.GetInstance<GameContext>();
      Context.EventsManager = EventsManager;

      enemiesManager = new EnemiesManager(Context, EventsManager, Container);
      AlliesManager = new AlliesManager(Context, EventsManager, Container);

      Persister = container.GetInstance<IPersister>();

      soundManager = new SoundManager(this, container);
    }

    public void Assert(bool assert, string info = "assert failed")
    {
      if (!assert)
      {
        Debug.Assert(false, info);
        EventsManager.AppendAction(new Events.GameStateAction() { Type = Events.GameStateAction.ActionType.Assert, Info = info });
      }
    }

    protected virtual EnemiesManager CreateEnemiesManager(GameContext context, EventsManager eventsManager)
    {
      return new EnemiesManager(Context, EventsManager, Container);
    }

    public void SetContext(AbstractGameLevel node, Hero hero, GameContextSwitchKind kind, Stairs stairs = null)
    {
      LootGenerator.LevelIndex = node.Index;
      if (kind == GameContextSwitchKind.NewGame)
      {
        if (node.GeneratorNodes != null && node.GeneratorNodes.Any())
          hero.DungeonNodeIndex = node.GeneratorNodes.First().NodeIndex;//TODOs
        else
          hero.DungeonNodeIndex = 0;//TODO
      }

      Context.Hero = hero;

      InitNode(node);

      Context.SwitchTo(node, hero, kind, stairs);

      PrintHeroStats("SetContext " + kind);
    }

    protected virtual void InitNodeOnLoad(AbstractGameLevel node)
    {
      (node as TileContainers.GameLevel).OnLoadDone();
    }

    protected virtual void InitNode(AbstractGameLevel node, bool fromLoad = false)
    {
      node.GetTiles<LivingEntity>().ForEach(i => i.EventsManager = eventsManager);
      node.Logger = this.Logger;
      if (fromLoad)
        InitNodeOnLoad(node);
    }

    public TileContainers.GameLevel LoadLevel(string heroName, int index)
    {
      var level = Persister.LoadLevel(heroName, index);
      InitNode(level, true);
      return level;
    }

    private void EventsManager_ActionAppended(object sender, GameAction e)
    {
      //var pac = e as PolicyAppliedAction;
      //if (pac!=null)
      //{
      //  if (pac.Policy.Kind == PolicyKind.SpellCast)
      //  {
      //    var ap = pac.Policy as SpellCastPolicy;
      //  }
      //  return;
      //}

      var isLivingEntityAction = e is LivingEntityAction;
      if (!isLivingEntityAction)
      {
        var gsa = e as GameStateAction;
        if(gsa!=null)
          Logger.LogInfo(gsa.Info);
        return;
      }

      var lea = e as LivingEntityAction;
      if (lea.Kind == LivingEntityActionKind.Died)
      {
        if (context.CurrentNode.HasTile(lea.InvolvedEntity))
        {
          context.CurrentNode.SetTile(context.CurrentNode.GenerateEmptyTile(), lea.InvolvedEntity.Point);
        }
        else
        {
          Logger.LogError("context.CurrentNode HasTile failed for " + lea.InvolvedEntity);
        }
        if (lea.InvolvedEntity is Enemy)
        {
          Hero.IncreaseExp(10);
          //var loot = LootGenerator.GetRandomLoot();
          context.CurrentNode.SetTile(new Tile(), lea.InvolvedEntity.Point);
          var loot = LootGenerator.TryGetRandomLootByDiceRoll(LootSourceKind.Enemy);
          if (loot != null)
          {
            AddLootReward(loot, lea.InvolvedEntity, false);
            //ReplaceTileByLoot(loot, lea.InvolvedEntity.Point);
            //ReplaceTile(loot, lea.InvolvedEntity.Point);
          }
          var extraLootItems = GetExtraLoot();
          foreach (var extraLoot in extraLootItems)
          {
            AddLootReward(extraLoot, lea.InvolvedEntity, true);
          }
        }
      }
    }

    public bool AddLootReward(Loot item, Tile positionSource, bool animated)
    {
      return AddLootToNode(item, positionSource, animated);
    }

    public bool AddLootToNode(Loot item, Tile positionSource, bool animated)
    {
      Tile dest = null;
      var tileAtPos = context.CurrentNode.GetTile(positionSource.Point);
      if (tileAtPos.IsEmpty)
        dest = tileAtPos;
      else
        dest = context.CurrentNode.GetClosestEmpty(positionSource, true);
      if (dest != null)
      {
        var set = ReplaceTileByLoot(item, dest.Point, animated, positionSource);
        return set;
      }

      Logger.LogError("AddLootReward no room! for a loot");
      return false;
    }

    List<Loot> extraLoot = new List<Loot>();
    List<Loot> GetExtraLoot()
    {
      extraLoot.Clear();
      if (GenerationInfo.DebugInfo.EachEnemyGivesPotion)
      {
        var potion = LootGenerator.GetRandomLoot(LootKind.Potion);
        extraLoot.Add(potion);
      }
      if (GenerationInfo.DebugInfo.EachEnemyGivesJewellery)
      {
        //var loot = LootGenerator.GetRandomJewellery();
        var loot = LootGenerator.GetRandomRing();
        extraLoot.Add(loot);
      }
      return extraLoot;
    }

    public void HandleHeroShift(TileNeighborhood neib)
    {
      int horizontal = 0; 
      int vertical = 0;
      var res = DungeonNode.GetNeighborPoint(new Tile() { Point = new Point(0, 0) }, neib);
      if (res.X != 0)
        horizontal = res.X;
      else
        vertical = res.Y;

      HandleHeroShift(horizontal, vertical);
    }

    public bool CanHeroDoAction()
    {
      if (!this.HeroTurn)
        return false;
      if (!Hero.Alive)
      {
        //AppendAction(new HeroAction() { Level = ActionLevel.Critical, KindValue = HeroAction.Kind.Died, Info = Hero.Name + " is dead!" });
        return false;
      }

      if (Hero.State != EntityState.Idle)
        return false;

      var ac = Context.TurnActionsCount[TurnOwner.Hero];
      if (ac == 1)
        return false;

      return true;
    }

    public void HandleHeroShift(int horizontal, int vertical)
    {
      if(!CanHeroDoAction())
        return;

      var newPos = GetNewPositionFromMove(Hero.Point, horizontal, vertical);
      if (!newPos.Possible)
      {
        return;
      }
      var hc = CurrentNode.GetHashCode();
      var tile = CurrentNode.GetTile(newPos.Point);
      //logger.LogInfo(" tile at " + newPos.Point + " = "+ tile);
      var res = InteractionResult.None;
      if(!tile.IsEmpty)
        res = InteractHeroWith(tile);
      
      if (res == InteractionResult.ContextSwitched || res == InteractionResult.Blocked)
        return;

      if (res == InteractionResult.Handled || res == InteractionResult.Attacked)
      {
        //ASCII printer needs that event
        //logger.LogInfo(" InteractionResult " + res + ", ac="  + ac);
        EventsManager.AppendAction(new LivingEntityAction(LivingEntityActionKind.Interacted) { InvolvedEntity = Hero });
      }
      else
      {
        //logger.LogInfo(" Hero ac ="+ ac);
        context.ApplyMovePolicy(Hero, newPos.Point, (e) => {
          OnHeroPolicyApplied(this, e);
        });
      }

      //TODO shall be here ?
      RemoveDeadEnemies();
    }

    public void SkipHeroTurn()
    {
      if (Context.HeroTurn)
        Context.MoveToNextTurnOwner();
    }

    private void RemoveDeadEnemies()
    {
      EnemiesManager.Enemies.RemoveAll(i => !i.Alive);
    }

    public T GetCurrentNode<T>() where T : AbstractGameLevel
    {
      return CurrentNode as T;
    }

    public virtual InteractionResult InteractHeroWith(Tile tile)
    {
      if (Interact != null)
      {
        var res = Interact(tile);
        if (res != InteractionResult.None)
          return res;
      }
      if (tile == null)
      {
        Logger.LogError("tile == null!!!");
        return InteractionResult.None;
      }
      bool tileIsDoor = tile is Tiles.Door;
      bool tileIsDoorBySumbol = tile.Symbol == Constants.SymbolDoor;

      if (tile is Enemy || tile is Dungeons.Tiles.Wall)
      {
        //Logger.LogInfo("Hero attacks " + tile);
       // var en = tile as Enemy;
        //if(!en.Alive)
        //  Logger.LogError("Hero attacks dead!" );
        //else
        //  Logger.LogInfo("Hero attacks en health = "+en.Stats.Health);
        Context.ApplyPhysicalAttackPolicy(Hero, tile, (p) => OnHeroPolicyApplied(this, p));

        return InteractionResult.Attacked;
      }
      else if (tileIsDoor || tileIsDoorBySumbol)
      {
        var door = tile as Tiles.Door;
        if (door.Opened)
          return InteractionResult.None;

        return CurrentNode.RevealRoom(door, Hero) ? InteractionResult.Handled : InteractionResult.None;
      }

      else if (tile is InteractiveTile)
      {
        if (tile is Stairs)
        {
          var stairs = tile as Stairs;
          var destLevelIndex = -1;
          if (stairs.StairsKind == StairsKind.LevelDown ||
          stairs.StairsKind == StairsKind.LevelUp)
          {
            var level = GetCurrentDungeonLevel();
            if (stairs.StairsKind == StairsKind.LevelDown)
            {
              destLevelIndex = level.Index + 1;
            }
            else if (stairs.StairsKind == StairsKind.LevelUp)
            {
              destLevelIndex = level.Index - 1;
            }
            if (DungeonLevelStairsHandler != null)
              return DungeonLevelStairsHandler(destLevelIndex, stairs);
          }
        }
        else //if (tile is Barrel)
        {
          Context.ApplyPhysicalAttackPolicy(Hero, tile, (policy) => OnHeroPolicyApplied(this, policy));
          return InteractionResult.Attacked;
        }
        //else if (tile is Chest)
        //{
        //  var chest = tile as Chest;
        //  var loot = LootGenerator.TryGetRandomLootByDiceRoll(chest.LootSourceKind);
        //  if (loot != null)
        //  {
        //    bool replaced = ReplaceTileByLoot(loot, tile.Point);
        //  }
          
        //}
        return InteractionResult.Blocked;//blok hero by default
      }
      //else if (tile is Dungeons.Tiles.IObstacle)
      //{
      //  return InteractionResult.Blocked;//blok hero by default
      //}
      return InteractionResult.None;
    }

    public void OnHeroPolicyApplied(object sender, Policies.Policy policy)
    {
      if (policy.Kind == PolicyKind.Attack)
      {
        var attackPolicy = policy as AttackPolicy;
        if (attackPolicy.Victim is Barrel || attackPolicy.Victim is Chest)
        {
          var lsk = LootSourceKind.Barrel;
          if (attackPolicy.Victim is Chest)
            lsk = (attackPolicy.Victim as Chest).LootSourceKind;
          var tileDest = LootGenerator.TryGetRandomLootByDiceRoll(lsk);
          //AddLootReward
          if (attackPolicy.Victim is Barrel)
          {
            bool repl = ReplaceTileByLoot(tileDest, attackPolicy.Victim.Point, false, attackPolicy.Victim);
            Assert(repl, "ReplaceTileByLoot " + tileDest);
            Debug.WriteLine("ReplaceTileByLoot " + tileDest + " " + repl);
          }
          else
            AddLootReward(tileDest, attackPolicy.Victim, true);
        }
      }
      if (policy is AttackPolicy || policy is SpellCastPolicy)
        RemoveDeadEnemies();
      context.IncreaseActions(TurnOwner.Hero);
      context.MoveToNextTurnOwner();
    }

    public bool ReplaceTileByLoot(Loot loot, Point point, bool animated, Tile positionSource)
    {
      //Assert(loot is Loot || loot.IsEmpty, "ReplaceTileByLoot failed");
      var prevTile = CurrentNode.ReplaceTile(loot, point);
      if (prevTile != null)
      {
        var it = prevTile as InteractiveTile;
        if(it!=null)
          this.EventsManager.AppendAction(new InteractiveTileAction(it) { KindValue = InteractiveTileAction.Kind.Destroyed });

        this.EventsManager.AppendAction(new LootAction(loot) { LootActionKind = LootActionKind.Generated, GenerationAnimated = animated, Source = positionSource });
        return true;
      }

      return false;
    }

    TileContainers.GameLevel GetCurrentDungeonLevel()
    {
      var dl = this.CurrentNode as TileContainers.GameLevel;
      return dl;
    }

    public string GetCurrentDungeonDesc()
    {
      GameState gameState = PrepareGameStateForSave();
      return gameState.ToString();
    }

    Func<Hero, GameState, AbstractGameLevel> worldLoader;

    public virtual void Load(string heroName)
    {
      persistancyWorker.Load(heroName, this, WorldLoader);
    }

    PersistancyWorker persistancyWorker = new PersistancyWorker();

    public virtual void Save()
    {
      persistancyWorker.Save(this, WorldSaver);
    }

    public virtual GameState PrepareGameStateForSave()
    {
      gameState.Settings.CoreInfo.LastSaved = DateTime.Now;
      gameState.HeroPathValue.Pit = "";
      
      if (CurrentNode is TileContainers.GameLevel)//TODO 
      {
        var dl = CurrentNode as TileContainers.GameLevel;
        gameState.HeroPathValue.Pit = dl.PitName;
        gameState.HeroPathValue.LevelIndex = dl.Index;
      }

      return gameState;
    }

    public MoveResult GetNewPositionFromMove(Point pos, int horizontal, int vertical)
    {
      if (horizontal != 0 || vertical != 0)
      {
        if (horizontal != 0)
        {
          pos.X += horizontal > 0 ? 1 : -1;
        }
        else if (vertical != 0)
        {
          pos.Y += vertical > 0 ? 1 : -1;
        }
        return new MoveResult(true, pos);
      }

      return new MoveResult(false, pos);
    }

    public bool CollectLoot(Loot lootTile, bool fromDistance)
    {
      if (Hero.Inventory.Add(lootTile))
      {
        //Hero.Inventory.Print(logger, "loot added");
        CurrentNode.RemoveLoot(lootTile.Point);
        EventsManager.AppendAction(new LootAction(lootTile) { LootActionKind = LootActionKind.Collected, CollectedFromDistance = fromDistance });
        if (lootTile is Equipment)
        {
          var eq = lootTile as Equipment;
          Hero.HandleEquipmentFound(eq);
          PrintHeroStats("loot On");
        }
        return true;
      }
      return false;
    }

    public bool CollectLootOnHeroPosition()
    {
      var lootTile = CurrentNode.GetLootTile(Hero.Point);
      if (lootTile != null)
      {
        return CollectLoot(lootTile, false);
      }

      return false;
    }

    public void PrintHeroStats(string context,bool onlyNonZero = true)
    {
      Logger.LogInfo("PrintHeroStats "+ context);
      //foreach (var stat in Hero.Stats.Stats.Values)
      //{
      //  //if(!onlyNonZero || stat.Value.TotalValue != 0)
      //  //  Logger.LogInfo(stat.Kind + ": " + stat.Value);
      //}
    }

    public void DoAlliesTurn(bool skipHero = false)
    {
      this.AlliesManager.MakeEntitiesMove(skipHero ? Hero : null);
      //DoEnemiesTurn();
    }

    public virtual Equipment GenerateRandomEquipment(EquipmentKind kind)
    {
      return lootGenerator.GetRandomEquipment(kind);
    }

    public void MakeGameTick()
    {
      if (context.PendingTurnOwnerApply)
      {
        if (context.TurnOwner == TurnOwner.Allies)
        {
          //logger.LogInfo("call to liesManager.MoveHeroAllies");
          context.PendingTurnOwnerApply = false;
          AlliesManager.MoveHeroAllies();
          
        }
        else if (context.TurnOwner == TurnOwner.Enemies)
        {
          //logger.LogInfo("call to EnemiesManager.MakeEntitiesMove");
          context.PendingTurnOwnerApply = false;
          EnemiesManager.MakeEntitiesMove();
          
        }
      }
    }

    public void SetGameState(GameState gameState)
    {
      this.gameState = gameState;
    }
  }
}
