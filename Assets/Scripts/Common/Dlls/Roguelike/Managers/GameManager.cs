﻿using Dungeons;
using Dungeons.Core;
using Dungeons.Fight;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Roguelike.Abilities;
using Roguelike.Abstract.Inventory;
using Roguelike.Abstract.Projectiles;
using Roguelike.Abstract.Spells;
using Roguelike.Abstract.Tiles;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Core.Managers;
using Roguelike.Managers.Policies;
using Roguelike.Effects;
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
using System.Security.Cryptography;
using Roguelike.Spells;
using Dungeons.Core.Policy;
using Dungeons.Tiles.Abstract;

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

  class FakeTarget : Tile, Tiles.Abstract.IObstacle
  {
    public Point Position => this.point;

    public HitResult OnHitBy(Dungeons.Tiles.Abstract.IProjectile proj, IPolicy policy)
    {
      return HitResult.Hit;
    }

    public bool CanBeHitBySpell()
    {
      return false;
    }

    public HitResult OnHitBy(Dungeons.Tiles.Abstract.IDamagingSpell md, IPolicy policy)
    {
      return HitResult.Hit;
    }

    public void PlayHitSound(Dungeons.Tiles.Abstract.IProjectile proj)
    {
      
    }

    public void PlayHitSound(Dungeons.Tiles.Abstract.IDamagingSpell spell)
    {
      
    }

    public HitResult OnHitBy(Dungeons.Tiles.Abstract.ILivingEntity livingEntity)
    {
      return HitResult.Hit;
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
    HeroInventoryManager heroInventoryManager;
    protected InputManager inputManager;
    IPersister persister;
    Dungeons.Core.ILogger logger;

    public Hero Hero { get => Context.Hero; }
    protected GameState gameState;

    public AnimalsManager AnimalsManager { get { return animalsManager; } }

    public bool HeroTurn { get => Context.HeroTurn; }

    public virtual void SetLoadedContext(AbstractGameLevel node, Hero hero)
    {
      SetContext(node, hero, GameContextSwitchKind.GameLoaded, () => { });
    }

    public SpellPolicyManager SpellManager { get; set; }
    public MeleePolicyManager MeleePolicyManager { get; set; }
    internal ProjectileFightItemPolicyManager ProjectileFightItemPolicyManager { get; set; }

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
    public Dungeons.Core.ILogger Logger { get => logger; set => logger = value; }
    public Func<Tile, InteractionResult> Interact;
    public Func<int, Stairs, InteractionResult> DungeonLevelStairsHandler;

    [JsonIgnore]
    public Container Container
    {
      get;
      set;
    }
    public Func<Hero, GameState, bool, AbstractGameLevel> WorldLoader { get => worldLoader; set => worldLoader = value; }
    public Action<bool> WorldSaver { get; set; }
    public GameState GameState { get => gameState; }
    public SoundManager SoundManager { get; set; }
    public LevelGenerator LevelGenerator { get => levelGenerator; set => levelGenerator = value; }
    public LootManager LootManager { get => lootManager; protected set => lootManager = value; }
    static GameManager _debugCurrentInstance;
    static EventsManager lastInst = null;

    public GameManager(Container container)
    {
      _debugCurrentInstance = this;
      Container = container;

      gameState = container.GetInstance<GameState>();
      LootGenerator = container.GetInstance<LootGenerator>();
      Logger = container.GetInstance<Dungeons.Core.ILogger>();
      levelGenerator = container.GetInstance<Dungeons.DungeonGenerator>() as LevelGenerator;

      EventsManager = container.GetInstance<EventsManager>();

      EventsManager.EventAppended += EventsManager_ActionAppended;
      EventsManager.GameManager = this;
      lastInst = EventsManager;

      lootManager = container.GetInstance<LootManager>();
      lootManager.GameManager = this;

      heroInventoryManager = container.GetInstance<HeroInventoryManager>();
      heroInventoryManager.GameManager = this;
      
      CreateInputManager();

      Context = container.GetInstance<GameContext>();
      Context.EventsManager = EventsManager;
      AttackPolicyDone += () =>
      {
        RemoveDead();
      };

      Context.TurnOwnerChanged += Context_TurnOwnerChanged;


      enemiesManager = new EnemiesManager(Context, EventsManager, Container, null, this);
      AlliesManager = new AlliesManager(Context, EventsManager, Container, enemiesManager, this);
      enemiesManager.AlliesManager = AlliesManager;

      animalsManager = new AnimalsManager(Context, EventsManager, Container, this);

      Persister = container.GetInstance<IPersister>();

      SoundManager = new SoundManager(this, container);
      SpellManager = new SpellPolicyManager(this);
      MeleePolicyManager = new MeleePolicyManager(this);
      ProjectileFightItemPolicyManager = new ProjectileFightItemPolicyManager(this);

      Logger.LogInfo("GameManager ctor end " + GetHashCode() + ", EventsManager: " + EventsManager.GetHashCode());
    }

    private void Context_TurnOwnerChanged(object sender, TurnOwner e)
    {
      if (e == TurnOwner.Hero)
      {
        if (GameSettings.Mechanics.AutoCollectLootOnEntering)
        {
          var lootTile = CurrentNode.GetLootTile(Hero.point);
          if (lootTile != null)
          {
            CollectLootOnHeroPosition();
            Context.MoveToNextTurnOwner();
          }
        }

        UseActiveAbilities(Hero, null, true);
      }
    }

    protected void DisconnectEvents()
    {
      EventsManager.EventAppended -= EventsManager_ActionAppended;
    }

    protected virtual void CreateInputManager()
    {
      inputManager = new InputManager(this);
    }

    public bool ApplyMovePolicy(LivingEntity entity, Point newPos, List<Point> fullPath = null, Action<Policy> OnApplied = null)
    {
      var movePolicy = Container.GetInstance<MovePolicy>();
      //Logger.LogInfo("moving " + entity + " to " + newPos + " mp = " + movePolicy);

      movePolicy.OnApplied += (s, ev) =>
      {
        if (OnApplied != null)
        {
          OnApplied(ev);
        }

      };

      var oldTile = CurrentNode.GetTile(newPos);
      if (oldTile is FightItem fi)
      {
        if (fi.FightItemKind == FightItemKind.HunterTrap && fi.FightItemState == FightItemState.Activated)
        {
          entity.OnHitBy(fi as ProjectileFightItem, movePolicy);
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

    public bool CanHeroDoAction(bool moving)
    {
      return inputManager.CanHeroDoAction(moving);
    }

    public void AssertExc(Exception ex)
    {
      Assert(false, ex.Message + "\r\n" + ex.StackTrace);
    }

    public void Assert(bool assert, string info = "assert failed")
    {
      if (!assert)
      {
        DebugHelper.Assert(false, info);
        EventsManager.AppendAction(new Events.GameStateAction() { Type = Events.GameStateAction.ActionType.Assert, Info = info });
      }
    }

    protected virtual EnemiesManager CreateEnemiesManager(GameContext context, EventsManager eventsManager)
    {
      return new EnemiesManager(Context, EventsManager, Container, AlliesManager, this);
    }

    public virtual void SetContext(AbstractGameLevel node, Hero hero, GameContextSwitchKind kind, Action after, Stairs stairsUsedByHero = null,
       Portal portal = null)
    {
      hero.Container = this.Container;

      LootGenerator.LevelIndex = node.Index;

      Context.Hero = hero;

      if (!node.Inited)
        InitNode(node, gameState, kind);

      Context.SwitchTo(node, hero, gameState, kind, AlliesManager, after, stairsUsedByHero, portal);

      if (kind == GameContextSwitchKind.NewGame)
      {
        gameState.HeroInitGamePosition = hero.point;
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
            AppendTile(item as Tile, CurrentNode.GetClosestEmpty(dead).point);
        }
      }
      this.CurrentNode.AppendDead(dead);
    }

    protected virtual void InitNodeOnLoad(AbstractGameLevel node)
    {
      node.OnLoadDone();
    }

    public Options GameSettings { get => Options.Instance; }

    protected virtual void InitNode(AbstractGameLevel node, GameState gs, GameContextSwitchKind context)
    {
      node.GetTiles<LivingEntity>().ForEach(i => i.Container = this.Container);
      node.Logger = this.Logger;
      if (context == GameContextSwitchKind.GameLoaded && !GameSettings.Serialization.RegenerateLevelsOnLoad)
        InitNodeOnLoad(node);

      node.Inited = true;
    }

    public TileContainers.GameLevel LoadLevel(string heroName, int index, bool quick)
    {
      var level = Persister.LoadLevel(heroName, index, quick);
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

    protected virtual void OnActionAppended(GameEvent ev)
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

      if (ev is LootAction la)
      {
        if (la.Kind == LootActionKind.Consumed)
        {
          if (la.LootOwner is Hero)//TODO
            Context.MoveToNextTurnOwner();
        }
        if (la.Kind == LootActionKind.Deactivated && la.Loot is ProjectileFightItem pfi && pfi.FightItemKind == FightItemKind.HunterTrap)
        {
          if (pfi.DeactivatedCount > 3)
          {
            CurrentNode.RemoveLoot(pfi.point);
            AppendAction(new LootAction(pfi, null) { Kind = LootActionKind.Destroyed });
          }
        }
        return;
      }

      var lea = ev as LivingEntityAction;
      if (lea != null)
      {
        if (lea.Kind == LivingEntityActionKind.Died)
        {
          if (context.CurrentNode.HasTile(lea.InvolvedEntity))
          {
            var set = context.CurrentNode.SetTile(context.CurrentNode.GenerateEmptyTile(), lea.InvolvedEntity.point);
            if (!set)
              Logger.LogError("SetTile empty failed for " + lea.InvolvedEntity);
          }
          else
          {
            Logger.LogError("context.CurrentNode HasTile failed for " + lea.InvolvedEntity);
          }
          if (lea.InvolvedEntity is Enemy || lea.InvolvedEntity is Animal)
          {
            ILootSource ls = lea.InvolvedEntity;
            if (lea.InvolvedEntity is Enemy enemy)
            {
              var exp = AdvancedLivingEntity.EnemyDamagingTotalExpAward[enemy.PowerKind] * enemy.Level / 10;
              Hero.IncreaseExp(exp);
              var allies = context.CurrentNode.GetActiveAllies();
              foreach (var al in allies)
                al.IncreaseExp(exp);
            }
            context.CurrentNode.SetTile(new Tile(), lea.InvolvedEntity.point);
            if (lea.InvolvedEntity.tag1 == "boar_butcher")
            {
              //GameState.History.LivingEntity.CountByTag1()
            }
            else
              lootManager.TryAddForLootSource(ls);
          }
        }


      }
      else if (ev is GameStateAction gsa)
      {
        if (gsa != null)
          Logger.LogInfo(gsa.Info);
      }
      else if (ev is InteractiveTileAction ita)
      {
        int k = 0;
        k++;
      }
    }
    public bool AddLootReward(Loot item, Animal lootSource, bool animated)
    {
      if (CurrentNode.GetTile(lootSource.GetPoint()).IsEmpty)
      {
        var repl = ReplaceTile(item, lootSource as Tile);
        Assert(repl, "ReplaceTileByLoot " + item);
        return repl;
      }
      else
        return AddLootReward(item, lootSource, true);//add loot at closest empty
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
      if (loot == null && ls is DeadBody)
      {
        loot = LootGenerator.GetRandomLoot(ls.Level);
      }

      return loot;
    }

    public ITilesAtPathProvider TilesAtPathProvider { get; set; }
    public HeroInventoryManager HeroInventoryManager { get => heroInventoryManager; set => heroInventoryManager = value; }

    public virtual void OnHeroPolicyApplied(Policy policy)
    {
      //SpellManager.OnHeroPolicyApplied(policy);
      //MeleePolicyManager.OnHeroPolicyApplied(policy);
      if (policy.Kind == PolicyKind.Move || policy.Kind == PolicyKind.ProjectileCast ||
          policy.Kind == PolicyKind.SpellCast)
        FinishHeroTurn(policy);
    
    }

    public void FinishHeroTurn(Policy policy)
    {
      RemoveDead();
      Context.IncreaseActions(TurnOwner.Hero, policy);
      Context.MoveToNextTurnOwner();
    }

    public void AppendAction(string info, ActionLevel lvl)
    {
      var ev = new GameEvent(info, lvl);
      this.EventsManager.AppendAction(ev);
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

    /// <summary>
    /// Appends tile, decreases scrolls count in the inv.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tile"></param>
    /// <param name="pt"></param>
    /// <returns></returns>
    public bool AppendTileByScrollUsage<T>(T tile, Point pt) where T : Tile
    {
      bool appended = AppendTile(tile, pt);
      if (appended)
      {
        if (tile is Portal)
        {
          return HandlePortalAdded();
        }
        //else
        //  Assert(false, "AppendTileByScrollUsage unknown tile!");
      }
      return appended;
    }

    protected virtual bool HandlePortalAdded()
    {
      if (Hero.ActiveSpellSource is Book)
      {
        return true;
      }
      var portalScroll = Hero.Inventory.GetItems<Roguelike.Tiles.Looting.Scroll>().Where(i => i.Kind == Spells.SpellKind.Portal).FirstOrDefault();
      return Hero.Inventory.Remove(portalScroll) != null;
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
        else if (tile is Loot loot)
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
                ac.InvolvedEntity = le; 
                ac.Kind = LivingEntityActionKind.AppendedToLevel;
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

    public bool ReplaceTile(Tile replacer, Portal toReplace)
    {
      return ReplaceTile<Tile>(replacer, toReplace);
    }

    public bool ReplaceTile(Tile replacer, Barrel toReplace)
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

    Func<Hero, GameState, bool, AbstractGameLevel> worldLoader;

    public virtual void Load
    (
      string heroName,
      bool quick,
      Action<Hero> postLoad = null,
      bool useSavePath = false
    )
    {
      persistancyWorker.Load(heroName, this, quick, WorldLoader);
      if (GameSettings.Serialization.RestoreHeroToSafePointAfterLoad)
      {

      }
      this.Hero.Abilities.EnsureItems();//new game version might added...
      if (GameState.CoreInfo.PermanentDeath)
      {
        var persister = Container.GetInstance<IPersister>();
        persister.DeleteGame(heroName, quick);
      }
    }

    PersistancyWorker persistancyWorker = new PersistancyWorker();

    bool supportPrepareForFullSave = true;//true - serialization sometimes crashed 
    public void PrepareForFullSave()
    {
      if (supportPrepareForFullSave)
        this.Hero.PrepareForFullSave();
    }

    public virtual void Save(bool quick, Roguelike.Serialization.Serialized serialized = null)
    {
      persistancyWorker.Save(this, WorldSaver, quick);
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

    public bool CollectLoot(Loot lootTile, bool fromDistance, Policy policy = null)
    {
      if (!Context.HeroTurn)
        return false;

      Inventory inv = Hero.Inventory;
      var gold = lootTile as Gold;

      if (lootTile is Recipe)
        inv = Hero.Crafting.Recipes.Inventory;

      var collected = false;
      if (gold != null)
        collected = true;
      else
      {
        if (lootTile is ProjectileFightItem pfi && pfi.IsThrowingTorch())
        {
          var fi = Hero.GetFightItemFromCurrentEq();
          if (fi != null && fi.IsThrowingTorch())
          {
            fi.Count += pfi.Count;
            collected = true;
          }
        }
        if (!collected && inv.Add(lootTile, new AddItemArg() { detailedKind = InventoryActionDetailedKind.Collected }))
          collected = true;
      }

      if (collected)
      {
        //Hero.Inventory.Print(logger, "loot added");
        if (gold != null)
          Hero.Gold += gold.Count;
        CurrentNode.RemoveLoot(lootTile.point);
        EventsManager.AppendAction(new LootAction(lootTile, Hero) { Kind = LootActionKind.Collected, CollectedFromDistance = fromDistance });
        if (lootTile is IEquipment eq)
        {
          Hero.HandleEquipmentFound(eq);
        }
        OnLootCollected(lootTile);
        if (policy == null || policy.ChangesTurnOwner)
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

    LivingEntity activeAbilityVictim;
    public void MakeGameTick()
    {
      if (activeAbilityVictim != null)
      {
        if (activeAbilityVictim.Alive && activeAbilityVictim.State != EntityState.Idle)
          return;
        activeAbilityVictim.MoveDueToAbilityVictim = false;
        activeAbilityVictim = null;
      }
      if (context.PendingTurnOwnerApply)
      {
        EntitiesManager mgr = GetCurrentEntitiesManager();
        if (mgr != null)
        {
          mgr.MakeTurn();
        }
      }
    }

    public EntitiesManager GetCurrentEntitiesManager()
    {
      EntitiesManager mgr = null;
      if (context.TurnOwner == TurnOwner.Allies)
      {
        mgr = AlliesManager;
        //logger.LogInfo("MakeGameTick call to AlliesManager.MoveHeroAllies");
      }
      else if (context.TurnOwner == TurnOwner.Enemies)
      {
        mgr = EnemiesManager;
        //logger.LogInfo("MakeGameTick call to EnemiesManager.MakeEntitiesMove");
      }
      else if (context.TurnOwner == TurnOwner.Animals)
      {
        mgr = animalsManager;
        //logger.LogInfo("MakeGameTick call to EnemiesManager.MakeEntitiesMove");
      }

      return mgr;
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

    public bool CanSell(Loot lootToSell, IInventoryOwner src, IInventoryOwner dest, int count, ref int price)
    {
      price = 0;
      bool goldInvolved = GetGoldInvolvedOnSell(src, dest);
      if (!goldInvolved)
        return true;

      price = src.GetPrice(lootToSell) * count;

      return dest.Gold >= price;
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
        if (eq != null && (!loot.StackedInInventory || loot.Name != eq.Name))
        {
          if (!destInv.Owner.MoveEquipmentCurrent2Inv(eq, ceAddArgs.cek))
            return null;
        }
      }

      var price = 0;
      var count = removeItemArg.StackedCount;
      if (count == 0)
        count = 1;
      if (!CanSell(loot, src, dest, count, ref price))
      {
        logger.LogInfo("dest.Gold < loot.Price");
        SoundManager.PlayBeepSound();
        AppendAction(new InventoryAction(destInv) { Kind = InventoryActionKind.ItemTooExpensive, Info = "ItemTooExpensive" });
        return null;
      }
      if (!destInv.CanAddLoot(loot))
      {
        logger.LogInfo("!dest.Inventory.CanAddLoot(loot)");
        AppendAction(new InventoryAction(destInv) { Info = "Not enough room in the inventory", Level = ActionLevel.Important, Kind = InventoryActionKind.NotEnoughRoom });
        return null;
      }

      var sold = srcInv.Remove(loot, removeItemArg);
      if (sold == null)
      {
        logger.LogError("!removed");
        return null;
      }
      if (sold is StackedLoot sl)
      {
        sold = sl.Clone(removeItemArg.StackedCount);
      }

      bool added = destInv.Add(sold, addItemArg);
      if (!added)
      {
        srcInv.Add(loot);
        logger.LogError("!added");
        return null;
      }
      //destInv.Owner.SyncShortcutsBarStackedLoot();

      if (price != 0)
      {
        dest.Gold -= price;
        src.Gold += price;
        SoundManager.PlaySound("COINS_Rattle_04_mono");//coind_drop
      }
      return sold;
    }

    public virtual bool GetGoldInvolvedOnSell(IInventoryOwner src, IInventoryOwner dest)
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
              if (fi.FightItemKind.IsBowLikeAmmunition())
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
      var args = new AddItemArg();
      args.notifyObservers = false;//prevent 100 sounds
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

            merch.Inventory.Add(eq, args);
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
          merch.Inventory.Add(lootGenerator.GetLootByTileName<Weapon>("solid_bow"), args);
        else if (levelIndex <= 6)
          merch.Inventory.Add(lootGenerator.GetLootByTileName<Weapon>("composite_bow"), args);
      }
    }

    public void RegenerateMerchantInv(Merchant merch)
    {
      PopulateMerchantInv(merch, this.Hero.Level);
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
      if (RandHelper.GetRandomDouble() > 0.7)
        merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.CannonBall) { Count = 4 });
      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.ThrowingKnife) { Count = 4 });
      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.ThrowingTorch) { Count = 4 });
      
      if (RandHelper.GetRandomDouble() > 0.5)
        merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.HunterTrap) { Count = 4 });

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

      if (!merch.Inventory.Items.Any(i => i is Book))
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
      enemy.LootSource = lootSource;
    }

    private void AppendEnemy(Enemy enemy, Point pt, bool replace)
    {
      enemy.Container = this.Container;
      enemy.CanFly = enemy.tag1.Contains("lost_soul");
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
      enemy.SetLevel(level);
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
        if (ally is Ally ally_)
          ally_.SetNextExpFromLevel();
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
        return advEnt.Inventory.Remove(spellSource) != null;

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

    public IHitable RecentlyHit { get; set; }
    public virtual void HandeTileHit(Policy policy, LivingEntity attacker, IHitable hitTile)
    {
      RecentlyHit = hitTile;
      var info = "";
      var hitBlocker = false;
      var chest = hitTile as Chest;
      var barrel = hitTile as Barrel;
      var policyIsMelee = policy.Kind == PolicyKind.MeleeAttack;
      var attackerName = attacker.Name;

      if (barrel != null && barrel.OutOfOrder)
      {
        AppendAction<HeroAction>((HeroAction ac) => { ac.Kind = HeroActionKind.HitLockedChest; ac.Info = "This element is out of order"; });
        SoundManager.PlayBeepSound();
        return;
      }

      if (hitTile is Wall || (chest != null) || (barrel != null))
      {
        hitBlocker = true;
        if (chest != null)
        {
          info = attackerName+" hit a ";
          if (chest.ChestVisualKind == ChestVisualKind.Chest)
            info += "chest";
          else if (chest.ChestVisualKind == ChestVisualKind.Grave)
            info += "gave";
        }
        else if (barrel != null)
          info = attackerName + " hit a barrel";
        else
          info = attackerName + " hit a wall";
      }

      if (hitBlocker)
        AppendAction<HeroAction>((HeroAction ac) => { ac.Kind = HeroActionKind.HitWall; ac.Info = info; });
      if (hitTile is Barrel || hitTile is Chest || hitTile is DeadBody)
      {
        var ls = hitTile as ILootSource;
        if (!GeneratesLoot(ls))
        {
          ReplaceTile(new Tile(), ls.GetPoint());
          return;
        }

        var chestWasLooted = chest!=null ? chest.IsLooted : false;
        this.LootManager.TryAddForLootSource(ls);
        if(chest!=null && chestWasLooted)
          HandleChestHit(ls);
        //Logger.LogInfo("TimeTracker TryAddForLootSource: " + tr.TotalSeconds);
      }
      else if (hitTile is TorchSlot ts)
      {
        if (ts.IsLooted)
        {
          var fi = Hero.GetFightItem(FightItemKind.ThrowingTorch);
          if (fi != null)
          {
            if (Hero.RemoveFightItem(fi))
            {
              ts.SetLooted(false);
              SoundManager.PlaySound("uncloth");
            }
          }
        }
        else
        {
          ts.SetLooted(true);
          var pfi = new ProjectileFightItem() { FightItemKind = FightItemKind.ThrowingTorch, Count = 1 };
          CollectLoot(pfi, false, policy);
        }
      }
      else if (hitTile is Candle candle)
      {
        if (candle.IsLooted)
        {
          candle.SetLooted(false);
          SoundManager.PlaySound("uncloth");
        }
        else
        {
          SoundManager.PlaySound("cloth");
          candle.SetLooted(true);
        }
      }
      else if (hitTile is CrackedStone cs)
      {
        info = attackerName + " hit a cracked stone";
        AppendAction<InteractiveTileAction>((InteractiveTileAction ac) =>
        {
          ac.InteractiveKind = InteractiveActionKind.Hit;
          ac.Info = info;
          ac.InvolvedTile = cs;
          ac.PlaySound = policyIsMelee;//TODO
        });
        if (cs.Destroyed)
        {
          ReplaceTile(new Tile(), cs.point);
        }
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



    public bool UtylizeSpellSource(LivingEntity caster, SpellSource spellSource, Abstract.Spells.ISpell spell)
    {
      try
      {
        if (spellSource.Kind == Spells.SpellKind.Skeleton)
        {
          var count = GetAlliesCount<AlliedEnemy>();
          if (count > 0)
          {
            var casterAdv = caster as AdvancedLivingEntity;
            if (casterAdv == null || !casterAdv.CanAddNextSkeleton(count))
            {
              ReportFailure("Currently you can not instantiate more skeletons");
              return false;
            }
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
          if (!(caster is God))
          {
            if (caster.Stats.Mana < spell.ManaCost)
            {
              ReportFailure("Not enough mana to cast a spell");
              return false;
            }
            caster.ReduceMana(spell.ManaCost);
          }
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

    public Roguelike.TileContainers.SurfaceSet GetSurfacesOil()
    {
      return CurrentNode.SurfaceSets.GetKind(SurfaceKind.Oil);
    }

    public virtual bool TryApplyAttackPolicy(ProjectileFightItem fi, Tile pointedTile, Action<Tile> beforAttackHandler = null)
    {

      return ProjectileFightItemPolicyManager.TryApplyAttackPolicy(fi, pointedTile, beforAttackHandler);
    }

    public virtual int GetAttackVictimsCount(LivingEntity advCaster)
    {
      if (advCaster.SelectedActiveAbilityKind == AbilityKind.ArrowVolley ||
          advCaster.SelectedActiveAbilityKind == AbilityKind.PiercingArrow)
      {
        var ab = advCaster.GetActiveAbility(advCaster.SelectedActiveAbilityKind);
        if (ab.CoolDownCounter == 0)
        {
          var maxVictims = 1;
          if (ab.Kind == Roguelike.Abilities.AbilityKind.ArrowVolley)
            maxVictims = (int)ab.PrimaryStat.Factor;
          else
            maxVictims = (int)ab.AuxStat.Factor;
          return maxVictims;//TODO
        }
      }
      else if (advCaster.SelectedActiveAbilityKind == AbilityKind.Cannon)
      {
        var ab = advCaster.GetActiveAbility(advCaster.SelectedActiveAbilityKind);
        if (ab.CoolDownCounter == 0)
        {
          var maxVictims = (int)ab.PrimaryStat.Factor;
          return maxVictims;
        }
      }
      return 1;
    }

    public void Log(string log)
    {
      //logger.LogInfo("gm: "+log);
    }

    public bool ApplyAttackPolicy
   (
     LivingEntity caster,//hero, enemy, ally
     Dungeons.Tiles.Abstract.IHitable target,
     ProjectileFightItem pfi,
     Action<Policy> BeforeApply = null,
     Action<Policy> AfterApply = null
   )
    {
      return ProjectileFightItemPolicyManager.ApplyAttackPolicy(caster, target, pfi, BeforeApply, AfterApply);
    }


    public AttackDescription CreateAttackDescription(LivingEntity caster, ProjectileFightItem pfi)
    {
      var fightItem = pfi ?? AttackDescription.GetActiveFightItem(caster);
      if (fightItem == null)
      {
        Logger.LogError("CreateAttackDescription GetActivatedFightItem == null");
        return null;
      }

      return new AttackDescription(caster, caster.UseAttackVariation, AttackKind.PhysicalProjectile);
    }

    //public void CallTryAddForLootSource(Dungeons.Tiles.Abstract.IHitable obstacle, Policy policy)
    //{
    //  if (obstacle is CrackedStone cs)
    //  {
    //  //  if (cs.Destroyed)
    //  //    ReplaceTile(new Tile(), cs.point);
    //  }
    //  else if(obstacle is ILootSource)
    //  {
    //    var le = obstacle is LivingEntity;
    //    if (!le)//le is handled specially
    //    {
    //      LootManager.TryAddForLootSource(obstacle as ILootSource);
    //    }
    //  }
      
    //}

    

    public void FinishPolicyApplied(LivingEntity caster, Action<Policy> AfterApply, Policy policy)
    {
      if (caster is Hero)
        OnHeroPolicyApplied(policy);

      if (AfterApply != null)
        AfterApply(policy);
    }

    public void ReportFailure(string infoToDisplay)
    {
      SoundManager.PlayBeepSound();
      if (infoToDisplay.Any())
      {
        AppendAction(new GameEvent() { Info = infoToDisplay, Level = ActionLevel.Important });
        Logger.LogInfo(infoToDisplay);
      }
    }

    public T GetStackedLootFromInv<T>(Inventory inv) where T : StackedLoot
    {
      return inv.GetStacked<T>().FirstOrDefault();
    }

    public Action<Policy, LivingEntity, IHitable> AttackPolicyInitializer;
    public Action AttackPolicyDone;


    public void ApplyHeroPhysicalAttackPolicy(IHitable target, bool allowPostAttackLogic)
    {
      Action<Policy> afterApply = (p) => { };
      if (allowPostAttackLogic)
        afterApply = (p) => OnHeroPolicyApplied(p);
            
      ApplyPhysicalAttackPolicy(Hero, target, afterApply, EntityStatKind.Unset);
    }

    public void ApplyPhysicalAttackPolicy(LivingEntity attacker, IHitable target, Action<Policy> afterApply, EntityStatKind esk)
    {
      MeleePolicyManager.ApplyPhysicalAttackPolicy(attacker, target, afterApply, null, esk);
    }

    public bool UseActiveAbilities(LivingEntity attacker, LivingEntity targetLe, bool turnStart)
    {
      bool done = false;
      var ale = attacker as AdvancedLivingEntity ;
      foreach (var aa in ale.Abilities.ActiveItems)
      {
        if (aa.RunAtTurnStart == turnStart)
        {
          if (HasAbilityActivated(aa, ale))
          {
            string reason;
            if (attacker.CanUseAbility(aa.Kind, CurrentNode, out reason))
            {
              UseActiveAbility(targetLe, attacker, ale.SelectedActiveAbility.Kind);
              done = true;
            }
          }
        }
      }

      return done;
    }

    protected virtual bool HasAbilityActivated(ActiveAbility ab, AdvancedLivingEntity ale)
    { 
      return ale.SelectedActiveAbility != null && ale.SelectedActiveAbility.Kind == ab.Kind;
    }

    public void UseActiveAbility(LivingEntity victim, LivingEntity abilityUser, Abilities.AbilityKind abilityKind)
    {
      var advEnt = abilityUser as AdvancedLivingEntity;
      if (!advEnt.Abilities.IsActive(abilityKind))
        return;

      bool activeAbility = true;
      UseAbility(victim, abilityUser, abilityKind, activeAbility);
    }

    public void UseAbility(LivingEntity victim, LivingEntity abilityUser, Abilities.AbilityKind abilityKind, bool activeAbility)
    {
      var ab = abilityUser.GetActiveAbility(abilityKind);

      bool used = false;
      if (abilityKind == Abilities.AbilityKind.StrikeBack)//StrikeBack is passive!
      {
        ApplyPhysicalAttackPolicy(abilityUser, victim, (p) =>
        {
          used = true;
          AppendUsedAbilityAction(abilityUser, abilityKind);
          if (AttackPolicyDone != null)
            AttackPolicyDone();
        }, EntityStatKind.ChanceToStrikeBack);
      }
      else if (abilityKind == AbilityKind.Stride)
      {
        int horizontal = 0, vertical = 0;
        var neibKind = GetTileNeighborhoodKindCompareToHero(victim);
        if (neibKind.HasValue)
        {
          InputManager.GetMoveData(neibKind.Value, out horizontal, out vertical);
          var newPos = InputManager.GetNewPositionFromMove(victim.point, horizontal, vertical);
          activeAbilityVictim = victim;// as Enemy;
          activeAbilityVictim.MoveDueToAbilityVictim = true;

          var desc = "";
          var attack = abilityUser.Stats.GetStat(EntityStatKind.Strength).SumValueAndPercentageFactor(ab.PrimaryStat, true);
          var damage = victim.CalcMeleeDamage(attack, ref desc);
          var inflicted = victim.InflictDamage(abilityUser, false, ref damage, ref desc);

          ApplyMovePolicy(victim, newPos.Point);
          used = true;
        }
      }
      else if (abilityKind == Abilities.AbilityKind.OpenWound)
      {
        if (victim != null)
        {
          //psk = EntityStatKind.BleedingExtraDamage;
          //ask = EntityStatKind.BleedingDuration;
          var duration = ab.AuxStat.Factor;
          var damage = Calculated.FactorCalculator.AddFactor(3, ab.PrimaryStat.Factor);//TODO  3
          victim.StartBleeding(damage, null, (int)duration);
          used = true;
        }
        else
        {
          //var leSrc = new AbilityLastingEffectSrc(ab, 0);
          //abilityUser.LastingEffectsSet.AddPercentageLastingEffect(EffectType.OpenWound, leSrc, abilityUser);
          //used = true;
        }
      }
      else if (abilityKind == Abilities.AbilityKind.ZealAttack)
      {
        used = true;
      }
      else
        used = UseActiveAbility(ab, abilityUser, false);

      if (used)
      {
        if (activeAbility)
        {
          HandleActiveAbilityUsed(abilityUser, abilityKind);
        }
      }
    }

    public bool UseActiveAbility(ActiveAbility ab, LivingEntity abilityUser, bool sendEvent)
    {
      bool used = false;
      var abilityKind = ab.Kind;
      var endTurn = false;
      if (ab.CoolDownCounter > 0)
        return false;

      if (abilityKind == AbilityKind.Smoke)
      {
        AddSmoke(abilityUser);
        endTurn = true;
        used = true;
        
      }
      else
        used = AddLastingEffectFromAbility(ab, abilityUser);

      if (used)
      {
        abilityUser.WorkingAbilityKind = abilityKind;
        if(sendEvent)
          HandleActiveAbilityUsed(abilityUser, abilityKind);
      }

      if(endTurn)
        Context.MoveToNextTurnOwner();//TODO call HandleHeroActionDone
      return used;
    }

    private void AddSmoke(LivingEntity abilityUser)
    {
      var smokeAb = Hero.GetActiveAbility(AbilityKind.Smoke);
      var smokes = CurrentNode.AddSmoke(Hero, (int)smokeAb.PrimaryStat.Factor, (int)smokeAb.AuxStat.Factor);
       AppendAction<TilesAppendedEvent>((TilesAppendedEvent ac) =>
      {
        ac.Tiles = smokes.Cast<Tile>().ToList();
      });
    }

    public void SpreadOil(Tile startingTile, int minRange = 1, int maxRange = 1)
    {
      var range = RandHelper.GetRandomDouble() > .5 ? minRange : maxRange;
      var surfaces = CurrentNode.SpreadOil(startingTile, minRange, maxRange, reveal:true);
      if (surfaces.Any())
      {
        AppendAction<TilesAppendedEvent>((TilesAppendedEvent ac) =>
        {
          ac.Tiles = surfaces.Cast<Tile>().ToList();
        });

        SoundManager.PlaySound("oil_splash");
      }
    }
        

    LastingEffect AddAbilityLastingEffectSrc(EffectType et, ActiveAbility ab, LivingEntity abilityUser, int abilityStatIndex = 0)
    {
      var src = new AbilityLastingEffectSrc(ab, abilityStatIndex);
      if (et != EffectType.IronSkin)
      {
        if(
          ab.Kind == AbilityKind.ElementalVengeance ||
          ab.Kind == AbilityKind.Rage 
          )
          src.Duration = 3;
        else
          src.Duration = 1;
      }
      else 
        src.Duration = (int)ab.AuxStat.Factor;
      return abilityUser.LastingEffectsSet.AddLastingEffect(et, src, abilityUser);
    }

    public bool AddLastingEffectFromAbility(ActiveAbility ab, LivingEntity abilityUser)
    {
      if (!ab.TurnsIntoLastingEffect)
        return false;

      if(ab.CoolDownCounter > 0)
        return false;

      bool used = false;
      var abilityKind = ab.Kind;

      if (ab.Kind == AbilityKind.ElementalVengeance)
      {
        var attacks = new EffectType[] { EffectType.FireAttack, EffectType.PoisonAttack, EffectType.ColdAttack };
        int i = 0;
        foreach (var et in attacks)
        {
          AddAbilityLastingEffectSrc(et, ab, abilityUser, i);
          i++;
        }
        used = true;
      }
      else if (ab.Kind == AbilityKind.IronSkin)
      {
        //var ab = abilityUser.GetActiveAbility(abilityKind);
        //var defensePercadd = ab.PrimaryStat.Factor;
        AddAbilityLastingEffectSrc(EffectType.IronSkin, ab, abilityUser);
        used = true;
      }
      else if (abilityKind == Abilities.AbilityKind.Rage)
      {
        AddAbilityLastingEffectSrc(EffectType.Rage, ab, abilityUser);
        used = true;
      }

      return used;
    }

    public void HandleActiveAbilityUsed(LivingEntity abilityUser, AbilityKind abilityKind)
    {
      var ab = abilityUser.GetActiveAbility(abilityKind);
      abilityUser.HandleActiveAbilityUsed(abilityKind);
      AppendUsedAbilityAction(abilityUser, abilityKind);
    }

    public void AppendUsedAbilityAction(LivingEntity abilityUser, Abilities.AbilityKind abilityKind)
    {
      EventsManager.AppendAction(new LivingEntityAction(LivingEntityActionKind.UsedAbility)
      {
        Info = abilityUser.Name + " used ability " + abilityKind,
        Level = ActionLevel.Important,
        InvolvedEntity = abilityUser
      });

      if(abilityKind == AbilityKind.Smoke)
        SoundManager.PlaySound("cloth");
    }

    TileNeighborhood? GetTileNeighborhoodKindCompareToHero(LivingEntity target)
    {
      TileNeighborhood? neib = null;
      if (target.Position.X > Hero.Position.X)
        neib = TileNeighborhood.East;
      else if (target.Position.X < Hero.Position.X)
        neib = TileNeighborhood.West;
      else if (target.Position.Y > Hero.Position.Y)
        neib = TileNeighborhood.South;
      else if (target.Position.Y < Hero.Position.Y)
        neib = TileNeighborhood.North;
      return neib;
    }

    public List<Enemy> GetRessurectTargets(Enemy enemyChemp)
    {
      var deadOnes = this.CurrentNode.GetDeadEnemies();
      var ressurectTargets = deadOnes.Where
      (
        i => i.DistanceFrom(enemyChemp) < 7
      &&
      (
        //i.Symbol.ToString().ToUpper() == enemyChemp.Symbol.ToString().ToUpper()//lynx and Skeleton had both E
        //||
        i.name == enemyChemp.name
      )
      ).ToList();

      return ressurectTargets;
    }

    public void SendCommand(Enemy enemyChemp, EnemyAction ea)
    {
      if (ea.CommandKind == EntityCommandKind.RaiseMyFriends)
      {
        foreach (var dead in GetRessurectTargets(enemyChemp))
        {
          var healthProvider = new Enemy(this.Container);
          healthProvider.SetLevel(dead.Level);
          CurrentNode.RemoveDead(dead);
          dead.Ressurect(healthProvider.Stats.Health / 2);

          var curr = CurrentNode.GetTile(dead.point);
          if (curr != null && !curr.IsEmpty)
          {
            var emp = CurrentNode.GetEmptyNeighborhoodPoint(curr);
            if (emp != null)
              CurrentNode.SetTile(dead, emp.Item1);
          }

          this.EnemiesManager.AddEntity(dead);
          AppendAction<LivingEntityAction>((LivingEntityAction ac) =>
          {
            ac.InvolvedEntity = dead;
            ac.Kind = LivingEntityActionKind.AppendedToLevel;
            ac.Info = dead.Name + " ressurected";
          });
        }
      }
    }

    virtual public void PrepareDiscussionForShowing(INPC npc)
    {
      
    }

    public void ForceNextTurnOwner()
    {
      this.EnemiesManager.ForcePendingForAllIdleFalse();
      this.AlliesManager.ForcePendingForAllIdleFalse();
      this.AnimalsManager.ForcePendingForAllIdleFalse();
      Hero.State = Roguelike.Tiles.LivingEntities.EntityState.Idle;//with state Attacking no move was possible
      Context.MoveToNextTurnOwner();
    }

    public bool CanCallIdentify(AdvancedLivingEntity priceProvider, Tile shownFor)
    {
      if (priceProvider is Hero)
      {
        return true;
      }

      return false;
    }

    public virtual void OnBeforeAlliesTurn()
    {
      var smokes = CurrentNode.Layers.GetTypedLayerTiles<ProjectileFightItem>(KnownLayer.Smoke);
      ProjectileFightItem smokeEnded = null;
      foreach (var smoke in smokes)
      {
        smoke.Durability--;
        if (smoke.Durability <= 0)
        {
          //CurrentNode.RemoveLoot(smoke.point);
          CurrentNode.Layers.GetLayer(KnownLayer.Smoke).Remove(smoke);
          AppendAction(new LootAction(smoke, null) { Kind = LootActionKind.Destroyed, Loot = smoke });
          smokeEnded = smoke;
        }
      }
      if (smokeEnded !=null && smokeEnded.ActiveAbilitySrc == AbilityKind.Smoke)
      {
        smokeEnded.Caller.HandleActiveAbilityEffectDone(AbilityKind.Smoke, false);

      }

      var surfaces = CurrentNode.SurfaceSets.Sets;
      foreach (var surfaceSet in surfaces)
      {
        foreach (var surface in surfaceSet.Value.Tiles.Values.ToList())
        {
          if (surface.SupportsDurability)
          {
            if (surface.IsBurning)
            {
              surface.Durability--;
              if (surface.Durability <= 0)
              {
                surfaceSet.Value.Tiles.Remove(surface.point);
                surface.ReportDestroyed();
                AppendAction(new SurfaceAction(surface, SurfaceActionKind.Destroyed));
              }
            }
          }
        }
      }
    }

    public virtual void OnAfterAlliesTurn()
    {

    }

    public bool UseActiveSpell(
     Tile pointedTile,
     LivingEntity caster,
     Dungeons.Tiles.Abstract.ISpell spell,
     ref ApplyAttackPolicyResult applyAttackPolicyResult,
     SpellSource spellSource)
    {
      bool applied = false;
      var target = pointedTile;
      var destroyable = target as IHitable;
      if (spell is ProjectiveSpell && !(spellSource is SwiatowitScroll))
      {
        if (destroyable != null)
        {
          applyAttackPolicyResult = SpellManager.ApplyAttackPolicy(caster, destroyable, spellSource);
          applied = applyAttackPolicyResult == ApplyAttackPolicyResult.OK;
        }
        else
        {
          ReportFailure("Target not destroyable");
        }
      }
      else if (spell is OffensiveSpell)
      {
        applied = SpellManager.ApplySpell(caster, spellSource, pointedTile) != null;
      }
      applyAttackPolicyResult = applied ? ApplyAttackPolicyResult.OK : ApplyAttackPolicyResult.Unset;
      return applied;
    }

    public bool UseSpell(
      God god,
      Tile pointedTile,
      ref ApplyAttackPolicyResult applyAttackPolicyResult
      
      )
    {
      Scroll spellSource = null;
      var spell = god.CreateSpell(out spellSource);
      return UseSpell(spell, spellSource, pointedTile, ref applyAttackPolicyResult, null);
    }

    public bool UseSpell(
      Abstract.Spells.ISpell spell,
      SpellSource spellSource,
      Tile pointedTile,
      ref ApplyAttackPolicyResult applyAttackPolicyResult,
      Action<bool> onBeforeUseSpell
      )
    {
      bool applied = false;
      if (spell is PassiveSpell)
      {
        applied = SpellManager.UsePassiveSpell(pointedTile, spell.Caller, spell, applied, spellSource);
        applyAttackPolicyResult = applied ? ApplyAttackPolicyResult.OK : ApplyAttackPolicyResult.Unset;
      } 
      else
      {
        var target = pointedTile;
        var destroyable = target as IHitable;
        if (onBeforeUseSpell != null && destroyable != null)
        {
          onBeforeUseSpell(true);
        }

        applied = UseActiveSpell(pointedTile, spell.Caller, spell, ref applyAttackPolicyResult, spellSource);

      }

      return applied;
    }

    public void HandleChestHit(ILootSource lootSource)
    {
      var chest = (lootSource as Chest);
      if (chest != null)
      {
        chest.RegistedHitWhenOpened();
        if (chest.Destroyed)
        {
          CurrentNode.ReplaceTile(new Tile(), chest.point);
          AppendAction(new InteractiveTileAction() { InteractiveKind = InteractiveActionKind.Destroyed, InvolvedTile = chest });
        }
        else
          AppendAction(new InteractiveTileAction() { InteractiveKind = InteractiveActionKind.HitWhenLooted, InvolvedTile = chest });
      }
    }
  }
}
