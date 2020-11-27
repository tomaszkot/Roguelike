using NUnit.Framework;
using Roguelike;
using Roguelike.History;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class SerializationTests : TestBase
  {
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
        Assert.AreNotEqual(hero.Point, pt);
        gameNode.SetTile(hero, pt.Value);

        eq = game.GameManager.LootGenerator.GetRandomEquipment(1);
        game.GameManager.GameState.History.Looting.GeneratedLoot.Add(new LootHistoryItem(eq));
        gameLevel = game.Level;
        heroPoint = hero.Point;

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
        Assert.True(heroLoaded.DistanceFrom(heroPoint) <=1);
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
    public void ManyGamesTest()
    {
      Action<RoguelikeGame, string, Loot> createGame = (RoguelikeGame game, string heroName, Loot loot) => {
        
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
