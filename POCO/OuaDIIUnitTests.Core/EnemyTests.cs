using NUnit.Framework;
using OuaDII.Tiles.LivingEntities;
using Roguelike;
using Roguelike.Attributes;
//using Roguelike.Tiles.LivingEntities;
//using Roguelike.Tiles.LivingEntities;
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
      var _info = CreateGenerationInfo();
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
    [Repeat(1)]
    public void TestGeneratedNames()
    {
      var _info = CreateGenerationInfo();
      _info.Counts.WorldEnemiesCount = 100;
      CreateWorld(true, _info);
      var enemies = GameManager.CurrentNode.GetTiles<Enemy>();
      foreach (var en in enemies)
      {
        if (en.tag1 != "pond_creature_ch")
        {
          Assert.True(en.tag1.ToLower().Contains(en.Name.ToLower()));
          Assert.True(en.tag1.ToLower().Contains(en.DisplayedName.ToLower()));
        }
      }
    }

    [Test]
    public void TestNames()
    {
      var _info = CreateGenerationInfo();
      _info.Counts.WorldEnemiesCount = 0;
      CreateWorld(true, _info);
      var enemy = new Enemy(GameManager.Container, EnemySymbols.GetSymbolFromName("lava_golem"));
      Assert.AreEqual(enemy.Symbol, EnemySymbols.CommonEnemySymbol);
      Assert.AreEqual(enemy.DisplayedName, "Enemy");
      enemy.InitFromTag("lava_golem", false);

      Assert.AreEqual(enemy.PowerKind, Roguelike.Tiles.LivingEntities.EnemyPowerKind.Plain);
      Assert.AreEqual(enemy.DisplayedName, "Lava Golem");
      Assert.AreEqual(enemy.EntityKind, Roguelike.Tiles.LivingEntities.EntityKind.Unset);
      enemy.SetLevel(1);
      Assert.Greater(enemy.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.ResistFire), enemy.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.ResistCold));

      var enemyCh = new Enemy(GameManager.Container, EnemySymbols.CommonEnemySymbol);
      enemyCh.InitFromTag("lava_golem_ch", false);
      Assert.AreEqual(enemyCh.DisplayedName, "Lava Golem");
      Assert.AreEqual(enemyCh.EntityKind, Roguelike.Tiles.LivingEntities.EntityKind.Unset);
      Assert.AreEqual(enemyCh.PowerKind, Roguelike.Tiles.LivingEntities.EnemyPowerKind.Champion);
      Assert.Greater(enemyCh.Stats.Health, enemy.Stats.Health);
      enemyCh.SetLevel(1);
      Assert.Greater(enemyCh.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.ResistFire), enemyCh.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.ResistCold));

      var anim = new Roguelike.Tiles.LivingEntities.Animal(GameManager.Container, Roguelike.Tiles.LivingEntities.AnimalKind.Hen);
      Assert.AreEqual(anim.DisplayedName, "Hen");
    }

    [Test]
    public void PlainEnemyPower()//SetEntitiesLevel set power
    {
      var _info = CreateGenerationInfo();
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

      Assert.Less(bat1.Stats.Defense, bat.Stats.Defense * 2);
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

    void RemoveEnemy(Enemy en)
    {
      GameManager.CurrentNode.SetTile(new Dungeons.Tiles.Tile(), en.Position);
    }

    [Test]
    [Repeat(1)]
    public void EnemyMeleePowerByLevel()
    {
      var _info = CreateGenerationInfo();
      _info.SetMinWorldSize(100);
      _info.Counts.WorldEnemiesCount = 0;
      CreateWorld(true, _info);
      var hero = GameManager.Hero;
      var heroGainedDamageLevel1 = 0.0;
      var enName = "bat";
      {
        //1st enemy at level 1
        var enemy1 = CreateEnemy(1, enName);

        //hero.AlwaysHit[Roguelike.Attributes.AttackKind.Melee] = true;
        heroGainedDamageLevel1 = AssertHeroGainsDamage(hero, enemy1);
        RemoveEnemy(enemy1);

        //2nd enemy at level 2
        var enemy2 = CreateEnemy(2, enName);
        var heroGainedDamageLevel2 = AssertHeroGainsDamage(hero, enemy2);
        Assert.Greater(heroGainedDamageLevel2, heroGainedDamageLevel1);
        RemoveEnemy(enemy2);
      }

      //3rd enemy at level 7
      var enemy3 = CreateEnemy(7, enName);
      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Defense, 30);//this is close to Defense at this stage of the game
      var heroGainedDamageLevel7 = AssertHeroGainsDamage(hero, enemy3);
      //heroHealth = hero.Stats.Health;
      Assert.False(hero.LastingEffects.Any());

      //result shall be close, but not guaranteed which is bigger
      var abs = Math.Abs(heroGainedDamageLevel7 - heroGainedDamageLevel1);
      var lessDamage = heroGainedDamageLevel7 < heroGainedDamageLevel1 ? heroGainedDamageLevel7 : heroGainedDamageLevel1;
      var perc = abs * 100 / lessDamage;
      Assert.Less(perc, 50f);

    }

    [Test]
    public void AnimalWildRageTest()
    {
      Enemy bear;
      PrepareWorld("Bear", out bear);
      var hero = GameManager.Hero;
      Assert.AreEqual(bear.EntityKind, Roguelike.Tiles.LivingEntities.EntityKind.Animal);
      this.GameManager.EnemiesManager.AddEntity(bear);

      PlaceCloseToHero(hero, bear);
      PrepareEntityForLongLiving(bear);
      PrepareEntityForLongLiving(hero);
      hero.d_immortal = true;
      bool hadRage = false;
      var enStr = bear.Stats.Strength;
      while (bear.Alive)
      {
        bear.OnMeleeHitBy(hero);
        GotoNextHeroTurn();
        if (bear.HasLastingEffect(Roguelike.Effects.EffectType.WildRage))
        {
          hadRage = true;
          Assert.Greater(bear.Stats.Strength, enStr);
          break;
        }
      }
      Assert.True(hadRage);
    }

    [Test]
    [Repeat(1)]
    public void UndeadHoochTest()
    {
      Enemy en;
      PrepareWorld("Skeleton", out en);
      en.Symbol = EnemySymbols.SkeletonSymbol;
      var hero = GameManager.Hero;
      Assert.AreEqual(en.EntityKind, Roguelike.Tiles.LivingEntities.EntityKind.Undead);
      GameManager.EnemiesManager.AddEntity(en);

      PlaceCloseToHero(hero, en);
      en.AlwaysHit[AttackKind.Melee] = true;
      PrepareEntityForLongLiving(en);
      PrepareEntityForLongLiving(hero);
      //hero.d_immortal = true;
      bool hadLe = false;
      var enStr = en.Stats.Strength;
      var enMelleInit = en.GetAttackValue(AttackKind.Melee);
      var heroHealthInit = hero.Stats.Health;
      hero.OnMeleeHitBy(en);
      var heroHealthInitDiff = heroHealthInit - hero.Stats.Health;
      Assert.Greater(heroHealthInitDiff, 0);
      while (en.Alive)
      {
        en.OnMeleeHitBy(hero);
        GotoNextHeroTurn();
        if (en.HasLastingEffect(Roguelike.Effects.EffectType.Hooch))
        {
          hadLe = true;
          Assert.Greater(en.Stats.Strength, enStr);
          var enMelleAfter = en.GetAttackValue(AttackKind.Melee);
          Assert.Greater(enMelleAfter.CurrentTotal, enMelleInit.CurrentTotal);
          var heroHealthAfter = hero.Stats.Health;
          hero.OnMeleeHitBy(en);
          var heroHealthAfterDiff = heroHealthAfter - hero.Stats.Health;
          Assert.Greater(heroHealthAfterDiff, heroHealthInitDiff);

          break;
        }
      }
      Assert.True(hadLe);
    }

    private void PrepareWorld(string enemyName, out Enemy bear)
    {
      var _info = CreateGenerationInfo();
      _info.SetMinWorldSize(100);
      _info.Counts.WorldEnemiesCount = 0;
      CreateWorld(true, _info);
      
      bear = Container.GetInstance<Enemy>();
      bear.Symbol = EnemySymbols.CommonEnemySymbol;
      bear.tag1 = enemyName.ToLower();
      bear.SetLevel(1);
      bear.Name = enemyName;
     
    }

    private static float AssertHeroGainsDamage(Roguelike.Tiles.LivingEntities.Hero hero, Enemy enemy)
    {
      var heroHealth = hero.Stats.Health;
      hero.OnMeleeHitBy(enemy);
      var heroHealthDiff1 = heroHealth - hero.Stats.Health;
      Assert.Greater(heroHealthDiff1, 0);
      return heroHealthDiff1;
    }

    private Enemy CreateEnemy(int level, string name)
    {
      var enemy1 = Container.GetInstance<Enemy>();
      InitEnemy(enemy1, level, name);
      return enemy1;
    }

    private void InitEnemy(Enemy enemy, int level, string name)
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
      var _info = CreateGenerationInfo();
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
        InitEnemy(enemy, 1, enName);

        hero.AlwaysHit[Roguelike.Attributes.AttackKind.Melee] = true;

        heroHealth = hero.Stats.Health;
        enemy.PrefferedFightStyle = Roguelike.Tiles.LivingEntities.PrefferedFightStyle.Magic;//use spells
        enemy.ActiveManaPoweredSpellSource = new Scroll(Roguelike.Spells.SpellKind.FireBall);

        Assert.True(GameManager.EnemiesManager.AttackIfPossible(enemy, hero));
        GameManager.CurrentNode.SetTile(new Dungeons.Tiles.Tile(), enemy.Position);
        diff1 = heroHealth - hero.Stats.Health;
        Assert.Greater(diff1, 0);

        var enemy2 = Container.GetInstance<Enemy>();
        InitEnemy(enemy2, 2, enName);
        heroHealth = hero.Stats.Health;
        Assert.True(GameManager.EnemiesManager.AttackIfPossible(enemy2, hero));
        var tmp = heroHealth - hero.Stats.Health;
        Assert.Greater(tmp, diff1);
        GameManager.CurrentNode.SetTile(new Dungeons.Tiles.Tile(), enemy2.Position);
      }
      var enemy1 = Container.GetInstance<Enemy>();
      InitEnemy(enemy1, 7, enName);

      heroHealth = hero.Stats.Health;
      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.ResistFire, 45);//this is close to Defense at this stage of the game
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
