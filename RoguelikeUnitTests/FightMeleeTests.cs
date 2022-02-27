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
        enemy.ActiveManaPoweredSpellSource = null;//this causes attack

        //hit enemy to force him to use effect
        enemy.OnMelleeHitBy(hero);

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

      Assert.Greater(ActivePlainEnemies.Count, 0);
      var enemy = ActivePlainEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      enemy.OnMelleeHitBy(hero);
      Assert.Greater(enemyHealth, enemy.Stats.Health);
      enemyHealth = enemy.Stats.Health;

      var wpn = GenerateRandomEqOnLevelAndCollectIt<Weapon>();
      enemy.OnMelleeHitBy(hero);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
    }

    [Test]
    public void StunnedEffectFromWeapon()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.ChanceToMeleeHit, 100);
      var wpn = GenerateEquipment<Weapon>("hammer");
      wpn.MakeMagic(EntityStatKind.ChanceToCauseStunning, 100);
      wpn.Identify();
      SetHeroEquipment(wpn);
      var ccs = hero.GetCurrentValue(EntityStatKind.ChanceToCauseStunning);
      Assert.AreEqual(ccs, 100);

      var enemy = ActivePlainEnemies.First();
      enemy.OnMelleeHitBy(hero);
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
        DoDamage(enemy, game.Hero, (LivingEntity attacker, LivingEntity victim) => { victim.OnMelleeHitBy(attacker); });

      else if (attackKind == AttackKind.PhysicalProjectile)
      {
        var en = enemy as Enemy;
        var fi = en.AddFightItem(FightItemKind.ThrowingKnife);
        fi.Count = 10;
        en.ActiveFightItem = fi as ProjectileFightItem;
        DoDamage(enemy, game.Hero, (LivingEntity attacker, LivingEntity victim) => CallDoDamagePhysicalProjectile(attacker, victim));
      }
      //else if (attackKind == AttackKind.WeaponElementalProjectile)
      //{
      //  healthChanges = DoDamage(game.Hero, enemy, (LivingEntity attacker, LivingEntity en) => CallDoDamageWeaponElementalProjectile(attacker, en));
      //}
      else if (attackKind == AttackKind.SpellElementalProjectile)
      {
        enemy.ActiveManaPoweredSpellSource = new Scroll(SpellKind.FireBall);
        enemy.Stats.SetNominal(EntityStatKind.Mana, 1000);
        DoDamage(enemy, game.Hero, (LivingEntity attacker, LivingEntity victim) => CallDoDamageSpellElementalProjectile(attacker, victim));
      }

    }

    [Test]
    [TestCase(AttackKind.Unset)]
    [TestCase(AttackKind.Melee)]
    [TestCase(AttackKind.PhysicalProjectile)]
    [TestCase(AttackKind.WeaponElementalProjectile)]
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
        healthChanges = DoDamage(game.Hero, enemy, (LivingEntity attacker, LivingEntity en) => { en.OnMelleeHitBy(attacker); });
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
        var wpn = game.GameManager.LootGenerator.GetLootByAsset("staff") as Weapon;
        SetHeroEquipment(wpn);
        var weapon = game.Hero.GetActiveWeapon();
        Assert.AreEqual(weapon.SpellSource.Kind, SpellKind.FireBall);
        Assert.True(game.GameManager.SpellManager.ApplyAttackPolicy(game.Hero, victim, weapon.SpellSource));
      }
      else 
      {
        game.GameManager.SpellManager.ApplyAttackPolicy(attacker, victim, attacker.ActiveManaPoweredSpellSource, null, (p) => {});
      }
    }


    private void CallDoDamageWeaponElementalProjectile(LivingEntity attacker, LivingEntity en)
    {
      Assert.True(UseFireBallSpellSource(attacker as Hero, en, true));
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
      Assert.Less(max, attacker.Stats.MeleeAttack / 3);

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
        var pfi = en.ActiveFightItem as ProjectileFightItem;

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

      Assert.Greater(victim.OnMelleeHitBy(hero), 0); 
    }

    [Test]
    public void RageScrollTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.UseAttackVariation = false;
      var enemy = AllEnemies.First();

      Func<float> hitEnemy = () =>
      {
        var enemyHealth = enemy.Stats.Health;
        enemy.OnMelleeHitBy(game.Hero);
        var lastEnemyHealthDiff = enemyHealth - enemy.Stats.Health;
        return lastEnemyHealthDiff;
      };
      var healthDiff = hitEnemy();

      var emp = CurrentNode.GetClosestEmpty(hero);
      CurrentNode.SetTile(enemy, emp.point);

      var scroll = new Scroll(SpellKind.Rage);
      hero.Inventory.Add(scroll);
      var attackPrev = hero.GetCurrentValue(EntityStatKind.MeleeAttack);
      var spell = game.GameManager.SpellManager.ApplyPassiveSpell(hero, scroll);
      Assert.NotNull(spell);
      Assert.Greater(hero.GetCurrentValue(EntityStatKind.MeleeAttack), attackPrev);

      var healthDiffRage = hitEnemy();
      Assert.Greater(healthDiffRage, healthDiff);//rage in work

      GotoSpellEffectEnd(spell);
      Assert.AreEqual(hero.GetCurrentValue(EntityStatKind.MeleeAttack), attackPrev);
      var healthDiffAfterRage = hitEnemy();
      var delta = Math.Abs(healthDiffAfterRage - healthDiff);
      Assert.Less(delta, 0.001);//rage was over

    }
  }
}
