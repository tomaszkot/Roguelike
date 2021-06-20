using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Generators;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FightMeleeTests : TestBase
  {
    [Test]
    public void NonPlainEnemyUsesEffects()
    {
      for (int loop = 0; loop < 1; loop++)
      {
        var game = CreateGame(numEnemies: 1, numberOfRooms: 1);
        var hero = game.Hero;

        var enemies = game.GameManager.CurrentNode.GetTiles<Enemy>();
        Assert.AreEqual(enemies.Count, 1);
        var enemy = enemies.Where(i => i.PowerKind != EnemyPowerKind.Plain).FirstOrDefault();
        if (enemy == null)
        {
          enemy = enemies.First();
          enemy.SetNonPlain(false);
        }

        Assert.AreEqual(enemy.LastingEffects.Count, 0);
        GenerationInfo.ChanceToTurnOnSpecialSkillByEnemy = 1f;

        var closeToHero = game.Level.GetClosestEmpty(hero, incDiagonals: false);
        game.Level.SetTile(enemy, closeToHero.point);
        enemy.ActiveManaPoweredSpellSource = null;//this causes attack

        //hit enemy to force him to use effect
        enemy.OnPhysicalHitBy(hero);

        game.GameManager.Context.TurnOwner = TurnOwner.Allies;
        game.GameManager.Context.PendingTurnOwnerApply = true;
        GotoNextHeroTurn(game);

        //enemy shall cast effect on itself or on hero
        var heroHasLastingEffect = hero.HasLastingEffect(EffectType.Inaccuracy) || hero.HasLastingEffect(EffectType.Weaken);
        if (!heroHasLastingEffect)
        {
          Assert.AreEqual(enemy.LastingEffects.Count, 1);
          var eff = enemy.LastingEffects[0].Type;
          Assert.True(LivingEntity.PossibleEffectsToUse.Contains(eff));
        }
      }
    }

    [Test]
    public void WeaponImpactTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.Greater(ActiveEnemies.Count, 0);
      var enemy = ActiveEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      enemy.OnPhysicalHitBy(hero);
      Assert.Greater(enemyHealth, enemy.Stats.Health);
      enemyHealth = enemy.Stats.Health;

      var wpn = GenerateRandomEqOnLevelAndCollectIt<Weapon>();
      enemy.OnPhysicalHitBy(hero);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
    }

    [Test]
    public void StunnedEffectFromWeapon()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.ChanceToHit, 100);
      var wpn = game.GameManager.LootGenerator.GetLootByTileName<Weapon>("hammer");
      wpn.MakeMagic(EntityStatKind.ChanceToCauseStunning, 100);
      wpn.Identify();
      hero.SetEquipment(wpn);
      var ccs = hero.GetCurrentValue(EntityStatKind.ChanceToCauseStunning);
      Assert.AreEqual(ccs, 100);

      var enemy = ActiveEnemies.First();
      enemy.OnPhysicalHitBy(hero);
      Assert.True(enemy.LastingEffects.Any());
      Assert.AreEqual(enemy.LastingEffects[0].Type, EffectType.Stunned);
      Assert.AreEqual(enemy.LastingEffects[0].Description, "Stunned");
      GotoNextHeroTurn();
      Assert.AreEqual(enemy.LastingEffects[0].Type, EffectType.Stunned);
      GotoNextHeroTurn();
      Assert.AreEqual(enemy.LastingEffects[0].Type, EffectType.Stunned);
      GotoNextHeroTurn();
      Assert.False(enemy.LastingEffects.Any());
    }

    [Test]
    public void KillEnemy()
    {
      var game = CreateGame();
      //var hero = game.Hero;

      var enemies = game.GameManager.EnemiesManager.AllEntities;
      var initEnemyCount = enemies.Count;
      Assert.Greater(initEnemyCount, 0);
      Assert.AreEqual(initEnemyCount, game.GameManager.CurrentNode.GetTiles<Enemy>().Count);

      var enemy = ActiveEnemies.First();
      while (enemy.Alive)
        InteractHeroWith(enemy as Enemy);
      var finalEnemyCount = enemies.Count;
      Assert.AreEqual(finalEnemyCount, initEnemyCount - 1);
      Assert.AreEqual(finalEnemyCount, game.GameManager.CurrentNode.GetTiles<Enemy>().Count);
    }

    [Test]
    public void WeaponDamageRandomisation()
    {
      var game = CreateGame();
      var en = game.GameManager.EnemiesManager.GetEnemies().Where(i => i.PowerKind == EnemyPowerKind.Champion).First();

      var attack = game.Hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Attack);

      var wpn = game.GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.AreEqual(wpn.PrimaryStatValue, 2);
      Assert.AreEqual(wpn.PrimaryStatDescription, "Attack: 1-3");

      game.Hero.SetEquipment(wpn);
      var attackWithWpn = game.Hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Attack);
      Assert.Greater(attackWithWpn, attack);

      var attackFormatted = game.Hero.GetFormattedStatValue(Roguelike.Attributes.EntityStatKind.Attack, false);
      Assert.AreEqual(attackFormatted, "16-18");

      var damages = new List<float>();
      for (int i = 0; i < 10; i++)
      {
        var damage = en.OnPhysicalHitBy(game.Hero);
        if (damage > 0)
          damages.Add(damage);
      }
      var grouped = damages.GroupBy(i => i);
      Assert.Greater(grouped.Count(), 1);
    }

    [Test]
    public void RageScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var enemy = AllEnemies.First();

      Func<float> hitEnemy = () =>
      {
        var enemyHealth = enemy.Stats.Health;
        enemy.OnPhysicalHitBy(game.Hero);
        var lastEnemyHealthDiff = enemyHealth - enemy.Stats.Health;
        return lastEnemyHealthDiff;
      };
      var healthDiff = hitEnemy();

      var emp = game.GameManager.CurrentNode.GetClosestEmpty(hero);
      game.GameManager.CurrentNode.SetTile(enemy, emp.point);

      var scroll = new Scroll(SpellKind.Rage);
      hero.Inventory.Add(scroll);
      var attackPrev = hero.GetCurrentValue(EntityStatKind.Attack);
      var spell = game.GameManager.SpellManager.ApplyPassiveSpell(hero, scroll);
      Assert.NotNull(spell);
      Assert.Greater(hero.GetCurrentValue(EntityStatKind.Attack), attackPrev);

      var healthDiffRage = hitEnemy();
      Assert.Greater(healthDiffRage, healthDiff);//rage in work

      GotoSpellEffectEnd(spell);
      Assert.AreEqual(hero.GetCurrentValue(EntityStatKind.Attack), attackPrev);
      var healthDiffAfterRage = hitEnemy();
      var delta = Math.Abs(healthDiffAfterRage - healthDiff);
      Assert.Less(delta, 0.001);//rage was over

    }
  }
}
