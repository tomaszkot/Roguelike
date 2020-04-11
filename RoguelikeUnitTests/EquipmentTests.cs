using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Generators;
using Roguelike.Tiles;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class EquipmentTests : TestBase
  {
    [Test]
    public void TestMagicLevel()
    {
      var game = CreateGame();

      var wpn = game.GameManager.LootGenerator.GetRandom(EquipmentKind.Weapon);
      Assert.IsFalse(wpn.GetMagicStats().Any());
      wpn.MakeMagic();
      Assert.NotNull(wpn.GetMagicStats().Single());

      var wpn1 = game.GameManager.LootGenerator.GetRandom(EquipmentKind.Weapon);
      wpn1.MakeMagic(true);
      Assert.AreEqual(wpn1.GetMagicStats().Count, 2);

    }

    [Test]
    public void HeroNoEquipment()
    {
      var game = CreateGame();
      var hero = game.Hero;
      Assert.AreEqual(hero.Stats.Attack, hero.Stats.Strength);
    }

    [Test]
    public void JewelleryTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      float ringLeft = 0;
      var statKind = EntityStatKind.Defence;
      float origHeroDef = game.Hero.GetTotalValue(EntityStatKind.Defence);
      {
        Jewellery juw = AddJewelleryToInv(game, statKind);
        game.Hero.MoveEquipmentInv2Current(juw, EquipmentKind.RingLeft);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + juw.PrimaryStatValue);
        ringLeft = juw.PrimaryStatValue;
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw));
      }
      {
        var juw1 = AddJewelleryToInv(game, statKind);

        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defence);
        game.Hero.MoveEquipmentInv2Current(juw1, EquipmentKind.RingRight);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + ringLeft + juw1.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw1));
      }

      float ringRight = 0;
      {
        var juw2 = AddJewelleryToInv(game, statKind);
        juw2.PrimaryStatValue *= 5;
        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defence);
        game.Hero.MoveEquipmentInv2Current(juw2, EquipmentKind.RingRight);
        ringRight = juw2.PrimaryStatValue;
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + ringLeft + juw2.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw2));
      }

      {
        //gen. ring
        var juwNotMatching = AddJewelleryToInv(game, statKind);
        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defence);
        var set = game.Hero.MoveEquipmentInv2Current(juwNotMatching, EquipmentKind.Amulet);//ring not matching amulet slot
        Assert.False(set);

        var juw33 = game.GameManager.LootGenerator.EquipmentFactory.GetRandomJewellery(EntityStatKind.Defence, EquipmentKind.Amulet);
        AddItemToInv(game, juw33);
        set = game.Hero.MoveEquipmentInv2Current(juw33, EquipmentKind.Amulet);
        Assert.True(set);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + ringLeft + ringRight + juw33.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw33));
      }

      var on = game.Hero.CurrentEquipment.PrimaryEquipment;
      var rr = on[EquipmentKind.RingRight];
      Assert.True(game.Hero.MoveEquipmentCurrent2Inv(rr, EquipmentKind.RingRight));
      var rl = on[EquipmentKind.RingLeft];
      Assert.True(game.Hero.MoveEquipmentCurrent2Inv(rl, EquipmentKind.RingLeft));
      var amu = on[EquipmentKind.Amulet];
      Assert.True(game.Hero.MoveEquipmentCurrent2Inv(amu, EquipmentKind.Amulet));
      Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef);

      Assert.True(game.Hero.Inventory.Contains(rr));
      Assert.True(game.Hero.Inventory.Contains(rl));
      Assert.True(game.Hero.Inventory.Contains(amu));

    }

    [Test]
    public void EquipmentPrimaryAndSpare()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var ek = EquipmentKind.Weapon;
      Equipment wpn = game.GameManager.GenerateRandomEquipment(ek);
      Assert.Greater(wpn.PrimaryStatValue, 0);
      hero.Inventory.Add(wpn);
      var attackBase = hero.GetCurrentValue(EntityStatKind.Attack);
      hero.MoveEquipmentInv2Current(wpn, ek);
      var attackWithWpn = hero.GetCurrentValue(EntityStatKind.Attack);
      Assert.Greater(attackWithWpn, attackBase);
      hero.MoveEquipmentCurrent2Inv(wpn, ek);

      Assert.AreEqual(attackBase, hero.GetCurrentValue(EntityStatKind.Attack));
    }

    [Test]
    public void EquipmentMagicProps()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Equipment wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      var att = wpn.PrimaryStatValue;
      var price = wpn.Price;
      Assert.Greater(price, 0);

      wpn.MakeMagic(EntityStatKind.Attack, 4);
      Assert.AreEqual(att+4, wpn.GetStats().GetTotalValue(EntityStatKind.Attack));
      Assert.Greater(wpn.Price, price);
    }

    [Test]
    public void EquipmentBasicProps()
    {
      var lg = new LootGenerator();
      {
        var kinds = new[] { EquipmentKind.Weapon, EquipmentKind.Armor, EquipmentKind.Shield, EquipmentKind.Helmet, EquipmentKind.RingLeft, EquipmentKind.Amulet };
        foreach(var kind in kinds)
        {
          var eq = lg.GetRandom(kind);
          Assert.Greater(eq.PrimaryStatValue, 0);

          var stats = eq.GetStats();
          Assert.AreEqual(stats.GetTotalValue(eq.PrimaryStatKind), eq.PrimaryStatValue);

          if (kind == EquipmentKind.Weapon)
          {
            Assert.AreEqual(eq.PrimaryStatKind, EntityStatKind.Attack);
            Assert.AreEqual(stats.Attack, eq.PrimaryStatValue);
          }
          else if (kind == EquipmentKind.Armor || kind == EquipmentKind.Gloves || kind == EquipmentKind.Helmet)
          {
            Assert.AreEqual(eq.PrimaryStatKind, EntityStatKind.Defence);
            Assert.AreEqual(stats.Defence, eq.PrimaryStatValue);
          }
        }
      }
    }

    [Test]
    public void BetterEquipmentPutOnHero()
    {
      var game = CreateGame();
      var hero = game.Hero;
      
      var lg = new LootGenerator();
      var eq1 = lg.GetRandom(EquipmentKind.Weapon);
      
      var heroStatBefore = hero.GetTotalValue(eq1.PrimaryStatKind);
      PutEqOnLevelAndCollectIt(eq1);

      var heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[EquipmentKind.Weapon], eq1);
      Assert.Greater(hero.GetTotalValue(eq1.PrimaryStatKind), heroStatBefore);
      heroStatBefore = hero.GetTotalValue(eq1.PrimaryStatKind);
      Assert.False(hero.Inventory.Contains(eq1));

      var eq2 = lg.GetRandom(EquipmentKind.Weapon);
      var wpnStatBefore = eq2.GetStats().GetTotalValue(eq2.PrimaryStatKind);
      eq2.MakeMagic(EntityStatKind.Attack, 5);
      //eq2.PrimaryStat.Value.Factor += 5;
      Assert.AreEqual(eq2.GetStats().GetTotalValue(eq2.PrimaryStatKind), wpnStatBefore + 5);
            
      PutEqOnLevelAndCollectIt(eq2);
      //Active Equipment is returned dynamically
      heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[EquipmentKind.Weapon], eq2);
      var heroStatAfter = hero.GetTotalValue(eq2.PrimaryStatKind);
      Assert.AreEqual(heroStatAfter, heroStatBefore + 5);
      Assert.False(hero.Inventory.Contains(eq2));
      Assert.True(hero.Inventory.Contains(eq1));//put back
    }

    [Test]
    public void EquipmentPutOnHero()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var heroStats = hero.Stats;

      //Attack
      var heroAttack = heroStats.Attack;
      Assert.AreEqual(heroStats.Strength, heroAttack);
      var attack1 = hero.GetCurrentValue(EntityStatKind.Attack);
      Assert.AreEqual(attack1, heroAttack);

      var lg = new LootGenerator();
      var kinds = new[] { EquipmentKind.Weapon, EquipmentKind.Armor, EquipmentKind.Shield, EquipmentKind.Helmet, EquipmentKind.RingLeft, EquipmentKind.Amulet };
      foreach (var kind in kinds)
      {
        var eq = lg.GetRandom(kind);
        Assert.Greater(eq.PrimaryStatValue, 0);

        var statBefore = heroStats.GetTotalValue(eq.PrimaryStatKind);
        PutEqOnLevelAndCollectIt(eq);
        Assert.Greater(heroStats.GetTotalValue(eq.PrimaryStatKind), statBefore);
      }
    }
  }
}
