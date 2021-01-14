using NUnit.Framework;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;
using System;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class SpellsTests : TestBase
  {
    public const int BaseFactor = 30;

    [Test]
    public void TestDescriptions()
    {
      var game = CreateGame();
      var spell = Scroll.CreateSpell(SpellKind.IronSkin, game.Hero);
      var castedSpell = spell as PassiveSpell;
      var features = castedSpell.GetFeatures();
      Assert.NotNull(features);
      
      Assert.AreEqual(Math.Round(castedSpell.StatKindEffective.Value, 3), 3.1);
      Assert.AreEqual(castedSpell.StatKindPercentage.Value, 31);
      Assert.AreEqual(features[1], "Defense: +" + (BaseFactor+1) + "%");
    }


    [Test]
    public void TourLastingTest()
    {
      var game = CreateGame();
      var spell = Scroll.CreateSpell(SpellKind.ManaShield, game.Hero) as ManaShieldSpell;
      Assert.AreEqual(spell.TourLasting, 5);
    }

    //[Test]
    //public void Test()
    //{
    //  var game = CreateGame();
    //  var spell = Scroll.CreateSpell(SpellKind.Transform, game.Hero);

    //  //game.Hero.us
    //}
  }
}
