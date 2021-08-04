using NUnit.Framework;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FightItemTests : TestBase
  {
    [Test]
    public void SpellPropertiesTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var fi = new ProjectileFightItem(FightItemKind.Stone, hero);
      var enemy = ActiveEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      UseFightItem(hero, enemy, fi);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.AreEqual(mana, hero.Stats.Mana);
      Assert.False(game.GameManager.HeroTurn);
    }

    private bool UseFightItem(Roguelike.Tiles.LivingEntities.Hero hero, Roguelike.Tiles.LivingEntities.Enemy enemy, ProjectileFightItem fi)
    {
      hero.Inventory.Add(fi);
      return game.GameManager.ApplyAttackPolicy(hero, enemy, fi);
    }
  }
}
