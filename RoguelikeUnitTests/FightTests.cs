using NUnit.Framework;
using Roguelike.Tiles;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FightTests : TestBase
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
  }
}
