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
      var game = CreateGame(false);
      Assert.Null(game.Hero);

      game.SetMaxLevelindex(1);
      var level0 = game.GenerateLevel<DungeonLevel>(0);

      Assert.NotNull(level0);
      Assert.AreEqual(level0.Index, 0);//1st level has index 0

      Assert.AreEqual(level0, game.Level);
      Assert.NotNull(level0.GetTiles<Hero>().Single());

      //1st level0 has only stairs down
      Assert.AreEqual(level0.GetTiles<Stairs>().Count, 1);
      Assert.AreEqual(level0.GetTiles<Stairs>()[0].Kind, StairsKind.LevelDown);

      var level1 = game.GenerateLevel< DungeonLevel>(1);
      Assert.AreNotEqual(level0, level1);

      //last level has NOT stairs down, but shall have up ones
      Assert.AreEqual(level1.GetTiles<Stairs>().Count, 1);
      Assert.AreEqual(level1.GetTiles<Stairs>()[0].Kind, StairsKind.LevelUp);
    }

  }
}
