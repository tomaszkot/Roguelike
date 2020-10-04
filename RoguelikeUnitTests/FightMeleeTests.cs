using NUnit.Framework;
using Roguelike;
using Roguelike.Tiles;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FightMeleeTests : TestBase
  {
    [Test]
    public void NonPlainEnemyUsesEffects()
    {
      var game = CreateGame(numEnemies:1, numberOfRooms:1);
      var hero = game.Hero;

      //Assert.AreEqual(game.GameManager.EnemiesManager.Enemies.Count, 1);
      var enemy = game.GameManager.EnemiesManager.GetEnemies().Where(i => i.PowerKind != EnemyPowerKind.Plain).FirstOrDefault();
      if (enemy == null)
      {
        enemy = game.GameManager.EnemiesManager.GetEnemies()[0];
        enemy.SetNonPlain(false);
        enemy = game.GameManager.EnemiesManager.GetEnemies().Where(i => i.PowerKind != EnemyPowerKind.Plain).First();
      }
      
      Assert.AreEqual(enemy.LastingEffects.Count, 0);
      GenerationInfo.ChanceToTurnOnSpecialSkillByEnemy = 1f;

      var closeHero = game.Level.GetClosestEmpty(hero);
      game.Level.SetTile(enemy, closeHero.Point);
      enemy.OnPhysicalHit(hero);

      game.GameManager.Context.TurnOwner = TurnOwner.Allies;
      game.GameManager.Context.PendingTurnOwnerApply = true;
      //game.GameManager.MakeGameTick();
      GotoNextHeroTurn(game);
      //Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Enemies);
      Assert.AreEqual(enemy.LastingEffects.Count, 1);
      var eff = enemy.LastingEffects[0].Type;
      Assert.True(LivingEntity.PossibleEffectsToUse.Contains(eff));
    }

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
