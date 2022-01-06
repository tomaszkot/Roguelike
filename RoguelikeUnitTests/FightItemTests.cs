using NUnit.Framework;
using Roguelike.Calculated;
using Roguelike.LootFactories;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FightItemTests : TestBase
  {
    const float StartAttack = 15.0f;

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

      //ThrowingKnife
      ActivateFightItem(FightItemKind.ThrowingKnife, hero);
      var expectedThrowingKnifeAttackValue = StartAttack / 2 + 3;
      AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedThrowingKnifeAttackValue);

      ////ExplosiveCocktail
      //ActivateFightItem(FightItemKind.ExplosiveCocktail, hero);
      //var expectedHunterTrapAttackValue = 4;
      //AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedHunterTrapAttackValue);

      //arrow
      ActivateFightItem(FightItemKind.PlainArrow, hero);
      var wpn = GenerateEquipment<Weapon>("Bow");
      Assert.True(SetHeroEquipment(wpn));
      var expectedBowAttackValue = Props.FightItemBaseDamage + 1 + Props.BowBaseDamage;
      AssertAttackValue(hero, Roguelike.Attributes.AttackKind.PhysicalProjectile, expectedBowAttackValue);
      Assert.Greater(expectedBowAttackValue, expectedThrowingKnifeAttackValue);

      //bolt
      ActivateFightItem(FightItemKind.PlainBolt, hero);
      wpn = GenerateEquipment<Weapon>("Crossbow");
      Assert.True(SetHeroEquipment(wpn));
      var expectedCrossbowAttackValue = Props.FightItemBaseDamage + 2 + Props.CrossbowBaseDamage;
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

    [Test]
    public void ArrowFightItemTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.UseAttackVariation = false;//other tests do it

      var fi = ActivateFightItem(FightItemKind.PlainArrow, hero);

      var enemy = ActiveEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      enemy.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Defense, 10);
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      var bow = GenerateEquipment<Weapon>("Bow");
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

    [Test]
    public void StoneFightItemTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var fi = ActivateFightItem(FightItemKind.Stone, hero);
      var enemy = ActiveEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      UseFightItem(hero, enemy, fi);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.AreEqual(mana, hero.Stats.Mana);
      Assert.False(game.GameManager.HeroTurn);
    }
        
    [Test]
    public void TestExplosiveOnHero()
    {
      var game = CreateGame();
      var en = PlainEnemies.First();
      en.AddFightItem(FightItemKind.ExplosiveCocktail);
      en.AddFightItem(FightItemKind.ExplosiveCocktail);
      var hero = game.GameManager.Hero;
      var beginHealth = hero.Stats.Health;

      var explosiveCocktail = en.GetFightItem(FightItemKind.ExplosiveCocktail) as ProjectileFightItem;
      var dam = explosiveCocktail.Damage;
      PlaceCloseToHero(en);
      Assert.True(game.GameManager.ApplyAttackPolicy(en, hero, explosiveCocktail, null, (p) => { }));
      var lifeDiff = beginHealth - hero.Stats.Health;
      
      Assert.Greater(lifeDiff, 0);
      while (hero.LastingEffects.Any())
        GotoNextHeroTurn();

      beginHealth = hero.Stats.Health;
      Assert.True(en.SetLevel(5));
      explosiveCocktail = en.GetFightItem(FightItemKind.ExplosiveCocktail) as ProjectileFightItem;
      //Assert.Greater(explosiveCocktail.Damage, dam);
      Assert.True(game.GameManager.ApplyAttackPolicy(en, hero, explosiveCocktail, null, (p) => { }));
      var lifeDiff1 = beginHealth - hero.Stats.Health;
      Assert.Greater(lifeDiff1, lifeDiff);
    }
  }
}
