using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract.Inventory;
using Roguelike.Abstract.Spells;
using Roguelike.Abstract.Tiles;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Generators;
using Roguelike.History;
using Roguelike.LootContainers;
using Roguelike.Policies;
using Roguelike.Serialization;
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

    public GameManager(Container container)
    {
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
      }

      PrintHeroStats("SetContext " + kind);
    }

    protected virtual void InitNodeOnLoad(AbstractGameLevel node)
    {
      (node as TileContainers.GameLevel).OnLoadDone();
    }

    protected virtual void InitNode(AbstractGameLevel node, GameState gs, GameContextSwitchKind context)
    {
      node.GetTiles<LivingEntity>().ForEach(i => i.Container = this.Container);
      node.Logger = this.Logger;
      if (context == GameContextSwitchKind.GameLoaded && !gs.Settings.CoreInfo.RegenerateLevelsOnLoad)
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

    public virtual Loot TryGetRandomLootByDiceRoll(LootSourceKind lsk, int level)
    {
      var loot = LootGenerator.TryGetRandomLootByDiceRoll(lsk, level, Hero.GetLootAbility());
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
      if (gameState.Settings.CoreInfo.RestoreHeroToSafePointAfterLoad)
      {

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
      gameState.Settings.CoreInfo.LastSaved = DateTime.Now;
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
        Context.MoveToNextTurnOwner();
        return true;
      }
      return false;
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

      if (destInv is CurrentEquipment ce && destInv.Owner == srcInv.Owner && addItemArg is CurrentEquipmentAddItemArg ceAddArgs)
      {
        var eq = ce.GetActiveEquipment()[ceAddArgs.cek];
        if (eq != null)
        {
          if (!srcInv.Owner.MoveEquipmentCurrent2Inv(eq, ceAddArgs.cek))
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
      foreach (var eqKind in eqKinds)
      {
        if (eqKind == EquipmentKind.Trophy || eqKind == EquipmentKind.Unset || eqKind == EquipmentKind.God)
          continue;

        var breakCount = 2;
        if (eqKind == EquipmentKind.Weapon)
          breakCount = 3;

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
            count++;
          }
          else
            attemptTries++;

          if (attemptTries == 5)
            break;
        }
      }
    }

    protected void PopulateMerchantInv(Merchant merch, int heroLevel)
    {
      merch.Inventory.Items.Clear();
      var lootKinds = Enum.GetValues(typeof(LootKind)).Cast<LootKind>()
        .Where(i => i != LootKind.Unset && i != LootKind.Other && i != LootKind.Seal && i != LootKind.SealPart && i != LootKind.Gold)
        .ToList();

      AddLootToMerchantInv(merch, lootKinds);

      //for (int i = 0; i < 4; i++)
      {
        var loot = new MagicDust();
        loot.Revealed = true;
        loot.Count = Generators.GenerationInfo.MaxMerchantMagicDust;
        merch.Inventory.Items.Add(loot);
      }

      //merch.Inventory.Add(new Hooch() { Revealed = true });

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

    public void AppendEnemy(ILootSource lootSource)
    {
      var enemy = CurrentNode.SpawnEnemy(lootSource);
      AppendEnemy(enemy, lootSource.GetPoint());
    }

    private void AppendEnemy(Enemy enemy, Point pt)
    {
      enemy.Container = this.Container;
      ReplaceTile(enemy, pt);
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
      AppendEnemy(enemy, pt);
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

      if (ally.TakeLevelFromCaster)// is TrainedHound)//TODO
      {
        var lvl = Hero.Level / 2;
        if (lvl == 0)
          lvl = 1;
        ally.SetLevel(lvl);
      }
      AlliesManager.AddEntity(le);

      var empty = CurrentNode.GetClosestEmpty(Hero, true, false);
      ReplaceTile<LivingEntity>(le, empty);
      le.PlayAllySpawnedSound();

      AppendAction(new AllyAction() { Info = le.Name + " has been added", InvolvedTile = ally, AllyActionKind = AllyActionKind.Created });
    }

    public bool CanUseScroll(LivingEntity caster, Scroll scroll)
    {
      if (scroll.Count <= 0)
      {
        logger.LogError("scroll.Count <= 0");
        return false;
      }

      return true;
    }

    public bool UtylizeScroll(LivingEntity caster, Scroll scroll)
    {
      if (!CanUseScroll(caster, scroll))
      {
        return false;
      }

      if (caster is AdvancedLivingEntity advEnt)
        return advEnt.Inventory.Remove(scroll);

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
          info = "Hero hit a chest";
        if (barrel != null)
          info = "Hero hit a barrel";
        else
          info = "Hero hit a wall";
      }

      if (hitBlocker)
        AppendAction<HeroAction>((HeroAction ac) => { ac.Kind = HeroActionKind.HitWall; ac.Info = info; });
      if (tile is Barrel || tile is Chest)
      {
        var ls = tile as ILootSource;
        if (!GeneratesLoot(ls))
        {
          ReplaceTile(new Tile(), ls.GetPoint());
          return;
        }
        var tr = new TimeTracker();
        this.LootManager.TryAddForLootSource(ls);
        Logger.LogInfo("TimeTracker TryAddForLootSource: " + tr.TotalSeconds);
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

    public bool UtylizeScroll(LivingEntity caster, Scroll scroll, ISpell spell)
    {
      //scroll was already used why check here?
      //if (!CanUseScroll(caster, scroll, spell))
      //{
      //  return false;
      //}
      try
      {
        if (scroll.Kind == Spells.SpellKind.Skeleton)
        {
          if (GetAlliesCount<AlliedEnemy>() > 0)
          {
            AppendAction(new GameInstructionAction() { Info = "Currently you can not instantiate more skeletons" }); ;
            return false;
          }
        }

        if (spell.Utylized)
          throw new Exception("spell.Utylized!");

        if (caster.Stats.Mana < spell.ManaCost)
        {
          SoundManager.PlayBeepSound();
          AppendAction(new GameEvent() { Info = "Not enough mana to cast a spell" });
          return false;
        }
        caster.ReduceMana(spell.ManaCost);
        spell.Utylized = true;
        if (caster is AdvancedLivingEntity advEnt)
          return advEnt.Inventory.Remove(scroll);
      }
      catch (Exception)
      {
        SoundManager.PlayBeepSound();
        throw;
      }

      return true;
    }
  }
}
