using NUnit.Framework;
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

      var gameNode = game.GenerateLevel<DungeonLevel>(0);
      hero = game.Hero;
      Assert.NotNull(hero);

      //move hero to rand position.
      var pt = gameNode.GetFirstEmptyPoint();
      Assert.AreNotEqual(hero.Point, pt);
      gameNode.SetTile(hero, pt.Value);

      game.GameManager.Save();

      game.GameManager.Load();

      //after load node shall be different
      Assert.AreNotEqual(gameNode, game.Level);
      var heroLoaded = game.Level.GetTiles<Hero>().Single();
      Assert.NotNull(heroLoaded);

      //hero position shall match
      Assert.AreEqual(heroLoaded.Point, pt);
      Assert.AreEqual(heroLoaded, game.Hero);
    }

  }
}
