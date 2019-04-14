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
      var enemyHealth1 = enemy.Stats.Health;
      enemy.OnPhysicalHit(hero);
      var enemyHealth2 = enemy.Stats.Health;
      var enemyHealthDiff2 = enemyHealth1 - enemyHealth2;
      Assert.Greater(enemyHealthDiff2, 0);

      var wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      hero.SetEquipment(EquipmentKind.Weapon, wpn);
      enemy.OnPhysicalHit(hero);
      var enemyHealthDiff3 = enemyHealth2 - enemy.Stats.Health;
      Assert.Greater(enemyHealthDiff3, enemyHealthDiff2);
    }
  }
}
