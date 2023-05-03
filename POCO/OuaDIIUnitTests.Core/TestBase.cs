using NUnit.Framework;
using OuaDII.Discussions;
using OuaDII.Generators;
using OuaDII.Quests;
using OuaDII.Serialization;
using OuaDII.State;
using OuaDII.TileContainers;
using Roguelike;
using Roguelike.Abilities;
using Roguelike.Abstract.Multimedia;
using Roguelike.Abstract.Projectiles;
using Roguelike.Attributes;
using Roguelike.Abstract.Spells;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Multimedia;
using Roguelike.State;
using Roguelike.Tiles;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Roguelike.Managers.Policies;

namespace OuaDIIUnitTests
{
    [TestFixture]
  class TestBase : ITestBase
  {
    GameManager gameManager;
    protected Container containerWorld;
    protected Container Container;

    public OuaDII.Managers.GameManager GameManager 
    { 
      get 
      {
        return gameManager as OuaDII.Managers.GameManager;
      }
    }

    [SetUp]
    public void Init()
    {
      //GenerationInfo.GenerateDynamicTiles = false;
      containerWorld = null;
      Container = null;
      gameManager = null;
      WriteLine("*");
      WriteLine("**");
      WriteLine("***");
      WriteLine("TestBase Init!!!! "+ TestContext.CurrentContext.Test.Name + " gameManager :"+ gameManager);
      Dungeons.Tiles.Tile.IncludeDebugDetailsInToString = true;
      OnInit();
    }

    protected void EnsureUniqNames(Enemy enFirst, Enemy enSec)
    {
      if (enFirst.Name == enSec.Name)
      {
        var newName = "Skeleton";
        if (enFirst.Name == "Skeleton")
          newName = "Spider";

        enSec.Name = newName;
      }
    }
    protected virtual void OnInit() { }

    [TearDown]
    public void Destroy()
    {
     
      try
      {
        Log("TestBase TearDown!!!! " + TestContext.CurrentContext.Test.Name + " gameManager :" + gameManager);
        if (containerWorld != null)
        {
          var pers = new JSONPersister(containerWorld);
          pers.DeleteTestingData();
        }
        WriteLine("***");
        WriteLine("**");
        WriteLine("*");
      }
      catch (Exception ex)
      {
        Log(ex.Message);
      }
    }

    private void Log(string v)
    {
      Debug.WriteLine(v);
    }

    protected Container CreateContainer()
    {
      containerWorld = new OuaDII.ContainerConfigurator().Container;
      InitContainer(containerWorld);

      //containerWorld.Register<IProjectilesFactory, Roguelike.Generators.ProjectilesFactory>();
      //containerWorld.Register<IPersister, OuaDII.Serialization.JSONPersister>();
      //containerWorld.Register<Roguelike.GameState, OuaDII.GameState>();


      Container = containerWorld;
      return Container;
    }

    private void InitContainer(Container container)
    {
      container.Register<IQuestRoomCreator, ProceduralQuestRoomCreator>();
      container.Register<ISoundPlayer, BasicSoundPlayer>();
      container.Register<IProjectilesFactory, Roguelike.Generators.ProjectilesFactory>();
      container.Register<IStaticSpellFactory, Roguelike.Generators.StaticSpellFactory>();
    }

    protected void CreateManager()
    {
      gameManager = containerWorld.GetInstance<GameManager>();
      gameManager.EventsManager.EventAppended += EventsManager_ActionAppended;
    }

    private void EventsManager_ActionAppended(object sender, GameEvent e)
    {
      if (e is GameStateAction gsa)
      {
        //TODO
        if (gsa.Type == GameStateAction.ActionType.AutoQuickSaveStarting)
          (gameManager as OuaDII.Managers.GameManager).DoAutoQuickSave();
      }
    }

    //GenerationInfo generationInfo;

    public virtual World CreateWorld(bool newGame, Dungeons.GenerationInfo info = null, string heroNameToLoad = "")
    {
      return CreateWorld(info, heroNameToLoad);
    }

