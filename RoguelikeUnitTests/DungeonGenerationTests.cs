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
  class DungeonGenerationTests : TestBase
  {
    [Test]
    public void NewGameTest()
    {
      GameManager.SetContext(GameNode, AddHero(), Roguelike.GameContextSwitchKind.NewGame);

      Assert.NotNull(GameNode.GetTiles<Hero>().Single());
    }

  }
}
