using NUnit.Framework;
using Roguelike;
using Roguelike.Tiles;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class ControllTests : TestBase
  {
    [Test]
    public void TestTurnOwner()
    {
      var game = CreateGame();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      game.GameManager.SkipHeroTurn();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Allies);
      game.GameManager.MakeGameTick();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Enemies);
      game.GameManager.MakeGameTick();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
    }
  }
}