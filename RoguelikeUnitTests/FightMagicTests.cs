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
      game.GameManager.Context.ApplySpellAttackPolicy(hero, enemy, scroll, null, (p) => game.GameManager.OnHeroPolicyApplied(this, p));

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.Greater(mana, hero.Stats.Mana);

      Assert.False(game.GameManager.HeroTurn);
    }

    [Test]
    public void EnemyAttackTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemy = game.GameManager.EnemiesManager.Enemies.Cast<Enemy>().First();
      enemy.PrefferedFightStyle = PrefferedFightStyle.Magic;
      var heroHealth = hero.Stats.Health;
      var mana = enemy.Stats.Mana;
      
      Assert.True(game.GameManager.HeroTurn);
      var emptyHeroNeib = game.Level.GetEmptyNeighborhoodPoint(game.Hero);
      game.GameManager.HandleHeroShift(emptyHeroNeib.Item2);
      Assert.False(game.GameManager.HeroTurn);

      emptyHeroNeib = game.Level.GetEmptyNeighborhoodPoint(game.Hero);
      var set = game.Level.SetTile(enemy, emptyHeroNeib.Item1);
      Assert.True(set);

      game.MakeGameTick();//make allies move
      Assert.AreEqual(game.GameManager.Context.TurnOwner, Roguelike.TurnOwner.Enemies);
      game.MakeGameTick();//make enemies move
      Assert.True(game.GameManager.HeroTurn);

      Assert.Greater(heroHealth, hero.Stats.Health);
      Assert.Greater(mana, enemy.Stats.Mana);//used mana
    }
  }
}
