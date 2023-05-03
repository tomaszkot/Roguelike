using Dungeons.Tiles;
using NUnit.Framework;
using OuaDII.Serialization;
using OuaDII.TileContainers;
using Roguelike.Settings;
using Roguelike.Tiles.Interactive;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class SerializationTests : TestBase
  {
    [TestCase(Roguelike.Tiles.LivingEntities.AllyBehaviour.GoFreely)]
    [TestCase(Roguelike.Tiles.LivingEntities.AllyBehaviour.StayClose)]
    [TestCase(Roguelike.Tiles.LivingEntities.AllyBehaviour.StayStill)]
    public void AllyStateSerialization(Roguelike.Tiles.LivingEntities.AllyBehaviour allyBehaviour)
    {
      CreateWorld();
      GameManager.AlliesManager.AllyBehaviour = allyBehaviour;
      GameManager.Save(false);
      Reload();
      Assert.AreEqual(GameManager.AlliesManager.AllyBehaviour, allyBehaviour);
    }

    [Test]
    [Repeat(1)]
    [Ignore("")]//TODO 
    public void TestSaveLoadInPit()
    {
      var world = CreateWorld();
      Assert.True(GameManager.CurrentNode is World);
      //var hero = GameManager.Hero;
      DungeonPit pit = GotoNonQuestPit(world);
      Assert.False(GameManager.CurrentNode is World);
      Assert.True(GameManager.CurrentNode.GetTiles<OuaDII.Tiles.LivingEntities.Hero>().Any());
      Reload();
      Assert.False(GameManager.CurrentNode is World);
    }

    [Test]
    public void HiddenTiles()
    {
      CreateWorld();
      var keysCount = GameManager.World.HiddenTiles.GetKeys().Count();
      var htOld = GameManager.World.HiddenTiles;
      //Assert.Greater(keysCount, 0);//TODO
      var hero = GameManager.Hero;
      GameManager.World.HiddenTiles._Name = "test";
      GameManager.Save(false);
      GameManager.World.HiddenTiles = new Dungeons.HiddenTiles();

      //CreateManager();//reset all//TODO

      GameManager.Load(hero.Name, false);
      var keysCountAfter = GameManager.World.HiddenTiles.GetKeys().Count();
      Assert.AreNotEqual(GameManager.World.HiddenTiles, htOld);
      Assert.AreEqual(GameManager.World.HiddenTiles._Name, "test");
      Assert.AreEqual(keysCountAfter, keysCount);
    }

    [Test]
    public void PermaDeathTest()
    {
      CreateWorld();
      var world = GameManager.World;
      var hero = GameManager.Hero;
      GameManager.GameState.CoreInfo.PermanentDeath = true;
      GameManager.Save(false);

      AssertGameExist(hero, true);

      GameManager.Load(hero.Name, false);
      AssertGameExist(hero, false);
    }

    private void AssertGameExist(Roguelike.Tiles.LivingEntities.Hero hero, bool exist)
    {
      var storedGamesProvider = new StoredGamesProvider(GameManager.Container);
      var games = storedGamesProvider.GetSavedGames(false);
      var any = games.Where(i => i.HeroName == hero.Name);
      if(exist)
        Assert.True(any.Any());
      else
        Assert.False(any.Any());
    }

    [Test]
    public void NewGameTest()
    {
      Assert.Null(GameManager);
      //Assert.Null(GameManager.World);
      SaveLoadHero("abc");
    }

   

    [Test]
    public void BarrelTilesTest()
    {
      CreateWorld();
      var world = GameManager.World;
      var hero = GameManager.Hero;
      var barrels = world.GetTiles<Roguelike.Tiles.Interactive.Barrel>();
      Assert.Greater(barrels.Count, 1);
      var barrel1 = barrels[0];
      var barrel2 = barrels[1];

      Assert.AreNotEqual(barrel1.Symbol, new Tile().Symbol);
      Assert.AreNotEqual(barrel2.Symbol, new Tile().Symbol);

      GameManager.InteractHeroWith(barrel1);
      Assert.AreNotEqual(world.GetTile(barrel1.point).Symbol, barrel1.Symbol);
      Assert.AreEqual(world.GetTile(barrel2.point).Symbol, barrel2.Symbol);

      SaveLoad();

      Assert.AreNotEqual(world.GetTile(barrel1.point).Symbol, barrel1.Symbol);
      Assert.AreEqual(world.GetTile(barrel2.point).Symbol, barrel2.Symbol);
    }

    [Test]
    public void ChestTilesTest()
    {
      CreateWorld();
      var world = GameManager.World;
      var hero = GameManager.Hero;
      var chests = world.GetTiles<Roguelike.Tiles.Interactive.Chest>();
      Assert.Greater(chests.Count, 1);
      var chest1 = chests[0];
      var chest2 = chests[1];

      Assert.AreEqual(chest1.Closed, true);
      Assert.AreEqual(chest2.Closed, true);

      GameManager.InteractHeroWith(chest1);
      Assert.AreNotEqual(chest1.Closed, true);
      Assert.AreEqual(chest2.Closed, true);

      SaveLoad();

      Assert.AreNotEqual(chest1.Closed, true);
      Assert.AreEqual(chest2.Closed, true);
    }

    [Test]
    public void EnemiesTilesTest()
    {
      CreateWorld();
      var world = GameManager.World;
      var hero = GameManager.Hero;
      var enemies = world.GetTiles<Roguelike.Tiles.LivingEntities.Enemy>();
      Assert.Greater(enemies.Count, 1);
      var enemy1 = enemies[0];
      var enemy2 = enemies[1];

      Assert.AreNotEqual(enemy1.Symbol, new Tile().Symbol);
      Assert.AreNotEqual(enemy2.Symbol, new Tile().Symbol);

      KillEnemy(enemy1);
      //GameManager.InteractHeroWith(enemy1);
      Assert.AreNotEqual(world.GetTile(enemy1.point).Symbol, enemy1.Symbol);
      Assert.AreEqual(world.GetTile(enemy2.point).Symbol, enemy2.Symbol);

      SaveLoad();

      Assert.AreNotEqual(world.GetTile(enemy1.point).Symbol, enemy1.Symbol);
      Assert.AreEqual(world.GetTile(enemy2.point).Symbol, enemy2.Symbol);
    }

    [Test]
    public void ApproachedByHeroTest()
    {
      CreateWorld();
      var world = GameManager.World;
      var hero = GameManager.Hero;
      hero.Name = "TilesStateTest";

      Assert.Greater(world.WorldSpecialTiles.GroundPortals.Count, 0);
      Assert.True(world.WorldSpecialTiles.GroundPortals.All(i => !i.ApproachedByHero));
      var firstPortal = world.WorldSpecialTiles.GroundPortals[0];

      firstPortal.ApproachedByHero = true;
      SaveLoad();
      var loadedPortal = GameManager.World.WorldSpecialTiles.GroundPortals[0];
      Assert.AreNotEqual(firstPortal, loadedPortal);
      Assert.True(loadedPortal.ApproachedByHero);
    }

    [Test]
    public void ManyGamesTest()
    {
      Assert.Null(GameManager);
      //Assert.Null(GameManager.Hero);
      //Assert.Null(GameManager.World);

      SaveLoadHero("Koto");
      SaveLoadHero("Edd");

      var storedGamesProvider = new StoredGamesProvider(GameManager.Container);
      var games = storedGamesProvider.GetSavedGamesNames();
      foreach (var game in games)
      {
        var name = Path.GetFileName(game);
        Console.WriteLine(name);
      }
      Assert.True(games.Any(i => Path.GetFileName(i) == "Koto"));
      Assert.True(games.Any(i => Path.GetFileName(i) == "Edd"));
    }

    private void SaveLoadHero(string name)
    {
      CreateWorld();
      var world = GameManager.World;
      Assert.NotNull(world);
      Assert.AreEqual(GameManager.GameState.CoreInfo.GameVersion, Roguelike.Abstract.Game.Version);
      var hero = GameManager.Hero;
      Assert.NotNull(hero);
      hero.Name = name;
      var heroInitPos = hero.point;

      //move hero to rand position.
      var pt = world.GetFirstEmptyPoint();
      Assert.AreNotEqual(hero.point, pt);
      world.SetTile(hero, pt.Value);

      SaveLoad();

      //after load node shall be different
      Assert.AreNotEqual(world, GameManager.World);
      var heroLoaded = GameManager.World.GetTiles<OuaDII.Tiles.LivingEntities.Hero>().Single();
      Assert.NotNull(heroLoaded);

      //hero position shall match
      if (GameManager.GameSettings.Serialization.RestoreHeroToSafePointAfterLoad)
        Assert.AreEqual(heroLoaded.point, heroInitPos);
      else
        Assert.AreEqual(heroLoaded.point, pt);

      Assert.AreEqual(heroLoaded, GameManager.Hero);
    }

    [Test]
    public void AutoQuickSaveAfterGettingQuest()
    {
      CreateWorld();
      Assert.AreEqual(GameManager.GameSettings.Serialization.AutoQuickSave, true);
      Assert.AreEqual(GameManager.SaveCounter, 0);
      AssignHourglassQuest();
      while (GameManager.AutoSaveContext != OuaDII.Managers.AutoSaveContextValues.Unset)
        Thread.Sleep(100);
      Assert.AreEqual(GameManager.SaveCounter, 1);
    }

    [Test]
    public void AutoQuickSaveAfterContextSwitched()
    {
      CreateWorld();
      Assert.AreEqual(GameManager.GameSettings.Serialization.AutoQuickSave, true);
      Assert.AreEqual(GameManager.SaveCounter, 0);

      var pit = GotoNonQuestPit(null);

      while (GameManager.AutoSaveContext != OuaDII.Managers.AutoSaveContextValues.Unset)
        Thread.Sleep(100);
      Assert.AreEqual(GameManager.SaveCounter, 1);
    }

    private void AssertGameExist(string heroName, bool exist, bool quickSave)
    {
      var storedGamesProvider = new StoredGamesProvider(GameManager.Container);
      var games = storedGamesProvider.GetSavedGames(false);
      var gameName = Roguelike.Serialization.JSONPersister.GetGameName(heroName, quickSave);
      var game = games.Where(i => i.SavedGameInfo.Name == heroName).FirstOrDefault();
      if (exist)
      {
        Assert.NotNull(game);
        Assert.AreEqual(game.State.QuickSave, quickSave);
      }
      else
        Assert.Null(game);
    }
        
    [Test]
    public void AutoQuickSaveVsSave()
    {
      {
        CreateWorld();
        Assert.AreEqual(GameManager.GameSettings.Serialization.AutoQuickSave, true);
        Assert.AreEqual(GameManager.SaveCounter, 0);

        var storedGamesProvider = new StoredGamesProvider(GameManager.Container);
        var heroName = "ut_hero";
        storedGamesProvider.DeleteGame(heroName, false);
        storedGamesProvider.DeleteGame(heroName, true);

        bool gameShallExist = false;

        AssertGameExist(heroName, gameShallExist, false);
        AssertGameExist(heroName, gameShallExist, true);

        GameManager.Hero.Name = heroName;
        SaveLoad();

        gameShallExist = true;
        AssertGameExist(heroName, gameShallExist, false);
        Assert.False(GameManager.Hero.Inventory.Items.Any());
        Assert.True(GameManager.Hero.Inventory.Add(new Roguelike.Tiles.Looting.MagicDust()));
        SaveLoad();
        
        Assert.AreEqual(GameManager.Hero.Inventory.Items.Count, 1);
        GameManager.Hero.Inventory.Add(new Roguelike.Tiles.Looting.Feather());

        bool quickSave = true;
        GameManager.Save(quickSave);
        GameManager.Load(GameManager.Hero.Name, quickSave);
        Assert.AreEqual(GameManager.Hero.Inventory.Items.Count, 2);

        quickSave = false;
        GameManager.Load(GameManager.Hero.Name, quickSave);
        Assert.AreEqual(GameManager.Hero.Inventory.Items.Count, 1);

        quickSave = true;
        GameManager.Load(GameManager.Hero.Name, quickSave, (Hero) => { });
        Assert.AreEqual(GameManager.Hero.Inventory.Items.Count, 2);
        quickSave = false;//quickSave->normal!
        GameManager.Save(quickSave);
        GameManager.Load(GameManager.Hero.Name, quickSave, (Hero) => { });
        Assert.AreEqual(GameManager.Hero.Inventory.Items.Count, 2);

        storedGamesProvider.DeleteGame(heroName, false);
        storedGamesProvider.DeleteGame(heroName, true);
      }
    }

    [Test]
    public void TestingDataTest()
    {
      CreateWorld();
      var td = new TestingData();
      Assert.AreNotEqual(td.AbilitiesPoints, 15);
      td.AbilitiesPoints = 15;
      Assert.AreEqual(td.AbilitiesPoints, 15);

      var pers = new JSONPersister(GameManager.Container);
      pers.SaveTestingData(td);
      var td1 = pers.LoadTestingData();
      Assert.AreEqual(td1.AbilitiesPoints, 15);

      pers.DeleteTestingData();
    }
  }
}
