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
using Serialization;
using Roguelike.Abstract;

namespace Roguelike.Managers
{
  public class GameManager
  {
    GameContext context;
    LootGenerator lootGenerator = new LootGenerator();
    EventsManager eventsManager;
    EnemiesManager enemiesManager;
    EntitiesManager alliesManager;

    IPersister persister;
    ILogger logger;
    // InputManager inputManager;

    public EnemiesManager EnemiesManager { get => enemiesManager; set => enemiesManager = value; }
    public World World { get => Context.World; set => Context.World = value; }

    
    public Hero Hero { get => Context.Hero; }
    public bool HeroTurn { get => Context.HeroTurn; }

    //public EventsManager ActionsManager { get => EventsManager; set => EventsManager = value; }
    public EventsManager EventsManager { get => eventsManager; set => eventsManager = value; }
    public GameContext Context { get => context; set => context = value; }
    public GameNode CurrentNode { get => context.CurrentNode; }
    public EntitiesManager AlliesManager { get => alliesManager; set => alliesManager = value; }
    internal LootGenerator LootGenerator { get => lootGenerator; set => lootGenerator = value; }

    //public InputManager InputManager { get => inputManager; set => inputManager = value; }

    //public GameManager(ILogger logger)
    //{

    //}

    public GameManager(ILogger logger)//World world, Hero hero, ILogger logger)
    {
      this.logger = logger;
      EventsManager = new EventsManager();
      EventsManager.ActionAppended += EventsManager_ActionAppended;

      Context = new GameContext(logger);// world, hero, logger);
      Context.EventsManager = EventsManager;

      enemiesManager = new EnemiesManager(Context, EventsManager);
      AlliesManager = new EntitiesManager(Context, EventsManager);

      persister = new JSONPersister();
    }

    public void SetContext(GameNode node, Hero hero, GameContextSwitchKind kind, Stairs stairs = null)
    {
      if(node is World)
        Context.World = node as World;
      Context.Hero = hero;

      InitNode(node);

      Context.SwitchTo(node, hero, kind, stairs);

      PrintHeroStats("SetContext "+ kind);
    }

    private void InitNode(World node, bool fromLoad)
    {
      InitNode(node as GameNode );

      foreach (var pit in node.Pits)
      {
        foreach (var dl in pit.Levels)
        {
          InitNode(dl);
          if (fromLoad)
            dl.OnLoadDone();
        }
      }
    }

    private void InitNode(GameNode node)
    {
      node.GetTiles<LivingEntity>().ForEach(i => i.EventsManager = eventsManager);
      node.Logger = this.logger;
    }
    
