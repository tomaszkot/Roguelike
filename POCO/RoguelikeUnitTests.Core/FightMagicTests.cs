using Dungeons.Core;
using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Generators;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Core.Utils;
using System;
using System.Linq;
using static Dungeons.TileContainers.DungeonNode;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FightMagicTests : TestBase
  {
    [TestCase(true)]
    [TestCase(false)]
    public void ManaBasedSpellSourcePropertiesTest(bool scroll)
    {
      var game = CreateGame();
      var hero = game.Hero;
      SpellSource spellSource = null;
      if (scroll)
        spellSource = new Scroll(SpellKind.FireBall);
      else
        spellSource = new Book(SpellKind.FireBall);

      {
        //var desc = spellSource.GetDescription();
        var extraDesc = spellSource.GetExtraStatDescription(hero, true);
        Assert.NotNull(extraDesc);
        Assert.Greater(extraDesc.GetDescription().Count(), 0);

        Assert.Greater(extraDesc.Level, 0);
        Assert.Greater(extraDesc.ManaCost, 0);
        Assert.Greater(extraDesc.Damage, 0);
      }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SpellSourceEntityStatKindPropertiesTest(bool scroll)
    {
      var game = CreateGame();
      var hero = game.Hero;
      SpellSource spellSource = null;
      if (scroll)
        spellSource = new Scroll(SpellKind.FireBall);
      else
        spellSource = new Book(Roguelike.Spells.SpellKind.FireBall);

      Assert.True(new Scroll(SpellKind.FireBall).IsOffensive);

      var spell = spellSource.CreateSpell(hero);
      var stats1 = spell.CreateSpellStatsDescription(true).GetEntityStats();
      Assert.AreEqual(stats1.Count(), 3);
      var mana1 = stats1.SingleOrDefault(i => i.Kind == EntityStatKind.Mana);
      Assert.NotNull(mana1);
      var attack1 = stats1.SingleOrDefault(i => i.Kind == EntityStatKind.ElementalSpellProjectilesAttack);
      Assert.NotNull(attack1);
      var range1 = stats1.SingleOrDefault(i => i.Kind == EntityStatKind.FireBallExtraRange);
      Assert.NotNull(range1);


      var statsNextLevel = spell.CreateSpellStatsDescription(false).GetEntityStats();
      Assert.AreEqual(statsNextLevel.Count(), 3);
      var mana2 = statsNextLevel.SingleOrDefault(i => i.Kind == EntityStatKind.Mana);
      Assert.NotNull(mana2);
      Assert.Greater(mana2.Value.TotalValue, mana1.Value.TotalValue);

      var attack2 = statsNextLevel.SingleOrDefault(i => i.Kind == EntityStatKind.ElementalSpellProjectilesAttack);
      Assert.NotNull(attack2);
      Assert.Greater(attack2.Value.TotalValue, attack1.Value.TotalValue);

      var range2 = statsNextLevel.SingleOrDefault(i => i.Kind == EntityStatKind.FireBallExtraRange);
      Assert.NotNull(range2);
      Assert.GreaterOrEqual(range2.Value.TotalValue, range1.Value.TotalValue);
    }

    

    [TestCase(true)]
    [TestCase(false)]
    public void SimpleSpellSourceOnEnemyTest(bool scroll)
    {
      var game = CreateGame();
      var hero = game.Hero;
      //SpellSource spellSource = scroll ? new Scroll ? 
      hero.AlwaysHit[AttackKind.SpellElementalProjectile] = true;
      var enemy = ActivePlainEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);

      UseFireBallSpellSource(hero, enemy, scroll);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.Greater(mana, hero.Stats.Mana);
      Assert.False(game.GameManager.HeroTurn);

      var diff = enemyHealth - enemy.Stats.Health;
    }

    [Test]
    public void SimpleSpellSourceOnBarrelTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      //SpellSource spellSource = scroll ? new Scroll ? 
      hero.AlwaysHit[AttackKind.SpellElementalProjectile] = true;
      var barrel = new Barrel(Container);
      PlaceCloseToHero(barrel);
      Assert.False(barrel.Destroyed);
      Assert.True(game.GameManager.HeroTurn);

      UseFireBallSpellSource(hero, barrel, true);
      Assert.True(barrel.Destroyed);
    }


    [TestCase(true, 1)]
    [TestCase(false, 1)]
    [TestCase(true, 5)]

    [Repeat(1)]
    public void ScrollPowerVSMeleeTest(bool scroll, int heroLevel)
    {
      var game = CreateGame();
      var hero = game.Hero;
      Assert.Less(hero.Stats.MeleeAttack, 20);
      var enemy = PlainNormalEnemies.First();
      int mult = 2;
      enemy.Stats.SetNominal(EntityStatKind.Health, 350 * mult);
      hero.Stats.SetNominal(EntityStatKind.Mana, 250 * mult);
      var enemyHealth = enemy.Stats.Health;
      var sk = SpellKind.FireBall;
      if (heroLevel > 1)
      {
        var po = 5 * 5;
        hero.LevelUpPoints = po;
        for (int i = 0; i < po / 3; i++)//3 stats
        {
          hero.IncreaseStatByLevelUpPoint(EntityStatKind.Strength);
          hero.IncreaseStatByLevelUpPoint(EntityStatKind.Health);
          hero.IncreaseStatByLevelUpPoint(EntityStatKind.Defense);
        }

        hero.Spells.GetState(sk).Level = 4;//rough estimation
      }
      var dcSpell = new OuaDDamageComparer(enemy, this);
      hero.AlwaysHit[AttackKind.Melee] = true;
      hero.AlwaysHit[AttackKind.SpellElementalProjectile] = true;
      for (int i = 0; i < 10 * mult; i++)
      {
        Assert.True(UseFireBallSpellSource(hero, enemy, scroll, sk));
        GotoNextHeroTurn();
      }

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      var damageScroll = enemyHealth - enemy.Stats.Health;
      dcSpell.RegisterHealth();

      //melee
      enemyHealth = enemy.Stats.Health;
      var dcMelee = new OuaDDamageComparer(enemy, this);
      var eq = "rusty_sword";
      if (heroLevel > 1)
        eq = "broad_sword";
      var wpn = GenerateEquipment<Weapon>(eq);
      Assert.True(wpn.MakeEnchantable());
      var gem = new Gem() { GemKind = heroLevel > 1 ? GemKind.Amber : GemKind.Ruby };
      string error;
      var crafter = new Roguelike.Crafting.LootCrafter(game.GameManager.Container);

      Assert.True(crafter.ApplyEnchant(gem, wpn, out error));
      //Assert.True(gem.ApplyTo(wpn, out error));

      Assert.True(SetHeroEquipment(wpn, CurrentEquipmentKind.Weapon));
      for (int i = 0; i < 10 * mult; i++)
      {
        enemy.OnMeleeHitBy(game.Hero);
      }
      dcMelee.RegisterHealth();
      var hp = dcMelee.HealthPercentage;
      var damageMelee = enemyHealth - enemy.Stats.Health;
      Assert.Greater(damageMelee, 15 * mult);
      AssertHealthDiffPercentageNotBigger(dcMelee, dcSpell, 140);
      //Assert.Less(Math.Abs(damageMelee - damageScroll), 30);//TODO %
    }

    [Test]
    public void KillEnemy()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var enemies = game.GameManager.EnemiesManager.AllEntities;
      var initEnemyCount = enemies.Count;
      Assert.Greater(initEnemyCount, 0);
      Assert.AreEqual(initEnemyCount, game.GameManager.CurrentNode.GetTiles<Enemy>().Count);

      var enemy = enemies.First();
      while (enemy.Alive)
      {
        //UseFireBallScroll(hero, enemy);
        enemy.OnMeleeHitBy(hero);
        GotoNextHeroTurn();
      }

      var finalEnemyCount = enemies.Count;
      Assert.AreEqual(finalEnemyCount, initEnemyCount - 1);
      Assert.AreEqual(finalEnemyCount, game.GameManager.CurrentNode.GetTiles<Enemy>().Count);
    }

    [Test]
    [Repeat(1)]
    public void EnemyAttackTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemy = AllEnemies.Cast<Enemy>().First();
      enemy.SetPrefferedFightStyle(PrefferedFightStyle.Magic);//use spells
      enemy.ActiveManaPoweredSpellSource = new Scroll(SpellKind.FireBall);
      enemy.AlwaysHit[AttackKind.SpellElementalProjectile] = true;

      Assert.True(game.GameManager.HeroTurn);
      TryToMoveHero();
      PlaceCloseToHero(enemy);

      GotoNextHeroTurn();
      var heroHealth = hero.Stats.Health;
      var mana = enemy.Stats.Mana;
      //if (heroHealth == hero.Stats.Health)
      {
        for (int i = 0; i < 10; i++)
        {
          game.GameManager.EnemiesManager.AttackIfPossible(enemy, hero);//TODO
          if (enemy.Stats.Mana < mana)
            break;
        }
      }
      if (heroHealth == hero.Stats.Health)
      {
        int k = 0;
        k++;
      }
      Assert.Greater(heroHealth, hero.Stats.Health);
      Assert.Greater(mana, enemy.Stats.Mana);//used mana
    }

    [Test]
    public void TransformScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Enemy enemy = AllEnemies.First();
      PassiveSpell spell;
      var scroll = PrepareScroll(hero, SpellKind.Transform, enemy);
      Assert.True(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));
      spell = game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(hero, scroll) as PassiveSpell;

      Assert.NotNull(spell);
      Assert.Greater(spell.Duration, 0);

      Assert.True(!game.GameManager.HeroTurn);
      var le = game.Hero.LastingEffectsSet.GetByType(Roguelike.Effects.EffectType.Transform);
      Assert.NotNull(le);
      Assert.False(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));
      Assert.AreEqual(le.Description, "Transform");

      GotoSpellEffectEnd(spell);

      Assert.True(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));
    }

    [Test]
    public void PerunScrollTest()
    {
      var game = CreateGame(genNumOfEnemies: 1);
      var hero = game.Hero;
      var sndPlayed = "";
      game.GameManager.SoundManager.PlayedSound += (object s, string snd) =>
      {
        sndPlayed = snd;
      };

      Enemy enemy = AllEnemies.First();
      var dc = new OuaDDamageComparer(enemy, this);
      var scroll = PrepareScroll(hero, SpellKind.Perun, enemy);

      game.GameManager.SpellManager.ApplySpell(hero, scroll, enemy);
      Assert.True(!game.GameManager.HeroTurn);
      dc.RegisterHealth();
      Assert.Greater(dc.HealthDifference, 0);
      Assert.Greater(sndPlayed.Length, 0);
    }

    [TestCase(SpellKind.Perun)]
    [TestCase(SpellKind.Swiatowit)]
    public void PerunScrollPowerIncTest(SpellKind sk)
    {
      var game = CreateGame();// genNumOfEnemies: 1);
      var hero = game.Hero;
      Container.GetInstance<ILogger>().LogLevel = LogLevel.Info;

      Enemy enemy = PlainEnemies.First();
      PrepareToBeBeaten(enemy);
      PrepareToBeBeaten(hero);
      
      var dc = new OuaDDamageComparer(enemy, this);
      var scroll = PrepareScroll(hero, sk, enemy, scrollCount:12);

      game.GameManager.SpellManager.ApplySpell(hero, scroll, enemy);
      dc.RegisterHealth();
      Assert.Greater(dc.HealthDifference, 0);
      int biggerCounter = 0;
      float damageMin = dc.HealthDifference;
      float damageMax = dc.HealthDifference;
      for (int level = 0; level < 10; level++)
      {
        hero.Level = level + 2;//2, 3...
        var hd = dc.HealthDifference;
        dc = new OuaDDamageComparer(enemy, this);
        GotoNextHeroTurn();
        hero.Consume(new Potion() { Kind = PotionKind.Mana });
        GotoNextHeroTurn();
        var spell = game.GameManager.SpellManager.ApplySpell(hero, scroll, enemy);
        Assert.NotNull(spell);
        Assert.Greater(spell.CurrentLevel, 1);
        dc.RegisterHealth();
        Assert.Greater(dc.HealthDifference, 0);
        //Assert.Greater(dc.HealthDifference, hd);
        if (dc.HealthDifference > hd)
          biggerCounter++;
        else {
          GotoNextHeroTurn();
          int k = 0;
          k++;
        }
        
        hd = dc.HealthDifference;
        if (hd > damageMax)
          damageMax = hd;
      }

      var expBiggerCounter = 8;
      if (sk == SpellKind.Swiatowit)
        expBiggerCounter = 6;
      Assert.Greater(biggerCounter, expBiggerCounter);
      Assert.Greater(damageMax / damageMin, 4);
    }

    [Test]
    public void SwiatowitScrollTest()
    {
      var game = CreateGame(genNumOfEnemies: 1);
      var hero = game.Hero;
      var sndPlayed = "";
      game.GameManager.SoundManager.PlayedSound += (object s, string snd) =>
      {
        sndPlayed = snd;
      };

      Enemy enemy = AllEnemies.First();
      var scroll = PrepareScroll(hero, SpellKind.Swiatowit, enemy);

      var enHealth = enemy.Stats.Health;
      game.GameManager.SpellManager.ApplySpell(hero, scroll, enemy);
      Assert.True(!game.GameManager.HeroTurn);
      Assert.Less(enemy.Stats.Health, enHealth);
      Assert.Greater(sndPlayed.Length, 0);
    }

    [Test]
    public void DziewannaScrollTest()
    {
      var game = CreateGame(genNumOfEnemies: 1);
      var hero = game.Hero;

      Enemy enemy = AllEnemies.First();
      var scroll = PrepareScroll(hero, SpellKind.Dziewanna, enemy);

      game.GameManager.SpellManager.ReportDelayedSpellDone += (s, e) =>
      {
        game.GameManager.SpellManager.OnSpellDone(game.Hero);
      };

      var enHealth = enemy.Stats.Health;
      var apples = game.GameManager.CurrentNode.GetTiles<Food>().Where(i => i.Kind == FoodKind.Apple && i.EffectType == Roguelike.Effects.EffectType.Poisoned);
      Assert.AreEqual(apples.Count(), 0);
      game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(hero, scroll);
      Assert.True(!game.GameManager.HeroTurn);
      apples = game.GameManager.CurrentNode.GetTiles<Food>().Where(i => i.Kind == FoodKind.Apple);
      var applesCount = apples.Count();
      Assert.Greater(applesCount, 0);

      for (int i = 0; i < 10; i++)
      {
        GotoNextHeroTurn();
        if (enemy.HasLastingEffect(Roguelike.Effects.EffectType.Poisoned)
          //|| enemy.HasLastingEffect(Roguelike.Effects.EffectType.ConsumedRawFood)
          )
          break;
      }
      Assert.True(
        enemy.HasLastingEffect(Roguelike.Effects.EffectType.Poisoned)
        //|| enemy.HasLastingEffect(Roguelike.Effects.EffectType.ConsumedRawFood)
        );
      var applesAfter = game.GameManager.CurrentNode.GetTiles<Food>().Where(i => i.Kind == FoodKind.Apple && i.EffectType == Roguelike.Effects.EffectType.Poisoned).ToList();
      Assert.Greater(applesCount, applesAfter.Count);
      Assert.Greater(enHealth, enemy.Stats.Health);
    }

    [Test]
    [Repeat(1)]
    public void ManaScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemy = AllEnemies.First();
      enemy.Symbol = EnemySymbols.SnakeSymbol;
      enemy.SetSpecialAttackStatFromSymbol();

      var scroll = PrepareScroll(hero, SpellKind.ManaShield, enemy);
      var spell = game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(hero, scroll) as PassiveSpell;
      Assert.NotNull(spell);

      var le = game.Hero.LastingEffectsSet.GetByType(Roguelike.Effects.EffectType.ManaShield);
      Assert.AreEqual(le.Description, "Mana Shield");

      var heroHealth = game.Hero.Stats.Health;
      game.Hero.OnMeleeHitBy(enemy);//PoisonBallSpell work on mana shields!
      Assert.AreEqual(game.Hero.Stats.Health, heroHealth);//mana shield

      GotoSpellEffectEnd(spell);

      heroHealth = game.Hero.Stats.Health;
      game.Hero.OnMeleeHitBy(enemy);
      Assert.Less(game.Hero.Stats.Health, heroHealth);//mana shield gone

    }

    [Test]
    [Repeat(1)]
    public void SkeletonScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var scroll = PrepareScroll(hero, SpellKind.Skeleton);
      var gm = game.GameManager;
      var enemiesCount = gm.CurrentNode.GetTiles<Ally>().Count;
      var spell = gm.SpellManager.ApplySpell(hero, scroll) as SkeletonSpell;
      Assert.NotNull(spell);
      Assert.NotNull(spell.Ally);
      Assert.True(!game.GameManager.HeroTurn);
      Assert.AreEqual(gm.CurrentNode.GetTiles<Ally>().Count, enemiesCount + 1);
      Assert.True(gm.AlliesManager.AllEntities.Contains((spell.Ally)));

      //go dungeon down
      var stairs = gm.CurrentNode.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelDown).SingleOrDefault();
      Assert.NotNull(stairs);
      var index = gm.CurrentNode.Index;
      var res = gm.InteractHeroWith(stairs);
      Assert.Greater(gm.CurrentNode.Index, index);

      Assert.True(gm.AlliesManager.AllEntities.Contains((spell.Ally)));
      Assert.True(gm.CurrentNode.GetTiles<Ally>().Contains(spell.Ally));
      if (!gm.CurrentNode.GetTiles<Hero>().Any())
      {
        int k = 0;
        k++;
      }
      Assert.True(gm.CurrentNode.GetTiles<Hero>().Any());
      spell.Ally.Name = "hero_ally";
      SaveLoad();

      Assert.AreEqual(gm.AlliesManager.AllEntities.Count, 1);
      Assert.AreEqual(gm.AlliesManager.AllEntities[0].Name, "hero_ally");
    }

    [Test]
    public void TeleportTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var heroPos = hero.point;
      TeleportByRange(1, false);
      Assert.AreNotEqual(heroPos, hero.point);
    }

    [Test]
    public void TeleportTestNotPassedAsTooFar()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var heroPos = hero.point;
      TeleportByRange(10, true);
      Assert.AreEqual(heroPos, hero.point);
    }

    private void TeleportByRange(int range, bool shallFail)
    {
      var hero = game.Hero;

      var scroll = PrepareScroll(hero, SpellKind.Teleport);
      Assert.AreEqual(hero.Inventory.GetItems<Scroll>().Count(), 1);
      var dest = game.GameManager.CurrentNode.GetEmptyTiles().Where(i => i.DistanceFrom(hero) == range).FirstOrDefault();
      var spell = game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(hero, scroll, dest.point);
      if (!shallFail)
      {
        Assert.NotNull(spell);
        Assert.AreEqual(hero.Inventory.GetItems<Scroll>().Count(), 0);
      }
    }

    [Test]
    public void SwapScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var heroPos = hero.Position;
      Enemy enemy = AllEnemies.First();

      PassiveSpell spell;
      var scroll = PrepareScroll(hero, SpellKind.SwapPosition);
      PlaceCloseToHero(enemy);
      var enemyPos = enemy.Position;
      spell = game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(hero, scroll, enemyPos) as PassiveSpell;
      Assert.NotNull(spell);
      Assert.True(!game.GameManager.HeroTurn);
      Assert.AreEqual(hero.Position, enemyPos);
      Assert.AreEqual(heroPos, enemy.Position);
    }

    [Test]
    public void CrackedStoneScrollTest()
    {
      var gi = new GenerationInfo();
      gi.MakeEmpty();
      gi.NumberOfRooms = 1;
      gi.GenerateEnemies = true;
      gi.ForcedNumberOfEnemiesInRoom = 1;
      var game = CreateGame(gi: gi);
      var hero = game.Hero;


      var stonePh = hero.Position;
      stonePh.X += 1;
      Assert.True(game.Level.SetEmptyTile(stonePh));
      var enemyPh = stonePh;
      enemyPh.X += 1;
      var enemy = AllEnemies.First();
      enemy.Name += " in test";
      Assert.True(game.Level.SetTile(enemy, enemyPh));

      PassiveSpell spell;
      var scroll = PrepareScroll(hero, SpellKind.CrackedStone);
      Assert.True(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));
      spell = game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(hero, scroll, stonePh) as PassiveSpell;

      Assert.NotNull(spell);
      var tile = game.Level.GetTile(stonePh);
      Assert.True(tile is CrackedStone);
      Assert.True(!game.GameManager.HeroTurn);
      Assert.True(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));
      var enPos = enemy.point;
      GotoNextHeroTurn();
      Assert.True(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));
      Assert.AreNotEqual(enemy.point, enPos);
      Assert.AreEqual(enemy.point.X, enPos.X);//shall try walk around the stone
    }

    [Test]
    public void FrightenScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Enemy enemy = AllEnemies.First();
      FrightenSpell spell;
      var scroll = PrepareScroll(hero, SpellKind.Frighten, enemy);
      Assert.True(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));

      spell = game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(hero, scroll) as FrightenSpell;

      Assert.NotNull(spell);
      Assert.Greater(spell.Duration, 0);
      Assert.Greater(spell.Range, 0);

      Assert.True(!game.GameManager.HeroTurn);
      var le = enemy.LastingEffectsSet.GetByType(Roguelike.Effects.EffectType.Frighten);
      Assert.NotNull(le);
      Assert.False(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));
      Assert.AreEqual(le.Description, "Frighten (Pending Turns: 2)");

      GotoSpellEffectEnd(spell);

      Assert.True(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));
    }


    }
}
