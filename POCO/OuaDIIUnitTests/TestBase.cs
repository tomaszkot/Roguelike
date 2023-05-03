using NUnit.Framework;
using OuaDII.Discussions;
using OuaDII.Extensions;
using OuaDII.Generators;
using OuaDII.Quests;
using OuaDII.TileContainers;
using Roguelike;
using Roguelike.Abilities;
using Roguelike.Abstract.Multimedia;
using Roguelike.Abstract.Projectiles;
using Roguelike.Attributes;
using Roguelike.Managers;
using Roguelike.Multimedia;
using Roguelike.Serialization;
using Roguelike.Strategy;
using Roguelike.Tiles;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class TestBase
  {
    GameManager gameManager;
    protected Container containerWorld;
    protected Container Container;

    public OuaDII.Managers.GameManager GameManager { get { return gameManager as OuaDII.Managers.GameManager; } }

    [SetUp]
    public void Init()
    {
      containerWorld = null;
      Container = null;
      gameManager = null;
      WriteLine("TestBase Init!!!! "+ TestContext.CurrentContext.Test.Name + " gameManager :"+ gameManager);
      Dungeons.Tiles.Tile.IncludeDebugDetailsInToString = true;
      OnInit();
    }

    protected virtual void OnInit() { }

    [TearDown]
    public void Destroy()
    {
      WriteLine("TestBase TearDown!!!! " + TestContext.CurrentContext.Test.Name  +" gameManager :" + gameManager);
    }

    private void CreateContainer()
    {
      containerWorld = new OuaDII.ContainerConfigurator().Container;
      containerWorld.Register<ISoundPlayer, BasicSoundPlayer>();
      containerWorld.Register<IQuestRoomCreator, ProceduralQuestRoomCreator>();
      containerWorld.Register<Roguelike.Strategy.ITilesAtPathProvider, TilesAtPathProvider>();
      containerWorld.Register<IProjectilesFactory, Roguelike.Generators.ProjectilesFactory>();
      //containerWorld.Register<IPersister, OuaDII.Serialization.JSONPersister>();
      //containerWorld.Register<Roguelike.GameState, OuaDII.GameState>();


      Container = containerWorld;
    }

    protected void CreateManager()
    {
      gameManager = containerWorld.GetInstance<GameManager>();
    }

    
    public virtual World CreateWorld(bool newGame = true, Dungeons.GenerationInfo info = null, string heroNameToLoad = "")
    {
      WriteLine("CreateWorld " + TestContext.CurrentContext.Test.Name);
      OuaDII.Managers.GameManager.GeneratePredefiniedRooms = true;
      CreateContainer();
      CreateManager();
      if (info == null)
      {
        var _info = new OuaDII.Generators.GenerationInfo();
        //keep small for test running fast
        _info.SetMinWorldSize(50);
        info = _info;
      }
      var generator = new WorldGenerator(containerWorld);
      var dungeon = generator.Generate(0, info);
      var world = dungeon as World;
      var hero = containerWorld.GetInstance<OuaDII.Tiles.LivingEntities.Hero>();
      hero.Name = "hero_ouadII";
      hero.Stats.SetNominal(EntityStatKind.Health, 50);
      if (heroNameToLoad.Any())
        GameManager.Load(heroNameToLoad);
      GameManager.SetContext(world, hero, newGame ? GameContextSwitchKind.NewGame : GameContextSwitchKind.GameLoaded);
      hero.Stats[EntityStatKind.Health].Nominal = 300;//sometimes heare was accidently dead
      return world;
    }

    public void MaximizeAbility(Ability ab, AdvancedLivingEntity le)
    {
      while(ab.Level  < ab.MaxLevel)
      {
        Assert.True(ab.IncreaseLevel(le));
      }
    }
    public void WaitForAbilityCooldown(Ability ab)
    {
      while (ab.CoolDownCounter > 0)
        GotoNextHeroTurn();
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

    protected DungeonPit GotoNonQuestPit(World world, string pitName = "")
    {
      var worldToUse = world ?? CreateWorld();
      var stairs = worldToUse.GetTiles<Roguelike.Tiles.Interactive.Stairs>();
      Assert.Greater(stairs.Count, 0);
      Assert.AreEqual(worldToUse.Pits.Count, stairs.Count);

      var pit = worldToUse.Pits.Where(i => i.QuestKind == OuaDII.Quests.QuestKind.Unset && (!pitName.Any() || i.Name == pitName)).FirstOrDefault();
      Assert.NotNull(pit);
      Assert.AreEqual(pit.Levels.Count, 0);
      //Assert.Greater(pit.LevelGenerator.MaxLevelIndex, 0);
      Assert.LessOrEqual(pit.LevelGenerator.MaxLevelIndex, new GenerationInfo().MaxLevelIndex);

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
      get { return GameManager.EnemiesManager.GetActiveEntities().Cast<Roguelike.Tiles.LivingEntities.Enemy>().ToList(); }
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> AllEnemies
    {
      get { return GameManager.EnemiesManager.AllEntities.Cast<Roguelike.Tiles.LivingEntities.Enemy>().ToList(); }
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
        return GameManager.SpellManager.ApplyAttackPolicy(caster, victim, spellSource) == Roguelike.Managers.ApplyAttackPolicyResult.OK;
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

      //if (hero.ActiveFightItem == null)
      //  return false;
      ////if (fi.FightItemKind == FightItemKind.Stone)
      ////  return GameManager.ApplyAttackPolicy(hero, enemy, fi); ??

      var res = GameManager.TryApplyAttackPolicy(fi, enemy);
      if (hero.SelectedActiveAbility!=null) 
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

    protected virtual void Reload(string merchantName)
    {
      if (merchantName == DiscussionFactory.MerchantZiemowitName)
      {
        var pit1 = GameManager.World.Pits.Where(i => i.Name == "pit_down_Smiths").Single();
        var les1 = pit1.Levels[0].GetTiles<LivingEntity>();
      }
      GameManager.Save();
      var heroName = GameManager.OuadHero.Name;
      var oldWorld = GameManager.World;
      GameManager.Load(heroName);
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

    protected static Ability ActivateAblityInHotBar(OuaDII.Tiles.LivingEntities.Hero hero, AbilityKind kind)
    {
      var ab = hero.Abilities.ActiveItems.Where(i => i.Kind == kind).First();
      //Assert.AreEqual(ab.PrimaryStat.Unit, EntityStatUnit.Percentage);
      int ind = 1;
      if(hero.ShortcutsBar.GetAt(1) != null)
        ind = 2;
      Assert.AreEqual(hero.ShortcutsBar.SetAt(ind, ab), true);
      hero.ShortcutsBar.ActiveItemDigit = ind;
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
      Console.WriteLine(line);
    }

    protected void CollectLoot(Loot loot)
    {
      var set = GameManager.CurrentNode.SetTile(GameManager.Hero, loot.point);
      GameManager.CollectLootOnHeroPosition();
    }
  }
}
