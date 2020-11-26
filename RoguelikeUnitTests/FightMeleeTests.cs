using NUnit.Framework;
using Roguelike;
using Roguelike.Effects;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
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

        var enemies = game.GameManager.CurrentNode.GetTiles<Enemy>().Where(i=> i.DungeonNodeIndex == hero.DungeonNodeIndex).ToList();
        Assert.AreEqual(enemies.Count, 1);
        var enemy = enemies.Where(i => i.PowerKind != EnemyPowerKind.Plain).FirstOrDefault();
        if (enemy == null)
        {
          enemy = enemies.First();
          enemy.SetNonPlain(false);
        }

        Assert.AreEqual(enemy.LastingEffects.Count, 0);
        GenerationInfo.ChanceToTurnOnSpecialSkillByEnemy = 1f;

        var closeHero = game.Level.GetClosestEmpty(hero);
        game.Level.SetTile(enemy, closeHero.Point);
        enemy.OnPhysicalHit(hero);

        game.GameManager.Context.TurnOwner = TurnOwner.Allies;
        game.GameManager.Context.PendingTurnOwnerApply = true;
        GotoNextHeroTurn(game);
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
      enemy.OnPhysicalHit(hero);
      Assert.Greater(enemyHealth, enemy.Stats.Health);
      enemyHealth = enemy.Stats.Health;

      var wpn = GenerateRandomEqOnLevelAndCollectIt<Weapon>();
      enemy.OnPhysicalHit(hero);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
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
      while(enemy.Alive)
        InteractHeroWith(enemy as Enemy);
      var finalEnemyCount = enemies.Count;
      Assert.AreEqual(finalEnemyCount, initEnemyCount - 1);
      Assert.AreEqual(finalEnemyCount, game.GameManager.CurrentNode.GetTiles<Enemy>().Count);
    }

    [Test]
    public void WeaponDamageRandomisation()
    {
      var game = CreateGame();
      var en = game.GameManager.EnemiesManager.GetEnemies().Where(i=>i.PowerKind == EnemyPowerKind.Champion).First();

      var attack = game.Hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Attack);

      var wpn = game.GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.AreEqual(wpn.PrimaryStatValue, 2);
      Assert.AreEqual(wpn.PrimaryStatDescription, "Attack: 1-3");
      
      game.Hero.SetEquipment(CurrentEquipmentKind.Weapon, wpn);
      var attackWithWpn = game.Hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Attack);
      Assert.Greater(attackWithWpn, attack);

      var attackFormatted = game.Hero.GetFormattedStatValue(Roguelike.Attributes.EntityStatKind.Attack);
      Assert.AreEqual(attackFormatted, "16-18");

      var damages = new List<float>();
      for (int i = 0; i < 10; i++)
      {
        var damage = en.OnPhysicalHit(game.Hero);
        if(damage > 0)
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
      var enemyHealth = enemy.Stats.Health;
      enemy.OnPhysicalHit(game.Hero);
      var enemyHealthDiff = enemyHealth - enemy.Stats.Health;
      enemyHealth = enemy.Stats.Health;

      var emp = game.GameManager.CurrentNode.GetClosestEmpty(hero);
      game.GameManager.CurrentNode.SetTile(enemy, emp.Point);

      var scroll = new Scroll(SpellKind.Rage);
      hero.Inventory.Add(scroll);
      var attackPrev = hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Attack);
      var spell = game.GameManager.ApplyPassiveSpell(hero, scroll);
      Assert.NotNull(spell);
      Assert.Greater(hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Attack), attackPrev);

      enemy.OnPhysicalHit(game.Hero);
      var enemyHealthDiffRage = enemyHealth - enemy.Stats.Health;
      Assert.Greater(enemyHealthDiffRage, enemyHealthDiff);//rage

      enemyHealth = enemy.Stats.Health;

      GotoSpellEffectEnd(spell);
      enemy.OnPhysicalHit(game.Hero);
      var enemyHealthDiffAterRage = enemyHealth - enemy.Stats.Health;
      Assert.AreEqual(enemyHealthDiff, enemyHealthDiffAterRage);//rage over

    }
  }
}