    public virtual World CreateWorld(Dungeons.GenerationInfo info = null, string heroNameToLoad = "")
    {
      WriteLine("CreateWorld " + TestContext.CurrentContext.Test.Name);
      OuaDII.Managers.GameManager.GeneratePredefiniedRooms = true;
      CreateContainer();
      

      CreateManager();
            
      World world = null;
      Hero hero = null;

      //TODO that mechnism shall be same in UI and tests
      GenerationInfo.GenerateDynamicTiles = false;
      var generator = new WorldGenerator(containerWorld, GameManager);
      generator.GenerateDynamicTiles = true;
            
      if (heroNameToLoad.Any())
      {
        var gi = info as GenerationInfo;
        world = generator.GenerateEmtyWorld(ref gi);
        GameManager.World = world;
        GameManager.Load(heroNameToLoad, false, (Hero) => { }, true);
        world = GameManager.World;
        hero = GameManager.Hero;
      }
      else
      {
        //this.generationInfo = EnsureGenerationInfo(info);
        var coreInfo = new CoreInfo();
        coreInfo.Difficulty = GenerationInfo.Difficulty;

        world = generator.Generate(0, info) as World;
        //world = new World();//simulate Unity UI
        //world.Create(generationInfo.MinNodeSize.Width, generationInfo.MinNodeSize.Height, generationInfo, generateContent: true);

        hero = world.GetTiles<Hero>().SingleOrDefault();
        if (hero == null)
        {
          int k = 0;
          k++;
        }
        var placesLayout = new PlacesLayout();
        GameManager.SetNewGameContext(world, hero, "hero_ouadII", coreInfo, placesLayout);
        GameManager.Hero.Stats[EntityStatKind.Health].Nominal = 300;//sometimes heare was accidently dead  
      }
                  
      return world;
    }

    private GenerationInfo EnsureGenerationInfo(Dungeons.GenerationInfo info)
    {
      var _info = info as GenerationInfo;
      if (_info == null)
        _info = CreateGenerationInfo();
      return _info;
    }

    protected GenerationInfo CreateGenerationInfo()
    {
      var _info = new GenerationInfo();

      _info.MakeEmpty();

      _info.GenerateEnemies = true;
      _info.GenerateInteractiveTiles = true;
      _info.Counts.WorldEnemiesCount = 40;
      _info.Counts.MushroomsCount = 3;
      _info.Counts.FoodCount = 2;
      _info.Counts.WorldChestsCount = 2;
      _info.Counts.WorldBarrelsCount = 2;
      _info.Counts.DeadBodiesCount = 2;
      _info.Counts.MagicDustsCount = 2;
      _info.Counts.PlantCount = 2;
        
      var gi = _info as GenerationInfo;
      //keep small for test running fast
      gi.SetMinWorldSize(50);
      gi.RevealTiles = true;

      return gi;
    }

    public void MaximizeAbility(Ability ab, AdvancedLivingEntity le)
    {
      while(ab.Level  < ab.MaxLevel)
      {
        Assert.True(ab.IncreaseLevel(le));
      }
    }
    
    public void GotoNextHeroTurn()
    {
      if (GameManager.Context.TurnOwner == TurnOwner.Hero)//TODO
        GameManager.Context.MoveToNextTurnOwner();
      GameManager.MakeGameTick();
      Assert.AreEqual(GameManager.Context.TurnOwner, TurnOwner.Enemies);


      //GameManager.Context.DoMoveToNextTurnOwner();
      GameManager.MakeGameTick();
      GameManager.MakeGameTick();
      Assert.AreEqual(GameManager.Context.TurnOwner, TurnOwner.Hero);
      //GameManager.MakeGameTick();

      //GameManager.Context.DoMoveToNextTurnOwner();
      //GameManager.MakeGameTick();
      //Assert.AreEqual(GameManager.Context.TurnOwner, TurnOwner.Hero);
      Assert.AreEqual(GameManager.Context.GetActionsCount(), 0);
    }

