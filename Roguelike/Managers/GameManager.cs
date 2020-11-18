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

    public GameManager(Container container)
    {
      Container = container;

      gameState = container.GetInstance<Roguelike.GameState>();
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

      enemiesManager = new EnemiesManager(Context, EventsManager, Container);
      AlliesManager = new AlliesManager(Context, EventsManager, Container);

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
      return new EnemiesManager(Context, EventsManager, Container);
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
      if (context == GameContextSwitchKind.GameLoaded)
        InitNodeOnLoad(node);
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
          if (lea.InvolvedEntity is Enemy)
          {
            Hero.IncreaseExp(10);
            //var loot = LootGenerator.GetRandomLoot();
            context.CurrentNode.SetTile(new Tile(), lea.InvolvedEntity.Point);
            var enemy = lea.InvolvedEntity as Enemy;
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
      return AddLootToNode(item, lootSource as Tile, animated);
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
        var set = ReplaceTile<Loot>(item, dest.Point, animated, lootSource);
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

    private void RemoveDeadEnemies()
    {
      EnemiesManager.RemoveDead();
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

    private void HandeTileHit(Tile tile)
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
      return LootGenerator.TryGetRandomLootByDiceRoll(lsk, level);
    }

    public virtual void OnHeroPolicyApplied(Policies.Policy policy)
    {
      if (policy.Kind == PolicyKind.Move)
      {

      }
      else if (policy.Kind == PolicyKind.Attack)
      {
        HandlePolicyApplied(policy);
      }
      if (policy is AttackPolicy || policy is SpellCastPolicy)
        RemoveDeadEnemies();
      context.IncreaseActions(TurnOwner.Hero);

      //  Logger.LogInfo("OnHeroPolicyApplied MoveToNextTurnOwner");
      context.MoveToNextTurnOwner();
    }

    public void OnHeroPolicyApplied(object sender, Policies.Policy policy)
    {
      OnHeroPolicyApplied(policy);
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
        if (tile is Roguelike.Tiles.InteractiveTile)
        {
          AppendAction<InteractiveTileAction>((InteractiveTileAction ac) =>
          {
            ac.InvolvedTile = tile as Roguelike.Tiles.InteractiveTile;
            ac.InteractiveKind = InteractiveActionKind.AppendedToLevel;
          });
        }
        else
          Assert(false, "AppendTile unknown tile!");
        return true;
      }

      return false;
    }

    public bool ReplaceTile<T>(T replacer, Point point, bool animated, Tile positionSource, AbstractGameLevel level = null) where T : Tile//T can be Loot, Enemy
    {
      //Assert(loot is Loot || loot.IsEmpty, "ReplaceTileByLoot failed");
      var node = level ?? CurrentNode;
      var prevTile = node.ReplaceTile(replacer, point);
      if (prevTile != null)//this normally shall always be not null
      {
        var it = prevTile as Tiles.InteractiveTile;
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
            var enemy = replacer as Enemy;
            if (enemy != null)
              AppendAction<EnemyAction>((EnemyAction ac) => { ac.Enemy = enemy; ac.Kind = EnemyActionKind.AppendedToLevel; ac.Info = enemy.Name + " spawned"; });
          }
        }
        return true;
      }
      Logger.LogError("ReplaceTile failed! prevTile == null");
      return false;
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
      name = GetPitDisplayName(gameState.HeroPathValue.Pit) + "/" + (gameState.HeroPathValue.LevelIndex+1);
      return name;
    }

    public virtual GameState PrepareGameStateForSave()
    {
      gameState.Settings.CoreInfo.LastSaved = DateTime.Now;
      gameState.HeroPathValue.Pit = "";

      if (CurrentNode is TileContainers.GameLevel)//TODO 
      {
        var gameLevel = CurrentNode as TileContainers.GameLevel;
        gameState.HeroPathValue.Pit = gameLevel.PitName;
        gameState.HeroPathValue.LevelIndex = gameLevel.Index;
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
      return lootGenerator.GetRandomEquipment(kind, Hero.Level);
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
      AdvancedLivingEntity src,
      InventoryBase srcInv,
      AdvancedLivingEntity dest,
      InventoryBase destInv,
      bool dragDrop = false,
      int stackedCount = 1//in case of stacked there can be one than more sold at time
    )
    {
      bool goldInvolved = src != dest;

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
        dest.Gold -= price* stackedCount;
        src.Gold += price* stackedCount;
        SoundManager.PlaySound("COINS_Rattle_04_mono");//coind_drop
      }
      return sold;
    }

    private void AddEqToMerchant(Merchant merch, Array lootKinds)//GameLevel level
    {
      for (int numOfLootPerKind = 0; numOfLootPerKind < 2; numOfLootPerKind++)
      {
        foreach (EquipmentKind lk in lootKinds)
        {
          if (lk == EquipmentKind.Trophy || lk == EquipmentKind.Unset || lk == EquipmentKind.God)
            continue;

          int levelIndex = Hero.Level;
          var loot = lootGenerator.GetRandomEquipment(lk, levelIndex);
          if (loot != null && !merch.Inventory.Items.Any(i => i.tag1 == loot.tag1))
          {
            loot.Revealed = true;
            merch.Inventory.Add(loot);
          }
        }
      }


      //var eqs = merch.Inventory.Items.Where(i => i is Equipment).Cast<Equipment>().ToList();
      //if (!eqs.Where(i => i.Enchantable).Any())
      //{
      //  var eq = CommonRandHelper.GetRandomElem<Equipment>(eqs);
      //  eq.MakeEnchantable();
      //}
    }

    protected void PopulateMerchantInv(Merchant merch)
    {
      var lootKinds = Enum.GetValues(typeof(LootKind));

      AddEqToMerchant(merch, lootKinds);

      //TODO
      //if (level.LevelIndex > 1)
      //{
      //  var loot = gm.LootManager.GenerateRecipe(null);
      //  loot.Revealed = true;
      //  merch.Inventory.Add(loot);
      //}

      //if (level.LevelIndex > 0)
      //{
      //  for (int i = 0; i < 4; i++)
      //  {
      //    var loot = new MagicDust();
      //    loot.Revealed = true;
      //    merch.Inventory.Add(loot);
      //  }
      //  merch.Inventory.Add(new Hooch() { Revealed = true });
      //}

      int magicCount = 0;
      int tries = 0;
      while (magicCount < 2 && tries < 50)//TODO
      {
        var loot = RandHelper.GetRandomElem<Loot>(merch.Inventory.Items) as Equipment;
        if (loot != null && loot.Class == EquipmentClass.Plain)
        {
          loot.SetClass(EquipmentClass.Magic, Hero.Level, null, magicCount == 0);
          magicCount++;
        }
        tries++;
      }

      //TODO scrolls

      int maxPotions = 4;
      for (int numOfLootPerKind = 0; numOfLootPerKind < maxPotions; numOfLootPerKind++)
      {
        var hp = new Potion();
        hp.SetKind(PotionKind.Health);
        hp.Revealed = true;
        merch.Inventory.Add(hp);

        var mp = new Potion();
        hp.SetKind(PotionKind.Mana);
        mp.Revealed = true;
        merch.Inventory.Add(mp);
      }
    }
  }
}
