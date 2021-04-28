using Dungeons;
using NUnit.Framework;
using Roguelike.Tiles.Interactive;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class InteractionTests : TestBase
  {
    [Test]
    public void TestBarrelsAndPlainChests()
    {
      var game = CreateGame(true);
      var gi = new GenerationInfo();
      Assert.Greater(gi.NumberOfRooms, 3);

      TestInteraction<Barrel>(game, true);
      TestInteraction<Chest>(game, false);
    }

    private void TestInteraction<T>(Roguelike.RoguelikeGame game, bool interShallBeDestroyed) where T : Roguelike.Tiles.Interactive.InteractiveTile, new()
    {
      var inters = game.Level.GetTiles<T>();
      var intersCount = inters.Count;
      Assert.GreaterOrEqual(intersCount, 5);
      foreach (var inter in inters)
      {
        InteractHeroWith(inter);
      }

      inters = game.Level.GetTiles<T>();

      Assert.AreEqual(inters.Count, interShallBeDestroyed ? 0 : intersCount);
    }

  }

}
