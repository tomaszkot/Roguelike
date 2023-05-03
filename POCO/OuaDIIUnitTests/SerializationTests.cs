using NUnit.Framework;
using OuaDII.Serialization;
using System;
using System.IO;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class SerializationTests : TestBase
  {
    [Test]
    public void HiddenTiles()
    {
      CreateWorld();
      var keysCount = GameManager.World.HiddenTiles.GetKeys().Count();
      var htOld = GameManager.World.HiddenTiles;
      Assert.Greater(keysCount, 0);
      var hero = GameManager.Hero;
      GameManager.World.HiddenTiles._Name = "test";
      GameManager.Save();
      GameManager.World.HiddenTiles = new Dungeons.HiddenTiles();

      //CreateManager();//reset all//TODO

      GameManager.Load(hero.Name);
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
      GameManager.Save();

      AssertGameExist(hero, true);

      GameManager.Load(hero.Name);
      AssertGameExist(hero, false);
    }

    private void AssertGameExist(Roguelike.Tiles.LivingEntities.Hero hero, bool exist)
    {
      var storedGamesProvider = new StoredGamesProvider(GameManager.Container);
      var games = storedGamesProvider.GetSavedGames();
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
    public void TilesStateTest()
    {
      CreateWorld();
      var world = GameManager.World;
      var hero = GameManager.Hero;
      hero.Name = "TilesStateTest";

      Assert.Greater(world.GroundPortals.Count, 0);
      Assert.True(world.GroundPortals.All(i => !i.ApproachedByHero));
      var firstPortal = world.GroundPortals[0];

      firstPortal.ApproachedByHero = true;
      GameManager.Save();

      GameManager.Load(hero.Name);
      var loadedPortal = GameManager.World.GroundPortals[0];
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
      var hero = GameManager.Hero;
      Assert.NotNull(hero);
      hero.Name = name;
      var heroInitPos = hero.point;

      //move hero to rand position.
      var pt = world.GetFirstEmptyPoint();
      Assert.AreNotEqual(hero.point, pt);
      world.SetTile(hero, pt.Value);

      GameManager.Save();

      GameManager.Load(hero.Name);

      //after load node shall be different
      Assert.AreNotEqual(world, GameManager.World);
      var heroLoaded = GameManager.World.GetTiles<OuaDII.Tiles.LivingEntities.Hero>().Single();
      Assert.NotNull(heroLoaded);

      //hero position shall match
      if (GameManager.GameSettings.Mechanics.RestoreHeroToSafePointAfterLoad)
        Assert.AreEqual(heroLoaded.point, heroInitPos);
      else
        Assert.AreEqual(heroLoaded.point, pt);

      Assert.AreEqual(heroLoaded, GameManager.Hero);
    }
  }
}
