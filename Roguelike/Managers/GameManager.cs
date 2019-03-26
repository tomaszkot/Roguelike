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
    EntitiesManager alliesManager;

    private IPersister persister;
    private ILogger logger;

    public EnemiesManager EnemiesManager { get => enemiesManager; set => enemiesManager = value; }
    public Hero Hero { get => Context.Hero; }
    public bool HeroTurn { get => Context.HeroTurn; }

    //public EventsManager ActionsManager { get => EventsManager; set => EventsManager = value; }
    public EventsManager EventsManager { get => eventsManager; set => eventsManager = value; }
    public GameContext Context { get => context; set => context = value; }
    public GameNode CurrentNode { get => context.CurrentNode; }
    public EntitiesManager AlliesManager { get => alliesManager; set => alliesManager = value; }
    internal LootGenerator LootGenerator { get => lootGenerator; set => lootGenerator = value; }
    public IPersister Persister { get => persister; set => persister = value; }
    public ILogger Logger { get => logger; set => logger = value; }
    public Func<Tile, InteractionResult> Interact;
    public Func<int, Stairs, InteractionResult> DungeonLevelStairsHandler;
    public LevelGenerator levelGenerator;

    public GameManager(Container container)
    {
      this.Logger = container.GetInstance<ILogger>();
      levelGenerator = new LevelGenerator(Logger);
      EventsManager = new EventsManager();
      EventsManager.ActionAppended += EventsManager_ActionAppended;

      Context = container.GetInstance<GameContext>();
      Context.EventsManager = EventsManager;

      enemiesManager = new EnemiesManager(Context, EventsManager);
      AlliesManager = new EntitiesManager(Context, EventsManager);

      Persister = container.GetInstance<JSONPersister>();
    }

    public void SetContext(GameNode node, Hero hero, GameContextSwitchKind kind, Stairs stairs = null)
    {
      Context.Hero = hero;

      InitNode(node);

      Context.SwitchTo(node, hero, kind, stairs);

      PrintHeroStats("SetContext "+ kind);
    }

    public virtual void InitNode(GameNode node, bool fromLoad)
    {
      InitNode(node as GameNode );
    }

    protected void InitNode(GameNode node)
    {
      node.GetTiles<LivingEntity>().ForEach(i => i.EventsManager = eventsManager);
      node.Logger = this.Logger;
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
            Logger.LogError("context.CurrentNode HasTile failed for " + lea.InvolvedEntity);
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

      if (Hero.State == EntityState.Attacking)
        return;

      var newPos = GetNewPositionFromMove(Hero.Point, horizontal, vertical);
      if (!newPos.Possible)
      {
        return;
      }
            
      var tile = CurrentNode.GetTile(newPos.Point);
      var res = InteractHeroWith(tile);
      if (res == InteractionResult.ContextSwitched || res == InteractionResult.Blocked)
        return;
      if (res == InteractionResult.Handled || res == InteractionResult.Attacked)
      {
        //ASCII printer needs that event
        EventsManager.AppendAction(new LivingEntityAction(LivingEntityAction.Kind.Interacted) { InvolvedEntity = Hero });
      }
      else
      {
        AlliesManager.MoveEntity(Hero, newPos.Point);
      }
      AlliesManager.MoveHeroAllies();

      EnemiesManager.Enemies.RemoveAll(i => !i.Alive);
      if(res != InteractionResult.Attacked)//Wait for attack to be finished (or close to be finished)
        Context.HeroTurn = false;
    }

    public T GetCurrentNode<T>() where T : GameNode
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
        Logger.LogInfo("Hero attacks "+tile);
        var en = tile as Enemy;
        var ap = AlliesManager.PolicyFactory(Hero, en);
        ap.Apply();
        Hero.State = EntityState.Attacking;
        return InteractionResult.Attacked;
      }

      if (tile is Tiles.Door)
      {
        if ((tile as Tiles.Door).Opened)
          return InteractionResult.None;
        return CurrentNode.RevealRoom((tile as Tiles.Door), Hero) ? InteractionResult.Handled : InteractionResult.None;
      }

      if (tile is Dungeons.Tiles.IObstacle)
      {
        if (tile is Stairs)
        {
          var stairs = tile as Stairs;
          var destLevelIndex = -1;
          if (stairs.Kind == StairsKind.LevelDown ||
          stairs.Kind == StairsKind.LevelUp)
          {
            var level = GetCurrentDungeonLevel();
            if (stairs.Kind == StairsKind.LevelDown)
            {
              destLevelIndex = level.Index + 1;
            }
            else if (stairs.Kind == StairsKind.LevelUp)
            {
              destLevelIndex = level.Index - 1;
            }
            if(DungeonLevelStairsHandler!=null)
              return DungeonLevelStairsHandler(destLevelIndex, stairs);
          }
        }
        return InteractionResult.Blocked;//blok hero by default
      }
      return InteractionResult.None;
    }

    DungeonLevel GetCurrentDungeonLevel()
    {
      var dl = this.CurrentNode as DungeonLevel;
      return dl;
    }

    public string GetCurrentDungeonDesc()
    {
      GameState gameState = CreateGameState();
      return gameState.ToString();
    }
    
    public virtual void Load()
    {
      persistancyWorker.Load(this);
    }

    PersistancyWorker persistancyWorker = new PersistancyWorker();

    public virtual void Save()
    {
      persistancyWorker.Save(this, null);
    }

    public virtual GameState CreateGameState()
    {
      GameState gameState = new GameState();
      gameState.LastSaved = DateTime.Now;
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
          this.EventsManager.AppendAction(new LootAction() {  });
          if (lootTile is Equipment)
          {
            var eq = lootTile as Equipment;
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

    public void PrintHeroStats(string context,bool onlyNonZero = true)
    {
      Logger.LogInfo("PrintHeroStats "+ context);
      foreach (var stat in Hero.Stats.Stats)
      {
        if(!onlyNonZero || stat.Value.TotalValue != 0)
          Logger.LogInfo(stat.Key + ": " + stat.Value);
      }
    }

    public void DoAlliesTurn(bool skipHero = false)
    {
      this.AlliesManager.MakeEntitiesMove(skipHero ? Hero : null);
      //DoEnemiesTurn();
    }

    public virtual Equipment GenerateRandomEquipment(EquipmentKind weapon)
    {
      return new LootGenerator().GetRandomWeapon();
    }
  }
}
