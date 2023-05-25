using NUnit.Framework;
using Roguelike;
using Roguelike.Core.Serialization;
using Roguelike.History;
using Roguelike.Serialization;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Linq;
using System.IO;

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
      SaveLoad();
      Assert.NotNull(game.GameManager.Hero);
      Assert.AreNotEqual(game.GameManager.Hero, hero);
      var persister = game.GameManager.Container.GetInstance<IPersister>();
      persister.DeleteGame(hero.Name, false);
      AssertLoadFailed(hero, persister);
    }

    private static void AssertLoadFailed(Hero hero, IPersister persister)
    {
      bool loadFailed = false;
      try
      {
        persister.LoadHero(hero.Name, false);
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
      SaveLoad();
       
      var persister = game.GameManager.Container.GetInstance<IPersister>();
      AssertLoadFailed(hero, persister);

    }

    [Test]
    [Repeat(1)]
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
        game.GameManager.Save(false);
      }
      {
        var game = CreateGame(false);
        var hero = game.Hero;

        game.GameManager.Load(heroName, false);
        Assert.AreEqual(hintsCount, game.GameManager.GameState.History.Hints.Hints.Count);

        Assert.AreNotEqual(game.Hero, hero);
        //after load node shall be different
        Assert.AreNotEqual(gameLevel, game.Level);
        var heroLoaded = game.Level.GetTiles<Hero>().Single();
        Assert.NotNull(heroLoaded);

        //hero position shall match
        var dist = heroLoaded.DistanceFrom(heroPoint);
        if (dist > 1)
        {
          var atPoint = game.Level.GetTile(heroPoint);
          int k = 0;
          k++;
        }
        if(!game.Level.Nodes[0].Secret)
          Assert.LessOrEqual(dist,  2);
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

        Save();
      }
      {
        var game = CreateGame(false);
        var hero = game.Hero;
        game.GameManager.Load(heroName, false);

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
        LootPropsTest(FoodKind.Meat, "Raw, yet nutritious piece of meat");
        LootPropsTest(FoodKind.Fish, "Raw, yet nutritious piece of fish");
        LootPropsTest(FoodKind.NiesiolowskiSoup, "Nutritious soup made of simple ingradients");
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
      SetHeroEquipment(wpn, CurrentEquipmentKind.Weapon);
      SaveLoad();

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

      SaveLoad();

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

      SaveLoad();
      
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

        SaveLoad();

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

    [Test]
    public void GodotSavedGamesList()
    {
      var game = CreateGame(true);
      var hero = game.Hero;
      hero.Name = "SaveHeroStatsTest";

      SaveLoad();

      var savedGamesList = SavedGames.GetSavedGamesList();
      Assert.Greater(savedGamesList.Count, 0);
      Assert.IsTrue(savedGamesList.Any(s => s == System.IO.Path.GetTempPath() + "Roguelike" + "\\" + "SaveHeroStatsTest"));
    }

  }
}
