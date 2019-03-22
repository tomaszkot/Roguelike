using NUnit.Framework;
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
      GameManager.SetContext(GameNode, AddHero(), Roguelike.GameContextSwitchKind.NewGame);

      Assert.AreEqual(GameNode, GameManager.Context.CurrentNode);
      var hero = GameNode.GetTiles<Hero>().Single();
      Assert.NotNull(hero);
      var pt = GameNode.GetFirstEmptyPoint();
      Assert.AreNotEqual(hero.Point, pt);
      GameNode.SetTile(hero, pt.Value);

      GameManager.Save();

      GameManager.Load();

      //after load node shall be different
      Assert.AreNotEqual(GameNode, GameManager.Context.CurrentNode);
      hero = GameNode.GetTiles<Hero>().Single();
      Assert.NotNull(hero);

      //hero position shall match
      Assert.AreEqual(hero.Point, pt);
    }

  }
}
