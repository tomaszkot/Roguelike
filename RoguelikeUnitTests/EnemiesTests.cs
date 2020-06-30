using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Managers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class EnemiesTests : TestBase
  {
    [Test]
    public void TestLevel()
    {
      var game = CreateGame();
      Assert.AreEqual(game.Hero.Level, 1);

      //TODO add to node during generation
      var enemy = new Enemy();
      Assert.AreEqual(enemy.Level, 1);
      var chemp = new Enemy();
      chemp.SetNonPlain(false);
      Assert.AreEqual(chemp.Level, 1);

      var boss = new Enemy();
      boss.SetNonPlain(true);
      Assert.AreEqual(boss.Level, 1);
    }

    float GetHitAttackValue(Enemy en)
    {
      return en.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Attack);
    }

    [Test]
    public void TestPower()
    {
      var game = CreateGame();

      var enemy = new Enemy();
      var chemp = new Enemy();
      chemp.SetNonPlain(false);
      var boss = new Enemy();
      boss.SetNonPlain(true);

      var enemyHit = GetHitAttackValue(enemy);
      Assert.Greater(GetHitAttackValue(chemp), enemyHit);
      Assert.Greater(GetHitAttackValue(boss), GetHitAttackValue(chemp));

      enemy.SetLevel(enemy.Level + 1);
      Assert.Greater(GetHitAttackValue(enemy), enemyHit);
    }

    void GoDown()
    {
      var down = game.Level.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelDown).SingleOrDefault();
      Assert.NotNull(down);
      //hero shall be on the level
      Assert.NotNull(game.Level.GetTiles<Hero>().SingleOrDefault());

      var result = game.GameManager.InteractHeroWith(down);
      Assert.AreEqual(result, InteractionResult.ContextSwitched);
    }

    [Test]
    public void TestPowerIncrease()
    {
      var game = CreateGame();
      Enemy lastPlain = null;
      Enemy lastChemp = null;
      Enemy lastBoss = null;
      var hero = game.Level.GetTiles<Hero>().SingleOrDefault();

      EntityStatKind[] statKinds = new[] { EntityStatKind.Attack, EntityStatKind.Defence, EntityStatKind.Magic };
      float lastDamageFromPlain = 0;
      float lastDamageFromChemp = 0;
      float lastDamageFromBoss = 0;
      Assert.Greater(Roguelike.GenerationInfo.MaxLevelIndex, 0);

      for (var levelIndex = 0; levelIndex < Roguelike.GenerationInfo.MaxLevelIndex; levelIndex++)
      {
        var enemies = game.GameManager.EnemiesManager.Enemies.Cast<Enemy>().ToList();
        Assert.Greater(enemies.Count, 2);

        var boss = enemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).First();
        boss.SetBoss();
        Assert.AreEqual(boss.PowerKind, EnemyPowerKind.Boss);
        var chemp = enemies.Where(i => i.PowerKind == EnemyPowerKind.Champion).First();
        var plain = enemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).First();

        if (levelIndex > 0)
        {
          Assert.Greater(plain.Level, lastPlain.Level);
          Assert.Greater(chemp.Level, lastChemp.Level);
          Assert.Greater(boss.Level, lastBoss.Level);

          foreach (var esk in statKinds)
          {
            Assert.Greater(plain.Stats.GetStat(esk).Value.TotalValue, lastPlain.Stats.GetStat(esk).Value.TotalValue);
            Assert.Greater(chemp.Stats.GetStat(esk).Value.TotalValue, lastChemp.Stats.GetStat(esk).Value.TotalValue);
            Assert.Greater(boss.Stats.GetStat(esk).Value.TotalValue, lastBoss.Stats.GetStat(esk).Value.TotalValue);
          }
        }

        float diffPlain = CheckHit(hero, lastDamageFromPlain, plain);
        float diffChemp = CheckHit(hero, lastDamageFromChemp, chemp);
        float diffBoss = CheckHit(hero, lastDamageFromBoss, boss);

        lastDamageFromPlain = diffPlain;
        lastDamageFromChemp = diffChemp;
        lastDamageFromBoss = diffBoss;

        lastPlain = plain;
        lastChemp = chemp;
        lastBoss = boss;
        
        GoDown();
      }

    }

    private static float CheckHit(Hero hero, float lastDamageFromPlain, Enemy plain)
    {
      var healthBefore = hero.Stats.Health;
      hero.OnPhysicalHit(plain);
      var healthAfter = hero.Stats.Health;
      Assert.Greater(healthBefore, healthAfter);
      var diffPlain = healthBefore - healthAfter;
      Assert.Greater(diffPlain, lastDamageFromPlain);
      return diffPlain;
    }
  }
}
