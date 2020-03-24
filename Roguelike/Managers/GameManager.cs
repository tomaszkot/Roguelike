using Dungeons.Core;
using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System.Drawing;
using Roguelike.InfoScreens;
using Roguelike.Generators;
using Dungeons;
using System.Diagnostics;
using Roguelike.Abstract;
using Roguelike.Serialization;
using SimpleInjector;
using Roguelike.Events;
using Newtonsoft.Json;
using Roguelike.Tiles.Interactive;
using Roguelike.Policies;

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
    LootGenerator lootGenerator = new LootGenerator();
    EventsManager eventsManager;
    EnemiesManager enemiesManager;
    AlliesManager alliesManager;

    private IPersister persister;
    private ILogger logger;

    public EnemiesManager EnemiesManager { get => enemiesManager; set => enemiesManager = value; }
    public Hero Hero { get => Context.Hero; }
    public bool HeroTurn { get => Context.HeroTurn; }

    //public EventsManager ActionsManager { get => EventsManager; set => EventsManager = value; }
    [JsonIgnore]
    public EventsManager EventsManager { get => eventsManager; set => eventsManager = value; }
    public GameContext Context { get => context; set => context = value; }
    public AbstractGameLevel CurrentNode { get => context.CurrentNode; }
    public AlliesManager AlliesManager { get => alliesManager; set => alliesManager = value; }
    public LootGenerator LootGenerator { get => lootGenerator; set => lootGenerator = value; }
    public IPersister Persister { get => persister; set => persister = value; }
    public ILogger Logger { get => logger; set => logger = value; }
    public Func<Tile, InteractionResult> Interact;
    public Func<int, Stairs, InteractionResult> DungeonLevelStairsHandler;
    public LevelGenerator levelGenerator;
    public Container Container { get; set; }
    public Func<Hero, GameState, AbstractGameLevel> WorldLoader { get => worldLoader; set => worldLoader = value; }
    public Action WorldSaver { get; set; }
    public SoundManager soundManager;

    public GameManager(Container container)
    {
      Container = container;
      
      Logger = container.GetInstance<ILogger>();
      levelGenerator = container.GetInstance<LevelGenerator>();
      EventsManager = new EventsManager();
      EventsManager.ActionAppended += EventsManager_ActionAppended;

      Context = container.GetInstance<GameContext>();
      Context.EventsManager = EventsManager;

      enemiesManager = new EnemiesManager(Context, EventsManager);
      AlliesManager = new AlliesManager(Context, EventsManager);

      Persister = container.GetInstance<JSONPersister>();

      soundManager = new SoundManager(this, container);
    }

    protected virtual EnemiesManager CreateEnemiesManager(GameContext context, EventsManager eventsManager)
    {
      return new EnemiesManager(Context, EventsManager);
    }

    public void SetContext(AbstractGameLevel node, Hero hero, GameContextSwitchKind kind, Stairs stairs = null)
    {
      if (kind == GameContextSwitchKind.NewGame)
      {
        if (node.GeneratorNodes !=null && node.GeneratorNodes.Any())
          hero.DungeonNodeIndex = node.GeneratorNodes.First().NodeIndex;//TODOs
        else
          hero.DungeonNodeIndex = 0;//TODO
      }

      Context.Hero = hero;

      InitNode(node);

      Context.SwitchTo(node, hero, kind, stairs);

      PrintHeroStats("SetContext "+ kind);
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

    public TileContainers.GameLevel LoadLevel(int index)
    {
      var level = Persister.LoadLevel(index);
      InitNode(level, true);
      return level;
    }

    private void EventsManager_ActionAppended(object sender, GameAction e)
    {
      if(e is LivingEntityAction)
      {
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
          }
        }
      }
    }

    public void HandleHeroShift(int horizontal, int vertical)
    {
      if (!HeroTurn)
        return;

      if (!Hero.Alive)
      {
        //AppendAction(new HeroAction() { Level = ActionLevel.Critical, KindValue = HeroAction.Kind.Died, Info = Hero.Name + " is dead!" });
        return;
      }

      if (Hero.State != EntityState.Idle)
        return;

      var ac = Context.TurnActionsCount[TurnOwner.Hero];
      if (ac == 1)
        return;

      var newPos = GetNewPositionFromMove(Hero.Point, horizontal, vertical);
      if (!newPos.Possible)
      {
        return;
      }
      var hc = CurrentNode.GetHashCode();
      var tile = CurrentNode.GetTile(newPos.Point);
      var res = InteractHeroWith(tile);


      if (res == InteractionResult.ContextSwitched || res == InteractionResult.Blocked)
        return;

      if (res == InteractionResult.Handled || res == InteractionResult.Attacked)
      {
        //ASCII printer needs that event
        logger.LogInfo(" InteractionResult " + res + ", ac="  + ac);
        EventsManager.AppendAction(new LivingEntityAction(LivingEntityActionKind.Interacted) { InvolvedEntity = Hero });
      }
      else
      {
        logger.LogInfo(" Hero ac ="+ ac);
        //AlliesManager.MoveEntity(Hero, newPos.Point);
        context.CreateMovePolicy(Hero, newPos.Point, (e) => {
          OnHeroAttackPolicyApplied(this, e);
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
      if (tile is Enemy)
      {
        Logger.LogInfo("Hero attacks " + tile);
        var en = tile as Enemy;
        var heroAttackPolicy = AlliesManager.AttackPolicy(Hero, en);
        heroAttackPolicy.OnApplied += OnHeroAttackPolicyApplied;
        heroAttackPolicy.Apply();

        return InteractionResult.Attacked;
      }

      else if (tile is Tiles.Door)
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
        else if (tile is Barrel)
        {
          var loot = LootGenerator.GetRandomLoot();
          ReplaceTile(loot, tile.Point);
        }
        return InteractionResult.Blocked;//blok hero by default
      }
      else if (tile is Dungeons.Tiles.IObstacle)
      {
        return InteractionResult.Blocked;//blok hero by default
      }
      return InteractionResult.None;
    }

    private void OnHeroAttackPolicyApplied(object sender, Policies.Policy e)
    {
      if(e is AttackPolicy)
        RemoveDeadEnemies();
      context.IncreaseActions(TurnOwner.Hero);
      context.MoveToNextTurnOwner();
    }

    internal bool ReplaceTile(Loot loot, Point point)
    {
      var prevTile = CurrentNode.ReplaceTile(loot, point);
      if (prevTile != null)
      {
        this.EventsManager.AppendAction(new InteractiveTileAction(prevTile as InteractiveTile) { KindValue = InteractiveTileAction.Kind.Destroyed });
        this.EventsManager.AppendAction(new LootAction(loot) { LootActionKind = LootActionKind.Generated });
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
      GameState gameState = CreateGameState();
      return gameState.ToString();
    }

    Func<Hero, GameState, AbstractGameLevel> worldLoader;

    public virtual void Load()
    {
      persistancyWorker.Load(this, WorldLoader);
    }

    PersistancyWorker persistancyWorker = new PersistancyWorker();

    public virtual void Save()
    {
      persistancyWorker.Save(this, WorldSaver);
    }

    public virtual GameState CreateGameState()
    {
      GameState gameState = new GameState();
      gameState.LastSaved = DateTime.Now;
      gameState.HeroPathValue.Pit = "";
      gameState.LastSaved = DateTime.Now;
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

    public bool CollectLootOnHeroPosition()
    {
      var lootTile = CurrentNode.GetLootTile(Hero.Point);
      if (lootTile != null)
      {
        if(Hero.Inventory.Add(lootTile))
        {
          //Hero.Inventory.Print(logger, "loot added");
          CurrentNode.RemoveLoot(lootTile.Point);
          this.EventsManager.AppendAction(new LootAction(lootTile) { LootActionKind = LootActionKind.Collected });
          if (lootTile is Equipment)
          {
            var eq = lootTile as Equipment;
            Hero.MoveEquipment(Hero.Inventory, Hero.CurrentEquipment, eq, eq.EquipmentKind);
            
            //Hero.SetEquipment(eq.EquipmentKind, eq);

            PrintHeroStats("loot On");
          }
          return true;
        }
      }

      return false;
    }

    public void PrintHeroStats(string context,bool onlyNonZero = true)
    {
      Logger.LogInfo("PrintHeroStats "+ context);
      foreach (var stat in Hero.Stats.Stats.Values)
      {
        if(!onlyNonZero || stat.Value.TotalValue != 0)
          Logger.LogInfo(stat.Kind + ": " + stat.Value);
      }
    }

    public void DoAlliesTurn(bool skipHero = false)
    {
      this.AlliesManager.MakeEntitiesMove(skipHero ? Hero : null);
      //DoEnemiesTurn();
    }

    public virtual Equipment GenerateRandomEquipment(EquipmentKind weapon)
    {
      return lootGenerator.GetRandomWeapon();
    }

    public void MakeGameTick()
    {
      if (context.PendingTurnOwnerApply)
      {
        if (context.TurnOwner == TurnOwner.Allies)
        {
          logger.LogInfo("call to liesManager.MoveHeroAllies");
          context.PendingTurnOwnerApply = false;
          AlliesManager.MoveHeroAllies();
          
        }
        else if (context.TurnOwner == TurnOwner.Enemies)
        {
          logger.LogInfo("call to EnemiesManager.MakeEntitiesMove");
          context.PendingTurnOwnerApply = false;
          EnemiesManager.MakeEntitiesMove();
          
        }

        
      }
    }
  }
}
