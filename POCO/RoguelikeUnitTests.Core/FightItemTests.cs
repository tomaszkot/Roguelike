using Dungeons.Core;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike.Abilities;
using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Events;
using Roguelike.LootFactories;
using Roguelike.Tiles;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FightItemTests : TestBase
  {
    readonly float StartAttack = Roguelike.Tiles.LivingEntities.Hero.GetStrengthStartStat();

    [Test]
    public void HunterTrapDeactivation()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var fi = new ProjectileFightItem(FightItemKind.HunterTrap, hero);
      fi.Count = 1;
      game.GameManager.CurrentNode.SetTileAtRandomPosition(fi);
      Assert.AreEqual(game.GameManager.CurrentNode.GetTile(fi.point), fi);

      for (int i = 0; i < 10; i++)
      {
        fi.SetState(FightItemState.Activated);
        fi.SetState(FightItemState.Busy);
        fi.SetState(FightItemState.Deactivated);
        game.GameManager.AppendAction(new LootAction(fi, null) { Kind = LootActionKind.Deactivated });
      }
      Assert.AreEqual(game.GameManager.CurrentNode.GetTile(fi.point).Symbol, new Tile().Symbol);
    }

    [Test]
    public void ArrowPower()
    {
      var game = CreateGame();
      var hero = game.Hero;

      ActivateFightItem(FightItemKind.PlainArrow, hero);
      var wpn = GenerateEquipment<Weapon>("Bow");
      Assert.True(SetHeroEquipment(wpn));
      var expectedBowAttackValue = Props.FightItemBaseDamage + 2 + Props.BowBaseDamage;
      AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedBowAttackValue);

      ActivateFightItem(FightItemKind.IronArrow, hero);
      var expectedBowAttackValueIron = expectedBowAttackValue + 3;
      AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedBowAttackValueIron);

      ActivateFightItem(FightItemKind.SteelArrow, hero);
      var expectedBowAttackValueSteel = expectedBowAttackValueIron + 5;
      AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedBowAttackValueSteel);

    }

    [Test]
    public void WeaponPower()
    {
      var game = CreateGame();
      var hero = game.Hero;

      //no weapon
      var ad = new AttackDescription(hero, false, Roguelike.Attributes.AttackKind.Melee);
      var meleeStart = ad.CurrentTotal;
      Assert.AreEqual(meleeStart, StartAttack);

      ad = new AttackDescription(hero, false, Roguelike.Attributes.AttackKind.PhysicalProjectile);
      Assert.AreEqual(ad.CurrentTotal, 0);

      //stone
      ActivateFightItem(FightItemKind.Stone, hero);
      ad = new AttackDescription(hero, false, Roguelike.Attributes.AttackKind.Melee);
      var meleeStart1 = ad.CurrentTotal;
      Assert.AreEqual(meleeStart1, meleeStart);
      var expectedStoneAttackValue = StartAttack/2 + 1;
      AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedStoneAttackValue);

      //Torch
      ActivateFightItem(FightItemKind.ThrowingTorch, hero);
      ad = new AttackDescription(hero, false, Roguelike.Attributes.AttackKind.Melee);
      meleeStart1 = ad.CurrentTotal;
      Assert.AreEqual(meleeStart1, meleeStart);
      var expectedTorchAttackValue = StartAttack / 2 + 1;
      AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedTorchAttackValue);

      //ThrowingKnife
      ActivateFightItem(FightItemKind.ThrowingKnife, hero);
      var expectedThrowingKnifeAttackValue = StartAttack / 2 + 3;
      AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedThrowingKnifeAttackValue);

      //arrow
      ActivateFightItem(FightItemKind.PlainArrow, hero);
      var wpn = GenerateEquipment<Weapon>("Bow");
      Assert.True(SetHeroEquipment(wpn));
      var expectedBowAttackValue = Props.FightItemBaseDamage + 2 + Props.BowBaseDamage;
      AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedBowAttackValue);
      Assert.Greater(expectedBowAttackValue, expectedThrowingKnifeAttackValue);

      //bolt
      ActivateFightItem(FightItemKind.PlainBolt, hero);
      wpn = GenerateEquipment<Weapon>("Crossbow");
      Assert.True(SetHeroEquipment(wpn));
      var expectedCrossbowAttackValue = Props.FightItemBaseDamage + 3 + Props.CrossbowBaseDamage;
      AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedCrossbowAttackValue);
      Assert.Greater(expectedCrossbowAttackValue, expectedBowAttackValue);
    }

    private static AttackDescription AssertAttackValue(Roguelike.Tiles.LivingEntities.Hero hero,
      Roguelike.Attributes.AttackKind kind,
      float expectedAttackValue)
    {
      AttackDescription ad = new AttackDescription(hero, false, kind);
      Assert.AreEqual(ad.CurrentTotal, expectedAttackValue);
      return ad;
    }

   

    private Enemy PrepareEnemy(bool addImmunities = true, float health  = 300)
    {
      var hero = game.Hero;
      var enemy = ActivePlainEnemies.First();
      enemy.Stats.Stats[EntityStatKind.Health].Value.Nominal = health;
      PlaceCloseToHero(enemy);
      if (addImmunities)
        enemy.AddImmunity(Roguelike.Effects.EffectType.Bleeding);

      if (!enemy.Name.Any())
        enemy.Name = "enemy";
      return enemy;
    }

    [TestCase(FightItemKind.PlainArrow, "Bow")]
    [TestCase(FightItemKind.IronArrow, "Bow")]
    [TestCase(FightItemKind.SteelArrow, "Bow")]
    [TestCase(FightItemKind.PoisonArrow, "Bow")]
    [TestCase(FightItemKind.PlainBolt, "Crossbow")]
    [TestCase(FightItemKind.IronBolt, "Crossbow")]
    [TestCase(FightItemKind.SteelBolt, "Crossbow")]
    [TestCase(FightItemKind.IceBolt, "Crossbow")]
    //[Repeat(1)]
    public void ArrowFightItemTest(FightItemKind fik, string weapon)
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.UseAttackVariation = false;//other tests do it
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;//TODO

      var fi = ActivateFightItem(fik, hero);

      var enemy = ActivePlainEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      enemy.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Defense, 10);
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      var bow = GenerateEquipment<Weapon>(weapon);
      Assert.True(SetHeroEquipment(bow));

      var tile = game.GameManager.CurrentNode.GetClosestEmpty(hero);
      game.GameManager.CurrentNode.SetTile(enemy, tile.point);

      Assert.True(UseFightItem(hero, enemy, fi));

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.AreEqual(mana, hero.Stats.Mana);
      Assert.False(game.GameManager.HeroTurn);
      var diffBow = enemyHealth - enemy.Stats.Health;
      enemyHealth = enemy.Stats.Health;

      GotoNextHeroTurn();
      fi = ActivateFightItem(FightItemKind.ThrowingKnife, hero);
      Assert.True(UseFightItem(hero, enemy, fi));
      Assert.Greater(enemyHealth, enemy.Stats.Health);
      var diffKnife = enemyHealth - enemy.Stats.Health;
      Assert.Greater(diffBow, diffKnife);
    }

    [TestCase(FightItemKind.PoisonArrow, "Bow")]
    [TestCase(FightItemKind.FireArrow, "Bow")]
    [TestCase(FightItemKind.IceBolt, "Crossbow")]
    //[Repeat(1)]
    public void ArrowLikeSpecialItemTest(FightItemKind fik, string weapon)
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.UseAttackVariation = false;//other tests do it
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;//TODO

      var fi = ActivateFightItem(fik, hero, 10);

      var enemy = ActivePlainEnemies.First();
      PrepareToBeBeaten(enemy);
      var enemyHealth = enemy.Stats.Health;

      var bow = GenerateEquipment<Weapon>(weapon);
      Assert.True(SetHeroEquipment(bow));

      var tile = game.GameManager.CurrentNode.GetClosestEmpty(hero);
      game.GameManager.CurrentNode.SetTile(enemy, tile.point);

      bool works = false;
      for (int i = 0; i < 10; i++)
      {
        Assert.True(UseFightItem(hero, enemy, fi));
        Assert.Greater(enemyHealth, enemy.Stats.Health);
        var diffBow = enemyHealth - enemy.Stats.Health;
        enemyHealth = enemy.Stats.Health;
        GotoNextHeroTurn();

        if (enemy.HasLastingEffect(Roguelike.Effects.EffectType.Poisoned) ||
            enemy.HasLastingEffect(Roguelike.Effects.EffectType.Frozen) ||
            enemy.HasLastingEffect(Roguelike.Effects.EffectType.Firing))
        {
          works = true;
          break;
        }
      }

      Assert.True(works);
    }

    [Test]
    public void StoneFightItemTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;//TODO

      var fi = ActivateFightItem(FightItemKind.Stone, hero);
      var fiCount = hero.Inventory.GetStackedCount(fi);
      var enemy = PrepareEnemy();
      var enemyHealth = enemy.Stats.Health;
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      Assert.True(UseFightItem(hero, enemy, fi));
      Assert.AreEqual(hero.Inventory.GetStackedCount(fi), fiCount-1);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.AreEqual(mana, hero.Stats.Mana);
      Assert.False(game.GameManager.HeroTurn);
    }

    [Test]
    [TestCase(FightItemKind.ExplosiveCocktail)]
    [TestCase(FightItemKind.PoisonCocktail)]
    [Repeat(1)]
    public void TestExplosiveOnHeroAutoMechanics(FightItemKind fightItemKind)
    {
      var game = CreateGame();
      var en = PlainEnemies.First();
      MakeEnemyThrowProjectileAtHero(en, this.Game.GameManager, fightItemKind, false, ()=>GotoNextHeroTurn(), true);
    }

    [Test]
    [TestCase(FightItemKind.ExplosiveCocktail)]
    [TestCase(FightItemKind.PoisonCocktail)]
    [Repeat(1)]
    public void TestExplosiveOnHeroWithEnemyLevel(FightItemKind fightItemKind)
    {
      var game = CreateGame();
      var en = PlainEnemies.First();
      en.AlwaysHit[AttackKind.PhysicalProjectile] = true;
      en.SelectedFightItem = en.SetActiveFightItem(fightItemKind);
      Assert.LessOrEqual(en.SelectedFightItem.Count, 4);
      if (en.SelectedFightItem.Count < 2)
        en.SelectedFightItem.Count = 2;
      Assert.AreEqual(game.GameManager.Hero.GetFightItemKindHitCounter(fightItemKind), 0);

      var hero = game.GameManager.Hero;
      hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;//TODO
      PrepareToBeBeaten(hero);
      var beginHeroHealth = hero.Stats.Health;

      var explosiveCocktail = en.GetFightItem(fightItemKind) as ProjectileFightItem;
      var dam = explosiveCocktail.Damage;
      PlaceCloseToHero(en);
      Assert.True(game.GameManager.ApplyAttackPolicy(en, hero, explosiveCocktail, null, (p) => { }));
      var heroLifeDiff = beginHeroHealth - hero.Stats.Health;

      Assert.Greater(heroLifeDiff, 0);
      while (hero.LastingEffects.Any())
        GotoNextHeroTurn();

      beginHeroHealth = hero.Stats.Health;
      Assert.True(en.SetLevel(5));
      explosiveCocktail = en.GetFightItem(fightItemKind) as ProjectileFightItem;

      Assert.True(game.GameManager.ApplyAttackPolicy(en, hero, explosiveCocktail, null, (p) => { }));
      var heroLifeDiff1 = beginHeroHealth - hero.Stats.Health;
      Assert.Greater(heroLifeDiff1, heroLifeDiff);
    }

    //TODO
    //[Test]
    //public void CannonFightItemTestEnemyTooClose()
    //{
    //  var game = CreateGame();
    //  var hero = game.Hero;
    //  hero.AlwaysHit[AttackKind.PhysicalProjectile] = true;

    //  var cannonBall = ActivateFightItem(FightItemKind.CannonBall, hero);
    //  var fiCount = hero.Inventory.GetStackedCount(cannonBall);
    //  var en1 = AllEnemies.First();
    //  var en2 = AllEnemies.Where(i => i!= en1).FirstOrDefault();
    //  BaseHelper.AlignOneAfterAnotherNextToHero(game.GameManager, en1, en2, hero);

    //  var enemy = en2;
    //  float enemyHealth = enemy.Stats.Health;

    //  Assert.True(game.GameManager.HeroTurn);
    //  Assert.False(UseFightItem(hero, enemy, cannonBall));
    //  Assert.AreEqual(hero.Inventory.GetStackedCount(cannonBall), fiCount - 1);

    //  Assert.Greater(enemyHealth, enemy.Stats.Health);
    //}

    

    private void WaitForAbilityCoolDown(Ability ab, bool exactlyZero = true)
    {
      if (exactlyZero)
      {
        int i = 0;
        while (ab.CoolDownCounter > 0)
        {
          Assert.True(game.Hero.Alive);
          GotoNextHeroTurn();
          i++;
          if (i > 30)
          {
            Assert.True(false);
          }
        }
        return;
      }
      var ct = ab.CoolDownCounter;
      for (int i = 0; i <= ct; i++)
        GotoNextHeroTurn();
    }

    private Enemy PrepareEnemyForCannonHit()
    {
      var hero = game.Hero;
      var enemy = PrepareEnemy(health:1000);
      PlaceEnemyForCannon(hero, enemy);
      return enemy;
    }

    private void PlaceEnemyForCannon(Hero hero, Enemy enemy)
    {
      if (enemy.DistanceFrom(hero) == 2)
        return;
      var empts = game.GameManager.CurrentNode.GetEmptyTiles().Where(i => i.DistanceFrom(hero) == 2);
      Assert.IsTrue(empts.Any());
      var place = empts.Where(i => i.point.X == hero.point.X || i.point.Y == hero.point.Y).First();
      game.GameManager.CurrentNode.SetTile(enemy, place.point);
    }

    [Test]
    public void CannonAttackDescTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var cannonBall = ActivateFightItem(FightItemKind.CannonBall, hero);
      AssertAttackValue(hero, AttackKind.PhysicalProjectile, FightItem.BaseCannonBallDamage);

      var ChanceToPhysicalProjectileHit = hero.Stats.GetCurrentValue(EntityStatKind.ChanceToPhysicalProjectileHit);
      Assert.AreEqual(ChanceToPhysicalProjectileHit, 75);
      var ChanceToCannonHit = hero.Stats.GetCurrentValue(EntityStatKind.CannonExtraChanceToHit);
      Assert.AreEqual(ChanceToCannonHit, 0);
      var ab = hero.GetActiveAbility(Roguelike.Abilities.AbilityKind.Cannon);
      MaximizeAbility(ab, hero);
      //TODO
    }
  }
}
