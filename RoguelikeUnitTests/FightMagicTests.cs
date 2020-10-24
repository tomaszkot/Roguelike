using NUnit.Framework;
using Roguelike.Policies;
using Roguelike.Spells;
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
      var spell = fireBallScroll.CreateSpell<OffensiveSpell>(hero);
      Assert.Greater(spell.Damage, 0);
      
    }

    [Test]
    public void SimpleBallsTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemy = ActiveEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
            
      UseScroll(game, hero, enemy);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.Greater(mana, hero.Stats.Mana);

      Assert.False(game.GameManager.HeroTurn);
    }

    private void UseScroll(Roguelike.RoguelikeGame game, Hero hero, LivingEntity enemy)
    {
      var scroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
      game.GameManager.Context.ApplySpellAttackPolicy(hero, enemy, scroll, null, (p) => game.GameManager.OnHeroPolicyApplied(this, p));
      
    }

    [Test]
    public void KillEnemy()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemies = game.GameManager.EnemiesManager.AllEntities;
      var initEnemyCount = enemies.Count;
      Assert.Greater(initEnemyCount, 0);
      Assert.AreEqual(initEnemyCount, game.GameManager.CurrentNode.GetTiles<Enemy>().Count);

      var enemy = enemies.First();
      while (enemy.Alive)
      {
        UseScroll(game, hero, enemy);
        GotoNextHeroTurn(game);
      }

      var finalEnemyCount = enemies.Count;
      Assert.AreEqual(finalEnemyCount, initEnemyCount - 1);
      Assert.AreEqual(finalEnemyCount, game.GameManager.CurrentNode.GetTiles<Enemy>().Count);
    }

    [Test]
    public void EnemyAttackTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemy = ActiveEnemies.Cast<Enemy>().First();
      enemy.PrefferedFightStyle = PrefferedFightStyle.Magic;//use spells
      var heroHealth = hero.Stats.Health;
      var mana = enemy.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      var emptyHeroNeib = game.Level.GetEmptyNeighborhoodPoint(game.Hero);
      game.GameManager.HandleHeroShift(emptyHeroNeib.Item2);
      Assert.False(game.GameManager.HeroTurn);

      emptyHeroNeib = game.Level.GetEmptyNeighborhoodPoint(game.Hero);
      var set = game.Level.SetTile(enemy, emptyHeroNeib.Item1);
      Assert.True(set);

      GotoNextHeroTurn(game);
      if (heroHealth == hero.Stats.Health)
      {
        game.GameManager.EnemiesManager.AttackIfPossible((enemy as Enemy), hero);//TODO
      }
      Assert.Greater(heroHealth, hero.Stats.Health);
      Assert.Greater(mana, enemy.Stats.Mana);//used mana
    }

    
  }
}