    private void EventsManager_ActionAppended(object sender, GenericEventArgs<GameAction> e)
    {
      if(e.EventData is LivingEntityAction)
      {
        var lea = e.EventData as LivingEntityAction;
        if (lea.KindValue == LivingEntityAction.Kind.Died)
        {
          if (context.CurrentNode.HasTile(lea.InvolvedEntity))
            context.CurrentNode.SetTile(context.CurrentNode.GenerateEmptyTile(), lea.InvolvedEntity.Point);
          else
          {
            logger.LogError("context.CurrentNode HasTile failed for " + lea.InvolvedEntity);
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

      var newPos = GetNewPositionFromMove(Hero.Point, horizontal, vertical);
      if (!newPos.Item1)
      {
        return;
      }
            
      var tile = CurrentNode.GetTile(newPos.Item2);
      bool contextSwitched = false;
      var handled = InteractWith(tile, ref contextSwitched);
      if (contextSwitched)
        return;
      if (handled)
      {
        //ASCII printer needs that event
        EventsManager.AppendAction(new LivingEntityAction(LivingEntityAction.Kind.Interacted) { InvolvedEntity = Hero });
      }
      else
      {
        if (!AlliesManager.MoveEntity(Hero, newPos.Item2))
          return;
      }
      AlliesManager.MoveHeroAllies();

      EnemiesManager.Enemies.RemoveAll(i => !i.Alive);

      Context.HeroTurn = false;
    }

    public T GetCurrentNode<T>() where T : GameNode
    {
      return CurrentNode as T;
    }

    private bool InteractWith(Tile tile, ref bool contextSwitched)
    {
      if (tile is Enemy)
      {
        logger.LogInfo("Hero attacks "+tile);
        var en = tile as Enemy;
        var ap = AlliesManager.PolicyFactory(Hero, en);
        ap.Apply();
        return true;
      }

      if (tile is Tiles.Door)
      {
        if ((tile as Tiles.Door).Opened)
          return false;
        return CurrentNode.RevealRoom((tile as Tiles.Door), Hero);
      }

        if (tile is Dungeons.Tiles.IObstacle)
      {
        if (tile is Stairs)
        {
          var stairs = tile as Stairs;
          if (stairs.Kind == StairsKind.PitDown || stairs.Kind == StairsKind.PitUp)
          {
            var world = World;//TODO World migh not be loaded!// GetCurrentNode<World>();
            var pit = world.GetPit(stairs.PitName);
            if (stairs.Kind == StairsKind.PitDown)
            {
              DungeonLevel level = null;
              if (!pit.Levels.Any())
              {
                var lg = new LevelGenerator(pit.Name, logger);
                level = lg.Generate() as DungeonLevel;
                level.Logger = logger;
                pit.AddLevel(level);
              }
              else
                level = pit.Levels.First();
              SetContext(level, Hero, GameContextSwitchKind.WorldSwitched, stairs);
              contextSwitched = true;

              var heroInW = world.GetTiles<Hero>().SingleOrDefault();
              var st = world.GetTiles<Stairs>();
              Debug.Assert(heroInW == null);
            }
            else
            {
              var st = world.GetTiles<Stairs>();
              SetContext(world, Hero, GameContextSwitchKind.WorldSwitched, stairs);
              contextSwitched = true;
              var st1 = world.GetTiles<Stairs>();
              int k = 0;
              k++;
            }
          }
        }
        return true;
      }
      return false;
    }

    public string GetCurrentDungeonDesc()
    {
      GameState gameState = CreateGameState();
      return gameState.ToString();
    }


    public void Load()
    {
      var world = persister.LoadWorld();
      this.World = world;
      world.Logger = this.logger;
      RestoreEmptyTiles(world);
      
      var pits = persister.LoadPits();
      world.Pits = pits;

      var hero = persister.LoadHero();

      var gs = persister.LoadGameState();

      var startingNode = world.PlaceLoadedHero(hero, gs);

      InitNode(world, true);

      context.SwitchTo(startingNode, hero, GameContextSwitchKind.GameLoaded);

      PrintHeroStats("load");

      //EventsManager.AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.GameFinished});
    }

    private void RestoreEmptyTiles(World world)
    {
      world.DoGridAction((int col, int row) =>
      {
        if (world.Tiles[row, col] == null)
        {
          world.SetEmptyTile(new Point(col, row));
        }
      });
    }

    public void Save()
    {
#if DEBUG
      var heros = CurrentNode.GetTiles<Hero>();
      var heroInNode = heros.SingleOrDefault();
      Debug.Assert(heroInNode != null);
#endif
      var nodeNodeName = CurrentNode.Name;
      //Hero is saved in a separate file, see persister.SaveHero
      if (!CurrentNode.SetEmptyTile(Hero.Point))
        logger.LogError("failed to reset hero on save");

      var world = World;
      if (world != null)
      {
        //optimize save/load time/storage
        world.DoGridAction((int col, int row) =>
        {
          if (world.IsTileEmpty(world.Tiles[row, col]))
          {
            world.SetTile(null, new Point(col, row));
          }
        });
        persister.SaveWorld(world);
        persister.SavePits(world.Pits);
      }
      persister.SaveHero(Hero);

      GameState gameState = CreateGameState();
      persister.SaveGameState(gameState);

      world.SetTile(Hero, Hero.Point);

      RestoreEmptyTiles(world);
    }

    private GameState CreateGameState()
    {
      GameState gameState = new GameState();
      gameState.LastSaved = DateTime.Now;
      gameState.HeroPathValue.World = World != null ? World.Name : "?";
      gameState.HeroPathValue.Pit = "";
      gameState.LastSaved = DateTime.Now;
      if (CurrentNode is DungeonLevel)
      {
        var dl = CurrentNode as DungeonLevel;
        gameState.HeroPathValue.Pit = dl.PitName;
        gameState.HeroPathValue.LevelIndex = dl.Index;
      }

      return gameState;
    }

    public Tuple<bool, Point> GetNewPositionFromMove(Point pos, int horizontal, int vertical)
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
        return new Tuple<bool, Point>(true, pos);
      }

      return new Tuple<bool, Point>(false, pos);
    }

    //public Tuple<bool, Point> MoveHero(int horizontal, int vertical)
    //{
    //  return alliesManager.MoveEntity(hero, horizontal, vertical);
    //}

    public bool CollectLootOnHeroPosition()
    {
      var lootTile = CurrentNode.GetLootTile(Hero.Point);
      if (lootTile != null)
      {
        if(Hero.Inventory.Add(lootTile))
        {
          //Hero.Inventory.Print(logger, "loot added");
          CurrentNode.RemoveLoot(lootTile.Point);
          this.EventsManager.AppendAction(new LootAction() {  });
          if (lootTile is Equipment)
          {
            var eq = lootTile as Equipment;
            //if (
            Hero.SetEquipment(eq.EquipmentKind, eq);

            PrintHeroStats("loot On");
            //{
            //  LootAction ac = null;
            //  if (eq != null)
            //    ac = new LootAction() { Info = Hero.Name + " put on " + eq, Loot = eq, KindValue = LootAction.Kind.PutOn, EquipmentKind = eq.EquipmentKind };
            //  //else
            //  //  ac = new LootAction() { Info = Name + " took off " + kind, Loot = null, KindValue = LootAction.Kind.TookOff, EquipmentKind = kind };
            //  EventsManager.AppendAction(ac);
            //}
          }
          return true;
        }
      }

      return false;
    }

    private void PrintHeroStats(string context,bool onlyNonZero = true)
    {
      logger.LogInfo("PrintHeroStats "+ context);
      foreach (var stat in Hero.Stats.Stats)
      {
        if(!onlyNonZero || stat.Value.TotalValue != 0)
          logger.LogInfo(stat.Key + ": " + stat.Value);
      }
    }

    public void DoAlliesTurn(bool skipHero = false)
    {
      this.AlliesManager.MakeEntitiesMove(skipHero ? Hero : null);
      //DoEnemiesTurn();
    }

    //public void DoEnemiesTurn()
    //{
    //  //EnemiesManager.Hero = Hero;
    //  EnemiesManager.MakeEntitiesMove();
    //}



  }
}
