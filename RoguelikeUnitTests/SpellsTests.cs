using NUnit.Framework;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class SpellsTests : TestBase
  {
    [Test]
    public void TestDescriptions()
    {
      var game = CreateGame();
      var spell = Scroll.CreateSpell(SpellKind.IronSkin, game.Hero);
      var castedSpell = spell as PassiveSpell;
      var features = castedSpell.GetFeatures();
      Assert.NotNull(features);

    }
  }
}
