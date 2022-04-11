using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Events;
using Roguelike.LootFactories;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
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

    private Enemy PrepareEnemy(bool addImmunities = true)
    {
      var hero = game.Hero;
      var enemy = ActivePlainEnemies.First();
      enemy.Stats.Stats[EntityStatKind.Health].Value.Nominal = 300;
      PlaceCloseToHero(enemy);
      if (addImmunities)
        enemy.AddImmunity(Roguelike.Effects.EffectType.Bleeding);

      if (!enemy.Name.Any())
        enemy.Name = "enemy";
      return enemy;
    }


    //[Test]
    //[Repeat(1)]
    //public void WeightedNetFightItemTest()
    //{
    //  var game = CreateGame();
    //  var hero = game.Hero;
    //  hero.UseAttackVariation = false;//other tests do it

    //  var fi = ActivateFightItem(FightItemKind.WeightedNet, hero);

    //  var enemy = PrepareEnemy();
    //  var enemyHealth = enemy.Stats.Health;
    //  //enemy.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Defense, 10);
    //  //var mana = hero.Stats.Mana;

    //  //var bow = GenerateEquipment<Weapon>("Bow");
    //  //Assert.True(SetHeroEquipment(bow));

    //  //var tile = game.GameManager.CurrentNode.GetClosestEmpty(hero);
    //  //game.GameManager.CurrentNode.SetTile(enemy, tile.point);

    //  Assert.True(UseFightItem(hero, enemy, fi));
    //  Assert.Greater(enemyHealth, enemy.Stats.Health);
    //  //Assert.AreEqual(mana, hero.Stats.Mana);
    //  //Assert.False(game.GameManager.HeroTurn);
    //  //var diffBow = enemyHealth - enemy.Stats.Health;
    //  //enemyHealth = enemy.Stats.Health;

    //  //GotoNextHeroTurn();
    //  //fi = ActivateFightItem(FightItemKind.ThrowingKnife, hero);
    //  //Assert.True(UseFightItem(hero, enemy, fi));
    //  //Assert.Greater(enemyHealth, enemy.Stats.Health);
    //  //var diffKnife = enemyHealth - enemy.Stats.Health;
    //  //Assert.Greater(diffBow, diffKnife);
    //}

    [Test]
    [Repeat(1)]
    public void ArrowFightItemTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.UseAttackVariation = false;//other tests do it

      var fi = ActivateFightItem(FightItemKind.PlainArrow, hero);

      var enemy = ActivePlainEnemies.First();
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
      var enemy = ActivePlainEnemies.First();
      var enemyHealth = enemy.Stats.Health;
      var mana = hero.Stats.Mana;

      Assert.True(game.GameManager.HeroTurn);
      UseFightItem(hero, enemy, fi);

      Assert.Greater(enemyHealth, enemy.Stats.Health);
      Assert.AreEqual(mana, hero.Stats.Mana);
      Assert.False(game.GameManager.HeroTurn);
    }
        
    [Test]
    [TestCase(FightItemKind.ExplosiveCocktail)]
    [TestCase(FightItemKind.PoisonCocktail)]
    public void TestExplosiveOnHero(FightItemKind fightItemKind)
    {
      var game = CreateGame();
      var en = PlainEnemies.First();

      en.ActiveFightItem = en.SetActiveFightItem(fightItemKind);
      Assert.LessOrEqual(en.ActiveFightItem.Count, 4);
      if (en.ActiveFightItem.Count < 2)
        en.ActiveFightItem.Count = 2;
      var hero = game.GameManager.Hero;
      var beginHealth = hero.Stats.Health;

      var explosiveCocktail = en.GetFightItem(fightItemKind) as ProjectileFightItem;
      var dam = explosiveCocktail.Damage;
      PlaceCloseToHero(en);
      Assert.True(game.GameManager.ApplyAttackPolicy(en, hero, explosiveCocktail, null, (p) => { }));
      var lifeDiff = beginHealth - hero.Stats.Health;
      
      Assert.Greater(lifeDiff, 0);
      while (hero.LastingEffects.Any())
        GotoNextHeroTurn();

      beginHealth = hero.Stats.Health;
      Assert.True(en.SetLevel(5));
      explosiveCocktail = en.GetFightItem(fightItemKind) as ProjectileFightItem;
      
      Assert.True(game.GameManager.ApplyAttackPolicy(en, hero, explosiveCocktail, null, (p) => { }));
      var lifeDiff1 = beginHealth - hero.Stats.Health;
      Assert.Greater(lifeDiff1, lifeDiff);
      Assert.LessOrEqual(en.ActiveFightItem.Count, 2);
    }
  }
}
