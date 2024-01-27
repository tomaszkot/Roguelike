using Dungeons;
using Dungeons.Core;
using Dungeons.Fight;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.Inventory;
using Roguelike.Abstract.Tiles;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Core.Managers;
using Roguelike.Managers.Policies;
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
using System.Drawing;
using System.Linq;
using Roguelike.Spells;
using Dungeons.Core.Policy;
using Dungeons.Tiles.Abstract;
using InteractiveTile = Roguelike.Tiles.Interactive.InteractiveTile;

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
    protected LootGenerator lootGenerator;
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

    public NpcManager NpcManager { get; set; }
    public EventsManager EventsManager { get => eventsManager; set => eventsManager = value; }
    public GameContext Context { get => context; set => context = value; }
    public AbstractGameLevel CurrentNode { get => context.CurrentNode; }
    public AlliesManager AlliesManager { get => alliesManager; set => alliesManager = value; }
    public LootGenerator LootGenerator { get => lootGenerator; set => lootGenerator = value; }

    public List<InteractiveTile> PossibleNpcDestMoveTargets = new List<InteractiveTile>();

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

      AbilityManager = new AbilityManager(this);

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
      Enemy.LevelStatIncreaseCalculated = (float inc) =>
      {
        if (GameState.CoreInfo.Demo)//demo has less emeies/worse balance
        {
          inc -= inc * 15f / 100f;
        }
        return inc;
      };

      enemiesManager = new EnemiesManager(Context, EventsManager, Container, null, this);
      AlliesManager = new AlliesManager(Context, EventsManager, Container, enemiesManager, this);
      enemiesManager.AlliesManager = AlliesManager;

      NpcManager = new NpcManager(Context, EventsManager, Container, this);

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

        var ab = Hero.SelectedActiveAbility as ActiveAbility;
        if (ab != null)
        {
          if (ab.RunAtTurnStart)
            AbilityManager.UseActiveAbility(Hero, true);
          else
          {
            AbilityManager.ActivateAbility(Hero, ab.Kind, null);
          }
        }

        if (Hero.DestPointDesc.State == DestPointState.StayingAtTarget)
        {
          var increasedStateCounter = Hero.DestPointDesc.IncreaseStateCounter();
          if (!increasedStateCounter)
            inputManager.HidingHeroInteractionOff(CurrentNode);
        }
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

      if (kind == GameContextSwitchKind.NewGame || kind == GameContextSwitchKind.GameLoaded)
      {
        PossibleNpcDestMoveTargets = node.GetTiles<Tiles.Interactive.InteractiveTile>().Where(i => i.IsNpcDestMoveTarget).ToList();
      }
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
          HandleLeDeath(lea);
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

    private void HandleLeDeath(LivingEntityAction lea)
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
          var exp = AdvancedLivingEntity.EnemyDamagingTotalExpAward[enemy.PowerKind] * enemy.Level;//* 2 with that level would big 1 higher
          Hero.IncreaseExp(exp);
          var allies = context.CurrentNode.GetActiveAllies();
          foreach (var al in allies)
            al.IncreaseExp(exp*.99f);
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

    public virtual void RemoveDead()
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
      if (Hero.SelectedSpellSource is Book)
      {
        return true;
      }
      //BUG was utylized twice!
      //var portalScroll = Hero.Inventory.GetItems<Roguelike.Tiles.Looting.Scroll>().Where(i => i.Kind == Spells.SpellKind.Portal).FirstOrDefault();
      return true;// Hero.Inventory.Remove(portalScroll) != null;
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
            {
              AppendAction((LivingEntityAction ac) =>
              {
                ac.InvolvedEntity = le;
                ac.Kind = LivingEntityActionKind.AppendedToLevel;
                ac.Info = le.Name + " spawned";
                ac.Silent = le.Herd == "ZyndramSquad";
              });
            }
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
      else if(lootTile is MagicDust)
        inv = Hero.Crafting.InvItems.Inventory;

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

    public virtual Equipment GenerateRandomEquipment(EquipmentKind kind)
    {
      return lootGenerator.GetRandomEquipment(kind, Hero.Level, Hero.GetLootAbility());
    }

    //Called every frame
    public virtual void MakeGameTick()
    {
      AbilityManager.MakeGameTick();

      if (context.PendingTurnOwnerApply)
      {
        EntitiesManager mgr = GetCurrentEntitiesManager();
        if (mgr != null)
        {
          mgr.MakeTurn();//make turn and if all done call Context.MoveToNextTurnOwner();
        }
      }
    }

    public EntitiesManager GetCurrentEntitiesManager()
    {
      EntitiesManager mgr = null;
      if (context.TurnOwner == TurnOwner.Allies)
      {
        mgr = AlliesManager;
      }
      else if (context.TurnOwner == TurnOwner.Enemies)
      {
        mgr = EnemiesManager;
      }
      else if (context.TurnOwner == TurnOwner.Animals)
      {
        mgr = animalsManager;
      }
      else if (context.TurnOwner == TurnOwner.Npcs)
      {
        mgr = NpcManager;
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

      if (!loot.IsSellable() && (dest is Merchant))
        return null;

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
        AppendAction(new InventoryAction(destInv) { Kind = InventoryActionKind.ItemTooExpensive, Info = "Not enough gold" });
        return null;
      }
      if (!destInv.CanAddLoot(loot) && destInv.Owner is Merchant merch)
      {
        RegenerateMerchantInv(merch, true);
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
          try
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
                  fi.Count = RandHelper.GetRandomInt(5) + 1;
              }
            }
          }
          catch (Exception ex)
          {
            Logger.LogError(ex);
          }
        }
      }

      GenerateMerchantEq(merch, true);
      GenerateMerchantEq(merch, false);
    }

    protected void AddSpecialBowLikeAmmo(Merchant merch, bool both)
    {
      bool arr = true;
      bool bolt = true;
      if (!both)
      {
        if (RandHelper.GetRandomDouble() > 0.5f)
          arr = false;
        else
          bolt = false;
      }
      if (bolt)
      {
        var pfiksSpecialBolt = new[]
        {
          FightItemKind.PoisonBolt, FightItemKind.IceBolt, FightItemKind.FireBolt
        };
        merch.Inventory.Items.Add(new ProjectileFightItem(pfiksSpecialBolt.GetRandomElem()) { Count = both ? 10 : 5 });
      }
      if (arr)
      {
        var pfiksSpecialArrow = new[]
        {
          FightItemKind.PoisonArrow, FightItemKind.IceArrow, FightItemKind.FireArrow
        };
        merch.Inventory.Items.Add(new ProjectileFightItem(pfiksSpecialArrow.GetRandomElem()) { Count = both ? 10 : 5 });
      }
    }
    protected virtual void GenerateMerchantEq(Merchant merch, bool magic)
    {
      var args = new AddItemArg();
      args.notifyObservers = false;//prevent 100 sounds
      var eqKinds = LootGenerator.GetEqKinds();
      eqKinds.Shuffle();
      int levelIndex = Hero.Level;
      bool rangeGen = false;
      Dictionary<Weapon.WeaponKind, int> wpnKindCounter = new Dictionary<Weapon.WeaponKind, int>();
      var kvs = EnumHelper.GetEnumValues<Weapon.WeaponKind>(true);
      foreach (var kv in kvs)
      {
        wpnKindCounter[kv] = 0;
      }

      foreach (var eqKind in eqKinds)
      {
        try
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
              if (wpn != null)
              {
                 if(wpn.IsBowLike)
                  rangeGen = true;

                wpnKindCounter[wpn.Kind]++;
              }
              count++;
            }
            else
              attemptTries++;

            if (attemptTries == 5)
              break;
          }
        }
        catch (Exception ex)
        {
          Logger.LogError(ex);
        }
      }

      if (merch.Name != "Basil")
      {
        foreach (var kv in wpnKindCounter)
        {
          if (kv.Value == 0)
          {
            var eq = lootGenerator.LootFactory.EquipmentFactory.GetWeapons(kv.Key, levelIndex).GetRandomElem();
            if (eq != null)
              merch.Inventory.Add(eq);
          }
        }

        AddSpecialBowLikeAmmo(merch, false);
      }

      if (!rangeGen)
      {
        try
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
        catch (Exception ex)
        {
          Logger.LogError(ex);
        }
      }
    }

    public void RegenerateMerchantInv(Merchant merch, bool rebind)
    {
      PopulateMerchantInv(merch, this.Hero.Level);
      if(rebind)
        AppendAction(new InventoryAction(merch.Inventory) { Info = "", Kind = InventoryActionKind.NeedsRebind });

    }
    public virtual void PopulateMerchantInv(Merchant merch, int heroLevel)
    {
      Logger.LogInfo("PopulateMerchantInv start");
      if (merch == null)
      {
        Logger.LogError("PopulateMerchantInv !merch");
        return;
      }
      merch.Inventory.Items.Clear();
      var lootKinds = Enum.GetValues(typeof(LootKind)).Cast<LootKind>()
        .Where(i => i != LootKind.Unset && i != LootKind.Other && i != LootKind.Seal && i != LootKind.SealPart
                   && i != LootKind.Gold && i != LootKind.Book && i != LootKind.FightItem)
        .ToList();

      Logger.LogInfo("PopulateMerchantInv AddLootToMerchantInv...");
      AddLootToMerchantInv(merch, lootKinds);

      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.Stone) { Count = 40 });
      //if (RandHelper.GetRandomDouble() > 0.7)
      //  merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.CannonBall) { Count = 8 });
      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.ThrowingKnife) { Count = 40 });
      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.ThrowingTorch) { Count = 20 });
      
      if (RandHelper.GetRandomDouble() > 0.5)
        merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.HunterTrap) { Count = 8 });

      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.PlainArrow) { Count = RandHelper.GetRandomInt(50) + 25 });
      merch.Inventory.Items.Add(new ProjectileFightItem(FightItemKind.PlainBolt) { Count = RandHelper.GetRandomInt(50) + 25 });

      Logger.LogInfo("PopulateMerchantInv after ProjectileFightItem...");
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

    public bool AddAlly(IAlly ally)
    {
      if (ally.Kind == AllyKind.Enemy)
      {
        if (AlliesManager.AllAllies.Where(i => i.Kind == AllyKind.Enemy).Any())
        {
          AppendAction(new GameInstructionAction() { Info = "Skeleton ally already used" }); ;
          SoundManager.PlayBeepSound();
          return false;
        }
      }
      ally.Active = true;
      var le = ally as LivingEntity;
      le.Container = this.Container;
      le.Revealed = true;

      if (ally.TakeLevelFromCaster)
      {
        float lvl = Hero.Level;// * 0.8f;
        if (lvl < 1)
          lvl = 1;

        ally.SetLevel((int)lvl, GameState.CoreInfo.Difficulty);
      }
      AlliesManager.AddEntity(le);

      if (!(ally is INPC))//Roslaw?
      {
        var empty = CurrentNode.GetClosestEmpty(Hero, true, false);
        ReplaceTile<LivingEntity>(le, empty);
        if (ally is Ally ally_)
          ally_.SetNextExpFromLevel();
        le.PlayAllySpawnedSound();
      }
      AppendAction(new AllyAction() { Info = le.Name + " has been added", InvolvedTile = ally, AllyActionKind = AllyActionKind.Created });
      return true;
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
      {
        return advEnt.RemoveFromInv(spellSource) != null;
      }
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
          info = attackerName + " hit a ";
          if (chest.ChestVisualKind == ChestVisualKind.Chest)
            info += "chest";
          else if (chest.ChestVisualKind == ChestVisualKind.Grave)
            info += "gave";
        }
        else if (barrel != null)
          info = attackerName + " hit a barrel";
        else
        {
          if(hitTile is Wall wall && wall.tag1.Contains("tree"))
            info = attackerName + " hit a tree";
          else
            info = attackerName + " hit a wall";
        }
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

        var chestWasLooted = chest != null ? chest.IsLooted : false;
        this.LootManager.TryAddForLootSource(ls);
        if (chest != null && chestWasLooted)
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
      else if (hitTile is InteractiveTile it && it.tag1.Contains("ladder"))
      {
        HandleLadder(it);
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
      else if (hitTile is Bonfire bf)
      {
        info = attackerName + " hit a bonfire";
        AppendAction<InteractiveTileAction>((InteractiveTileAction ac) =>
        {
          ac.InteractiveKind = InteractiveActionKind.Hit;
          ac.Info = info;
          ac.InvolvedTile = bf;
          ac.PlaySound = policyIsMelee;//TODO
        });
       
      }

    }

    protected virtual void HandleLadder(InteractiveTile it)
    {
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
          if (!(caster is God) && !(caster.tag1.StartsWith("fallen_one")))
          {
            if(!TryToBurnMana(caster, spell.ManaCost, false))
              return false;
          }
        }

        spell.Utylized = true;
        var utylized = UtylizeSpellSource(caster, spellSource);
        Logger.LogInfo("UtylizeSpellSource by caster: " + utylized);
      }
      catch (Exception)
      {
        SoundManager.PlayBeepSound();
        throw;
      }

      return true;
    }

    int godBeepCoolDown = 0;
    public bool TryToBurnMana(LivingEntity caster, int manaCost, bool god)
    {
      if (caster.Stats.Mana < manaCost)
      {
        if (caster is Hero)
        {
          var me = "Not enough mana to cast";
          if (god)
            me += " Slavic god's spell";
          else
            me += " a spell";

         
          var sound = !god || godBeepCoolDown == 0;
          if (god)
          {
            godBeepCoolDown++;// == 0
            //godBeepCoolDown = 10;
          }

          ReportFailure(me, sound);
        }
        return false;
      }
      caster.ReduceMana(manaCost);
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

      return new AttackDescription(caster, caster.UseAttackVariation, AttackKind.PhysicalProjectile, null, pfi);
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
      else if (caster is God)
        AlliesManager.OnPolicyApplied(policy);

      if (AfterApply != null)
        AfterApply(policy);
    }

    public void ReportFailure(string infoToDisplay, bool playBeepSound = true)
    {
      if(playBeepSound)
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

    public AbilityManager AbilityManager { get; private set; }
   

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

    public void DoCommand(Enemy enemyChemp, CommandUseInfo command) 
    {
      if (command.Kind == EntityCommandKind.Resurrect)
      {
        var rts = GetRessurectTargets(enemyChemp);
        foreach (var dead in rts)
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
      Logger.LogError("ForceNextTurnOwner! TurnOwner: " + Context.TurnOwner);
      this.EnemiesManager.ForcePendingForAllIdleFalse();
      this.AlliesManager.ForcePendingForAllIdleFalse();
      this.AnimalsManager.ForcePendingForAllIdleFalse();
      Hero.State = EntityState.Idle;//with state Attacking no move was possible
      Context.MoveToNextTurnOwner();
    }

    public bool CanCallIdentify(LivingEntity priceProvider, Tile shownFor)
    {
      if (priceProvider is Hero)
      {
        return true;
      }

      return false;
    }

    public virtual void OnBeforeAlliesTurn()
    {
      AbilityManager.HandleSmokeTiles();

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


    //bool IsUsing(LivingEntity le, DestPointDesc dpd)
    //{
    //  return dpd.MoveOnPathTarget == 
    //        dpd.State == DestPointState.StayingAtTarget;
    //}

    public bool IsUsingPrivy(LivingEntity le, DestPointDesc dpd)
    {
      return dpd.MoveOnPathTarget is Privy && 
        le.point == dpd.MoveOnPathTarget.point && 
        dpd.State == DestPointState.StayingAtTarget;
      
    }

    public virtual God createGod(SpellKind sk)
    {
      var god = new God(Container);
      return god;
    }

    public virtual List<LivingEntity> GetLivingEntitiesForGodSpell(bool allies, SpellKind sk)
    {
      var les = new List<LivingEntity>();
      if (sk == SpellKind.Wales && allies)
      {
        les.Add(Hero);
        foreach (var ally in AlliesManager.AllAllies)
        {
          if (ally is God)
            continue;

          les.Add(ally as LivingEntity);
        }
      }
      return les;
    }

    public IEnumerable<LivingEntity> GetEnemiesForGodAttack(int range)//SwiatowitScroll.MaxRange
    {
      return EnemiesManager.GetActiveEnemiesInvolved()
                .Where(i => i.DistanceFrom(Hero) <= range)
                .OrderBy(i => i.DistanceFrom(Hero))
                .Take(5);
    }

    public bool IsEnemyToHero(Tile tile)
    {
      LivingEntity le = null;
      if (tile is INPC npc)
        le = npc.LivingEntity;
      else
        le = tile as LivingEntity;

      if(le is null)
        return false;
      return EnemiesManager.Contains(le);
    }

    public virtual int GetStartWalkToInteractiveTurnsCount()
    {
      return 15;
    }

    public void MakeFakeClones(LivingEntity fo, LivingEntity victomToSorround, bool sorroundHero)
    {
      for (int i = 0; i < 3; i++)
      {
        var le = new Enemy(Container);
        le.tag1 = "cloned_" + fo.tag1;
        var emp = this.CurrentNode.GetClosestEmpty(this.Hero);
        le.SetRandomSpellSource();
        var lev = fo.Level / 3;
        if (lev == 0)
          lev = 1;
        AppendEnemy(le, emp.point, lev);
        le.SetFakeLevel(victomToSorround.Level);

        le.Name = fo.Name;
        le.DisplayedName = fo.DisplayedName;
      }
    }

    static Dictionary<string, string> MessageToVoice = new Dictionary<string, string>()
    {
      { "Kill Him!", "kill_him"},
      { "I'll crush you!", "crush_you"},
      { "Join our amy of skeletons", "join_skeletons"},
      { "I'll burn you", "I_will_burn_you"},
      { "You'll be my slave for enhernity ", "my_slaves"}
    };
    public string SayEnemyWelcomeVoice(Enemy enemyTile)
    {
      var message = MessageToVoice.Keys.ToList().GetRandomElem();
      var  voice = MessageToVoice[message];
      if (enemyTile.tag1.StartsWith("fallen_one"))
      {
        message = "I'll crush you!";
        voice = "FallenOne_"+ voice;
      }
      SoundManager.PlayVoice(voice);
      return message;
    }

    public bool IsAnyManagerBusy(EntitiesManager.BusyOnesCheckContext context, ref string message)
    {
      message = "";
      bool busy = false;
      var mgs = new EntitiesManager[] { AlliesManager, AnimalsManager, EnemiesManager, NpcManager };

      foreach (var mg in mgs)
      {
        var bos = mg.FindBusyOnes(context);
        
        if (bos.Any())
        {
          busy = true;
          message += mg.GetType() + " has busy ones: " + bos.Count + ", 1st: " + bos.First() + "; ";
        }
        if(context == EntitiesManager.BusyOnesCheckContext.ForceIdle)
          mg.CheckBusyOnes = true;
      }

      return busy;
    }
  }
}
