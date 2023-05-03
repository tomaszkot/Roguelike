using NUnit.Framework;
using OuaDII.Tiles.LivingEntities;
using Roguelike;
using Roguelike.Tiles.Looting;
using System;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class EnemyTests : TestBase
  {

    [Test]
    public void EnemyDisplayName()//SetEntitiesLevel set power
    {
      var _info = new OuaDII.Generators.GenerationInfo();
      _info.SetMinWorldSize(100);
      _info.Counts.WorldEnemiesCount = 0;
      CreateWorld(true, _info);
      {
        var en = Container.GetInstance<Enemy>();
        en.Symbol = EnemySymbols.BatSymbol;
        Assert.AreEqual(en.Name, "Bat");
        Assert.AreEqual(en.DisplayedName, "Bat");
      }
      {
        var en = Container.GetInstance<Enemy>();
        en.Name = "wolf_skeleton";
        Assert.AreEqual(en.Name, "wolf_skeleton");
        Assert.AreEqual(en.DisplayedName, "Wolf's Skeleton");
      }
    }

    [Test]
    public void TestNames()
    {
      var _info = new OuaDII.Generators.GenerationInfo();
      _info.Counts.WorldEnemiesCount = 0;
      CreateWorld(true, _info);
      var enemy = new Enemy(GameManager.Container, EnemySymbols.GetSymbolFromName("lava_golem"));
      Assert.AreEqual(enemy.Symbol, EnemySymbols.CommonEnemySymbol);
      Assert.AreEqual(enemy.DisplayedName, "Enemy");
      enemy.InitFromTag("lava_golem");
      
      Assert.AreEqual(enemy.PowerKind, Roguelike.Tiles.LivingEntities.EnemyPowerKind.Plain);
      Assert.AreEqual(enemy.DisplayedName, "Lava golem");
      Assert.AreEqual(enemy.EntityKind, Roguelike.Tiles.LivingEntities.EntityKind.Unset);
      enemy.SetLevel(1);
      Assert.Greater(enemy.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.ResistFire), enemy.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.ResistCold));

      var enemyCh = new Enemy(GameManager.Container, EnemySymbols.CommonEnemySymbol);
      enemyCh.InitFromTag("lava_golem_ch");
      Assert.AreEqual(enemyCh.DisplayedName, "Lava golem");
      Assert.AreEqual(enemyCh.EntityKind, Roguelike.Tiles.LivingEntities.EntityKind.Unset);
      Assert.AreEqual(enemyCh.PowerKind, Roguelike.Tiles.LivingEntities.EnemyPowerKind.Champion);
      Assert.Greater(enemyCh.Stats.Health, enemy.Stats.Health);
      enemyCh.SetLevel(1);
      Assert.Greater(enemyCh.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.ResistFire), enemyCh.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.ResistCold));
    }

    [Test]
    public void PlainEnemyPower()//SetEntitiesLevel set power
    {
      var _info = new OuaDII.Generators.GenerationInfo();
      _info.SetMinWorldSize(100);
      _info.Counts.WorldEnemiesCount = 0;
      CreateWorld(true, _info);

      var bear = Container.GetInstance<Enemy>();
      bear.Symbol = EnemySymbols.CommonEnemySymbol;
      bear.tag1 = "bear";
      bear.SetLevel(1);

      var bat = Container.GetInstance<Enemy>();
      bat.Symbol = EnemySymbols.BatSymbol;
      bat.tag1 = "bat";
      bat.SetLevel(1);
      bat.Container = Container;

      var bat1 = Container.GetInstance<Enemy>();
      bat1.Symbol = EnemySymbols.BatSymbol;
      bat1.tag1 = "bat";
      bat1.SetLevel(2);
      bat1.Container = Container;

      Assert.Greater(bat1.Stats.Defense, bat.Stats.Defense);
      Assert.Greater(bat1.Stats.Health, bat.Stats.Health);

      Assert.Less(bat1.Stats.Defense, bat.Stats.Defense*2);
      Assert.Less(bat1.Stats.Health, bat.Stats.Health * 2);

      Assert.Greater(bear.Stats.MeleeAttack, bat.Stats.MeleeAttack);
      Assert.Less(bear.Stats.MeleeAttack, bat.Stats.MeleeAttack * 3);
      var hero = GameManager.Hero;
      hero.AlwaysHit[Roguelike.Attributes.AttackKind.Melee] = true;
      int counter = 0;
      while (bat.Alive)
      {
        bat.OnMeleeHitBy(hero);
        counter++;
        GotoNextHeroTurn();
      }

      
      int counter2 = 0;
      while (bat1.Alive)
      {
        bat1.OnMeleeHitBy(hero);
        counter2++;
        GotoNextHeroTurn();
      }
      Assert.Greater(counter2, counter);
      //GameManager.World.SetTile(enemy, GameManager.World.GetRandomEmptyTile().Point);
    }

    [Test]
    public void EnemyMeleePowerByLevel()
    {
      var _info = new OuaDII.Generators.GenerationInfo();
      _info.SetMinWorldSize(100);
      _info.Counts.WorldEnemiesCount = 0;
      CreateWorld(true, _info);
      var hero = GameManager.Hero;
      var heroHealthDiff1 = 0.0;
      var enName = "bat";
      float heroHealth = 0;
      {
        var enemy1 = Container.GetInstance<Enemy>();
        //1st enemy at level 1
        CreateEnemy(enemy1, 1, enName);

        hero.AlwaysHit[Roguelike.Attributes.AttackKind.Melee] = true;

        heroHealthDiff1 = AssertHeroDamage(hero, enemy1);
        GameManager.CurrentNode.SetTile(new Dungeons.Tiles.Tile(), enemy1.Position);

        //2nd enemy at level 2
        var enemy2 = Container.GetInstance<Enemy>();
        CreateEnemy(enemy2, 2, enName);
        var tmp = AssertHeroDamage(hero, enemy2);
        Assert.Greater(tmp, heroHealthDiff1);
        GameManager.CurrentNode.SetTile(new Dungeons.Tiles.Tile(), enemy2.Position);
      }

      //3rd enemy at level 7
      var enemy3 = Container.GetInstance<Enemy>();
      CreateEnemy(enemy3, 7, enName);

      heroHealth = hero.Stats.Health;
      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Defense, 30);//this is close to defence at this stage of the game
      hero.OnMeleeHitBy(enemy3);
      Assert.False(hero.LastingEffects.Any());

      var diff2 = heroHealth - hero.Stats.Health;
      var abs = Math.Abs(diff2 - heroHealthDiff1);
      var less = diff2 < heroHealthDiff1 ? diff2 : heroHealthDiff1;
      var perc = abs*100 / less;
      Assert.Less(perc, 29f);

    }

    private static float AssertHeroDamage(Roguelike.Tiles.LivingEntities.Hero hero, Enemy enemy)
    {
      var heroHealth = hero.Stats.Health;
      hero.OnMeleeHitBy(enemy);
      var heroHealthDiff1 = heroHealth - hero.Stats.Health;
      Assert.Greater(heroHealthDiff1, 0);
      return heroHealthDiff1;
    }

    private void CreateEnemy(Enemy enemy, int level, string name)
    {
      enemy.Symbol = EnemySymbols.CommonEnemySymbol;
      enemy.tag1 = name;
      enemy.Name = name;
      enemy.SetLevel(level);
      enemy.AlwaysHit[Roguelike.Attributes.AttackKind.Melee] = true;
      enemy.Container = this.GameManager.Container;

      var emptyHeroNeib = GameManager.CurrentNode.GetEmptyNeighborhoodPoint(GameManager.Hero, Dungeons.TileContainers.DungeonNode.EmptyNeighborhoodCallContext.Move);
      var set = GameManager.CurrentNode.SetTile(enemy, emptyHeroNeib.Item1);
      Assert.True(set);
    }

    [Test]
    public void EnemyMagicPowerByLevel()
    {
      var _info = new OuaDII.Generators.GenerationInfo();
      _info.SetMinWorldSize(100);
      _info.Counts.WorldEnemiesCount = 0;
      CreateWorld(true, _info);
      var hero = GameManager.Hero;
      hero.ImmuneOnEffects = true;
      var diff1 = 0.0;
      float heroHealth = 0;
      var enName = "Druid";
      {
        var enemy = Container.GetInstance<Enemy>();
        CreateEnemy(enemy, 1, enName);

        hero.AlwaysHit[Roguelike.Attributes.AttackKind.Melee] = true;

        heroHealth = hero.Stats.Health;
        enemy.PrefferedFightStyle = Roguelike.Tiles.LivingEntities.PrefferedFightStyle.Magic;//use spells
        enemy.ActiveManaPoweredSpellSource = new Scroll(Roguelike.Spells.SpellKind.FireBall);
        
        Assert.True(GameManager.EnemiesManager.AttackIfPossible(enemy, hero));
        GameManager.CurrentNode.SetTile(new Dungeons.Tiles.Tile(), enemy.Position);
        diff1 = heroHealth - hero.Stats.Health;
        Assert.Greater(diff1, 0);

        var enemy2 = Container.GetInstance<Enemy>();
        CreateEnemy(enemy2, 2, enName);
        heroHealth = hero.Stats.Health;
        Assert.True(GameManager.EnemiesManager.AttackIfPossible(enemy2, hero));
        var tmp = heroHealth - hero.Stats.Health;
        Assert.Greater(tmp, diff1);
        GameManager.CurrentNode.SetTile(new Dungeons.Tiles.Tile(), enemy2.Position);
      }
      var enemy1 = Container.GetInstance<Enemy>();
      CreateEnemy(enemy1, 7, enName);

      heroHealth = hero.Stats.Health;
      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.ResistFire, 45);//this is close to defence at this stage of the game
      Assert.True(GameManager.EnemiesManager.AttackIfPossible(enemy1, hero));
      Assert.False(hero.LastingEffects.Any());

      var diff2 = heroHealth - hero.Stats.Health;
      var abs = Math.Abs(diff2 - diff1);
      var less = diff2 < diff1 ? diff2 : diff1;
      var perc = abs * 100 / less;
      Assert.Greater(diff2, diff1);
      Assert.Less(perc, 430f);

    }
  }
}
