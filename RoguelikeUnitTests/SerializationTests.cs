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
      var gameNode = CreateNewGame<GameNode>();

      Assert.AreEqual(gameNode, GameManager.Context.CurrentNode);
      var hero = gameNode.GetTiles<Hero>().Single();
      Assert.NotNull(hero);
      var pt = gameNode.GetFirstEmptyPoint();
      Assert.AreNotEqual(hero.Point, pt);
      gameNode.SetTile(hero, pt.Value);

      GameManager.Save();

      GameManager.Load();

      //after load node shall be different
      Assert.AreNotEqual(gameNode, GameManager.Context.CurrentNode);
      hero = gameNode.GetTiles<Hero>().Single();
      Assert.NotNull(hero);

      //hero position shall match
      Assert.AreEqual(hero.Point, pt);
    }

  }
}
