using NUnit.Framework;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class SpellPropsTests : TestBase
  {
    [Test]
    public void FireSpellPropertiesTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      {
        var fireBallScroll = new Scroll(SpellKind.FireBall);
        var spell = fireBallScroll.CreateSpell<OffensiveSpell>(hero) as FireBallSpell;

        var d1 = spell.Damage;
        Assert.Greater(d1, 0);
        var r1 = spell.Range;
        Assert.AreEqual(r1, 4);
        Assert.AreEqual(spell.Duration, 0);

        IncreaseSpell(hero, SpellKind.FireBall);

        spell = fireBallScroll.CreateSpell<OffensiveSpell>(hero) as FireBallSpell;
        Assert.Greater(spell.Damage, d1);
        Assert.AreEqual(spell.Range, r1 + 1);

        var spellDesc = spell.CreateSpellStatsDescription(true);
        var stats = spellDesc.GetEntityStats();
        Assert.AreEqual(stats.Length, 3);//mana, range, damage

      }
      {
        var fireBallBook = new Book(SpellKind.FireBall);
        var spellFromBook = fireBallBook.CreateSpell<OffensiveSpell>(hero);
        Assert.Greater(spellFromBook.Damage, 0);
      }
    }
        

    [Test]
    public void CrackedStoneSpellPropertiesTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var scroll = new Scroll(SpellKind.CrackedStone);
      var spell = scroll.CreateSpell<CrackedStoneSpell>(hero);

      var dur = spell.Durability;
      Assert.Greater(dur, 0);

      IncreaseSpell(hero, SpellKind.CrackedStone);
      spell = scroll.CreateSpell<CrackedStoneSpell>(hero);
      var dur1 = spell.Durability;
      Assert.Greater(dur1, dur);
    }

    [Test]
    public void FrightenSpellPropertiesTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var scroll = new Scroll(SpellKind.Frighten);
      var spell = scroll.CreateSpell<FrightenSpell>(hero);

      var dur = spell.Duration;
      Assert.Greater(dur, 0);

      var range = spell.Range;
      Assert.Greater(range, 0);

      var descNext = spell.CreateSpellStatsDescription(false);

      IncreaseSpell(hero, SpellKind.Frighten);

      spell = scroll.CreateSpell<FrightenSpell>(hero);
      var dur1 = spell.Duration;
      Assert.AreEqual(dur1, dur+1);
      Assert.AreEqual(descNext.Duration, dur1);

      var range1 = spell.Range;
      Assert.AreEqual(range1, range+1);
    }
  }
}
