using Dungeons.Core.Tiles.Abstract;
using Dungeons.Tiles;
using OuaDII.Discussions;
using OuaDII.Extensions;
using OuaDII.Generators;
using OuaDII.LootFactories.Equipment;
using OuaDII.Quests;
using OuaDII.Serialization;
using OuaDII.State;
using OuaDII.TileContainers;
using OuaDII.Tiles.Interactive;
using OuaDII.Tiles.Looting;
using Roguelike;
using Roguelike.Abilities;
using Roguelike.Abstract.Inventory;
using Roguelike.Crafting;
using Roguelike.Discussions;
using Roguelike.Events;
using Roguelike.LootContainers;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Settings;
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;


namespace OuaDII.Managers
{
  public enum AutoSaveContextValues { Unset, NewGame, ContextSwitched, QuestAssigned, LevelUp, PlaceFound, QuestDone }
  public class GameManager : Roguelike.Managers.GameManager
  {
    public const string GameOnePitDown = "pit_down_big_blocked";
    public GameContext OuaDContext { get { return Context as GameContext; } }
    public World World { get => OuaDContext.World; set => OuaDContext.World = value; }
    public QuestManager QuestManager { get; set; }
    public PortalManager PortalManager { get; set; }

    public IPersister OuaDPersister
    {
      get { return Persister as IPersister; }
    }

    public OuaDII.Tiles.LivingEntities.Hero OuadHero
    {
      get { return Hero as OuaDII.Tiles.LivingEntities.Hero; }
    }

    public GameManager(Container container) : base(container)
    {
      QuestManager = new QuestManager(this);
      PortalManager = new PortalManager(this);
      LootManager.PowerfulEnemyLoot["Miller Bratomir"] = "Kafar";

      DungeonLevelStairsHandler = (int destLevelIndex, Stairs stairs) =>
      {
        var currentLevel = CurrentNode as GameLevel;
        var pit = GetPitByLevel(currentLevel);
        GameLevel destlevel = null;
        if (stairs.StairsKind == StairsKind.LevelDown)
        {
          if (pit.Levels.Count <= destLevelIndex)
            destlevel = AddNewLevel(pit);
          else
            destlevel = pit.Levels[destLevelIndex];
        }
        else if (stairs.StairsKind == StairsKind.LevelUp)
        {
          destlevel = pit.Levels[destLevelIndex];
        }
        else
          Logger.LogError("stairs.Kind == " + stairs.Kind);

        SetContext(destlevel, Hero, GameContextSwitchKind.DungeonSwitched, () => { }, stairs);
        return InteractionResult.ContextSwitched;
      };

      this.Context.ContextSwitched += Context_ContextSwitched;
    }

    private void Context_ContextSwitched(object sender, ContextSwitch arg)
    {
      if (arg.Kind == GameContextSwitchKind.DungeonSwitched)
        EnsureAutoSave(AutoSaveContextValues.ContextSwitched);
    }


    public AutoSaveContextValues AutoSaveContext { get; private set; }
    bool asyncSave = true;
    int quickSaveAttempts = 0;

    public bool EnsureAutoSave(AutoSaveContextValues context)
    {
      if (AutoSaveContext != AutoSaveContextValues.Unset)
        return false;//TODO queue
      AutoSaveContext = context;
      if (context == AutoSaveContextValues.NewGame || asyncSave)
      {
        quickSaveAttempts = 5;
        DoAutoQuickSave();
      }
      else//game must trigger it after showing a hint panel
        AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.AutoQuickSaveStarting, Info = "Auto Quick Saving..." });