    protected DungeonPit GotoNonQuestPit
    (
      World world, string pitName = "", 
      KeyPuzzle keyPuzzle = KeyPuzzle.Unset,
      Dungeons.DungeonLayouterKind defaultForcedDungeonLayouterKind = Dungeons.DungeonLayouterKind.Unset
    )
    {
      var worldToUse = world ?? CreateWorld();
      var stairs = worldToUse.GetTiles<Stairs>();
      Assert.Greater(stairs.Count, 0);
      Assert.AreEqual(worldToUse.Pits.Count, stairs.Count);

      var pit = worldToUse.Pits.Where(i => i.QuestKind == QuestKind.Unset && (!pitName.Any() || i.Name == pitName)).FirstOrDefault();
      Assert.NotNull(pit);
      Assert.AreEqual(pit.Levels.Count, 0);
      //Assert.Greater(pit.LevelGenerator.MaxLevelIndex, 0);
      Assert.LessOrEqual(pit.LevelGenerator.MaxLevelIndex, GenerationInfo.DefaultMaxLevelIndex);
      pit.LevelGenerator.KeyPuzzle = keyPuzzle;

      var stair = stairs.FirstOrDefault(i => i.PitName == pit.Name);
      Assert.NotNull(stair);
      GameManager.InteractHeroWith(stair);//will use LevelGenerator assigned to pit!
      Assert.AreEqual(pit.Levels.Count, 1);
      return pit;
    }

