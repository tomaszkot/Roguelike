﻿using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Managers.Policies;
using Roguelike.Effects;
using Roguelike.Generators;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Tiles.Abstract;
using Roguelike.Calculated;
using Dungeons.Core;
using System.Diagnostics;

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
        var game = CreateGame(genNumOfEnemies: 1, numberOfRooms: 1);
        var hero = game.Hero;

        var enemies = CurrentNode.GetTiles<Enemy>();
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
        enemy.SelectedManaPoweredSpellSource = null;//this causes attack

        //hit enemy to force him to use effect
        enemy.OnMeleeHitBy(hero);

        game.GameManager.Context.TurnOwner = TurnOwner.Allies;
        game.GameManager.Context.PendingTurnOwnerApply = true;
        GotoNextHeroTurn();

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
      hero.AlwaysHit[AttackKind.Melee] = true;
      Assert.Greater(ActivePlainEnemies.Count, 0);
      var enemy = ActivePlainEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      InteractHeroWith(enemy);
      Assert.Greater(enemyHealth, enemy.Stats.Health);
      enemyHealth = enemy.Stats.Health;
      
      var wpn = GenerateRandomEqOnLevelAndCollectIt<Weapon>();
      GotoNextHeroTurn();
      InteractHeroWith(enemy);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
    }

    [Test]
    [Repeat(1)]
    public void StunnedEffectFromWeapon()
    {
      var game = CreateGame();// genNumOfEnemies:1);
      var hero = game.Hero;
      hero.Immortal = true;
      hero.Stats.SetNominal(EntityStatKind.ChanceToMeleeHit, 100);
      var wpn = GenerateEquipment<Weapon>("hammer");
      //wpn.MakeMagic(EntityStatKind.ChanceToCauseStunning, 10);
      //wpn.Identify();
      SetHeroEquipment(wpn);
      //hero.Stats.GetStat(EntityStatKind.ChanceToCauseStunning).Value.Nominal = 40;
      var ccs = hero.GetCurrentValue(EntityStatKind.ChanceToCauseStunning);
      Assert.AreEqual(ccs, 10);
      //Assert.AreEqual(AllEnemies.Count, 1);
      int effCounter = 0;
      Enemy enemy = ActivePlainEnemies.Where(i => !i.IsImmuned(EffectType.Stunned)).First();//AllEnemies.First();//;
      enemy.Immortal = true;
      PrepareToBeBeaten(enemy);

      for (int ei = 0; ei < 50; ei++)
      {
        PlaceCloseToHero(enemy);
        enemy.OnMeleeHitBy(hero);

        if (enemy.HasLastingEffect(EffectType.Stunned))
        {
          effCounter++;
          var le = enemy.LastingEffectsSet.GetByType(EffectType.Stunned);
          Assert.AreEqual(le.Description, "Stunned (Pending Turns: 3)");
          Assert.Less(le.PendingTurns, 5);
          Debug.WriteLine("bl "+ enemy+" has le stune: "+le);

          for (int i = 0; i < 5; i++)
          {
            var pt = le.PendingTurns;
            GotoNextHeroTurn();
            if (le.PendingTurns == pt && pt != 0)
            {
              int k = 0;
              k++;
            }
            Assert.Less(le.PendingTurns, pt);
            Debug.WriteLine("  il " + enemy + " has le stune: " + le);
            if (!enemy.HasLastingEffect(EffectType.Stunned))
              break;
          }
          Debug.WriteLine("al " + enemy + " has le stune: " + le);
        }
        //GotoNextHeroTurn();
        if (enemy.HasLastingEffect(EffectType.Stunned))
        {
          //enemy.ApplyLastingEffects();
          int k = 0;
          k++;
        }
        
        Assert.False(enemy.HasLastingEffect(EffectType.Stunned));
        //game.Level.SetEmptyTile(enemy.point);//make room for next one
      }
      Assert.Greater(effCounter, 0);
      Assert.Less(effCounter, 10);
    }

    [Test]
    public void KillEnemy()
    {
      var game = CreateGame();
      //var hero = game.Hero;

      var enemies = game.GameManager.EnemiesManager.AllEntities;
      var initEnemyCount = enemies.Count;
      Assert.Greater(initEnemyCount, 0);
      Assert.AreEqual(initEnemyCount, CurrentNode.GetTiles<Enemy>().Count);

      var enemy = ActivePlainEnemies.First();
      while (enemy.Alive)
        InteractHeroWith(enemy as Enemy);
      var finalEnemyCount = enemies.Count;
      Assert.AreEqual(finalEnemyCount, initEnemyCount - 1);
      Assert.AreEqual(finalEnemyCount, CurrentNode.GetTiles<Enemy>().Count);
    }

    [Test]
    [TestCase(AttackKind.Unset)]
    [TestCase(AttackKind.PhysicalProjectile)]
    [TestCase(AttackKind.SpellElementalProjectile)]
    public void DamageFromEnemiesVaries(AttackKind attackKind)
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.Greater(ActivePlainEnemies.Count, 0);
      var enemy = ActivePlainEnemies.First();
      var emp = game.Level.GetEmptyTiles().Where(i => i.DistanceFrom(hero) < 6 && i.DistanceFrom(hero) > 1).First();
      game.Level.SetTile(enemy, emp.point);
      //PlaceCloseToHero(enemy);
      if (attackKind == AttackKind.Unset)
        DoDamage(enemy, game.Hero, (LivingEntity attacker, LivingEntity victim) => { victim.OnMeleeHitBy(attacker); });

      else if (attackKind == AttackKind.PhysicalProjectile)
      {
        var en = enemy as Enemy;
        var fi = en.AddFightItem(FightItemKind.ThrowingKnife);
        fi.Count = 15;
        en.SelectedFightItem = fi as ProjectileFightItem;
        DoDamage(enemy, game.Hero, (LivingEntity attacker, LivingEntity victim) => CallDoDamagePhysicalProjectile(attacker, victim));
      }
      //else if (attackKind == AttackKind.WeaponElementalProjectile)
      //{
      //  healthChanges = DoDamage(game.Hero, enemy, (LivingEntity attacker, LivingEntity en) => CallDoDamageWeaponElementalProjectile(attacker, en));
      //}
      else if (attackKind == AttackKind.SpellElementalProjectile)
      {
        enemy.SelectedManaPoweredSpellSource = new Scroll(SpellKind.FireBall);
        enemy.Stats.SetNominal(EntityStatKind.Mana, 1000);
        DoDamage(enemy, game.Hero, (LivingEntity attacker, LivingEntity victim) => CallDoDamageSpellElementalProjectile(attacker, victim));
      }

    }

    [Test]
    [TestCase(AttackKind.Unset)]
    [TestCase(AttackKind.Melee)]
    [TestCase(AttackKind.PhysicalProjectile)]
    [TestCase(AttackKind.WeaponElementalProjectile)]
    //[Repeat(5)]
    [TestCase(AttackKind.SpellElementalProjectile)]
    public void HeroDamageRandomization(AttackKind attackKind)
    {
      var game = CreateGame();
      game.Hero.Stats.GetStat(EntityStatKind.Health).Value.Nominal = 500;
      game.Hero.Stats.GetStat(EntityStatKind.Mana).Value.Nominal = 500;

      var enemy = game.GameManager.EnemiesManager.GetEnemies().Where(i => i.PowerKind == EnemyPowerKind.Champion).First();
      List<float> healthChanges = null;
      PlaceCloseToHero(enemy);
      if (attackKind == AttackKind.Unset)
      {
        healthChanges = DoDamage(game.Hero, enemy, (LivingEntity attacker, LivingEntity en) => { en.OnMeleeHitBy(attacker); });
      }
      else if (attackKind == AttackKind.Melee)
      {
        healthChanges = DoDamage(game.Hero, enemy, (LivingEntity attacker, LivingEntity en) => CallDoDamageMelee(attacker, en));
      }
      else if (attackKind == AttackKind.PhysicalProjectile)
      {
        healthChanges = DoDamage(game.Hero, enemy, (LivingEntity attacker, LivingEntity en) => CallDoDamagePhysicalProjectile(attacker, en));
      }
      else if (attackKind == AttackKind.WeaponElementalProjectile)
      {
        healthChanges = DoDamage(game.Hero, enemy, (LivingEntity attacker, LivingEntity en) => CallDoDamageWeaponElementalProjectile(attacker, en));
      }
      else if (attackKind == AttackKind.SpellElementalProjectile)
      {
        healthChanges = DoDamage(game.Hero, enemy, (LivingEntity attacker, LivingEntity en) => CallDoDamageSpellElementalProjectile(attacker, en));
      }
    }

    private void CallDoDamageSpellElementalProjectile(LivingEntity attacker, LivingEntity victim)
    {
      if (attacker is Hero)
      {
        Assert.True(UseFireBallSpellSource(attacker as Hero, victim, true));
      }
      else 
      {
        game.GameManager.SpellManager.ApplyAttackPolicy(attacker, victim, attacker.SelectedManaPoweredSpellSource, null, (p) => {});
      }
    }
       
    private void CallDoDamageWeaponElementalProjectile(LivingEntity attacker, LivingEntity en)
    {
      Assert.True(attacker is Hero);
      HeroUseWeaponElementalProjectile(en);
    }

    List<float> DoDamage(LivingEntity attacker, LivingEntity victim, Action<LivingEntity, LivingEntity> hitVictim)
    {
      var healthChanges = new List<float>();
      for (int ind = 0; ind < 10; ind++)
      {
        var health = victim.Stats.Health;
        hitVictim(attacker, victim);
        healthChanges.Add(health - victim.Stats.Health);
        if (attacker is Hero)
          GotoNextHeroTurn();
        else
        {

          if (game.GameManager.Context.TurnOwner != TurnOwner.Enemies)
          {
            game.GameManager.SkipHeroTurn();
            game.MakeGameTick();
          }
          Assert.AreEqual(game.GameManager.Context.TurnOwner, Roguelike.TurnOwner.Enemies);
        }
      }

      var groupedChanges = healthChanges.GroupBy(i => i).ToList();
      Assert.Greater(groupedChanges.Count, 3);
      var min = groupedChanges.Min(i => i.Key);
      var max = groupedChanges.Max(i => i.Key);
      Assert.Greater(max, min);
      //Assert.Less(max, attacker.Stats.MeleeAttack / 3);

      return healthChanges;
    }

    private void CallDoDamagePhysicalProjectile(LivingEntity attacker, LivingEntity victim)
    {
      if (attacker is Hero)
      {
        var hero = attacker as Hero;
        var fi = ActivateFightItem(FightItemKind.PlainArrow, hero);
        var wpn = GenerateEquipment<Weapon>("Bow");
        Assert.True(SetHeroEquipment(wpn));
        Assert.True(UseFightItem(hero, victim, fi));
      }
      else 
      {
        var en = attacker as Enemy;
        var pfi = en.SelectedFightItem as ProjectileFightItem;

        if (attacker.DistanceFrom(victim) <= 1)
          return;

        Assert.True(game.GameManager.ApplyAttackPolicy(attacker, victim, pfi, null, (p) => { }));
      }
    }

    private void CallDoDamageMelee(LivingEntity _hero, LivingEntity victim)
    {
      var hero = _hero as Hero;
      if (hero.GetActiveWeapon() != null)
        hero.MoveEquipmentCurrent2Inv(hero.GetActiveWeapon(), CurrentEquipmentKind.Weapon);
      var attack = hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.MeleeAttack);

      var wpn = GenerateEquipment<Weapon>("rusty_sword");
      Assert.AreEqual(wpn.PrimaryStatValue, 3);
      Assert.AreEqual(wpn.PrimaryStatDescription, "Melee Attack: 2-4");

      SetHeroEquipment(wpn);
      var attackWithWpn = hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.MeleeAttack);
      Assert.Greater(attackWithWpn, attack);

      var attackFormatted = hero.GetFormattedStatValue(Roguelike.Attributes.EntityStatKind.MeleeAttack, false);
      Assert.True(attackFormatted.Contains("-"));//e.g. "17-19");

      Assert.Greater(victim.OnMeleeHitBy(hero), 0); 
    }

    [Test]
    [Repeat(1)]
    public void TestAlwaysCausedEffect()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.AlwaysHit[AttackKind.Melee] = true;
      hero.SetAlwaysCausesLastingEffect(EffectType.Bleeding, true);
      var wpn = GenerateEquipment<Weapon>("rusty_sword");
      SetHeroEquipment(wpn);

      var ens = ActivePlainEnemies.Where(i => !i.IsImmuned(EffectType.Bleeding)).ToList();
      var initEnemyCount = ens.Count;
      Assert.Greater(initEnemyCount, 0);
      foreach (var en in ens)
      {
        Assert.False(en.LastingEffects.Any());
        InteractHeroWith(en as Enemy);
        Assert.True(en.LastingEffects.Any());
      }
    }

    
  }
}
