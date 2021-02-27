using Dungeons.Core;
using Dungeons.Tiles;
using System;
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
using Roguelike.LootContainers;
using Roguelike.Tiles.Looting;
using System.Collections.Generic;
using Roguelike.Spells;
using Roguelike.History;
using Roguelike.State;
using Roguelike.LootFactories;
using Roguelike.Attributes;

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
    LevelGenerator levelGenerator;
    LootManager lootManager;
    protected InputManager inputManager;

    IPersister persister;
    ILogger logger;

    public EnemiesManager EnemiesManager { get => enemiesManager; set => enemiesManager = value; }
    public Hero Hero { get => Context.Hero; }
    protected GameState gameState;

    public bool HeroTurn { get => Context.HeroTurn; }

    public virtual void SetLoadedContext(AbstractGameLevel node, Hero hero)
    {
      SetContext(node, hero, GameContextSwitchKind.GameLoaded);
    }

    [JsonIgnore]
    public EventsManager EventsManager { get => eventsManager; set => eventsManager = value; }

    //TODO shall be Jsonignored
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

      enemiesManager = new EnemiesManager(Context, EventsManager, Container, null);
      AlliesManager = new AlliesManager(Context, EventsManager, Container, enemiesManager);
      enemiesManager.AlliesManager = AlliesManager;

      Persister = container.GetInstance<IPersister>();

      SoundManager = new SoundManager(this, container);
    }

    protected virtual void CreateInputManager()
    {
      inputManager = new InputManager(this);
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
      return new EnemiesManager(Context, EventsManager, Container, AlliesManager);
    }

    public virtual void SetContext(AbstractGameLevel node, Hero hero, GameContextSwitchKind kind, Stairs stairs = null)
    {
      hero.Container = this.Container;
            
      LootGenerator.LevelIndex = node.Index;//TODO
      //if (kind == GameContextSwitchKind.NewGame)
      //{
      //  if (node.GeneratorNodes != null && node.GeneratorNodes.Any())
      //    hero.DungeonNodeIndex = node.GeneratorNodes.First().NodeIndex;//TODOs
      //  else
      //    hero.DungeonNodeIndex = 0;//TODO
      //}

      Context.Hero = hero;

      if(!node.Inited)
        InitNode(node, gameState, kind);

      Context.SwitchTo(node, hero, gameState, kind, stairs);

      if (kind == GameContextSwitchKind.NewGame)
      {
        gameState.HeroInitGamePosition = hero.Point;
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
        Logger.LogError("Hero saved in level! "+ heros.First());//normally hero is saved in separate file
      }
      return level;
    }

    private void EventsManager_ActionAppended(object sender, GameAction e)
    {
      OnActionAppended(e);
    }

    protected virtual void OnActionAppended(GameAction e)
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
        if (la.LootActionKind == LootActionKind.Consumed)
        {
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
            context.CurrentNode.SetTile(context.CurrentNode.GenerateEmptyTile(), lea.InvolvedEntity.Point);
          }
          else
          {
            Logger.LogError("context.CurrentNode HasTile failed for " + lea.InvolvedEntity);
          }
          if (lea.InvolvedEntity is Enemy enemy)
          {
            //TODO based on enemy level, corelate iwth nextExp
            var exp = 10;
            if (enemy.PowerKind == EnemyPowerKind.Boss)
              exp = 100;
            else if (enemy.PowerKind == EnemyPowerKind.Champion)
              exp = 50;
            Hero.IncreaseExp(exp);
            //var loot = LootGenerator.GetRandomLoot();
            context.CurrentNode.SetTile(new Tile(), lea.InvolvedEntity.Point);
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
      if(res)
        item.Source = lootSource;
      return res;
    }

    public bool AddLootToNode(Loot item, Tile lootSource, bool animated)
    {
      Tile dest = null;
      var tileAtPos = context.CurrentNode.GetTile(lootSource.Point);
      if (tileAtPos.IsEmpty)
        dest = tileAtPos;
      else
        dest = context.CurrentNode.GetClosestEmpty(lootSource, true);
      if (dest != null)
      {
        //Logger.LogInfo("AddLootToNode calling ReplaceTile" + item + ", pt: "+ dest.Point);
        var set = ReplaceTileTyped<Loot>(item, dest.Point, animated, lootSource);
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

    private void RemoveDead()
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

    void HandlePolicyApplied(Policy policy)
    {
      var attackPolicy = policy as AttackPolicy;
      //var chest = attackPolicy.Victim as Chest;
      //if (chest != null)
      //  return;
      HandeTileHit(attackPolicy.Victim);
    }

    bool islandEnemiesAdded = false;
    private void AppendIslandEnemies(Chest chest)
    {
      if (chest.OriginMap == "Island_Interactive" && chest.ChestKind == Roguelike.Tiles.Interactive.ChestKind.Gold)
      {
        if (CurrentNode.HiddenTiles.Any())
        {
          islandEnemiesAdded = true;
          int counter = 0;
          foreach (var tile in CurrentNode.HiddenTiles)
          {
            var en = tile as Roguelike.Tiles.Enemy;
            if (counter == 0)
            {
              var key = new Key();

              key.Kind = KeyKind.Chest;
              key.KeyName = chest.KeyName;
              en.DeathLoot = key;
              en.SetChampion();
              
            }
            AppendEnemy(en, tile.Point, chest.Level+1);
            counter++;
          }
        }
      }
    }

    protected virtual void HandeTileHit(Tile tile)
    {
      var info = "";
      var hitBlocker = false;
      var chest = tile as Chest;
      var barrel = tile as Barrel;
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

      if (chest != null && chest.Closed)
      {
        if (chest.Locked)
        {
          var key = Hero.Inventory.GetItems<Key>().Where(i => i.KeyName == chest.KeyName).FirstOrDefault();
          if (key == null)
          {
            AppendAction<HeroAction>((HeroAction ac) => { ac.Kind = HeroActionKind.HitLockedChest; ac.Info = "Chest is locked, a key is needed to open it."; });
            if (!islandEnemiesAdded)
              AppendIslandEnemies(chest);
            return;
          }
          else
          {
            chest.Locked = false;
          }
        }
      }
      
      if (hitBlocker)
        AppendAction<HeroAction>((HeroAction ac) => { ac.Kind = HeroActionKind.HitWall; ac.Info = info; });
      if (tile is Barrel || tile is Chest)
      {
        var tr = new TimeTracker();
        this.lootManager.TryAddForLootSource(tile as ILootSource);
        Logger.LogInfo("TimeTracker TryAddForLootSource: "+ tr.TotalSeconds);
      }
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

    public virtual void OnHeroPolicyApplied(Policies.Policy policy)
    {
      if (policy.Kind == PolicyKind.Move)
      {

      }
      else if (policy.Kind == PolicyKind.Attack)
      {
        HandlePolicyApplied(policy);
        if (HeroBulkAttackTargets == null)
        {
          var ap = policy as AttackPolicy;
          var en = ap.Victim as Enemy;
          if (en != null)
          {
            FindBulkAttackTargets(ap.Victim as Enemy);
            if (HeroBulkAttackTargets.Any())
            {
              var target = HeroBulkAttackTargets.GetRandomElem();
              HeroBulkAttackTargets.Remove(target);
              context.ApplyPhysicalAttackPolicy(Hero, target, (p)=> { });
            }
          }
        }
      }
      if (policy is AttackPolicy || policy is SpellCastPolicy)
        RemoveDead();
      context.IncreaseActions(TurnOwner.Hero);

      //  Logger.LogInfo("OnHeroPolicyApplied MoveToNextTurnOwner");
      HeroBulkAttackTargets = null;
      context.MoveToNextTurnOwner();
    }

    public List<Enemy> HeroBulkAttackTargets { get; set; }

    void FindBulkAttackTargets(Enemy lastTarget)
    {
      HeroBulkAttackTargets = new List<Enemy>();
      var hero = Hero;
      var sb = hero.GetTotalValue(EntityStatKind.ChanceToBulkAttack);
      //sb = 100.0f;
      if (sb > 0)
      {
        var fac = sb;// sb.GetFactor(true);
        if (fac / 100f > RandHelper.GetRandomDouble())
        {
          HeroBulkAttackTargets = CurrentNode.GetNeighborTiles<Enemy>(hero)
          .Where(i => i != lastTarget && !i.HeroAlly)
          .ToList();

          if (HeroBulkAttackTargets.Any())
            AppendAction(new LivingEntityAction(LivingEntityActionKind.BulkAttack)
            { Info = hero.Name + " used ability Bulk Attack", Level = ActionLevel.Important , InvolvedEntity = hero });
        }
      }
    }

    public void OnHeroPolicyApplied(object sender, Policies.Policy policy)
    {
      OnHeroPolicyApplied(policy);
    }

    public void AppendAction(GameAction ac)
    {
      this.EventsManager.AppendAction(ac);
    }

    public void AppendAction<T>(Action<T> init) where T : GameAction, new()
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
          AppendAction<LootAction>((LootAction ac) => { ac.Loot = loot; ac.LootActionKind = LootActionKind.Generated; ac.GenerationAnimated = animated; ac.Source = positionSource; });
          GameState.History.Looting.GeneratedLoot.Add(new LootHistoryItem(loot));
        }
        else
        {
          if (replacer != null)
          {
            var le = replacer as LivingEntity;
            if (le != null)
              AppendAction<LivingEntityAction>((LivingEntityAction ac) => { ac.InvolvedEntity = le; ac.Kind = LivingEntityActionKind.AppendedToLevel; 
                ac.Info = le.Name + " spawned"; });
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
      return ReplaceTileTyped<T>(replacer, toReplace.Point, false, toReplace);
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

      if (CurrentNode is TileContainers.GameLevel)//TODO 
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

      InventoryBase inv = Hero.Inventory;
      var gold = lootTile as Gold;
      
      if (lootTile is Recipe)
        inv = Hero.Crafting.Recipes;
      if (gold !=null || inv.Add(lootTile, detailedKind: InventoryActionDetailedKind.Collected))
      {
        //Hero.Inventory.Print(logger, "loot added");
        if(gold != null)
          Hero.Gold += gold.Count;
        CurrentNode.RemoveLoot(lootTile.Point);
        EventsManager.AppendAction(new LootAction(lootTile) { LootActionKind = LootActionKind.Collected, CollectedFromDistance = fromDistance });
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
      var lootTile = CurrentNode.GetLootTile(Hero.Point);
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
        if(mgr!=null)
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
      AdvancedLivingEntity src,
      AdvancedLivingEntity dest,
      bool dragDrop = false,
      int stackedCount = 1//in case of stacked there can be one than more sold at time
    )
    {
      return SellItem(loot, src, src.Inventory, dest, dest.Inventory, dragDrop, stackedCount);
    }

    public Loot SellItem
    (
      Loot loot,
      Roguelike.Abstract.IAdvancedEntity src,
      InventoryBase srcInv,
      Roguelike.Abstract.IAdvancedEntity dest,
      InventoryBase destInv,
      bool dragDrop = false,
      int stackedCount = 1//in case of stacked there can be one than more sold at time
    )
    {
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

      var removed = srcInv.Remove(loot, stackedCount);
      if (!removed)
      {
        logger.LogError("!removed");
        return null;
      }
      Loot sold = loot;
      if (loot.StackedInInventory)
      {
        sold = (loot as StackedLoot).Clone(stackedCount);
      }

      var detailedKind = InventoryActionDetailedKind.Unset;
      if (dragDrop)
        detailedKind = InventoryActionDetailedKind.TradedDragDrop;
      bool added = destInv.Add(sold, detailedKind: detailedKind);
      if (!added)//TODO revert item to src inv
      {
        logger.LogError("!added");
        return null;
      }

      if (goldInvolved)
      {
        dest.Gold -= price * stackedCount;
        src.Gold += price * stackedCount;
        SoundManager.PlaySound("COINS_Rattle_04_mono");//coind_drop
      }
      return sold;
    }

    protected virtual bool GetGoldInvolvedOnSell(Abstract.IAdvancedEntity src, Abstract.IAdvancedEntity dest)
    {
      return src != dest;
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
      var eqKinds = Enum.GetValues(typeof(EquipmentKind)).Cast<EquipmentKind>().ToList();
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

            //TODO Items used to avoid sound
            merch.Inventory.Items.Add(eq);
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
        //TODO
        loot.Count = 4;
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
      enemy.Container = this.Container;
      ReplaceTile(enemy, lootSource as Tile);
      EnemiesManager.AddEntity(enemy);
    }

    public void AppendEnemy(Enemy enemy, Point pt, int level)
    {
      enemy.Level = level;
      enemy.Container = this.Container;
      ReplaceTile(enemy, pt);
      EnemiesManager.AddEntity(enemy);
    }

    public void AddAlly<T>() where T : LivingEntity, new()
    {
      var ally = new T();
      ally.Revealed = true;
      AddAlly(ally);
    }

    public void AddAlly(LivingEntity le)
    {
      le.Container = this.Container;
      AlliesManager.AddEntity(le);

      var empty = CurrentNode.GetClosestEmpty(Hero, true, false);
      ReplaceTile<LivingEntity>(le, empty);
      le.PlayAllySpawnedSound();
    }

    public OffensiveSpell ApplyOffensiveSpell(LivingEntity caster, Scroll scroll)
    {
      if (!context.UtylizeScroll(caster, scroll))
        return null;

      if (scroll.CreateSpell(caster) is OffensiveSpell ps)
      {
        if (ps is SkeletonSpell skeletonSpell)
        {
          AddAlly(skeletonSpell.Enemy);
        }
        if (caster is Hero)
          context.MoveToNextTurnOwner();

        return ps;
      }
      else
        logger.LogError("!OffensiveSpell " + scroll);

      return null;
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

    //TODO move it somewhere
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


    public PassiveSpell ApplyPassiveSpell(LivingEntity caster, Scroll scroll, Point? destPoint = null)
    {
      if (!context.CanUseScroll(caster, scroll))
        return null;
           
      if (scroll.CreateSpell(caster) is PassiveSpell ps)
      {
        if (ps.Kind == SpellKind.Teleport)
        {
          if (destPoint != null)
          {
            var currentTile = CurrentNode.GetTile(destPoint.Value);
            var teleportSpell = ps as TeleportSpell;
            if (teleportSpell.Range < Hero.DistanceFrom(currentTile))
            {
              SoundManager.PlayBeepSound();
              EventsManager.AppendAction(new Events.GameInstructionAction() { Info = "Range of spell is too small (max:"+ teleportSpell.Range + ")" });
              return null;
            }

            if (currentTile.IsEmpty || currentTile is Loot)
              CurrentNode.SetTile(Hero, destPoint.Value);
            else
            {
              SoundManager.PlayBeepSound();
              return null;
            }
          }
        }
        else
          caster.ApplyPassiveSpell(ps);

        context.UtylizeScroll(caster, scroll);
        AppendAction<LivingEntityAction>((LivingEntityAction ac) => 
        { ac.Kind = LivingEntityActionKind.Teleported; ac.Info = Hero.Name+" used " + scroll.Kind.ToDescription() + " scroll"; ac.InvolvedEntity = caster; });

        if (caster is Hero)
          context.MoveToNextTurnOwner();

        return ps;
      }
      else
        logger.LogError("!PassiveSpell " + scroll);

      return null;
    }

    public bool IsAlly(LivingEntity le)
    {
      return this.AlliesManager.Contains(le);
    }

    public Loot GetBestLoot(EnemyPowerKind powerKind, int level, LootHistory lootHistory)
    {
      var loot = LootGenerator.GetBestLoot(powerKind, level, lootHistory, Hero.GetAbility(Abilities.AbilityKind.LootingMastering) as Abilities.LootAbility);
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

    
  }
}
