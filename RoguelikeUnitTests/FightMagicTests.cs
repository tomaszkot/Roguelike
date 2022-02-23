using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
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
    public void SpellSourcePropertiesTest(bool scroll)
    {
      var game = CreateGame();
      var hero = game.Hero;
      SpellSource spellSource = null;
      if (scroll)
        spellSource = new Scroll(Roguelike.Spells.SpellKind.FireBall);
      else
        spellSource = new Book(Roguelike.Spells.SpellKind.FireBall);

      {
        //var desc = spellSource.GetDescription();
        var extraDesc = spellSource.GetExtraStatDescription(hero, true);
        Assert.NotNull(extraDesc);
        Assert.Greater(extraDesc.GetDescription().Count(), 0);

        Assert.Greater(extraDesc.Level , 0);
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
      var range1 = stats1.SingleOrDefault(i => i.Kind == EntityStatKind.ElementalProjectilesRange);
      Assert.NotNull(range1);

      var stats2 = spell.CreateSpellStatsDescription(false).GetEntityStats();
      Assert.AreEqual(stats2.Count(), 3);
      var mana2 = stats2.SingleOrDefault(i => i.Kind == EntityStatKind.Mana);
      Assert.NotNull(mana2);
      Assert.Greater(mana2.Value.TotalValue, mana1.Value.TotalValue);

      var attack2 = stats2.SingleOrDefault(i => i.Kind == EntityStatKind.ElementalSpellProjectilesAttack);
      Assert.NotNull(attack2);
      Assert.Greater(attack2.Value.TotalValue, attack1.Value.TotalValue);

      var range2 = stats2.SingleOrDefault(i => i.Kind == EntityStatKind.ElementalProjectilesRange);
      Assert.NotNull(range2);
      Assert.GreaterOrEqual(range2.Value.TotalValue, range1.Value.TotalValue);
    }

    [Test]
    public void SpellPropertiesTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      {
        var fireBallScroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
        var spell = fireBallScroll.CreateSpell<OffensiveSpell>(hero);
        Assert.Greater(spell.Damage, 0);
      }
      {
        var fireBallBook = new Book(Roguelike.Spells.SpellKind.FireBall);
        var spellFromBook = fireBallBook.CreateSpell<OffensiveSpell>(hero);
        Assert.Greater(spellFromBook.Damage, 0);
      }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SimpleSpellSourceTest(bool scroll)
    {
      var game = CreateGame();
      var hero = game.Hero;
      //SpellSource spellSource = scroll ? new Scroll ? 

      var enemy = ActiveEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);

      UseFireBallSpellSource(hero, enemy, scroll);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.Greater(mana, hero.Stats.Mana);
      Assert.False(game.GameManager.HeroTurn);

      var diff = enemyHealth - enemy.Stats.Health;
    }

    

    [TestCase(true)]
    [TestCase(false)]
    public void ScrollPowerVSMeleeTest(bool scroll)
    {
      var game = CreateGame();
      var hero = game.Hero;
      Assert.Less(hero.Stats.MeleeAttack, 20);
      var enemy = PlainNormalEnemies.First();
      enemy.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Health, 350);
      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Mana, 250);
      var enemyHealth = enemy.Stats.Health;

      for (int i = 0; i < 10; i++)
      {
        Assert.True(UseFireBallSpellSource(hero, enemy, scroll));
        GotoNextHeroTurn();
      }

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      var diffScroll = enemyHealth - enemy.Stats.Health;

      //melee
      enemyHealth = enemy.Stats.Health;
      var wpn = GenerateEquipment<Weapon>("rusty_sword");
      SetHeroEquipment(wpn, CurrentEquipmentKind.Weapon);
      for (int i = 0; i < 10; i++)
      {
        enemy.OnMelleeHitBy(game.Hero);
      }
      var diffMelee = enemyHealth - enemy.Stats.Health;
      Assert.Greater(diffMelee, 20);
      Assert.Less(Math.Abs(diffMelee - diffScroll), 30);//TODO %
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
        enemy.OnMelleeHitBy(hero);
        GotoNextHeroTurn(game);
      }

      var finalEnemyCount = enemies.Count;
      Assert.AreEqual(finalEnemyCount, initEnemyCount - 1);
      Assert.AreEqual(finalEnemyCount, game.GameManager.CurrentNode.GetTiles<Enemy>().Count);
    }

    [Test]
    public void EnemyAttackTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemy = AllEnemies.Cast<Enemy>().First();
      enemy.PrefferedFightStyle = PrefferedFightStyle.Magic;//use spells
      enemy.ActiveManaPoweredSpellSource = new Scroll(SpellKind.FireBall);
     
      var mana = enemy.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      TryToMoveHero();

      var emptyHeroNeib = game.Level.GetEmptyNeighborhoodPoint(game.Hero, EmptyNeighborhoodCallContext.Move);
      var set = game.Level.SetTile(enemy, emptyHeroNeib.Item1);
      Assert.True(set);

      GotoNextHeroTurn(game);
      var heroHealth = hero.Stats.Health;
      //if (heroHealth == hero.Stats.Health)
      {
        for (int i = 0; i < 10; i++)
        {
          game.GameManager.EnemiesManager.AttackIfPossible(enemy as Enemy, hero);//TODO
          if (enemy.Stats.Mana < mana)
            break;
        }
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
      spell = game.GameManager.SpellManager.ApplyPassiveSpell(hero, scroll);
      Assert.NotNull(spell);

      Assert.True(!game.GameManager.HeroTurn);
      var le = game.Hero.LastingEffectsSet.GetByType(Roguelike.Effects.EffectType.Transform);
      Assert.NotNull(le);
      Assert.False(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));

      GotoSpellEffectEnd(spell);

      Assert.True(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));
    }

    [Test]
    public void DziewannaScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Enemy enemy = AllEnemies.First();
      var scroll = PrepareScroll(hero, SpellKind.Dziewanna, enemy);

      var enHealth = enemy.Stats.Health;
      var apples = game.GameManager.CurrentNode.GetTiles<Food>().Where(i => i.Kind == FoodKind.Apple && i.EffectType == Roguelike.Effects.EffectType.Poisoned);
      Assert.AreEqual(apples.Count(), 0);
      game.GameManager.SpellManager.ApplyPassiveSpell(hero, scroll);
      Assert.True(!game.GameManager.HeroTurn);
      apples = game.GameManager.CurrentNode.GetTiles<Food>().Where(i => i.Kind == FoodKind.Apple);
      var applesCount = apples.Count();
      Assert.Greater(applesCount, 0);
      
      for (int i = 0; i < 10; i++)
      {
        GotoNextHeroTurn();
        if (enemy.HasLastingEffect(Roguelike.Effects.EffectType.Poisoned))
          break;
      }
      Assert.True(enemy.HasLastingEffect(Roguelike.Effects.EffectType.Poisoned));
      var applesAfter = game.GameManager.CurrentNode.GetTiles<Food>().Where(i => i.Kind == FoodKind.Apple && i.EffectType == Roguelike.Effects.EffectType.Poisoned).ToList();
      Assert.Greater(applesCount, applesAfter.Count);
      Assert.Greater(enHealth, enemy.Stats.Health);
    }

    private Scroll PrepareScroll(Hero hero, SpellKind spellKind, Enemy enemyToPlaceNearby = null)
    {
      var emp = game.GameManager.CurrentNode.GetClosestEmpty(hero);
      if (enemyToPlaceNearby != null)
        Assert.True(game.GameManager.CurrentNode.SetTile(enemyToPlaceNearby, emp.point));
      var scroll = new Scroll(spellKind);
      hero.Inventory.Add(scroll);

      return scroll;
    }

    [Test]
    public void ManaScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemy = AllEnemies.First();
      enemy.Symbol = EnemySymbols.SnakeSymbol;
      enemy.SetSpecialAttackStat();
      
      var scroll = PrepareScroll(hero, SpellKind.ManaShield, enemy);
      var spell = game.GameManager.SpellManager.ApplyPassiveSpell(hero, scroll);
      Assert.NotNull(spell);

      var heroHealth = game.Hero.Stats.Health;
      game.Hero.OnMelleeHitBy(enemy);//PoisonBallSpell work on mana shields!
      Assert.AreEqual(game.Hero.Stats.Health, heroHealth);//mana shield

      GotoSpellEffectEnd(spell);

      heroHealth = game.Hero.Stats.Health;
      game.Hero.OnMelleeHitBy(enemy);
      Assert.Less(game.Hero.Stats.Health, heroHealth);//mana shield gone

    }

    [Test]
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
      Assert.AreEqual(gm.CurrentNode.GetTiles<Ally>().Count, enemiesCount + 1);
      Assert.True(gm.AlliesManager.AllEntities.Contains((spell.Ally)));

      //go dungeon down
      var stairs = gm.CurrentNode.GetTiles<Stairs>().Where(i => i.StairsKind == StairsKind.LevelDown).SingleOrDefault();
      Assert.NotNull(stairs);
      var index = gm.CurrentNode.Index;
      gm.InteractHeroWith(stairs);
      Assert.Greater(gm.CurrentNode.Index, index);

      Assert.True(gm.AlliesManager.AllEntities.Contains((spell.Ally)));
      Assert.True(gm.CurrentNode.GetTiles<Ally>().Contains(spell.Ally));
      spell.Ally.Name = "hero_ally";
      gm.Save();
      gm.Load(hero.Name);

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
    public void TeleportTestFailed()
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
      var spell = game.GameManager.SpellManager.ApplyPassiveSpell(hero, scroll, dest.point);
      if (!shallFail)
      {
        Assert.NotNull(spell);
        Assert.AreEqual(hero.Inventory.GetItems<Scroll>().Count(), 0);
      }
    }
  }
}
