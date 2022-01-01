using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract.Inventory;
using Roguelike.Abstract.Projectiles;
using Roguelike.Abstract.Spells;
using Roguelike.Abstract.Tiles;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Generators;
using Roguelike.History;
using Roguelike.LootContainers;
using Roguelike.Policies;
using Roguelike.Serialization;
using Roguelike.Settings;
using Roguelike.State;
using Roguelike.Strategy;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.Managers
{
  public enum InteractionResult { None, Handled, ContextSwitched, Attacked, Blocked };
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
    AnimalsManager animalsManager;
    LevelGenerator levelGenerator;
    LootManager lootManager;
    protected InputManager inputManager;

    IPersister persister;
    ILogger logger;

    public Hero Hero { get => Context.Hero; }
    protected GameState gameState;

    public bool HeroTurn { get => Context.HeroTurn; }

    public virtual void SetLoadedContext(AbstractGameLevel node, Hero hero)
    {
      SetContext(node, hero, GameContextSwitchKind.GameLoaded);
    }

    public SpellManager SpellManager { get; set; }
    public EnemiesManager EnemiesManager { get => enemiesManager; set => enemiesManager = value; }
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

    [JsonIgnore]
    public Container Container
    {
      get;
      set;
    }
    public Func<Hero, GameState, AbstractGameLevel> WorldLoader { get => worldLoader; set => worldLoader = value; }
    public Action WorldSaver { get; set; }
    public GameState GameState { get => gameState; }
    public SoundManager SoundManager { get; set; }
    public LevelGenerator LevelGenerator { get => levelGenerator; set => levelGenerator = value; }
    public LootManager LootManager { get => lootManager; protected set => lootManager = value; }
    static GameManager _debugCurrentInstance;

    public GameManager(Container container)
    {
      _debugCurrentInstance = this;
      Container = container;

      gameState = container.GetInstance<GameState>();
      LootGenerator = container.GetInstance<LootGenerator>();
      Logger = container.GetInstance<ILogger>();
      levelGenerator = container.GetInstance<LevelGenerator>();
      EventsManager = container.GetInstance<EventsManager>();
      EventsManager.GameManager = this;
      lootManager = container.GetInstance<LootManager>();
      lootManager.GameManager = this;
      CreateInputManager();

      EventsManager.ActionAppended += EventsManager_ActionAppended;

      Context = container.GetInstance<GameContext>();
      Context.EventsManager = EventsManager;
      Context.AttackPolicyDone += () =>
      {
        RemoveDead();
      };

      enemiesManager = new EnemiesManager(Context, EventsManager, Container, null, this);
      AlliesManager = new AlliesManager(Context, EventsManager, Container, enemiesManager, this);
      enemiesManager.AlliesManager = AlliesManager;

      animalsManager = new AnimalsManager(Context, EventsManager, Container, this);

      Persister = container.GetInstance<IPersister>();

      SoundManager = new SoundManager(this, container);
      SpellManager = new SpellManager(this);
    }

    public void DisconnectEvents()
    {
      //EventsManager.ActionAppended -= EventsManager_ActionAppended;
    }

    protected virtual void CreateInputManager()
    {
      inputManager = new InputManager(this);
    }

    public bool ApplyMovePolicy(LivingEntity entity, Point newPos, List<Point> fullPath, Action<Policy> OnApplied)
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

      var oldTile = CurrentNode.GetTile(newPos);
      if (oldTile is FightItem fi)
      {
        if (fi.FightItemKind == FightItemKind.HunterTrap && fi.FightItemState == FightItemState.Activated)
        {
          entity.OnHitBy(fi as ProjectileFightItem);
          //var bleed = entity.StartBleeding(fi.Damage, null, fi.TurnLasting);
          //bleed.Source = fi;

          //fi.SetState(FightItemState.Busy);
          //SoundManager.PlaySound("trap");
        }
      }

      if (movePolicy.Apply(CurrentNode, entity, newPos, fullPath))
      {
        EventsManager.AppendAction(new LivingEntityAction(kind: LivingEntityActionKind.Moved)
        {
          Info = entity.Name + " moved",
          InvolvedEntity = entity,
          MovePolicy = movePolicy
        });

        return true;
      }

      return false;
    }

    public bool CanHeroDoAction()
    {
      return inputManager.CanHeroDoAction();
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
      return new EnemiesManager(Context, EventsManager, Container, AlliesManager, this);
    }

    public virtual void SetContext(AbstractGameLevel node, Hero hero, GameContextSwitchKind kind, Stairs stairs = null)
    {
      hero.Container = this.Container;

      LootGenerator.LevelIndex = node.Index;
      
      Context.Hero = hero;

      if (!node.Inited)
        InitNode(node, gameState, kind);

      Context.SwitchTo(node, hero, gameState, kind, AlliesManager, stairs);

      if (kind == GameContextSwitchKind.NewGame)
      {
        gameState.HeroInitGamePosition = hero.point;
        var allEnemies = node.GetTiles<Enemy>();
        var chemps = allEnemies.Where(i => i.PowerKind == EnemyPowerKind.Champion).ToList();
        var plains = allEnemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).ToList();
        foreach (var chemp in chemps)
        {
          var chempHerdMembers = GetHerdMembers(chemp, allEnemies);
          foreach (var chempHerdMember in chempHerdMembers)
          {
            chempHerdMember.Herd = chemp.Herd;
          }
          
        }
      }

      var persister = Container.GetInstance<IPersister>();

      PrintHeroStats("SetContext " + kind);
    }

    public virtual void HandleDeath(LivingEntity dead)
    {
      GameState.History.LivingEntity.AddItem(new Roguelike.History.LivingEntityHistoryItem(dead));
      if (dead is Ally ally)
      {
        foreach (var item in ally.Inventory.Items)
          AppendTile(item, CurrentNode.GetClosestEmpty(dead).point);

        foreach (var item in ally.CurrentEquipment.GetActiveEquipment().Values)
        {
          if (item != null)
            AppendTile(item, CurrentNode.GetClosestEmpty(dead).point);
        }
      }
    }

    public List<Enemy> GetHerdMembers(Enemy chemp, List<Enemy> allEnemies = null)
    {
      if(allEnemies == null)
        allEnemies = CurrentNode.GetTiles<Enemy>();
      var plains = allEnemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).ToList();
      var chempHerdMembers = plains.Where(i => i.DistanceFrom(chemp) < 5).ToList();
      return chempHerdMembers;
    }

    protected virtual void InitNodeOnLoad(AbstractGameLevel node)
    {
      (node as TileContainers.GameLevel).OnLoadDone();
    }

    public Options GameSettings { get => Options.Instance; }

    protected virtual void InitNode(AbstractGameLevel node, GameState gs, GameContextSwitchKind context)
    {
      node.GetTiles<LivingEntity>().ForEach(i => i.Container = this.Container);
      node.Logger = this.Logger;
      if (context == GameContextSwitchKind.GameLoaded && !GameSettings.Mechanics.RegenerateLevelsOnLoad)
        InitNodeOnLoad(node);

      node.Inited = true;
    }

    public TileContainers.GameLevel LoadLevel(string heroName, int index)
    {
      var level = Persister.LoadLevel(heroName, index);
      InitNode(level, gameState, GameContextSwitchKind.GameLoaded);

      var heros = level.GetTiles<Hero>();
      if (heros.Any())
      {
        Logger.LogError("Hero saved in level! " + heros.First());//normally hero is saved in separate file
      }
      return level;
    }

    private void EventsManager_ActionAppended(object sender, GameEvent e)
    {
      OnActionAppended(e);
    }

    protected virtual void OnActionAppended(GameEvent e)
    {
      if (this != _debugCurrentInstance)
        throw new Exception("this != _debugCurrentInstance");
      //var pac = e as PolicyAppliedAction;
      //if (pac!=null)
      //{
      //  if (pac.Policy.Kind == PolicyKind.SpellCast)
      //  {
      //    var ap = pac.Policy as SpellCastPolicy;
      //  }
      //  return;
      //}

      var la = e as LootAction;
      if (la != null)
      {
        if (la.Kind == LootActionKind.Consumed)
        {
          if (la.LootOwner is Hero)//TODO
            Context.MoveToNextTurnOwner();
        }
        return;
      }


      var isLivingEntityAction = e is LivingEntityAction;
      if (!isLivingEntityAction)
      {
        var gsa = e as GameStateAction;
        if (gsa != null)
          Logger.LogInfo(gsa.Info);
        return;
      }
      else
      {
        var lea = e as LivingEntityAction;
        if (lea.Kind == LivingEntityActionKind.Died)
        {
          if (context.CurrentNode.HasTile(lea.InvolvedEntity))
          {
            context.CurrentNode.SetTile(context.CurrentNode.GenerateEmptyTile(), lea.InvolvedEntity.point);
          }
          else
          {
            Logger.LogError("context.CurrentNode HasTile failed for " + lea.InvolvedEntity);
          }
          if (lea.InvolvedEntity is Enemy enemy)
          {
            var exp = AdvancedLivingEntity.EnemyDamagingTotalExpAward[enemy.PowerKind] * enemy.Level / 10;
            Hero.IncreaseExp(exp);
            var allies = context.CurrentNode.GetActiveAllies();
            foreach (var al in allies)
              al.IncreaseExp(exp);

            context.CurrentNode.SetTile(new Tile(), lea.InvolvedEntity.point);
            var lootItems = lootManager.TryAddForLootSource(enemy);
            //Logger.LogInfo("Added loot count: "+ lootItems.Count + ", items: ");
            //lootItems.ForEach(i=> Logger.LogInfo("Added loot" + i));
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="lootSource">Barrel, Chest, Enemy...</param>
    /// <param name="animated"></param>
    /// <returns></returns>
    public bool AddLootReward(Loot item, ILootSource lootSource, bool animated)
    {
      if (item == null)
        return false;
      var res = AddLootToNode(item, lootSource as Tile, animated);
      if (res)
        item.Source = lootSource;
      return res;
    }

    public bool AddLootToNode(Loot item, Tile lootSource, bool animated)
    {
      Tile dest = null;
      var tileAtPos = context.CurrentNode.GetTile(lootSource.point);
      if (tileAtPos.IsEmpty)
        dest = tileAtPos;
      else
      {
        var empites = context.CurrentNode.GetEmptyNeighborhoodTiles(lootSource, true);
        if (empites.Any())
          dest = empites.First();
      }
      if (dest != null)
      {
        //Logger.LogInfo("AddLootToNode calling ReplaceTile" + item + ", pt: "+ dest.Point);
        var set = ReplaceTileTyped<Loot>(item, dest.point, animated, lootSource);
        return set;
      }

      Logger.LogError("AddLootReward no room! for a loot");
      return false;
    }

    public InteractionResult HandleHeroShift(TileNeighborhood neib)
    {
      return inputManager.HandleHeroShift(neib);
    }

    public InteractionResult HandleHeroShift(int horizontal, int vertical)
    {
      return inputManager.HandleHeroShift(horizontal, vertical);
    }

    public Func<bool> HeroMoveAllowed;

    public void SkipHeroTurn()
    {
      if (Context.HeroTurn)
        Context.MoveToNextTurnOwner();
    }

    public void RemoveDead()
    {
      EnemiesManager.RemoveDead();
      ReportHeroDeathIfNeeded();
    }

    private void ReportHeroDeathIfNeeded()
    {
      if (!Hero.Alive)//strike back of enemy could kill hero
        Context.ReportHeroDeath();
    }

    public T GetCurrentNode<T>() where T : AbstractGameLevel
    {
      return CurrentNode as T;
    }

    public virtual InteractionResult InteractHeroWith(Tile tile)
    {
      return inputManager.InteractHeroWith(tile);
    }

    public virtual InteractionResult HandlePortalCollision(Portal portal)
    {
      return InteractionResult.Blocked;
    }

    public virtual Loot TryGetRandomLootByDiceRoll(LootSourceKind lsk, ILootSource ls)
    {
      var loot = LootGenerator.TryGetRandomLootByDiceRoll(lsk, ls.Level, Hero.GetLootAbility());
      //if (loot is Equipment eq)
      //{
      //  EnsureMaterialFromLootSource(eq);
      //}
      return loot;
    }

    public ITilesAtPathProvider TilesAtPathProvider { get; set; }



    public virtual void OnHeroPolicyApplied(Policies.Policy policy)
    {
      SpellManager.OnHeroPolicyApplied(policy);
    }

    public void AppendAction(GameEvent ac)
    {
      this.EventsManager.AppendAction(ac);
    }

    public void AppendAction<T>(Action<T> init) where T : GameEvent, new()
    {
      var action = new T();
      if (init != null)
        init(action);

      this.EventsManager.AppendAction(action);
    }

    public bool AppendTileByScrollUsage<T>(T tile, Point pt) where T : Tile
    {
      bool appended = AppendTile(tile, pt);
      if (appended)
      {
        if (tile is Portal)
        {
          var portalScroll = Hero.Inventory.GetItems<Roguelike.Tiles.Looting.Scroll>().Where(i => i.Kind == Spells.SpellKind.Portal).FirstOrDefault();
          return Hero.Inventory.Remove(portalScroll);
        }
        else
          Assert(false, "AppendTileByScrollUsage unknown tile!");
      }
      return appended;
    }

    public bool AppendTile(Tile tile, Point point)
    {
      var prevTile = CurrentNode.ReplaceTile(tile, point);
      if (prevTile != null)//this normally shall always be not null
      {
        if (tile is Tiles.Interactive.InteractiveTile)
        {
          AppendAction<InteractiveTileAction>((InteractiveTileAction ac) =>
          {
            ac.InvolvedTile = tile as Tiles.Interactive.InteractiveTile;
            ac.InteractiveKind = InteractiveActionKind.AppendedToLevel;
          });
        }
        else if(tile is Loot loot)
        {
          AppendAction<LootAction>((LootAction ac) => 
          { 
            ac.Loot = loot; 
            ac.Kind = LootActionKind.Generated; 
            ac.GenerationAnimated = false; 
            ac.Source = null; 
          });
        }
        else
          Assert(false, "AppendTile unknown tile!");
        return true;
      }

      return false;
    }

    public bool ReplaceTile(Tile replacer, Point point, bool animated, Tile positionSource, AbstractGameLevel level = null)
    {
      //Assert(loot is Loot || loot.IsEmpty, "ReplaceTileByLoot failed");
      var node = level ?? CurrentNode;
      var prevTile = node.ReplaceTile(replacer, point);
      if (prevTile != null)//this normally shall always be not null
      {
        var it = prevTile as Tiles.Interactive.InteractiveTile;
        if (it != null)//barrel could be destroyed
        {
          if (it == positionSource)
            AppendAction<InteractiveTileAction>((InteractiveTileAction ac) => { ac.InvolvedTile = it; ac.InteractiveKind = InteractiveActionKind.Destroyed; });
        }
        var loot = replacer as Loot;
        if (loot != null)
        {
          AppendAction<LootAction>((LootAction ac) => { ac.Loot = loot; ac.Kind = LootActionKind.Generated; ac.GenerationAnimated = animated; ac.Source = positionSource; });
          GameState.History.Looting.GeneratedLoot.Add(new LootHistoryItem(loot));
        }
        else
        {
          if (replacer != null)
          {
            var le = replacer as LivingEntity;
            if (le != null)
              AppendAction<LivingEntityAction>((LivingEntityAction ac) =>
              {
                ac.InvolvedEntity = le; ac.Kind = LivingEntityActionKind.AppendedToLevel;
                ac.Info = le.Name + " spawned";
              });
          }
        }
        return true;
      }
      Logger.LogError("ReplaceTile failed! prevTile == null");
      return false;
    }

    public bool ReplaceTile(Tile replacer, Point destPoint)
    {
      var toReplace = CurrentNode.GetTile(destPoint);
      return ReplaceTile<Tile>(replacer, toReplace);
    }

    public bool ReplaceTile(Tile replacer, Tile toReplace)
    {
      return ReplaceTile<Tile>(replacer, toReplace);
    }

    protected bool ReplaceTile<T>(T replacer, Tile toReplace) where T : Tile//T can be Loot, Enemy
    {
      return ReplaceTileTyped<T>(replacer, toReplace.point, false, toReplace);
    }

    protected bool ReplaceTileTyped<T>(T replacer, Point point, bool animated, Tile positionSource, AbstractGameLevel level = null) where T : Tile//T can be Loot, Enemy
    {
      return ReplaceTile(replacer, point, animated, positionSource, level);
    }

    public TileContainers.GameLevel GetCurrentDungeonLevel()
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
      if (GameSettings.Mechanics.RestoreHeroToSafePointAfterLoad)
      {

      }
      if (GameState.CoreInfo.PermanentDeath)
      {
        var persister = Container.GetInstance<IPersister>();
        persister.DeleteGame(heroName);
      }
      //Hero.RestoreState(gameState);

      //var inters =  this.CurrentNode.GetTiles<Roguelike.Tiles.InteractiveTile>();
      //foreach (var inter in inters)
      //{
      //  inter.ResetToDefaults();      
      //}
    }

    PersistancyWorker persistancyWorker = new PersistancyWorker();

    public void PrepareForSave()
    {
      this.Hero.PrepareForSave();
    }

    public virtual void Save()
    {
      persistancyWorker.Save(this, WorldSaver);
    }

    public virtual string GetPitDisplayName(string pitID)
    {
      return pitID;
    }

    public virtual string GetCurrentNodeName()
    {
      string name = "";
      var gs = PrepareGameStateForSave();
      name = gameState.HeroPath.GetDisplayName();
      return name;
    }

    public virtual GameState PrepareGameStateForSave()
    {
      gameState.CoreInfo.LastSaved = DateTime.Now;
      gameState.HeroPath.Pit = "";

      if (CurrentNode is TileContainers.GameLevel)
      {
        var gameLevel = CurrentNode as TileContainers.GameLevel;
        gameState.HeroPath.Pit = gameLevel.PitName;
        gameState.HeroPath.LevelIndex = gameLevel.Index;
      }

      return gameState;
    }

    public bool CollectLoot(Loot lootTile, bool fromDistance)
    {
      if (!Context.HeroTurn)
        return false;

      Inventory inv = Hero.Inventory;
      var gold = lootTile as Gold;

      if (lootTile is Recipe)
        inv = Hero.Crafting.Recipes.Inventory;
      if (gold != null || inv.Add(lootTile, new AddItemArg() { detailedKind = InventoryActionDetailedKind.Collected }))
      {
        //Hero.Inventory.Print(logger, "loot added");
        if (gold != null)
          Hero.Gold += gold.Count;
        CurrentNode.RemoveLoot(lootTile.point);
        EventsManager.AppendAction(new LootAction(lootTile, Hero) { Kind = LootActionKind.Collected, CollectedFromDistance = fromDistance });
        if (lootTile is Equipment)
        {
          var eq = lootTile as Equipment;
          Hero.HandleEquipmentFound(eq);
          PrintHeroStats("loot On");
        }
        OnLootCollected(lootTile);
        Context.MoveToNextTurnOwner();
        return true;
      }
      return false;
    }

    protected virtual void OnLootCollected(Loot lootTile)
    {
      
    }

    public bool CollectLootOnHeroPosition()
    {
      var lootTile = CurrentNode.GetLootTile(Hero.point);
      if (lootTile != null)
      {
        return CollectLoot(lootTile, false);
      }

      return false;
    }

    public void PrintHeroStats(string context, bool onlyNonZero = true)
    {
      Logger.LogInfo("PrintHeroStats " + context);
      //foreach (var stat in Hero.Stats.Stats.Values)
      //{
      //  //if(!onlyNonZero || stat.Value.TotalValue != 0)
      //  //  Logger.LogInfo(stat.Kind + ": " + stat.Value);
      //}
    }

    //public void DoAlliesTurn(bool skipHero = false)
    //{
    //  this.AlliesManager.MakeEntitiesMove(skipHero ? Hero : null);
    //  //DoEnemiesTurn();
    //}

    public virtual Equipment GenerateRandomEquipment(EquipmentKind kind)
    {
      return lootGenerator.GetRandomEquipment(kind, Hero.Level, Hero.GetLootAbility());
    }

    public void MakeGameTick()
    {
      if (context.PendingTurnOwnerApply)
      {
        EntitiesManager mgr = null;
        if (context.TurnOwner == TurnOwner.Allies)
        {
          mgr = AlliesManager;
          //logger.LogInfo("MakeGameTick call to AlliesManager.MoveHeroAllies");
          //context.PendingTurnOwnerApply = false;
          //AlliesManager.MoveHeroAllies();

        }
        else if (context.TurnOwner == TurnOwner.Enemies)
        {
          mgr = EnemiesManager;
          //logger.LogInfo("MakeGameTick call to EnemiesManager.MakeEntitiesMove");
          //context.PendingTurnOwnerApply = false;
          //EnemiesManager.MakeEntitiesMove();
        }
        else if (context.TurnOwner == TurnOwner.Animals)
        {
          mgr = animalsManager;
          //logger.LogInfo("MakeGameTick call to EnemiesManager.MakeEntitiesMove");
          //context.PendingTurnOwnerApply = false;
          //EnemiesManager.MakeEntitiesMove();
        }
        if (mgr != null)
          mgr.MakeTurn();
      }
    }

    public void SetGameState(GameState gameState)
    {
      this.gameState = gameState;
    }

    public Loot SellItem
    (
      Loot loot,
      IInventoryOwner src,
      IInventoryOwner dest,
      RemoveItemArg removeItemArg = null
    )
    {
      return SellItem(loot, src, src.Inventory, dest, dest.Inventory, removeItemArg);
    }

    public Loot SellItem
    (
      Loot loot,
      IInventoryOwner src,
      Inventory srcInv,
      IInventoryOwner dest,
      Inventory destInv,
      RemoveItemArg removeItemArg = null,
      AddItemArg addItemArg = null
    )
    {
      if (removeItemArg == null)
        removeItemArg = new RemoveItemArg();

      var detailedKind = InventoryActionDetailedKind.Unset;
      if (removeItemArg.DragDrop)
        detailedKind = InventoryActionDetailedKind.TradedDragDrop;
      if (addItemArg == null)
      {
        if (destInv is CurrentEquipment)
        {
          var eq = loot as Equipment;
          if (eq == null)
            return null;
          addItemArg = new CurrentEquipmentAddItemArg()
          { detailedKind = detailedKind, cek = eq.EquipmentKind.GetCurrentEquipmentKind(CurrentEquipmentPosition.Unset) };
        }
        else
          addItemArg = new AddItemArg() { detailedKind = detailedKind };
      }

      if (!destInv.CanAcceptItem(loot, addItemArg))
      {
        return null;
      }

      if (destInv is CurrentEquipment ce && 
         (destInv.Owner == srcInv.Owner || destInv.Owner is Ally) 
         && addItemArg is CurrentEquipmentAddItemArg ceAddArgs)
      {
        var eq = ce.GetActiveEquipment()[ceAddArgs.cek];
        if (eq != null)
        {
          if (!destInv.Owner.MoveEquipmentCurrent2Inv(eq, ceAddArgs.cek))
            return null;
        }
      }

      bool goldInvolved = GetGoldInvolvedOnSell(src, dest);

      var price = 0;
      if (goldInvolved)
      {
        price = src.GetPrice(loot);// (int)(loot.Price * srcInv.PriceFactor * stackedCount);

        if (dest.Gold < price)
        {
          logger.LogInfo("dest.Gold < loot.Price");
          SoundManager.PlayBeepSound();
          return null;
        }
      }
      if (!destInv.CanAddLoot(loot))
      {
        logger.LogInfo("!dest.Inventory.CanAddLoot(loot)");
        SoundManager.PlayBeepSound();
        return null;
      }

      var removed = srcInv.Remove(loot, removeItemArg);
      if (!removed)
      {
        logger.LogError("!removed");
        return null;
      }
      Loot sold = loot;
      if (loot.StackedInInventory)
      {
        sold = (loot as StackedLoot).Clone(removeItemArg.StackedCount);
      }

      bool added = destInv.Add(sold, addItemArg);
      if (!added)
      {
        srcInv.Add(loot);
        logger.LogError("!added");
        return null;
      }

      if (goldInvolved)
      {
        dest.Gold -= price * removeItemArg.StackedCount;
        src.Gold += price * removeItemArg.StackedCount;
        SoundManager.PlaySound("COINS_Rattle_04_mono");//coind_drop
      }
      return sold;
    }

    protected virtual bool GetGoldInvolvedOnSell(IInventoryOwner src, IInventoryOwner dest)
    {
      return src.GetGoldWhenSellingTo(dest);
    }

    private void AddLootToMerchantInv(Merchant merch, List<LootKind> lootKinds)
    {
      for (int numOfLootPerKind = 0; numOfLootPerKind < 2; numOfLootPerKind++)
      {
        foreach (var lootKind in lootKinds)
        {
          int levelIndex = Hero.Level;
          Loot loot = null;
          loot = lootGenerator.GetRandomLoot(lootKind, levelIndex);
          if (loot is Equipment)
            continue;//generated lower
          if (loot != null && !merch.Inventory.Items.Any(i => i.tag1 == loot.tag1))
          {
            loot.Revealed = true;
            //TODO use Items to avoid sound
            merch.Inventory.Items.Add(loot);
            if (loot is FightItem fi)
            {
              if (fi.FightItemKind == FightItemKind.PlainBolt ||
                fi.FightItemKind == FightItemKind.PlainArrow)
              {
                fi.Count = RandHelper.GetRandomInt(50) + 25;
              }
              else
                fi.Count = RandHelper.GetRandomInt(3) + 1;
            }
          }
        }
      }

      GenerateMerchantEq(merch, true);
      GenerateMerchantEq(merch, false);
    }

    private void GenerateMerchantEq(Merchant merch, bool magic)
    {
      var eqKinds = LootGenerator.GetEqKinds();
      eqKinds.Shuffle();
      int levelIndex = Hero.Level;
      bool rangeGen = false;
      foreach (var eqKind in eqKinds)
      {
        if (eqKind == EquipmentKind.Trophy || eqKind == EquipmentKind.Unset || eqKind == EquipmentKind.God)
          continue;

        var breakCount = 2;
        if (eqKind == EquipmentKind.Weapon)
          breakCount = 5;

        int count = 0;
        int attemptTries = 0;
        while (count < breakCount)
        {
          var eq = lootGenerator.GetRandomEquipment(eqKind, levelIndex, merch.GetLootAbility());
          if (eq != null && !merch.Inventory.Items.Any(i => i.tag1 == eq.tag1))
          {
            if (eq.Class == EquipmentClass.Unique)
              continue;
            eq.Revealed = true;
            if (magic)
              eq.MakeMagic();
            else
              eq.MakeEnchantable();

            merch.Inventory.Add(eq);
            var wpn = eq as Weapon;
            if (wpn != null && wpn.IsBowLike)
              rangeGen = true;
            count++;
          }
          else
            attemptTries++;

          if (attemptTries == 5)
            break;
        }
      }

      if (!rangeGen)
      {
        if (levelIndex <= 2)
        {
          var bow = lootGenerator.GetLootByTileName<Weapon>("crude_crossbow");
          if (bow.Damage == 0)
          {
            int kk = 0;
            kk++;
          }
          merch.Inventory.Add(bow);
        }
        else if (levelIndex <= 4)
          merch.Inventory.Add(lootGenerator.GetLootByTileName<Weapon>("solid_bow"));
        else if (levelIndex <= 6)
          merch.Inventory.Add(lootGenerator.GetLootByTileName<Weapon>("composite_bow"));
      }
    }

    protected virtual void PopulateMerchantInv(Merchant merch, int heroLevel)
    {
      merch.Inventory.Items.Clear();
      var lootKinds = Enum.GetValues(typeof(LootKind)).Cast<LootKind>()
        .Where(i => i != LootKind.Unset && i != LootKind.Other && i != LootKind.Seal && i != LootKind.SealPart 
                   && i != LootKind.Gold && i != LootKind.Book && i != LootKind.FightItem)
        .ToList();

      AddLootToMerchantInv(merch, lootKinds);

      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.Stone) { Count = 4 });
      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.ThrowingKnife) { Count = 4 });
      
      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.PlainArrow) { Count = RandHelper.GetRandomInt(50) + 25 });
      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.PlainBolt) { Count = RandHelper.GetRandomInt(50) + 25 });

      //for (int i = 0; i < 4; i++)
      {
        var loot = new MagicDust();
        loot.Revealed = true;
        loot.Count = Generators.GenerationInfo.MaxMerchantMagicDust;
        merch.Inventory.Items.Add(loot);
      }

      {
        var loot = new Hooch();
        loot.Revealed = true;
        loot.Count = Generators.GenerationInfo.MaxMerchantHooch;
        merch.Inventory.Items.Add(loot);
      }

      if(!merch.Inventory.Items.Any(i=>i is Book))
        merch.Inventory.Items.Add(lootGenerator.GetRandomLoot(LootKind.Book, 1));

   
      //int maxPotions = 4;
      //for (int numOfLootPerKind = 0; numOfLootPerKind < maxPotions; numOfLootPerKind++)
      //{
      //  var hp = new Potion();
      //  hp.SetKind(PotionKind.Health);
      //  hp.Revealed = true;
      //  merch.Inventory.Add(hp);

      //  var mp = new Potion();
      //  hp.SetKind(PotionKind.Mana);
      //  mp.Revealed = true;
      //  merch.Inventory.Add(mp);
      //}
    }

    List<ILootSource> lootSourcesWithDelayedEnemies = new List<ILootSource>();
    public void RegisterDelayedEnemy(ILootSource lootSource)
    {
      lootSourcesWithDelayedEnemies.Add(lootSource);
    }

    public bool HasDelayedEnemy(ILootSource lootSource)
    {
      return lootSourcesWithDelayedEnemies.Contains(lootSource);
    }

    public void AppendDelayedEnemy(ILootSource lootSource)
    {
      if (HasDelayedEnemy(lootSource))
      {
        lootSourcesWithDelayedEnemies.Remove(lootSource);
        AppendEnemy(lootSource);
      }
    }

    public void AppendEnemy(ILootSource lootSource)
    {
      var enemy = CurrentNode.SpawnEnemy(lootSource);
      AppendEnemy(enemy, lootSource.GetPoint(), lootSource is Barrel);
    }

    private void AppendEnemy(Enemy enemy, Point pt, bool replace)
    {
      enemy.Container = this.Container;
      if (replace)
        ReplaceTile(enemy, pt);
      else
      {
        var empty = CurrentNode.GetClosestEmpty(CurrentNode.GetTile(pt), true, false);
        ReplaceTile(enemy, empty.point);
        //CurrentNode.SetTile(enemy, empty.point);
      }
      EnemiesManager.AddEntity(enemy);
    }

    public void AppendTile(Tile tile, Point pt, int level)
    {
      if (tile is Enemy en)
        AppendEnemy(en, pt, level);
      else
        ReplaceTile(tile, pt);
    }

    public void AppendEnemy(Enemy enemy, Point pt, int level)
    {
      enemy.Level = level;
      AppendEnemy(enemy, pt, true);
    }

    public T AddAlly<T>() where T : class, IAlly
    {
      var ally = this.Container.GetInstance<T>();
      AddAlly(ally);
      return ally;
    }

    public void AddAlly(IAlly ally)
    {
      ally.Active = true;
      var le = ally as LivingEntity;
      le.Container = this.Container;
      le.Revealed = true;

      if (ally.TakeLevelFromCaster)
      {
        float lvl = Hero.Level * 0.8f;
        if (lvl < 1)
          lvl = 1;

        ally.SetLevel((int)lvl, GameState.CoreInfo.Difficulty);
      }
      AlliesManager.AddEntity(le);

      if (ally.Kind != AllyKind.Paladin)
      {
        var empty = CurrentNode.GetClosestEmpty(Hero, true, false);
        ReplaceTile<LivingEntity>(le, empty);
        le.PlayAllySpawnedSound();

        AppendAction(new AllyAction() { Info = le.Name + " has been added", InvolvedTile = ally, AllyActionKind = AllyActionKind.Created });
      }
    }

    public bool CanUseSpellSource(LivingEntity caster, SpellSource scroll)
    {
      if (scroll.Count <= 0)
      {
        logger.LogError("gm SpellSource.Count <= 0");
        return false;
      }

      return true;
    }

    public bool UtylizeSpellSource(LivingEntity caster, SpellSource spellSource)
    {
      if (!CanUseSpellSource(caster, spellSource))
      {
        return false;
      }

      if (spellSource is Scroll && caster is AdvancedLivingEntity advEnt)
        return advEnt.Inventory.Remove(spellSource);

      if (spellSource is WeaponSpellSource)
      {
        spellSource.Count--;
        var ave = caster as AdvancedLivingEntity;
        var wpn = ave.GetActiveWeapon();
        wpn.UpdateMagicWeaponDesc();
      }

      return true;
    }

    public bool IsAlly(LivingEntity le)
    {
      return this.AlliesManager.Contains(le);
    }

    public Loot GetBestLoot(EnemyPowerKind powerKind, int level, LootHistory lootHistory)
    {
      var loot = LootGenerator.GetBestLoot(powerKind, level, lootHistory, Hero.GetLootAbility());
      //if (loot is Equipment eq)
      //{
      //   EnsureMaterialFromLootSource(eq);
      //}
      return loot;
    }

    public static bool IsMaterialAware(Equipment eq)
    {
      return eq.IsMaterialAware();
    }


    public virtual void HandeTileHit(Tile tile)
    {
      var info = "";
      var hitBlocker = false;
      var chest = tile as Chest;
      var barrel = tile as Barrel;
      if (barrel != null && barrel.OutOfOrder)
      {
        AppendAction<HeroAction>((HeroAction ac) => { ac.Kind = HeroActionKind.HitLockedChest; ac.Info = "This element is out of order"; });
        SoundManager.PlayBeepSound();
        return;
      }

      if (tile is Wall || (chest != null) || (barrel != null))// && !chest.Closed))
      {
        hitBlocker = true;
        if (chest != null)
        {
          info = "Hero hit a ";
          if(chest.ChestVisualKind == ChestVisualKind.Chest)
            info += "chest";
          else if(chest.ChestVisualKind == ChestVisualKind.Grave)
            info += "gave";
        }
        else if (barrel != null)
          info = "Hero hit a barrel";
        else
          info = "Hero hit a wall";
      }

      if (hitBlocker)
        AppendAction<HeroAction>((HeroAction ac) => { ac.Kind = HeroActionKind.HitWall; ac.Info = info; });
      if (tile is Barrel || tile is Chest || tile is DeadBody)
      {
        var ls = tile as ILootSource;
        if (!GeneratesLoot(ls))
        {
          ReplaceTile(new Tile(), ls.GetPoint());
          return;
        }
        //var tr = new TimeTracker();
        this.LootManager.TryAddForLootSource(ls);
        //Logger.LogInfo("TimeTracker TryAddForLootSource: " + tr.TotalSeconds);
      }
    }

    protected virtual bool GeneratesLoot(ILootSource ls)
    {
      return true;
    }

    int GetAlliesCount<T>() where T : IAlly
    {
      return this.AlliesManager.AllEntities.Count(i => i is T);
    }

    public T TryAddAlly<T>() where T : class, IAlly
    {
      if (GetAlliesCount<T>() == 0)
      {
        return AddAlly<T>();
      }
      AppendAction(new GameInstructionAction() { Info = "Currently you can not have more allies" }); ;
      return default(T);
    }

    

    public bool UtylizeSpellSource(LivingEntity caster, SpellSource spellSource, ISpell spell)
    {
      try
      {
        if (spellSource.Kind == Spells.SpellKind.Skeleton)
        {
          if (GetAlliesCount<AlliedEnemy>() > 0)
          {
            ReportFailure("Currently you can not instantiate more skeletons");
            return false;
          }
        }

        if (spell.Utylized)
          throw new Exception("spell.Utylized!");

        if (spellSource is WeaponSpellSource wss)
        {
          if (wss.Count <= 0)
          {
            ReportFailure("Not enough charges to cast a spell");
            return false;
          }

        }
        else
        {
          if (caster.Stats.Mana < spell.ManaCost)
          {
            ReportFailure("Not enough mana to cast a spell");
            return false;
          }
          caster.ReduceMana(spell.ManaCost);
        }
                
        spell.Utylized = true;
        UtylizeSpellSource(caster, spellSource);
        
      }
      catch (Exception)
      {
        SoundManager.PlayBeepSound();
        throw;
      }

      return true;
    }

    public void SaveGameOptions()
    {
      Persister.SaveOptions(Options.Instance);
    }

    public bool TryApplyAttackPolicy(ProjectileFightItem fi, Tile pointedTile, Action<Tile> beforAttackHandler = null)
    {
      if (!CanHeroDoAction())
        return false;

      var hero = this.Hero;
      fi.Caller = this.Hero;

      if (fi.FightItemKind == FightItemKind.PlainBolt ||
          fi.FightItemKind == FightItemKind.PlainArrow)
      {
        var wpn = hero.GetActiveWeapon();
        if (wpn == null 
           || (wpn.Kind != Roguelike.Tiles.Weapon.WeaponKind.Crossbow && 
               wpn.Kind != Roguelike.Tiles.Weapon.WeaponKind.Bow))
        {
          AppendAction(new Roguelike.Events.GameEvent() { Info = "Proper weapon not equipped" });
          return false;
        }
      }

      var target = pointedTile;/// CurrentGameGrid.GetTileAt(pointedTile);
      var inReach = hero.IsTileInProjectileFightItemReach(fi, target);
      if (!inReach)
      {
        this.SoundManager.PlayBeepSound();
        AppendAction(new GameEvent() { Info = "Target out of range" });
        return false;
      }

      var destroyable = target;// as Roguelike.Tiles.Abstract.IDestroyable;
      if (destroyable != null)
      {
        if(beforAttackHandler!=null)
          beforAttackHandler(target);
        return ApplyAttackPolicy(hero, destroyable, fi);
      }
      
      SoundManager.PlayBeepSound();
      return false;
    }

    public bool ApplyAttackPolicy
    (
      LivingEntity caster,//hero, enemy, ally
      Tile target,
      ProjectileFightItem fi,
      Action<Policy> BeforeApply = null,
      Action<Policy> AfterApply = null
    )
    {
      if (fi.Count <= 0)
      {
        logger.LogError("gm fi.Count <= 0");
        return false;
      }

      caster.RemoveFightItem(fi);
      var destFi = fi.Clone(1) as ProjectileFightItem;

      return DoApply(caster, target, destFi, BeforeApply, AfterApply);
    }

    private bool DoApply(LivingEntity caster, Tile target, ProjectileFightItem fi, Action<Policy> BeforeApply, Action<Policy> AfterApply)
    {
      fi.Caller = caster;
      var policy = Container.GetInstance<ProjectileCastPolicy>();
      policy.Target = target;
      policy.ProjectilesFactory = Container.GetInstance<IProjectilesFactory>();
      policy.Projectile = fi;
      policy.Caster = caster;
      if (BeforeApply != null)
        BeforeApply(policy);

      policy.OnApplied += (s, e) =>
      {
        var le = policy.TargetDestroyable is LivingEntity;
        if (!le)//le is handled specially
        {
          this.LootManager.TryAddForLootSource(policy.Target as ILootSource);
        }
        if (caster is Hero)
          OnHeroPolicyApplied(policy);

        if (AfterApply != null)
          AfterApply(policy);
      };

      policy.Apply(caster);
      return true;
    }

    public void ReportFailure(string infoToDisplay)
    {
      SoundManager.PlayBeepSound();
      if(infoToDisplay.Any())
        AppendAction(new GameEvent() { Info = infoToDisplay, Level = ActionLevel.Important });
    }
  }
}
