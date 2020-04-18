using NUnit.Framework;
using Roguelike.Policies;
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

      Assert.True(game.GameManager.HeroTurn);

      var scroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
      var policy = Container.GetInstance<SpellCastPolicy>();
      policy.Target = enemy;
      policy.Scroll = scroll;
      policy.OnApplied += (s, e) =>
      {
        game.GameManager.OnHeroPolicyApplied(this, policy);
      };
      hero.UseScroll(policy);
      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.Greater(mana, hero.Stats.Mana);

      Assert.False(game.GameManager.HeroTurn);
    }
  }
}
