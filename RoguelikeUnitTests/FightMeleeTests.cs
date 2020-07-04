using NUnit.Framework;
using Roguelike.Tiles;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FightMeleeTests : TestBase
  {
    [Test]
    public void EquipmentImpactTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.Greater(game.GameManager.EnemiesManager.Enemies.Count, 0);
      var enemy = game.GameManager.EnemiesManager.Enemies.First();
      var enemyHealth = enemy.Stats.Health;
      enemy.OnPhysicalHit(hero);
      Assert.Greater(enemyHealth, enemy.Stats.Health);
      enemyHealth = enemy.Stats.Health;

      var wpn = GenerateRandomEqOnLevelAndCollectIt<Weapon>();
      enemy.OnPhysicalHit(hero);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
    }

    [Test]
    public void KillEnemy()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var initEnemyCount = game.GameManager.EnemiesManager.Enemies.Count;
      Assert.Greater(initEnemyCount, 0);
      Assert.AreEqual(initEnemyCount, game.GameManager.CurrentNode.GetTiles<Enemy>().Count);

      var enemy = game.GameManager.EnemiesManager.Enemies.First();
      while(enemy.Alive)
        InteractHeroWith(enemy as Enemy);
      var finalEnemyCount = game.GameManager.EnemiesManager.Enemies.Count;
      Assert.AreEqual(finalEnemyCount, initEnemyCount - 1);
      Assert.AreEqual(finalEnemyCount, game.GameManager.CurrentNode.GetTiles<Enemy>().Count);
    }
  }
}