    public List<T> GetDiff<T>(List<T> prevLoot) where T : Roguelike.Tiles.Loot
    {
      var lootAfter = this.gameManager.Context.CurrentNode.GetTiles<T>();
      var newLoot = lootAfter.Except(prevLoot).ToList();
      return newLoot;
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> Enemies
    {
      get { return GameManager.EnemiesManager.GetActiveEntities().Cast<Enemy>().ToList(); }
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> AllEnemies
    {
      get { return GameManager.EnemiesManager.AllEntities.Cast<Enemy>().ToList(); }
    }

    public int KillAllEnemies()
    {
      var enemies = AllEnemies.OrderBy(i => i.DistanceFrom(gameManager.Hero)).ToList();//order so that weak enemies throws early loot
      for (int i = 0; i < enemies.Count; i++)
      {
        var en = enemies[i];
        KillEnemy(en);
      }

      return enemies.Count;
    }

    public void KillEnemy(LivingEntity en)
    {
      while (en.Alive)
        en.OnMeleeHitBy(GameManager.Hero);

      GameManager.EnemiesManager.RemoveDead();
    }

    protected DungeonPit InteractHeroWithPit(QuestKind questKind)
    {
      var pit = GameManager.World.Pits.Where(i => i.QuestKind == questKind).FirstOrDefault();
      Assert.NotNull(pit);
      // Assert.True(!pit.Levels.Any());

      //go to pit
      var stairs = GameManager.World.GetTiles<Stairs>();
      var pitStairs = stairs.Where(i => i.PitName == pit.Name).FirstOrDefault();
      Assert.NotNull(pitStairs);
      GameManager.InteractHeroWith(pitStairs);
      return pit;
    }

    protected bool UseActiveSpellSource(Hero caster, IDestroyable victim)
    {
      return UseSpellSource(caster, victim, caster.ActiveManaPoweredSpellSource);
    }

    protected bool UseSpellSource(Hero caster, IDestroyable victim, SpellSource spellSource)
    {
      if (victim is LivingEntity le)
        le.Stats[EntityStatKind.ChanceToEvadeElementalProjectileAttack].Factor = 0;

      if (spellSource is Scroll || spellSource is Book)
        caster.Inventory.Add(spellSource);
      if (caster is Hero)
      {
        
        GameManager.Hero.ActiveManaPoweredSpellSource = spellSource;
        return GameManager.SpellManager.ApplyAttackPolicy(caster, victim, spellSource) == ApplyAttackPolicyResult.OK;
      }
      return false;
    }

    protected void Craft(OuaDII.Tiles.LivingEntities.Hero hero, List<Loot> lootToCraft, RecipeKind recipeKind)
    {
      foreach (var loot in lootToCraft)
      {
        Assert.True(hero.Inventory.Add(loot));
      }

      foreach (var loot in lootToCraft)
        Assert.NotNull(GameManager.SellItem(loot, hero, hero.Crafting.InvItems));

      var rec = new Recipe(recipeKind);
      var crafted = GameManager.Craft(rec);
      Assert.True(crafted.Success);
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> PlainEnemies
    {
      get { return GameManager.EnemiesManager.AllEntities.Cast<Enemy>().Where(i => i.PowerKind == EnemyPowerKind.Plain).ToList(); }
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> ChampionEnemies
    {
      get { return GameManager.EnemiesManager.AllEntities.Cast<Enemy>().Where(i => i.PowerKind == EnemyPowerKind.Champion).ToList(); }
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> BossEnemies
    {
      get { return GameManager.EnemiesManager.AllEntities.Cast<Enemy>().Where(i => i.PowerKind == EnemyPowerKind.Boss).ToList(); }
    }

    public T GenerateEquipment<T>(string tileName) where T : Equipment
    {
      return GameManager.LootGenerator.GetLootByTileName<T>(tileName) as T;
    }

    protected bool UseFightItem(Hero hero, Enemy enemy, ProjectileFightItem fi)
    {
      if(!hero.Inventory.Contains(fi))
        hero.Inventory.Add(fi);

      var res = GameManager.TryApplyAttackPolicy(fi, enemy);
      if (!res)
      {
        int k = 0;
        k++;
      }
      Assert.True(res);
      if (hero.SelectedActiveAbility!=null && hero.SelectedActiveAbility.UsesCoolDownCounter) 
        Assert.Greater(hero.SelectedActiveAbility.CoolDownCounter, 0);
      return res;
    }

    protected StatValue AddFireAttackFromGem(Weapon wpn)
    {
      StatValue swordFireAttack;
      AddAttackFromGem(wpn, GemKind.Ruby);
              
      swordFireAttack = wpn.GetStats()[EntityStatKind.FireAttack];
      Assert.AreEqual(swordFireAttack.TotalValue, 1);
      return swordFireAttack;
    }

    protected void AddAttackFromGem(Weapon wpn, GemKind gem)
    {
      var gem1 = new Gem(gem);
      GameManager.Hero.Inventory.Add(gem1);
      wpn.MakeEnchantable();
      var res = GameManager.Craft(new List<Loot> { gem1, wpn }, new Recipe(RecipeKind.EnchantEquipment), null);
      Assert.True(res.Success);
      Assert.True(wpn.GetMagicStats().Any());
    }

    protected NPC GetNPC(string NPCName, bool assert = true)
    {
      NPC npc = null;
      //if (DiscussionFactory.MerchantZiemowitName == NPCName)
      //{
      //  if (GameManager.CurrentNode is OuaDII.TileContainers.World)
      //    GotoPit("pit_down_Smiths");
      //}
      npc = GameManager.GetNPC(NPCName);
      if(assert)
        Assert.NotNull(npc);
      return npc;
    }

    protected virtual void GotoPit(string pitName)
    {
      var stairsPitDown = GameManager.World.GetAllStairs(StairsKind.PitDown);
      foreach (var stair in stairsPitDown)
      {
        var pit = GameManager.World.GetPit(stair.PitName);
        if (stair.PitName != pitName)
          continue;
        GameManager.InteractHeroWith(stair);

        if (pitName == "pit_down_Smiths")
        {
          //var npc = GetNPC(DiscussionFactory.MerchantZiemowitName, false);
          //if (npc == null)
          //{
          //  var generator = new WorldGenerator(containerWorld);
          //  generator.CreateMerchant(GameManager.CurrentNode, DiscussionFactory.MerchantZiemowitName);
          //npc = GameManager.CurrentNode.GetTiles<Merchant>().SingleOrDefault();
          //  npc.Discussion = new DiscussionFactory(Container).Create(npc);
          //  GameManager.ConnectCheating(npc, npc.Discussion.AsOuaDDiscussion());
          //}
        }

        break;
      }
    }

    protected virtual void Reload(string merchantName = "")
    {
      if (merchantName == DiscussionFactory.MerchantZiemowitName)
      {
        var pit1 = GameManager.World.Pits.Where(i => i.Name == "pit_down_Smiths").Single();
        var les1 = pit1.Levels[0].GetTiles<LivingEntity>();
      }
      GameManager.Save(false);
      var heroName = GameManager.OuadHero.Name;
      var oldWorld = GameManager.World;
      var gi = new GenerationInfo();
      gi.MinNodeSize = new System.Drawing.Size(oldWorld.Width, oldWorld.Height);
      CreateWorld(false, gi, heroName);
      //GameManager.Load(heroName);
      Assert.AreNotEqual(oldWorld, GameManager.World);
      if (merchantName == DiscussionFactory.MerchantZiemowitName)
      {
        var pit = GameManager.World.Pits.Where(i => i.Name == "pit_down_Smiths").Single();
        var les = pit.Levels[0].GetTiles<LivingEntity>();
      }
    }

    protected bool SetHeroEquipment(Equipment eq, CurrentEquipmentKind cek = CurrentEquipmentKind.Unset)
    {
      if (eq == null)
      {
        var eqUsed = GameManager.Hero.GetActiveEquipment()[cek];
        if (eqUsed == null)
          return true;
        //var res = GameManager.Hero.RemoveEquipment(eqUsed, cek);
        //return res;
      }

      if (!GameManager.Hero.Inventory.Contains(eq))
        GameManager.Hero.Inventory.Add(eq);

      return GameManager.Hero.MoveEquipmentInv2Current(eq, cek);
    }

    protected void PlaceCloseToHero(Roguelike.Tiles.LivingEntities.Hero hero, Enemy enemy)
    {
      var empty = GameManager.CurrentNode.GetClosestEmpty(hero);
      GameManager.CurrentNode.SetTile(enemy, empty.point);
    }
    protected void PlaceCloseToHero(Roguelike.Tiles.LivingEntities.Hero hero, Loot loot)
    {
      var empty = GameManager.CurrentNode.GetClosestEmpty(hero);
      GameManager.CurrentNode.SetTile(loot, empty.point);
    }

    protected static ActiveAbility ActivateActiveAblityInHotBar(OuaDII.Tiles.LivingEntities.Hero hero, AbilityKind activeAbilityKind)
    {
      var ab = hero.Abilities.ActiveItems.Where(i => i.Kind == activeAbilityKind).First();
      if (ab.Level == 0)
        hero.IncreaseAbility(activeAbilityKind);
      //Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);
      int ind = 1;
      if(hero.ShortcutsBar.GetAt(1) != null)
        ind = 2;
      Assert.AreEqual(hero.ShortcutsBar.SetAt(ind, ab), true);
      hero.SelectedActiveAbility = ab;
      Assert.AreEqual(hero.SelectedActiveAbility, ab);
      Assert.AreEqual(hero.ShortcutsBar.ActiveItemDigit, ind);
      return ab;
    }

    protected Weapon GetTestBow(int magicDamageAmount = 0, EntityStatKind esk = EntityStatKind.Unset)
    {
      return GetTestWeapon("bow", magicDamageAmount, esk);
    }

    protected Weapon GetTestWeapon(string name, int magicDamageAmount = 0, EntityStatKind esk = EntityStatKind.Unset)
    {
      Assert.True(magicDamageAmount > 0 || esk == EntityStatKind.Unset);
      Assert.True(magicDamageAmount == 0 || esk != EntityStatKind.Unset);
      var lg = GameManager.LootGenerator;
      var wpn = lg.GetLootByTileName<Weapon>(name);
      wpn.StableDamage = true;

      if (magicDamageAmount > 0)
      {
        wpn.MakeMagic(esk, 2);
        wpn.Identify();
      }
      Assert.NotNull(wpn);
      return wpn;
    }

    protected void PrepareEntityForLongLiving(LivingEntity en, int health = 300)
    {
      MakeEntityLongLiving(en, health);
    }

    protected void MakeEntityLongLiving(LivingEntity en, int health = 300)
    {
      en.Stats[EntityStatKind.Health].Nominal = health;
    }

    protected void WriteLine(string line)
    {
      Debug.WriteLine(line);
    }

    protected void CollectLoot(Loot loot)
    {
      var set = GameManager.CurrentNode.SetTile(GameManager.Hero, loot.point);
      GameManager.CollectLootOnHeroPosition();
    }

    protected ActiveAbility PrepareActiveAbility(OuaDII.Tiles.LivingEntities.Hero hero, AbilityKind abilityKind, int increaseCount = 1)
    {
      return PrepareAbility(hero, abilityKind, increaseCount) as ActiveAbility;
    }

    protected Ability PrepareAbility(OuaDII.Tiles.LivingEntities.Hero hero, AbilityKind abilityKind, int increaseCount = 1)
    {
      var ability = hero.Abilities.ActiveItems.Where(i => i.Kind == abilityKind).SingleOrDefault();
      if (ability == null)
        hero.Abilities.PassiveItems.Where(i => i.Kind == abilityKind).SingleOrDefault();

      Assert.NotNull(ability);
      Assert.AreEqual(ability.Level, 0);
      Assert.AreEqual(ability.PrimaryStat.Factor, 0);
      Assert.AreEqual(ability.AuxStat.Factor, 0);
      Assert.AreEqual(ability.PrimaryStat.Value.TotalValue, 0);
      Assert.AreEqual(ability.AuxStat.Value.TotalValue, 0);

      //MaximizeAbility(ability, hero);
      for (int i = 0; i < increaseCount; i++)
        Assert.True(ability.IncreaseLevel(hero));

      if (abilityKind != AbilityKind.ArrowVolley && abilityKind != AbilityKind.PiercingArrow)
        Assert.Greater(ability.PrimaryStat.Factor, 0);

      return ability;
    }

    protected void Save()
    {
      GameManager.Save(false);
    }

    protected void Load()
    {
      GameManager.Load(GameManager.Hero.Name, false);
    }

    protected void SaveLoad()
    {
      Save();
      Reload();
    }

    protected OuaDII.Discussions.DiscussPanel AssignHourglassQuest()
    {
      OuaDII.Discussions.DiscussPanel panel;
      OuaDII.Discussions.DiscussionTopic quest;
      var questKind = QuestKind.HourGlassForMiller;
      Hero hero = GameManager.OuadHero;
      panel = CreatePanel(hero, "Lionel");
      Assert.IsEmpty(hero.Inventory.GetItems().Where(i => i.name == "Hourglass"));

      Assert.AreEqual(hero.Quests.Count, 0);
      var whatsUp = panel.GetTopic(Roguelike.Discussions.KnownSentenceKind.WhatsUp);
      panel.ChooseDiscussionTopic(whatsUp);
      var whatsSituation = panel.BoundTopics.TypedItems[0].Item;
      panel.ChooseDiscussionTopic(whatsSituation);

      var canHandle = panel.BoundTopics.TypedItems[0].Item;
      panel.ChooseDiscussionTopic(canHandle);

      //var anyOtherTask = panel.BoundTopics.TypedItems[0].Item;
      //panel.ChooseDiscussionTopic(anyOtherTask);

      var t3 = panel.GetParentTopic(questKind);
      panel.ChooseDiscussionTopic(t3);
      quest = panel.GetTopic(questKind);
      Assert.NotNull(quest);
      panel.ChooseDiscussionTopic(quest);
      return panel;
    }

    protected OuaDII.Discussions.DiscussPanel CreatePanel(Hero hero, string merchantName)
    {
      var merchant = GetNPC(merchantName);
      Assert.NotNull(merchant.Discussion);
      Assert.AreEqual(hero.Quests.Count, 0);

      var discussPanel = new OuaDII.Discussions.DiscussPanel(GameManager.QuestManager, GameManager.OuadHero);
      discussPanel.BindTopics(merchant.Discussion.MainItem, merchant);
      return discussPanel;
    }
  }
}
