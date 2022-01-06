using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Managers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class EnemiesTests : TestBase
  {

    [Test]
    public void TestPhysicalDamage()
    {
      var game = CreateGame();
      var enemy = SpawnEnemy();
      enemy.Symbol = EnemySymbols.SkeletonSymbol;

      Assert.False(enemy.GetNonPhysicalDamages().Any());
      enemy.Symbol = EnemySymbols.SpiderSymbol;
      Assert.True(enemy.GetNonPhysicalDamages().Any());

      var poisonVal = enemy.GetNonPhysicalDamages()[EntityStatKind.PoisonAttack];
      Assert.Greater(poisonVal, 0);
      Assert.AreEqual(enemy.Level, 1);

      var enemy1 = SpawnEnemy();
      enemy1.Symbol = EnemySymbols.SpiderSymbol;
      Assert.True(enemy1.SetLevel(2));
      Assert.AreEqual(enemy1.Level, 2);
      Assert.Greater(enemy1.GetNonPhysicalDamages()[EntityStatKind.PoisonAttack], poisonVal);
    }

    [Test]
    public void TestLevel()
    {
      var game = CreateGame();
      Assert.AreEqual(game.Hero.Level, 1);

      //TODO add to node during generation
      var enemy = SpawnEnemy();
      Assert.AreEqual(enemy.Level, 1);
      var chemp = SpawnEnemy();
      chemp.SetNonPlain(false);
      Assert.AreEqual(chemp.Level, 1);

      var boss = SpawnEnemy();
      boss.SetNonPlain(true);
      Assert.AreEqual(boss.Level, 1);
    }

    float GetHitAttackValue(Enemy en)
    {
      return en.GetCurrentValue(Roguelike.Attributes.EntityStatKind.MeleeAttack);
    }

    [Test]
    public void TestPower()
    {
      var game = CreateGame();

      var enemy = SpawnEnemy();
      var chemp = SpawnEnemy();
      chemp.SetNonPlain(false);
      var boss = SpawnEnemy();
      boss.SetNonPlain(true);

      var enemyHitValue = GetHitAttackValue(enemy);
      Assert.Greater(GetHitAttackValue(chemp), enemyHitValue);
      Assert.Greater(GetHitAttackValue(boss), GetHitAttackValue(chemp));

      SetEnemyLevel(enemy, enemy.Level + 1);
      var newLevelEnemyHitValue = GetHitAttackValue(enemy);
      Assert.Greater(newLevelEnemyHitValue, enemyHitValue);
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

      EntityStatKind[] statKinds = new[] { EntityStatKind.MeleeAttack, EntityStatKind.Defense, EntityStatKind.Magic };
      float lastDamageFromPlain = 0;
      float lastDamageFromChemp = 0;
      float lastDamageFromBoss = 0;

      var gi = new Roguelike.Generators.GenerationInfo();
      for (var levelIndex = 0; levelIndex < gi.MaxLevelIndex; levelIndex++)
      {
        var enemies = AllEnemies;
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
      hero.OnMelleeHitBy(plain);
      var healthAfter = hero.Stats.Health;
      Assert.Greater(healthBefore, healthAfter);
      var diffPlain = healthBefore - healthAfter;
      Assert.Greater(diffPlain, lastDamageFromPlain);
      return diffPlain;
    }

    [Test]
    public void TestSpeedInWater()
    {
      var game = CreateGame(true, 1, 1);
      var enemies = game.Level.GetTiles<Enemy>();
      Assert.AreEqual(enemies.Count, 1);
      var enemy = enemies.First();
      enemy.SetSurfaceSkillLevel(SurfaceKind.ShallowWater, 1);

      var closeTile = game.Level.GetEmptyTiles().Where(i => i.DistanceFrom(game.Hero) == 3).FirstOrDefault();
      game.Level.SetTile(enemy, closeTile.point);

      var enemyDistFromHero = enemy.DistanceFrom(game.Hero);
      //Assert.Less(enemyDistFromHero, 4);
      Assert.AreEqual(enemyDistFromHero, 3);

      var emptyCount = game.Level.GetEmptyTiles().Count;
      game.Level.GetEmptyTiles().ForEach(i => game.Level.SetTile(new Surface() { Kind = SurfaceKind.ShallowWater }, i.point));
      Assert.AreEqual(emptyCount, game.Level.GetEmptyTiles().Count);//empty are not affected by surface

      game.Level.SetTile(new Surface() { Kind = SurfaceKind.ShallowWater }, enemy.point);
      //check if water is under the enemy
      Assert.AreEqual(game.Level.GetSurfaceKindUnderTile(enemy), SurfaceKind.ShallowWater);
      enemies = game.Level.GetTiles<Enemy>();
      Assert.AreEqual(enemies.Count, 1);

      //make enemy turn
      var enTurnsCount = game.GameManager.Context.TurnCounts[TurnOwner.Enemies];
      game.GameManager.SkipHeroTurn();
      GotoNextHeroTurn();
      var enTurnsCountAfter = game.GameManager.Context.TurnCounts[TurnOwner.Enemies];
      Assert.Greater(enTurnsCountAfter, enTurnsCount);

      //check pos
      var enemyDistFromHeroNew = enemy.DistanceFrom(game.Hero);
      Assert.AreEqual(enemyDistFromHeroNew, enemyDistFromHero - 2);
    }

  }

}
