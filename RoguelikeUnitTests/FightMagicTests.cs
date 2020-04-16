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
    public void SimpleBallsTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var fireBallScroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
      var spell = fireBallScroll.CreateSpell(hero);
      Assert.Greater(spell.Damage, 0);
      var enemy = game.GameManager.EnemiesManager.Enemies.First();
      var enemyHealth = enemy.Stats.Health;
      enemy.OnHitBy(spell);
      Assert.Greater(enemyHealth, enemy.Stats.Health);
    }
  }
}