      return true;
    }

    public bool DoAutoQuickSave(Roguelike.Serialization.Serialized serialized = null)
    {
      bool result = false;
      if (!GameSettings.Serialization.AutoQuickSave)
        return result;

      if (asyncSave)
      {
        if (serialized == null)
        {
          serialized = new Roguelike.Serialization.Serialized();
          serialized.HeroName = Hero.Name;
          serialized.Hero = Persister.MakeHero(Hero);
        }

        Task.Run(() =>
        {
          Save(true, serialized);
          result = true;
        }).ContinueWith(task =>
        {
          if (!result && quickSaveAttempts > 0)
          {
            quickSaveAttempts--;
            DoAutoQuickSave(serialized);
          }
          else
            AutoSaveContext = AutoSaveContextValues.Unset;
        }
        );
      }
      else
      {
        Save(true);
        AutoSaveContext = AutoSaveContextValues.Unset;
        result = true;
      }

      return result;
    }

    DungeonPit GetPitByLevel(GameLevel level)
    {
      var pit = this.World.Pits.Where(i => i.Levels.Any(j => j == level)).SingleOrDefault();
      return pit;
    }

    protected override void InitNode(AbstractGameLevel node, Roguelike.State.GameState gs, GameContextSwitchKind context)
    {
      if (node.Inited)
        Assert(false);
      base.InitNode(node, gs, context);
      if (node is World world)
      {
        //var hiddenOnes = world.HiddenTiles;
        //hiddenOnes.vaForEach()
      }
      PrepareLevels(node, gs, context);

    }

    public static bool GeneratePredefiniedRooms;//TODO for UT

    private void PrepareLevels(AbstractGameLevel node, Roguelike.State.GameState gs, GameContextSwitchKind context)
    {
      if (context == GameContextSwitchKind.DungeonSwitched)
        return;
      var world = node as World;
      if (world == null)
        world = World;
      if (context == GameContextSwitchKind.NewGame && World == null)
        World = world;

      foreach (var pit in world.Pits)
      {
        if(
          (pit.QuestKind == QuestKind.Smiths || pit.QuestKind == QuestKind.Malbork)
          && GeneratePredefiniedRooms)
        {
          if (!pit.Levels.Any())
          {
            SetLevelAtIndex(gs, pit, 0);
            continue;
          }
        }
        for (int levelIndex = 0; levelIndex < pit.Levels.Count; levelIndex++)
        {
          var dl = pit.Levels[levelIndex];
          var regenerateLevel = true;// pit.QuestKind != QuestKind.Smiths;
          if (regenerateLevel)
          {
            SetLevelAtIndex(gs, pit, levelIndex);
          }
          else
          {
            base.InitNode(dl, gs, context);
            if (context == GameContextSwitchKind.GameLoaded)
              dl.OnLoadDone();
          }
        }
      }
    }

    private void SetLevelAtIndex(Roguelike.State.GameState gs, DungeonPit pit, int levelIndex)
    {
      var newLevel = CreateLevel(pit, levelIndex);//created by UI (Game) on BL (tests)
      newLevel.Index = levelIndex;

      if (pit.QuestKind == QuestKind.Smiths)
      {
        Merchant destMerch;
        if (pit.Levels.Any())
        {
          var srcMerch = pit.Levels[0].GetTiles<Merchant>().SingleOrDefault();//save data shall not be lost
          destMerch = newLevel.GetTiles<Merchant>().SingleOrDefault();
          if (destMerch == null)
          {
            newLevel.SetTile(srcMerch, newLevel.GetEmptyTiles().First().point);
          }
          else
            destMerch.Discussion = srcMerch.Discussion;
        }
        else
        {
          var srcMerch = newLevel.GetTiles<Merchant>().SingleOrDefault();
          if (srcMerch == null)
          {
            var generator = new WorldGenerator(Container, this);
            generator.CreateMerchant(newLevel, DiscussionFactory.MerchantZiemowitName);
            var npc = newLevel.GetTiles<Merchant>().SingleOrDefault();
            npc.Discussion = new DiscussionFactory(Container).Create(npc);//BUG!!!
          }
        }
        destMerch = newLevel.GetTiles<Merchant>().SingleOrDefault();
        InitDiscussion(destMerch, destMerch.Discussion.AsOuaDDiscussion());
      }

      if (pit.Levels.Count <= levelIndex)
        pit.AddLevel(newLevel);
      else
        pit.SetLevel(levelIndex, newLevel);

      var heroPathValue = gs.HeroPath;
      if (!string.IsNullOrEmpty(heroPathValue.Pit))
      {
        if (heroPathValue.Pit == pit.Name && heroPathValue.LevelIndex > levelIndex)
        {
          //?? details?
          //newLevel.Reveal(true);//TODO, due to bugs with reveal of rooms it's better to do it for whole level  
        }

      }
    }

    private void RestoreEmptyTiles(World world)
    {
      world.DoGridAction((int col, int row) =>
      {
        if (world.Tiles[row, col] == null)
        {
          var pt = new Point(col, row);
          world.SetEmptyTile(pt);
          var tile = world.GetTile(pt);
          tile.Revealed = true;
        }
      });
    }

    public override void Load
    (
      string heroName, 
      bool quick, 
      Action<Hero> postLoad = null,
      bool useSavePath = false
    )
    {
      context.Hero = null;
      var persistancyWorker = new Roguelike.Serialization.PersistancyWorker();

      //load world
      var node = persistancyWorker.Load(heroName, this, quick, (Hero hero, Roguelike.State.GameState gs, bool quick) =>
      {
       hero.Abilities.EnsureItems();//new game version might added...
       context.Hero = hero;
       
       var world = this.World;
       if(world!=null)//ut can have null
        Logger.LogInfo("Walls: " + world.GetTiles<Wall>().Count);
       this.OuaDPersister.LoadWorld(hero.Name, quick, world);
       Logger.LogInfo("Walls01: " + world.GetTiles<Wall>().Count);
       world.Logger = this.Logger;
       RestoreEmptyTiles(world);

       var pits = OuaDPersister.LoadPits(hero.Name, quick);
       var pitEntries =  world.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.PitDown).ToList();
      foreach (var pe in pitEntries)
      {
          if (!pits.Any(i=>i.Name == pe.pitName))
          {
            var pit = new DungeonPit();
            pit.Name = pe.pitName;
            pits.Add(pit);
          }
      }
       world.Pits = pits;

       //InitNode(world, gs, GameContextSwitchKind.GameLoaded);//will be done on ContextChange
       this.AlliesManager.AllyBehaviour = (gs as State.GameState).AllyBehaviour;
       var startingNodeInfo = context.PlaceHeroAtDungeon(world, gs, GameContextSwitchKind.GameLoaded, null);
       Logger.LogInfo("Walls1: " + world.GetTiles<Wall>().Count);
       if (postLoad!=null)
        postLoad(hero);

       return startingNodeInfo.Node;
     });
            
      var hero = context.Hero;
      if (hero.State != EntityState.Idle)
        hero.State = EntityState.Idle;

      if(postLoad!=null)
        SetLoadedContext(node, hero);
      
      if (GameState.CoreInfo.PermanentDeath)
      {
        //var persister = Container.GetInstance<IPersister>();
        OuaDPersister.DeleteGame(heroName, quick);
        OuaDPersister.DeleteGame(heroName, !quick);
      }
      PrintHeroStats("GameManager load end");
      //EventsManager.AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.GameFinished});
    }

    public override void SetLoadedContext(AbstractGameLevel node, Roguelike.Tiles.LivingEntities.Hero hero)
    {
      base.SetLoadedContext(node, hero);
    }

    public bool SaveInProgress { get; set; }
    bool saveInProgressIsQuick;

    public int SaveCounter { get; set; }

    public override void Save(bool quick, Roguelike.Serialization.Serialized serialized = null)
    {
      try
      {
        if (SaveInProgress && saveInProgressIsQuick == quick)
          throw new Exception("SaveInProgress");//TODO saveInProgressIsQuick is async , queue it

        SaveInProgress = true;
        saveInProgressIsQuick = quick;
        context.Logger.LogInfo("Save " + Hero.Name + " " + gameState);
        var persistancyWorker = new Roguelike.Serialization.PersistancyWorker();

        var world = World;
        OuadGameState.AllyBehaviour = this.AlliesManager.AllyBehaviour;

        persistancyWorker.Save(this, (bool quick) =>
        {
          if (world != null)
          {
            OuaDPersister.SaveWorld(Hero.Name, world, quick);
            var pitsToSave = world.Pits.Where(i => i.QuestKind != QuestKind.Unset).ToList();
            OuaDPersister.SavePits(Hero.Name, pitsToSave, quick);
          }
        }, quick, serialized);

        SaveCounter++;
      }
      finally
      {
        SaveInProgress = false;
      }
      //na catch - throw it outside
    }

    protected override void InitNodeOnLoad(AbstractGameLevel node)
    {
    }

    public override void OnHeroPolicyApplied(Roguelike.Policies.Policy policy)
    {
      if (policy.Kind == PolicyKind.Move && CurrentNode == this.World)
      {
        var gp = this.World.WorldSpecialTiles.GroundPortals.FirstOrDefault(i => i.point == Hero.point);
        if (gp != null)
        {
          EventsManager.AppendAction(new InteractiveTileAction(gp) { InteractiveKind = InteractiveActionKind.HitGroundPortal });
          return;
        }
      }
      base.OnHeroPolicyApplied(policy);
    }

    public override InteractionResult InteractHeroWith(Tile tile)
    {
      var result = InteractionResult.None;

      if (tile is Tiles.LivingEntities.Paladin)
      {
        //gm.AppendAction<NPCAction>((NPCAction ac) => { ac.NPCActionKind = NPCActionKind.Engaged; ac.InvolvedTile = npc; });
        result = InteractionResult.Blocked;
        return result;
      }

      if (tile is Dungeons.Tiles.IObstacle)
      {
        if (tile is Stairs)
        {
          var stairs = tile as Stairs;
          if (stairs.StairsKind == StairsKind.PitDown || stairs.StairsKind == StairsKind.PitUp)
          {
            var world = World;
            var pit = world.GetPit(stairs.PitName);
            if (stairs.PitName == GameOnePitDown)
            {
              SoundManager.PlayBeepSound();

              EventsManager.AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.HitGameOneEntry, Info = "Dungeon from part one of the game - buried with stones" });
              return InteractionResult.Blocked;
            }

            if (stairs.StairsKind == StairsKind.PitDown)
            {
              GameLevel level = null;
              if (!pit.Levels.Any())
              {
                level = AddNewLevel(pit);
              }
              else
                level = pit.Levels.First();

              if(level!=CurrentNode)//avoid double SetContext for the same mouse click
                SetContext(level, Hero, GameContextSwitchKind.DungeonSwitched, () => { }, stairs);

              //var heroInW = world.GetTiles<Roguelike.Tiles.LivingEntities.Hero>().SingleOrDefault();
              //var st = world.GetTiles<Stairs>();
              //Debug.Assert(heroInW == null);
            }
            else
            {
              if (world != CurrentNode)//avoid double SetContext for the same mouse click
              {
                var st = world.GetTiles<Stairs>();
                SetContext(world, Hero, GameContextSwitchKind.DungeonSwitched, () => { }, stairs);
              }
            }
            result = InteractionResult.ContextSwitched;
          }
        }
        //else if (tile is global::OuaDII.Tiles.Interactive.GroundPortal)
        //{
        //  EventsManager.AppendAction(new InteractiveTileAction(tile as InteractiveTile) { InteractiveKind = InteractiveActionKind.HitGroundPortal });
        //  return InteractionResult.None;
        //}
      }
      if (result == InteractionResult.None)
        result = base.InteractHeroWith(tile);
      return result;
    }

    private GameLevel CreateLevel(DungeonPit pit, int levelIndex)
    {
      GameLevel level;
      var lg = pit.LevelGenerator ?? World.CreatePitGenerator(pit);
      level = lg.GenerateLevel(pit, null, levelIndex);
      level.BossRoomLeverSet = lg.LeverSet;
      if (pit.Name.Contains("Mine"))
      {
        var roslaw = level.GetTiles<Tiles.LivingEntities.Paladin>().Where(i => i.tag1.Contains("Roslaw")).FirstOrDefault();
        if (roslaw != null)
        {
          if (this.GameState.History.WasEngaged("paladin__name__Roslaw"))
          {
            level.SetTile(new Tile(), roslaw.point);
          }
          else
          {
            roslaw.SetLevel(15);
            if (roslaw.Stats.Health < 500)
            {
              roslaw.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Health, 500);
            }
            roslaw.Discussion = Container.GetInstance<DiscussionFactory>().Create(roslaw);
            roslaw.SetHasUrgentTopic(true);
          }
        }
      }
      return level;
    }

    public GameLevel AddNewLevel(DungeonPit pit)
    {
      var level = CreateLevel(pit, pit.Levels.Count);
      if (level != null)
        pit.AddLevel(level);
      return level;
    }

    public override Roguelike.State.GameState PrepareGameStateForSave()
    {
      var gameState = base.PrepareGameStateForSave();
      gameState.HeroPath.World = World != null ? World.Name : "?";

      return gameState;
    }

    protected override Roguelike.Managers.EnemiesManager CreateEnemiesManager(Roguelike.GameContext context, EventsManager eventsManager)
    {
      return new OuaDII.Managers.EnemiesManager(Context as OuaDII.GameContext, EventsManager, Container, AlliesManager, this);
    }

    public override void SetContext
    (
      AbstractGameLevel node, 
      Roguelike.Tiles.LivingEntities.Hero hero, 
      GameContextSwitchKind kind,
      Action after,    
      Stairs stair = null,
      Roguelike.Tiles.Interactive.Portal portal = null
      )
    {
      if (kind == GameContextSwitchKind.NewGame)
      {
        
      }

      try
      {
        Logger.LogInfo("Trying to load testing data...");
        var pers = new JSONPersister(this.Container);
        var td = pers.LoadTestingData();
        if (td != null)
        {
          hero.AbilityPoints += td.AbilitiesPoints;
          hero.LevelUpPoints += td.LevelUpPoints;
          hero.Level += td.HeroLevel;
          Logger.LogInfo("testing data added...");
        }
        else
          Logger.LogInfo("not testing data available...");
      }
      catch (Exception ex)
      {
        Logger.LogError("testing data ex:"+ex);
      }

      base.SetContext(node, hero, kind, after, stair, portal);

      if (kind == GameContextSwitchKind.NewGame ||
          kind == GameContextSwitchKind.GameLoaded ||
          kind == GameContextSwitchKind.DungeonSwitched)
      {
        SetNPCContext(node, kind);
      }

      if (kind == GameContextSwitchKind.NewGame ||
        kind == GameContextSwitchKind.GameLoaded)
      {
        OuadHero.ShortcutsBar.ActiveItemDigitSet += ShortcutsBar_ActiveItemDigitSet;
      }

      if (kind != GameContextSwitchKind.NewGame)
        World.EnsurePitsGenerators();

    }

    private void ShortcutsBar_ActiveItemDigitSet(object sender, EventArgs e)
    {
      if (OuadHero.ShortcutsBar.ActiveItemDigit >= 0)
      { 
        //AddLastingEffectFromAbility()
      }
    }

    private void SetNPCContext(AbstractGameLevel node, GameContextSwitchKind kind)
    {
      var NPCs = node.GetTiles<Roguelike.Tiles.LivingEntities.INPC>();

      if (!node.EventsHooked)
      {
        var discussionFactory = this.Container.GetInstance<DiscussionFactory>();
        NPCs.ForEach(inpc =>
        {
          var hounds = node.GetNeighborTiles<TrainedHound>(inpc as Tile, true);
          if (hounds.Any())
            inpc.TrainedHound = hounds.First();

          InitDiscussion(kind, inpc, discussionFactory);
        });
      }

      var merchs = NPCs.Where(i => i is OuaDII.Tiles.LivingEntities.Merchant).Cast<OuaDII.Tiles.LivingEntities.Merchant>().ToList(); //node.GetTiles<OuaDII.Tiles.LivingEntities.Merchant>();
      merchs.ForEach(merch =>
      {
        PopulateMerchantInv(merch, Hero.Level);
        //if (merch.Name.Contains("Lionel"))
        //{
        //  merch.Discussion = DiscussionFactory.CreateForLionel(merch.TrainedHound != null);
        //}
        //else
        //  merch.Discussion = DiscussionFactory.Create(merch.Name, false);
      });

      var paladin = this.AlliesManager.AllAllies.Where(i => i is OuaDII.Tiles.LivingEntities.Paladin && i.Name.Contains("Roslaw")).FirstOrDefault();
      if (node is World && paladin != null && GameState.History.WasEngaged((paladin as LivingEntity).tag1))
      {
        this.AlliesManager.RemoveAlly(paladin);
      }
    }

    public void InitDiscussion(GameContextSwitchKind kind, INPC inpc, DiscussionFactory discussionFactory)
    {
      if (kind == GameContextSwitchKind.NewGame ||
          inpc.Discussion == null || inpc.Discussion.MainItem.Topics.Count() == 0)
      {
        inpc.Discussion = discussionFactory.Create(inpc);
        if (inpc.Discussion.MainItem.Topics.Count > 1)//0 is Bye
          inpc.SetHasUrgentTopic(true);
      }

      var disc = inpc.Discussion as OuaDII.Discussions.Discussion;
      if (kind == GameContextSwitchKind.NewGame ||
          kind == GameContextSwitchKind.GameLoaded)
      {
        InitDiscussion(inpc, disc);
      }
    }

    public void InitDiscussion(INPC inpc, Discussions.Discussion disc)
    {
      //disc.CheatingDetected += (object s, Roguelike.Quests.QuestRequirement qr) =>
      //{

      //};

      disc.SetParents();
    }

    protected override void OnActionAppended(GameEvent e)
    {
      base.OnActionAppended(e);
      if (e is LivingEntityAction lea)
      {
        if (lea.Kind == LivingEntityActionKind.Moved && lea.InvolvedEntity is Roguelike.Tiles.LivingEntities.Hero)
        {
          if (this.CurrentNode.ApproachableByHero != null)
          {
            var gps = this.CurrentNode.ApproachableByHero.Where(i => i.DistanceFrom(this.Hero) < 6 && !i.ApproachedByHero).ToList();
            gps.ForEach(i =>
            {
              if (i.Activate())
              {
                if (!string.IsNullOrEmpty(i.ActivationSound))
                  SoundManager.PlaySound(i.ActivationSound);
              }
            });
          }
        }
        else if (lea.Kind == LivingEntityActionKind.Died)
        {
          if (lea.InvolvedEntity is Enemy en)
          {
            if (en.PowerKind == EnemyPowerKind.Boss)
            {
              if (en.tag1 == "Miller Bratomir")
              {
                var hourGlassQuest = this.OuadHero.GetQuest(QuestKind.HourGlassForMiller);
                if (hourGlassQuest != null && hourGlassQuest.Status == Roguelike.Quests.QuestStatus.Accepted)
                {
                  hourGlassQuest.Status = Roguelike.Quests.QuestStatus.FailedToDo;
                  AppendAction(new QuestAction() { QuestID = (int)QuestKind.HourGlassForMiller, QuestActionKind = QuestActionKind.Unset, Info = "Quest aborted" });
                  GetNPC("Lionel").Discussion.AsOuaDDiscussion().UpdateQuestDiscussion(QuestKind.HourGlassForMiller, KnownSentenceKind.AwaitingReward, null, hourGlassQuest);
                }
              }
            }
            if(en.tag1 == "boar_butcher")
            {
              this.QuestManager.SetQuestAwaitingReward(QuestKind.KillBoar);
              //this.QuestManager.GetHeroQuest(QuestKind.KillBoar).Status = Roguelike.Quests.QuestStatus.Done;
            }
          }
        }
      }

      else if (e is LootAction la)
      {
        //TODO
        //if (la.EquipmentKind == EquipmentKind.God)
        //{
        //  if (la.Kind == LootActionKind.PutOn)
        //  {
        //    var god = new God(Container);
        //    god.Active = true;
        //    AlliesManager.AddEntity(god);
        //  }
        //  else if (la.Kind == LootActionKind.PutOff)
        //  {
        //    var god = AlliesManager.AllAllies.Where(i => i is God).SingleOrDefault();
        //    god.Active = false;
        //    AlliesManager.RemoveAlly(god);
        //  }
        //}
      }
      if (e is ShortcutsBarAction sa)
      {
        if (sa.Kind == ShortcutsBarActionKind.ShortcutsBarChanged)
        {
          if (OuadHero.ShortcutsBar.GetAt(sa.Digit) is ActiveAbility ab)
          {
            if (ab.AutoApply)
            {
              UseActiveAbility(null, Hero, ab.Kind);
            }
          }
        }
      }

      QuestManager.HandleGameAction(e);
    }

    bool RemoveFromInv(Loot srcLoot, int stackedCount, Inventory inv)
    {
      var lootToDel = inv.Get(srcLoot);
      if (lootToDel != null)
      {
        var removeRes = inv.Remove(srcLoot, new RemoveItemArg() { StackedCount = stackedCount });
        return removeRes != null;
      }
      return false;
    }

    public CraftingResult Craft(List<Loot> lootItemsToConvert, Recipe recipe, Inventory craftingInNonPanelSrcInv)
    {
      var crafter = Container.GetInstance<LootCrafterBase>();
      var result = crafter.Craft(recipe, lootItemsToConvert);

      if (result.Success)
      {
        var srcInvs = HeroInventoryManager.GetHeroInventories();
        foreach (var srcLoot in lootItemsToConvert)
        {
          //DeleteCraftedLoot - normally true but in rare cases when Eq is enhanced/fixed (e.g.Magical weapon recharge) false
          if (result.DeleteCraftedLoot || !result.LootItems.Contains(srcLoot))
          {
            int stackedCount = 1;
            if (craftingInNonPanelSrcInv == null)
            {
              if (srcLoot is StackedLoot stacked)
              {
                if (stacked is MagicDust)
                  stackedCount = recipe.MagicDustRequired;
                else
                {
                  stackedCount = 1;// stacked.Count;
                  if (recipe.Kind == RecipeKind.Arrows ||
                    recipe.Kind == RecipeKind.Bolts)
                  {
                    if (srcLoot is Feather || srcLoot is Hazel)
                      stackedCount = (result.LootItems[0] as StackedLoot).Count;
                  }
                  else if (recipe.Kind == RecipeKind.Toadstools2Potion && srcLoot is Mushroom)
                    stackedCount = 3;
                }
              }
            }

            var srcInv = craftingInNonPanelSrcInv != null ? craftingInNonPanelSrcInv : Hero.Crafting.InvItems.Inventory;
            bool removeRes = RemoveFromInv(srcLoot, stackedCount, srcInv);
            if (!removeRes)
            {
              //TODO can it happen?
              foreach (var inv in srcInvs)
              {
                var lootToDel = inv.Get(srcLoot);
                if (lootToDel != null)
                {
                  removeRes = inv.Remove(srcLoot, new RemoveItemArg() { StackedCount = stackedCount }) != null;
                  if (removeRes)
                    break;
                }
              }
            }
            else
            {

              if (srcLoot is StackedLoot sl && srcInv.GetStackedCount(sl) == 0)
                sl.Count = 0;

            }
            Assert(removeRes, "failed to remove " + srcLoot);
          }
        }
        if (result.DeleteCraftedLoot)
        {
          //TODO
          var srcInv = craftingInNonPanelSrcInv != null ? craftingInNonPanelSrcInv : Hero.Crafting.InvItems.Inventory;
          foreach (var li in result.LootItems)
            srcInv.Add(li);
        }
        SoundManager.PlaySound("crafting");
      }
      else
        SoundManager.PlayBeepSound();

      if (result.Success)
        Hero.RecalculateStatFactors(false);
      return result;
    }

    public CraftingResult Craft(Recipe recipe)
    {
      var lootToConvert = Hero.Crafting.InvItems.Inventory.Items.ToList();
      return Craft(lootToConvert, recipe, null);
    }

    public State.GameState OuadGameState => this.gameState as OuaDII.State.GameState;


    public override Loot TryGetRandomLootByDiceRoll(LootSourceKind lsk, ILootSource ls)
    {
      if (lsk != LootSourceKind.DeluxeGoldChest && lsk != LootSourceKind.GoldChest /*&& level == 1*/)//level == 1 was rare in UT
      {
        var loot = OuadGameState.ChanceAtGameStart.TryGenerate(this.LootGenerator, OuadGameState, ls);
        if (loot != null)
        {
          var eq = loot as Equipment;
          if (eq == null || eq.LevelIndex >= ls.Level)//test LootLevelMatchesEnemyLevel needs it
          {
            //if (eq != null)
            //  EnsureMaterialFromLootSource(eq);
            return loot;
          }
        }
      }
      return base.TryGetRandomLootByDiceRoll(lsk, ls);
    }

    public override string GetCurrentNodeName()
    {
      string name = World.Name;
      if (!(CurrentNode is World))
        name = base.GetCurrentNodeName();

      return name;
    }

    protected override bool GeneratesLoot(ILootSource ls)
    {
      var tile = ls as Tile;
      return tile.tag1 != "barrel_special_pcim_pond";
    }

    public override string GetPitDisplayName(string pitID)
    {
      return DungeonPit.GetPitDisplayName(pitID);
    }

    public override bool GetGoldInvolvedOnSell(IInventoryOwner src, IInventoryOwner dest)
    {
      if (src is HeroChest || dest is HeroChest)
        return false;
      return base.GetGoldInvolvedOnSell(src, dest);
    }

    public List<Tile> AppendHiddenTiles(string tilesKey, ILootSource trigger = null)
    {
      var tiles = CurrentNode.HiddenTiles.Get(tilesKey);
      int counter = 0;
      foreach (var tile in tiles.Tiles)
      {
        var en = tile as Roguelike.Tiles.LivingEntities.Enemy;
        if (counter == 0 && tilesKey.Contains("Island_Enemies"))
        {
          var chest = trigger as Chest;
          var key = new Key();
          key.Kind = KeyKind.Chest;
          key.KeyName = chest.KeyName;
          en.DeathLoot = key;
          if (en.PowerKind == EnemyPowerKind.Plain)
            en.SetNonPlain(false, true);
        }
        if (tilesKey == QuestManager.PondCreatureMap)
          tile.point = CurrentNode.GetEmptyNeighborhoodPoint(Hero, Dungeons.TileContainers.DungeonNode.EmptyNeighborhoodCallContext.LootPlacement).Item1;
        AppendTile(en, tile.point, trigger != null ? trigger.Level + 1 : en.Level);
        counter++;
      }

      return tiles.Tiles.ToList();
    }

    bool drowned_man_chest_generated = false;

    public override void HandeTileHit(LivingEntity attacker, Tile tile, Policy policy)
    {
      if (tile is Barrel barrel)
      {
        if (tile.tag1 == "barrel_special_pcim_pond")
        {
          if (barrel.OutOfOrder)
          {
            var quest = OuadHero.GetQuest(QuestKind.CreatureInPond);
            if (quest != null)
            {
              barrel.OutOfOrder = false;
            }
          }
          if (!barrel.OutOfOrder)
            AppendHiddenTiles(barrel.UnhidingMapName, barrel);
        }
      }

      if (tile is Chest)
      {
        var chest = tile as Chest;
        if (chest != null && chest.Closed)
        {
          if (chest.Locked && chest.ChestKind == Roguelike.Tiles.Interactive.ChestKind.Gold
             && chest.OriginMap.Contains("Island_Interactive"))
          {
            var key = Hero.Inventory.GetItems<Key>().Where(i => i.KeyName == chest.KeyName).FirstOrDefault();
            if (key == null)
            {
              AppendAction<HeroAction>((HeroAction ac) => { ac.Kind = HeroActionKind.HitLockedChest; ac.Info = "Chest is locked, a key is needed to open it."; });
              chest.KeyName = "drowned_man_chest";
              if (!drowned_man_chest_generated)
              {
                drowned_man_chest_generated = true;
                AppendHiddenTiles(chest.UnhidingMapName, chest);
              }
              return;
            }
            else
            {
              chest.Locked = false;
            }
          }
        }
      }
      base.HandeTileHit(attacker, tile, policy);
      if (tile is GodGatheringSlot ggs)
      {
        HandleStatueHit(ggs);
      }
    }

    void HandleStatueHit(GodGatheringSlot ggs)
    {
      if (ggs.IsOn)
      {
        //ReportFailure("God statue already set");
        var stat = new GodStatue();
        stat.GodKind = ggs.GodKind;
        if (Hero.Inventory.Add(stat))
        {
          SendStatueSlotEvent(ggs, false);
        }
        return;
      }

      var statue = OuadHero.GetGodStatue(ggs.GodKind);
      if (statue != null)
      {
        var removed = OuadHero.Inventory.Remove(statue);
        if (removed != null)
        {
          SendStatueSlotEvent(ggs, true);
          if (World.WorldSpecialTiles.GodGatheringSlots.All(i => i.IsOn))
            OpenGathering();
        }
      }
      else
      {
        AppendAction<OuaDII.Events.HeroAction>((OuaDII.Events.HeroAction ac) =>
        {
          ac.Kind = HeroActionKind.HitWall;
          ac.SpecificKind = Events.SpecificHeroActionKind.HitGatheringGodSlotNoStatueAvailable;
          ac.Info = "God " + ggs.GodKind + " statue not found in the inventory";
          ac.InvolvedTile = ggs;
        });
      }
    }

    public void OpenGathering()
    {

      {
        Stairs pitStairs = GetGatheringStairs();
        if (pitStairs != null)
        {
          pitStairs.Closed = false;
          AppendAction<OuaDII.Events.HeroAction>((OuaDII.Events.HeroAction ac) =>
          {
            ac.Kind = HeroActionKind.HitWall;
            ac.SpecificKind = Events.SpecificHeroActionKind.OpenedGatheringEntry;
            ac.Info = "Entry to the gathering dungeon opened";
            ac.InvolvedTile = pitStairs;
          });
          SoundManager.PlaySound("gathering_open");
          //SorroundHeroOnGathering();
        }
      }
    }

    private Stairs GetGatheringStairs()
    {
      return World.GetAllStairs(StairsKind.PitDown).Where(i => i.PitName == DungeonPit.GetFullPitName(DungeonPit.PitGathering)).SingleOrDefault();
    }

    Enemy sorroundSender;
    public void SorroundHeroOnGathering()
    {
      Stairs pitStairs = GetGatheringStairs();
      
      for (int i = 0; i < 4; i++)
      {

        var enemy = new Enemy(this.Container);
        enemy.tag1 = "lost_soul";
        enemy.Herd = "lost_souls_gate";
        if (i == 1)
        {
          enemy.SetNonPlain(false);
        }
        var empty = CurrentNode.GetClosestEmpty(pitStairs);
        if (pitStairs.Level <= 0)
          pitStairs.Level = 10;
        AppendEnemy(enemy, empty.point, pitStairs.Level);
        if (enemy.PowerKind != EnemyPowerKind.Plain)
        {
          sorroundSender = enemy;
        }
      }

      //place them around entry
      var herdOnes = EnemiesManager.GetEnemies().Where(i => i.Herd == sorroundSender.Herd).ToList();
      herdOnes.ForEach(i => ApplyMovePolicy(i, i.point));
    }

    public void SendSorroundOrder()
    {
      MakeBattleOrder(BattleOrder.Surround, sorroundSender);
      SoundManager.PlaySound("SurroundHim");
      sorroundSender = null;
    }

    public void MakeBattleOrder(BattleOrder order, LivingEntity sender)
    {
      if (sender is Enemy en)
      {
        var herdOnes = EnemiesManager.GetEnemies().Where(i => i.Herd == sender.Herd).ToList();
        herdOnes.ForEach(i => i.BattleOrder = order);

        AppendAction<LivingEntityAction>((LivingEntityAction ac) =>
        {
          ac.Kind = LivingEntityActionKind.MadeBattleOrder;
          ac.InvolvedEntity = sender;
        });
      }
    }

    private void SendStatueSlotEvent(GodGatheringSlot ggs, bool setOn)
    {
      ggs.IsOn = setOn;
      //statue.SetActive(true);
      AppendAction<OuaDII.Events.HeroAction>((OuaDII.Events.HeroAction ac) =>
      {
        ac.Kind = HeroActionKind.HitWall;
        ac.SpecificKind = Events.SpecificHeroActionKind.HitGatheringGodSlot;
        if(setOn)
          ac.Info = "God " + ggs.GodKind + " statue set in the gathering slot";
        else
          ac.Info = "God " + ggs.GodKind + " statue removed from the gathering slot";
        ac.InvolvedTile = ggs;
      });
    }

    public T GetNPC<T>(string name) where T : OuaDII.Tiles.LivingEntities.NPC
    {
      return World.GetTiles<T>().Where(i => i.Name == name).Single();
    }

    public NPC GetNPC(string name)
    {
      var npc = World.GetTiles<NPC>().Where(i => i.Name == name).SingleOrDefault();
      if (npc == null)
        npc = CurrentNode.GetTiles<NPC>().Where(i => i.Name == name).SingleOrDefault();

      if (npc == null)
      {
        foreach (var pit in World.Pits)
        {
          foreach (var level in pit.Levels)
          {
            npc = level.GetTiles<NPC>().Where(i => i.Name == name).SingleOrDefault();

            if (npc != null)
              return npc;
          }
        }
      }
      return npc;
    }



    public override void HandleDeath(LivingEntity dead)
    {
      base.HandleDeath(dead);

      var quest = QuestManager.GetQuestKindFromHerd(dead.Herd);
      if (quest != QuestKind.Unset)
      {
        QuestManager.HandleQuestStatus(quest);
      }
    }

    protected override void OnLootCollected(Loot lootTile)
    {
      if (lootTile.tag1 == "magic_fern")
      {
        var enemies = EnemiesManager.GetEnemies().Where(i => i.Herd == "SwampCursedTrees").ToList();
        enemies.ForEach(i => i.State = EntityState.Idle);
      }
    }

    protected override void PopulateMerchantInv(Merchant merch, int heroLevel)
    {
      base.PopulateMerchantInv(merch, heroLevel);

      var houndEqFactory = new HoundEqFactory(this.Container);
      for (int i = 0; i < 2; i++)
      {
        var loot = houndEqFactory.GetRandom(heroLevel);
        if (loot != null)
          merch.Inventory.Items.Add(loot);
      }
      //lootGenerator.LootFactory.BooksFactory
    }

    public static string GetAssetName(Weapon magicalWeapon)
    {
      string input = magicalWeapon.tag1;
      string pattern = @"\d+$";
      string replacement = "";
      Regex rgx = new Regex(pattern);
      string result = rgx.Replace(input, replacement);
      result += 1;
      return result;
    }

    public override bool TryApplyAttackPolicy(ProjectileFightItem fi, Tile pointedTile, Action<Tile> beforAttackHandler = null)
    {
      var res = false;
      fi.ActiveAbilitySrc = Hero.SelectedActiveAbilityKind;
      try
      {
        res = base.TryApplyAttackPolicy(fi, pointedTile, beforAttackHandler);
      }
      catch (Exception ex)
      {
        fi.ActiveAbilitySrc = Roguelike.Abilities.AbilityKind.Unset;
        AssertExc(ex);
        //throw;cause unity editor to crash!
        return false;

      }
      fi.ActiveAbilitySrc = Roguelike.Abilities.AbilityKind.Unset;
      return res;
    }

    public override InteractionResult HandlePortalCollision(Roguelike.Tiles.Interactive.Portal portal)
    {
      var ouadPortal = portal as OuaDII.Tiles.Interactive.Portal;
      //if (portal.PortalKind == PortalDirection.Src)
      {
        AppendAction<InteractiveTileAction>((InteractiveTileAction ac) =>
        { ac.InteractiveKind = InteractiveActionKind.HitPortal; ac.InvolvedTile = portal; });

        UsePortal(portal, portal.PortalKind == PortalDirection.Src ? GroundPortalKind.Camp : GroundPortalKind.Unset);
      }
      return InteractionResult.Blocked;
    }

    protected override void CreateInputManager()
    {
      inputManager = new InputManager(this);
    }

    void UsePortal(Roguelike.Tiles.Interactive.Portal portal, GroundPortalKind knownPortalDestination)
    {
      PortalManager.UsePortal(portal, knownPortalDestination);
    }

    public void SetNewGameContext(World predefiniedWorld, Hero hero, string heroName, Roguelike.State.CoreInfo info, PlacesLayout placesLayout)
    {
      //var hero = predefiniedWorld.GetTiles<OuaDII.Tiles.LivingEntities.Hero>().SingleOrDefault();
      if (hero == null)
        return;
      GameState.CoreInfo = info;
      OuadGameState.PlacesLayout = placesLayout;
      hero.Name = heroName;

      SetContext(predefiniedWorld, hero, GameContextSwitchKind.NewGame, () => { });
    }

    override public void PrepareDiscussionForShowing(INPC npc)
    {
      if (npc.LivingEntity.tag1 == "ally_NPC_Julek")
      {
        var count = this.GameState.History.LivingEntity.CountByTag1("boar_butcher");
        if (count == 1)
        {
          var disc = npc.Discussion.AsOuaDDiscussion();
          var taskProgress = disc.GetTopicByQuest(QuestKind.KillBoar);
          if(taskProgress!=null && QuestManager.GetHeroQuest(QuestKind.KillBoar) == null)
            disc.MainItem.Topics.Remove(disc.MainItem.GetTopic(KnownSentenceKind.WhatsUp));//TODO change disc
          //disc.RemoveDoneQuest()
          int k = 0;
          k++;
        }
      }
    }

    public override void OnBeforeAlliesTurn()
    {
      if (god != null && Hero.CurrentEquipment.GodActivated)
      {
        AlliesManager.AddEntity(god);
      }
      base.OnBeforeAlliesTurn();
      
    }

    public override void OnAfterAlliesTurn()
    {
      AlliesManager.RemoveEntity(god);
    }

    OuaDII.Tiles.LivingEntities.God god;
    public void SetGodPowerState(GodStatue godStatue, bool setOn)
    {
      if (setOn)
      {
        //AlliesManager.RemoveGod();//remove if any
        if (god == null || god.GodStatue != godStatue)
        {
          god = new OuaDII.Tiles.LivingEntities.God(Container);
          god.GodStatue = godStatue;
          god.Alive = true;
          god.Revealed = true;
          god.Point = Hero.point;
          god.GameManager = this;
        }
        //AlliesManager.AddEntity(god);
        Hero.CurrentEquipment.GodActivated = true;
      }
      else
      {
        god = null;
        Hero.CurrentEquipment.GodActivated = false;
        //god = AlliesManager.RemoveGod() as OuaDII.Tiles.LivingEntities.God;
      }
      //god.GodStatue.SetPowerActive(setOn);
    }

    protected override bool HasAbilityActivated(ActiveAbility ab, AdvancedLivingEntity ale)
    {
      var baseRes = base.HasAbilityActivated(ab, ale);
      if (baseRes)
        return true;

      return false;
    }
  }
}

