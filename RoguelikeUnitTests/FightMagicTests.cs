using NUnit.Framework;
using Roguelike.Spells;
using Roguelike.Tiles;
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
    [Test]
    public void SpellPropertiesTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var fireBallScroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
      var spell = fireBallScroll.CreateSpell<OffensiveSpell>(hero);
      Assert.Greater(spell.Damage, 0);
      
    }

    [Test]
    public void SimpleScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemy = ActiveEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
            
      UseScroll(game, hero, enemy);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.Greater(mana, hero.Stats.Mana);
      Assert.False(game.GameManager.HeroTurn);

      var diff = enemyHealth- enemy.Stats.Health;
    }

    [Test]
    public void ScrollPowerVSMeleeTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemy = AllEnemies.First();
      enemy.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Health, 100);
      var enemyHealth = enemy.Stats.Health;

      for (int i = 0; i < 10; i++)
      {
        UseScroll(game, hero, enemy);
        GotoNextHeroTurn();
      }

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      var diffScroll = enemyHealth - enemy.Stats.Health;

      //melee
      var wpn = game.GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      game.Hero.SetEquipment(CurrentEquipmentKind.Weapon, wpn);
      for (int i = 0; i < 10; i++)
      {
        enemy.OnPhysicalHit(game.Hero);
        //GotoNextHeroTurn();
      }
      var diffMelee = enemyHealth - enemy.Stats.Health;
      Assert.Greater(diffMelee, 50);
      Assert.Less(Math.Abs(diffMelee - diffScroll), 30);//TODO %
    }

    private void UseScroll(Roguelike.RoguelikeGame game, Hero hero, LivingEntity enemy)
    {
      var scroll = new Scroll(Roguelike.Spells.SpellKind.FireBall);
      hero.Inventory.Add(scroll);
      game.GameManager.Context.ApplySpellAttackPolicy(hero, enemy, scroll, null, (p) => game.GameManager.OnHeroPolicyApplied(this, p));
      
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
        UseScroll(game, hero, enemy);
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
      var heroHealth = hero.Stats.Health;
      var mana = enemy.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      TryToMoveHero();
      
      var emptyHeroNeib = game.Level.GetEmptyNeighborhoodPoint(game.Hero, EmptyNeighborhoodCallContext.Move);
      var set = game.Level.SetTile(enemy, emptyHeroNeib.Item1);
      Assert.True(set);

      GotoNextHeroTurn(game);
      if (heroHealth == hero.Stats.Health)
      {
        game.GameManager.EnemiesManager.AttackIfPossible(enemy as Enemy, hero);//TODO
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
      spell = game.GameManager.ApplyPassiveSpell(hero, scroll);
      Assert.NotNull(spell);

      Assert.True(!game.GameManager.HeroTurn);
      var le = game.Hero.LastingEffectsSet.GetByType(Roguelike.Effects.EffectType.Transform);
      Assert.NotNull(le);
      Assert.False(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));

      GotoSpellEffectEnd(spell);

      Assert.True(game.GameManager.EnemiesManager.ShallChaseTarget(enemy, game.Hero));
    }

    private Scroll PrepareScroll(Hero hero, SpellKind spellKind, Enemy enemy = null)
    {
      var emp = game.GameManager.CurrentNode.GetClosestEmpty(hero);
      if(enemy!=null)
        Assert.True(game.GameManager.CurrentNode.SetTile(enemy, emp.point));
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

      var scroll = PrepareScroll(hero, SpellKind.ManaShield, enemy);
      var spell = game.GameManager.ApplyPassiveSpell(hero, scroll);
      Assert.NotNull(spell);
            
      var heroHealth = game.Hero.Stats.Health;
      game.Hero.OnPhysicalHit(enemy);
      Assert.AreEqual(game.Hero.Stats.Health, heroHealth);//mana shield

      GotoSpellEffectEnd(spell);

      heroHealth = game.Hero.Stats.Health;
      game.Hero.OnPhysicalHit(enemy);
      Assert.Less(game.Hero.Stats.Health, heroHealth);//mana shield gone

    }

    [Test]
    public void SkeletonScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var scroll = PrepareScroll(hero, SpellKind.Skeleton);
      var enemiesCount = game.GameManager.CurrentNode.GetTiles<Enemy>().Count;
      var spell = game.GameManager.ApplyOffensiveSpell(hero, scroll) as SkeletonSpell;
      Assert.NotNull(spell);
      Assert.NotNull(spell.Enemy);
      Assert.AreEqual(game.GameManager.CurrentNode.GetTiles<Enemy>().Count, enemiesCount+1);

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
      var dest = game.GameManager.CurrentNode.GetEmptyTiles().Where(i=>i.DistanceFrom(hero) == range).FirstOrDefault();
      var spell = game.GameManager.ApplyPassiveSpell(hero, scroll, dest.point);
      if (!shallFail)
      {
        Assert.NotNull(spell);
        Assert.AreEqual(hero.Inventory.GetItems<Scroll>().Count(), 0);
      }
    }
  }
}
