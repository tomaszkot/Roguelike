using NUnit.Framework;
using Roguelike.Spells;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using System;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class SpellsTests : TestBase
  {
    public const int BaseFactor = 30;

    [TestCase(true)]
    [TestCase(false)]
    public void HitBarrelTest(bool scroll)
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.True(game.GameManager.HeroTurn);
      var barrel = game.Level.GetTiles<Barrel>().First();
      Assert.AreEqual(game.Level.GetTile(barrel.point), barrel);

      Assert.AreEqual(game.GameManager.Context.TurnOwner, Roguelike.TurnOwner.Hero);
      Assert.True(UseFireBallSpellSource(hero, barrel, scroll));
      Assert.AreNotEqual(game.Level.GetTile(barrel.point), barrel);
      Assert.AreEqual(game.GameManager.Context.TurnOwner, Roguelike.TurnOwner.Allies);
    }

    [Test]
    public void TestDescriptions()
    {
      var game = CreateGame();
      var scroll = new Scroll(SpellKind.IronSkin);
      var spell = scroll.CreateSpell(game.Hero);
      var castedSpell = spell as PassiveSpell;
      var features = castedSpell.CreateSpellStatsDescription(true);
      Assert.NotNull(features);

      Assert.AreEqual(Math.Round(castedSpell.StatKindEffective.Value, 3), 3.1);
      Assert.AreEqual(castedSpell.StatKindPercentage.Value, 31);
      //Assert.AreEqual(features[1], "Defense: +" + (BaseFactor + 1) + "%");
    }


    [Test]
    public void TourLastingTest()
    {
      var game = CreateGame();
      var scroll = new Scroll(SpellKind.ManaShield);
      var spell = scroll.CreateSpell(game.Hero) as ManaShieldSpell;
      Assert.AreEqual(spell.Duration, 5);
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
