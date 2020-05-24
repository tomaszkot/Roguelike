using NUnit.Framework;
using Roguelike;
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
      var game = CreateGame(false);
      var hero = game.Hero;
      Assert.Null(hero);
      Assert.Null(game.Level);

      var gameNode = game.GenerateLevel(0);
      hero = game.Hero;
      //hero.Name = "Koto";
      Assert.NotNull(hero);

      //move hero to rand position.
      var pt = gameNode.GetFirstEmptyPoint();
      Assert.AreNotEqual(hero.Point, pt);
      gameNode.SetTile(hero, pt.Value);

      game.GameManager.Save();

      game.GameManager.Load(hero.Name);

      Assert.AreNotEqual(game.Hero, hero);
      //after load node shall be different
      Assert.AreNotEqual(gameNode, game.Level);
      var heroLoaded = game.Level.GetTiles<Hero>().Single();
      Assert.NotNull(heroLoaded);

      //hero position shall match
      Assert.AreEqual(heroLoaded.Point, pt);
      Assert.AreEqual(heroLoaded, game.Hero);
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
        var wpn = game.GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Weapon);
        createGame(game, "Koto", wpn);
      }
      {
        var game = CreateGame(false);
        var gameNode = game.GenerateLevel(0);
        var arm = game.GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Armor);
        createGame(game, "Edd", arm);
      }
    }

  }
}
