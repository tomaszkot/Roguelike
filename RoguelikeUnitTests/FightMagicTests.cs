using NUnit.Framework;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FightMagicTests : TestBase
  {
    [Test]
    public void SpellPropertiesTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var fireBallScroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
      var spell = fireBallScroll.CreateSpell(hero);
      Assert.Greater(spell.Damage, 0);
      
    }

    [Test]
    public void SimpleBallsTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      
      var enemy = game.GameManager.EnemiesManager.Enemies.First();
      var enemyHealth = enemy.Stats.Health;
      var mana = hero.Stats.Mana;

      var fireBallScroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
      hero.UseScroll(fireBallScroll, enemy);
      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.Greater(mana, hero.Stats.Mana);
    }
  }
}
