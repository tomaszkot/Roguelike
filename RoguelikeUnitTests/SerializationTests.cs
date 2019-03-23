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
      Assert.Null(Hero);
      Assert.Null(GameManager.Context.CurrentNode);

      var gameNode = CreateNewDungeon();
                
      Assert.NotNull(Hero);

      //move hero to rand position.
      var pt = gameNode.GetFirstEmptyPoint();
      Assert.AreNotEqual(Hero.Point, pt);
      gameNode.SetTile(Hero, pt.Value);

      GameManager.Save();

      GameManager.Load();

      //after load node shall be different
      Assert.AreNotEqual(gameNode, GameManager.Context.CurrentNode);
      var hero = GameManager.Context.CurrentNode.GetTiles<Hero>().Single();
      Assert.NotNull(hero);

      //hero position shall match
      Assert.AreEqual(hero.Point, pt);
      Assert.AreEqual(hero, Hero);
    }

  }
}
