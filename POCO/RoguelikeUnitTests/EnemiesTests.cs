//using Dungeons;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class EnemiesTests : TestBase
  {

    [Test]
    //[Repeat(10)]
    public void TestCommandRaiseMyFriends()
    {
      var game = CreateGame(numEnemies:0);
      game.Hero.d_immortal = true;

      var enemyPlain = SpawnEnemy();
      Assert.AreEqual(enemyPlain.Symbol, 's');
      PlaceCloseToHero(enemyPlain);

      var enemyChemp = SpawnEnemy();
      enemyChemp.Symbol = EnemySymbols.SkeletonSymbol;
      enemyChemp.SetNonPlain(false);
      PlaceCloseToHero(enemyChemp);

      List<Enemy> de = game.Level.GetDeadEnemies();
      Assert.AreEqual(de.Count, 0);
      Assert.AreEqual(game.Level.GetTiles<Enemy>().Count, 2);
      var ens = game.GameManager.EnemiesManager.GetEnemies();
      Assert.AreEqual(ens.Count, 2);

      //kill plain one
      while (enemyPlain.Alive)
      {
        enemyPlain.OnMeleeHitBy(game.Hero);
        GotoNextHeroTurn();
      }
      GotoNextHeroTurn();
      
      //make sure only chemp is alive
      ens = game.GameManager.EnemiesManager.GetEnemies();
      Assert.AreEqual(ens.Count, 1);
      Assert.AreEqual(ens[0], enemyChemp);
      var atPlainPos = game.Level.GetTile(enemyPlain.point);
      Assert.True(atPlainPos.IsEmpty || atPlainPos is Loot);
      Assert.AreEqual(game.Level.GetTiles<Enemy>().Count, 1);


      de = game.Level.GetDeadEnemies();
      Assert.AreEqual(de.Count, 1);
      var ea = new EnemyAction();
      ea.CommandKind = EntityCommandKind.RaiseMyFriends;
      ea.Kind = EnemyActionKind.SendComand;
      game.GameManager.SendCommand(enemyChemp, ea);

      de = game.Level.GetDeadEnemies();
      Assert.AreEqual(de.Count, 0);
      ens = game.GameManager.EnemiesManager.GetEnemies();
      Assert.AreEqual(ens.Count, 2);
      Assert.AreEqual(ens[1], enemyPlain);
      Assert.AreEqual(ens[1].Alive, true);
      Assert.Greater(ens[1].Stats.Health, 0);
    }

    [Test]
    public void TestPhysicalDamage()
    {
      var game = CreateGame();
      var enemy = SpawnEnemy();
      enemy.Symbol = EnemySymbols.SkeletonSymbol;
      Assert.AreEqual(enemy.Stats.Strength, enemy.Stats[EntityStatKind.MeleeAttack].TotalValue);

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
    //[Repeat(5)]
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
        boss.SetNonPlain(true) ;
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
      hero.OnMeleeHitBy(plain);
      var healthAfter = hero.Stats.Health;
      Assert.Greater(healthBefore, healthAfter);
      var diffPlain = healthBefore - healthAfter;
      Assert.Greater(diffPlain, lastDamageFromPlain);
      return diffPlain;
    }

    [Test]
    public void TestIsImmuned()
    {
      var game = CreateGame();
      var hero = game.Level.GetTiles<Hero>().SingleOrDefault();
      var enemy = SpawnEnemy();

      enemy.Symbol = 's';
      Assert.AreEqual(enemy.Name, "Skeleton");
      Assert.True(enemy.IsImmuned(Roguelike.Effects.EffectType.Bleeding));
    }

    [Test]
    [Repeat(1)]
    public void TestSpeedInWater()
    {
      var gi = new GenerationInfo();
      gi.GenerateRandomInterior = false;
      gi.GenerateRandomStonesBlocks = false;
      gi.GenerateInteractiveTiles = false;
      var game = CreateGame(true, 1, 1, gi);
      var walls = game.Level.GetTiles<Wall>().Where(i=> !i.IsSide).ToList();
      //Assert.AreEqual(walls.Count, 0);
      var enemies = game.Level.GetTiles<Enemy>();
      Assert.AreEqual(enemies.Count, 1);
      var enemy = enemies.First();
      enemy.SetSurfaceSkillLevel(SurfaceKind.ShallowWater, 1);
      enemy.ActiveFightItem = null;//make sure it will move

      //put enemy close to hero
      var closeTile = game.Level.GetEmptyTiles().Where(i => i.DistanceFrom(game.Hero) == 3).FirstOrDefault();
      Assert.True(game.Level.SetTile(enemy, closeTile.point));
      var enemyDistFromHero = enemy.DistanceFrom(game.Hero);
      Assert.AreEqual(enemyDistFromHero, 3);
      Debug.WriteLine("init enemy pos: " + enemy.Position);

      //set water at each empty tile
      var emptyCount = game.Level.GetEmptyTiles().Count;
      game.Level.GetEmptyTiles().ForEach(i => game.Level.SetTile(new Surface() { Kind = SurfaceKind.ShallowWater }, i.point));
      Assert.AreEqual(emptyCount, game.Level.GetEmptyTiles().Count);//empty are not affected by surface

      //set water under enemy
      game.Level.SetTile(new Surface() { Kind = SurfaceKind.ShallowWater }, enemy.point);
      Assert.AreEqual(game.Level.GetSurfaceKindUnderTile(enemy), SurfaceKind.ShallowWater);
      enemies = game.Level.GetTiles<Enemy>();
      Assert.AreEqual(enemies.Count, 1);

      //make enemy turn
      var enTurnsCount = game.GameManager.Context.TurnCounts[TurnOwner.Enemies];
      game.GameManager.SkipHeroTurn();
      GotoNextHeroTurn();
      var enTurnsCountAfter = game.GameManager.Context.TurnCounts[TurnOwner.Enemies];
      Assert.Greater(enTurnsCountAfter, enTurnsCount);
      Assert.IsNull(enemy.ActiveFightItem);

      //check pos, enemy shall make 2 steps
      var enemyDistFromHeroNew = enemy.DistanceFrom(game.Hero);
      if (enemyDistFromHeroNew != enemyDistFromHero - 2)
      {
        var enNeibs = game.Level.GetNeighborTiles(game.Hero);
        enNeibs.ForEach(i =>
        {
          Debug.Write("tile neib to hero: "+i + ", Surface: "+ game.Level.GetSurfaceKindUnderTile(i) + "\r\n");
        }
        );
      }
      Assert.AreEqual(enemyDistFromHeroNew, enemyDistFromHero - 2);
    }

  }

}
