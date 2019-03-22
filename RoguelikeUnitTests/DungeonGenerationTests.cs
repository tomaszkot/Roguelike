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
      var gameNode = CreateNewGame<GameNode>();

      Assert.NotNull(gameNode.GetTiles<Hero>().Single());
    }

  }
}
