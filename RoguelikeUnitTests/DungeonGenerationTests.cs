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
  class DungeonGenerationTests : TestBase
  {
    [Test]
    public void NewGameTest()
    {
      var gameNode = CreateNewDungeon();

      Assert.NotNull(gameNode);
      Assert.AreEqual(gameNode.Index, 0);//1st level has index 0

      Assert.AreEqual(gameNode, GameManager.Context.CurrentNode);
      Assert.NotNull(gameNode.GetTiles<Hero>().Single());

      //1st level has only stairs down
      Assert.AreEqual(gameNode.GetTiles<Stairs>().Count, 1);
      Assert.AreEqual(gameNode.GetTiles<Stairs>()[0].Kind, StairsKind.LevelDown);
    }

  }
}
