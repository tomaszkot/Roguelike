using NUnit.Framework;
using Roguelike;
using Roguelike.History;
using Roguelike.Serialization;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class SerializationTests : TestBase
  {
    [Test]
    public void DeleteGameTest()
    {
      var game = CreateGame(true);
      var hero = game.Hero;
      game.GameManager.Save();
      game.GameManager.Load(hero.Name);
      Assert.NotNull(game.GameManager.Hero);
      Assert.AreNotEqual(game.GameManager.Hero, hero);
      var persister = game.GameManager.Container.GetInstance<IPersister>();
      persister.DeleteGame(hero.Name);
      AssertLoadFailed(hero, persister);
    }

    private static void AssertLoadFailed(Hero hero, IPersister persister)
    {
      bool loadFailed = false;
      try
      {
        persister.LoadHero(hero.Name);
      }
      catch (Exception)
      {
        loadFailed = true;
      }
      Assert.True(loadFailed);
    }

    [Test]
    public void PermaDeathTest()
    {
       var game = CreateGame(true);
       game.GameManager.GameState.CoreInfo.PermanentDeath = true;
       var hero = game.Hero;
       game.GameManager.Save();
       game.GameManager.Load(hero.Name);

      var persister = game.GameManager.Container.GetInstance<IPersister>();
      AssertLoadFailed(hero, persister);

    }

    [Test]
    public void NewGameTest()
    {
      string heroName;
      GameLevel gameLevel = null;
      System.Drawing.Point heroPoint;
      Equipment eq;
      var hintsCount = 0;
      {
        var game = CreateGame(false);
        var hero = game.Hero;
        Assert.Null(hero);
        Assert.Null(game.Level);

        var gameNode = game.GenerateLevel(0);
        hero = game.Hero;
        //hero.Name = "Koto";
        Assert.NotNull(hero);
        heroName = hero.Name;

        //move hero to rand position.
        var pt = gameNode.GetFirstEmptyPoint();
        Assert.AreNotEqual(hero.point, pt);
        gameNode.SetTile(hero, pt.Value);

        eq = game.GameManager.LootGenerator.GetRandomEquipment(1, null);
        game.GameManager.GameState.History.Looting.GeneratedLoot.Add(new LootHistoryItem(eq));
        gameLevel = game.Level;
        heroPoint = hero.point;

        hintsCount = game.GameManager.GameState.History.Hints.Hints.Count;
        game.GameManager.Save();
      }
      {
        var game = CreateGame(false);
        var hero = game.Hero;

        game.GameManager.Load(heroName);
        Assert.AreEqual(hintsCount, game.GameManager.GameState.History.Hints.Hints.Count);

        Assert.AreNotEqual(game.Hero, hero);
        //after load node shall be different
        Assert.AreNotEqual(gameLevel, game.Level);
        var heroLoaded = game.Level.GetTiles<Hero>().Single();
        Assert.NotNull(heroLoaded);

        //hero position shall match
        Assert.True(heroLoaded.DistanceFrom(heroPoint) <= 1);
        Assert.AreEqual(heroLoaded, game.Hero);
        Assert.AreEqual(game.GameManager.GameState.History.Looting.GeneratedLoot.Count, 1);
        Assert.AreEqual(game.GameManager.GameState.History.Looting.GeneratedLoot[0].Name, eq.Name);
        Assert.AreEqual(hintsCount, game.GameManager.GameState.History.Hints.Hints.Count);
      }
    }


    [Test]
    public void LootHistoryTest()
    {
      string heroName;
      string eqName;
      {
        var game = CreateGame(false);

        var gameNode = game.GenerateLevel(0);
        var hero = game.Hero;
        heroName = hero.Name;
        Assert.NotNull(hero);

        var eq1 = game.GameManager.LootGenerator.GetLootByAsset("rusty_sword");
        eqName = eq1.Name;
        game.GameManager.GameState.History.Looting.AddLootHistory(new LootHistoryItem(eq1));
        Assert.AreEqual(game.GameManager.GameState.History.Looting.GeneratedLoot.Count, 1);

        var eq2 = game.GameManager.LootGenerator.GetLootByAsset("rusty_sword");
        game.GameManager.GameState.History.Looting.AddLootHistory(new LootHistoryItem(eq2));
        Assert.AreEqual(game.GameManager.GameState.History.Looting.GeneratedLoot.Count, 1);//duplicate not added

        game.GameManager.Save();
      }
      {
        var game = CreateGame(false);
        var hero = game.Hero;
        game.GameManager.Load(heroName);

        Assert.AreEqual(game.GameManager.GameState.History.Looting.GeneratedLoot.Count, 1);
        Assert.AreEqual(game.GameManager.GameState.History.Looting.GeneratedLoot[0].Name, eqName);
      }
    }

    [Test]
    public void LootPropsTest()
    {
      for (int i = 0; i < 1; i++)
      {
        LootPropsTest(FoodKind.Plum, "Sweet, delicious fruit");
        LootPropsTest(FoodKind.Meat, "Raw yet nutritious piece of meat");
        LootPropsTest(FoodKind.Fish, "Raw yet nutritious piece of fish");
      }
    }

    [Test]
    public void WeaponStateTest()
    {
      var game = CreateGame(true);
      var hero = game.Hero;
      var wpn = game.GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Weapon, 1);
      var li = wpn.LevelIndex;
      Assert.GreaterOrEqual(wpn.LevelIndex, 0);
      hero.SetEquipment(wpn, CurrentEquipmentKind.Weapon);
      game.GameManager.Save();
      game.GameManager.Load(hero.Name);
      var loadedHero = game.GameManager.Hero;
      Assert.AreEqual(loadedHero.GetActiveWeapon().tag1, wpn.tag1);
      Assert.AreEqual(loadedHero.GetActiveWeapon().LevelIndex, wpn.LevelIndex);

    }

    void LootPropsTest(FoodKind kind, string desc)
    {
      var game = CreateGame(true);
      var hero = game.Hero;
      hero.Name = "LootPropsTest";
      var loot = new Food(kind);
      hero.Inventory.Add(loot);
      Assert.AreEqual(loot.PrimaryStatDescription, desc);
      Assert.AreEqual(game.Hero.Inventory.Items.Count, 1);

      game.GameManager.Save();
      game.GameManager.Load(hero.Name);
      var loadedHero = game.GameManager.Hero;
      Assert.AreNotEqual(hero, loadedHero);
      Assert.AreEqual(loadedHero.Name, "LootPropsTest");
      Assert.AreEqual(game.Hero.Inventory.Items.Count, 1);
      var lootLoaded = game.Hero.Inventory.Items.ElementAt(0) as Food;

      Assert.AreEqual(loot.Kind, lootLoaded.Kind);
      Assert.AreEqual(loot.PrimaryStatDescription, lootLoaded.PrimaryStatDescription);
    }

    [Test]
    public void SaveHeroStatsTest()
    {
      var game = CreateGame(true);
      var hero = game.Hero;
      hero.Name = "SaveHeroStatsTest";

      hero.LevelUpPoints = 5;

      var health = hero.Stats[Roguelike.Attributes.EntityStatKind.Health].TotalValue;
      hero.IncreaseStatByLevelUpPoint(Roguelike.Attributes.EntityStatKind.Health);
      Assert.Greater(hero.Stats[Roguelike.Attributes.EntityStatKind.Health].TotalValue, health);
      health = hero.Stats[Roguelike.Attributes.EntityStatKind.Health].TotalValue;

      game.GameManager.Save();
      game.GameManager.Load(hero.Name);
      var loadedHero = game.GameManager.Hero;
      Assert.AreEqual(health, loadedHero.Stats[Roguelike.Attributes.EntityStatKind.Health].TotalValue);
    }

    [Test]
    public void ManyGamesTest()
    {
      Action<RoguelikeGame, string, Loot> createGame = (RoguelikeGame game, string heroName, Loot loot) =>
      {

        var hero = game.Hero;
        hero.Name = heroName;

        hero.Inventory.Add(loot);
        Assert.AreEqual(game.Hero.Inventory.Items.Count, 1);

        game.GameManager.Save();
        game.GameManager.Load(hero.Name);

        Assert.AreEqual(game.GameManager.Hero.Name, heroName);
        Assert.AreEqual(game.Hero.Inventory.Items.Count, 1);
        Assert.AreEqual(game.Hero.Inventory.Items[0].Name, loot.Name);
      };
      {
        var game = CreateGame(false);
        var gameNode = game.GenerateLevel(0);
        var wpn = game.GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Weapon, 1);
        createGame(game, "Koto", wpn);
      }
      {
        var game = CreateGame(false);
        var gameNode = game.GenerateLevel(0);
        var arm = game.GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Armor, 1);
        createGame(game, "Edd", arm);
      }
    }

  }
}
