﻿using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Generators;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
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
      {
        var wpn = game.GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Weapon, 1);
        Assert.IsFalse(wpn.GetMagicStats().Any());
        wpn.MakeMagic();
        Assert.False(wpn.GetMagicStats().Any());
        wpn.Identify();
        Assert.True(wpn.GetMagicStats().Any());
      }
      {
        var wpn1 = game.GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Weapon, 1);
        var ms = wpn1.GetMagicStats();
        Assert.False(ms.Any());
        wpn1.MakeMagic(true);
        ms = wpn1.GetMagicStats();
        Assert.False(ms.Any());
        wpn1.Identify();
        Assert.AreEqual(wpn1.GetMagicStats().Count, 2);
      }
    }

    [Test]
    public void HeroNoEquipment()
    {
      var game = CreateGame();
      var hero = game.Hero;
      Assert.AreEqual(hero.Stats.MeleeAttack, hero.Stats.Strength);
    }

    [Test]
    public void JewelleryLeftRigthFoundTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var statKind = EntityStatKind.Defense;
      float juw1StatValue = 0;
      float origHeroDef = game.Hero.GetTotalValue(EntityStatKind.Defense);
      {
        var juw1 = AddJewelleryToInv(game, statKind);//inc Defense
        game.Hero.HandleEquipmentFound(juw1);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defense), origHeroDef + juw1.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw1));
        juw1StatValue = juw1.PrimaryStatValue;
      }
      {
        //add second ring
        var juw2 = AddJewelleryToInv(game, statKind);//inc Defense
        game.Hero.HandleEquipmentFound(juw2);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defense), origHeroDef + juw1StatValue + juw2.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw2));
      }

    }

    [Test]
    public void JewelleryTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      float ringLeft = 0;
      var statKind = EntityStatKind.Defense;
      float origHeroDef = game.Hero.GetTotalValue(EntityStatKind.Defense);
      {
        Jewellery juw = AddJewelleryToInv(game, statKind);//inc Defense
        game.Hero.MoveEquipmentInv2Current(juw, CurrentEquipmentKind.RingLeft);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defense), origHeroDef + juw.PrimaryStatValue);
        ringLeft = juw.PrimaryStatValue;
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw));
      }
      {
        //add second ring
        var juw1 = AddJewelleryToInv(game, statKind);

        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defense);
        game.Hero.MoveEquipmentInv2Current(juw1, CurrentEquipmentKind.RingRight);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defense), origHeroDef + ringLeft + juw1.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw1));
      }

      float ringRight = 0;
      {
        var juw2 = AddJewelleryToInv(game, statKind);
        juw2.PrimaryStatValue *= 5;
        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defense);
        game.Hero.MoveEquipmentInv2Current(juw2, CurrentEquipmentKind.RingRight);
        ringRight = juw2.PrimaryStatValue;
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defense), origHeroDef + ringLeft + juw2.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw2));
      }

      {
        //gen. ring
        var juwNotMatching = AddJewelleryToInv(game, statKind);
        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defense);
        var set = game.Hero.MoveEquipmentInv2Current(juwNotMatching, CurrentEquipmentKind.Amulet);//ring not matching amulet slot
        Assert.False(set);

        var juw33 = game.GameManager.LootGenerator.LootFactory.EquipmentFactory.GetRandomJewellery(EntityStatKind.Defense, EquipmentKind.Amulet);
        AddItemToInv(juw33);
        set = game.Hero.MoveEquipmentInv2Current(juw33, CurrentEquipmentKind.Amulet);
        Assert.True(set);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defense), origHeroDef + ringLeft + ringRight + juw33.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw33));
      }

      var on = game.Hero.CurrentEquipment.PrimaryEquipment;
      var rr = on[CurrentEquipmentKind.RingRight];
      Assert.True(game.Hero.MoveEquipmentCurrent2Inv(rr, CurrentEquipmentPosition.Right));
      var rl = on[CurrentEquipmentKind.RingLeft];
      Assert.True(game.Hero.MoveEquipmentCurrent2Inv(rl, CurrentEquipmentPosition.Left));
      var amu = on[CurrentEquipmentKind.Amulet];
      Assert.True(game.Hero.MoveEquipmentCurrent2Inv(amu, CurrentEquipmentPosition.Unset));
      Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defense), origHeroDef);

      Assert.True(game.Hero.Inventory.Contains(rr as Loot));
      Assert.True(game.Hero.Inventory.Contains(rl as Loot));
      Assert.True(game.Hero.Inventory.Contains(amu as Loot));

    }

    [Test]
    public void EquipmentPrimaryAndSpare()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      Assert.Greater(wpn.PrimaryStatValue, 0);
      hero.Inventory.Add(wpn);
      var attackBase = hero.GetCurrentValue(EntityStatKind.MeleeAttack);
      hero.MoveEquipmentInv2Current(wpn, CurrentEquipmentKind.Weapon);
      var attackWithWpn = hero.GetCurrentValue(EntityStatKind.MeleeAttack);
      Assert.Greater(attackWithWpn, attackBase);
      hero.MoveEquipmentCurrent2Inv(wpn, CurrentEquipmentPosition.Unset);

      Assert.AreEqual(attackBase, hero.GetCurrentValue(EntityStatKind.MeleeAttack));
    }

    [Test]
    public void EquipmentMagicProps()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var esk = EntityStatKind.MeleeAttack;
      Equipment wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      var att = wpn.PrimaryStatValue;
      Assert.AreEqual(wpn.PrimaryStatKind, esk);
      var price = wpn.Price;
      Assert.Greater(price, 0);

      var ms = wpn.GetMagicStats();
      Assert.AreEqual(ms.Count, 0);//not magic item

      wpn.MakeMagic(esk, 4);
      ms = wpn.GetMagicStats();
      Assert.AreEqual(ms.Count, 0);//not identied
      wpn.Identify();
      ms = wpn.GetMagicStats();
      Assert.AreEqual(ms.Count, 1);
      Assert.AreEqual(att + 4, wpn.GetStats().GetTotalValue(esk));
      Assert.Greater(wpn.Price, price);
    }

    [Test]
    public void EquipmentBasicProps()
    {
      var lg = new LootGenerator(Container);
      lg.LevelIndex = 1;
      {
        var kinds = new[] { EquipmentKind.Weapon, EquipmentKind.Armor, EquipmentKind.Shield, EquipmentKind.Helmet, EquipmentKind.Ring, EquipmentKind.Amulet };
        foreach (var kind in kinds)
        {
          var eq = lg.GetRandomEquipment(kind, 1);
          Assert.Greater(eq.PrimaryStatValue, 0);

          var stats = eq.GetStats();
          Assert.AreEqual(stats.GetTotalValue(eq.PrimaryStatKind), eq.PrimaryStatValue);

          if (kind == EquipmentKind.Weapon)
          {
            Assert.AreEqual(eq.PrimaryStatKind, EntityStatKind.MeleeAttack);
            Assert.AreEqual(stats.MeleeAttack, eq.PrimaryStatValue);
          }
          else if (kind == EquipmentKind.Armor || kind == EquipmentKind.Glove || kind == EquipmentKind.Helmet)
          {
            Assert.AreEqual(eq.PrimaryStatKind, EntityStatKind.Defense);
            Assert.AreEqual(stats.Defense, eq.PrimaryStatValue);
          }
        }
      }
    }

    [Test]
    public void BetterEquipmentPutOnHero()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var lg = game.GameManager.LootGenerator;
      var eq1 = lg.GetRandomEquipment(EquipmentKind.Weapon, 1);

      var heroStatBefore = hero.GetTotalValue(eq1.PrimaryStatKind);
      PutEqOnLevelAndCollectIt(eq1);

      var heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], eq1);
      Assert.Greater(hero.GetTotalValue(eq1.PrimaryStatKind), heroStatBefore);
      heroStatBefore = hero.GetTotalValue(eq1.PrimaryStatKind);
      Assert.False(hero.Inventory.Contains(eq1));//it is on so not in inv

      var eq2 = lg.GetRandomEquipment(EquipmentKind.Weapon, 1);
      var wpnStatBefore = eq2.GetStats().GetTotalValue(eq2.PrimaryStatKind);
      eq2.MakeMagic(EntityStatKind.MeleeAttack, 5);
      eq2.Identify();
      Assert.AreEqual(eq2.GetStats().GetTotalValue(eq2.PrimaryStatKind), wpnStatBefore + 5);

      PutEqOnLevelAndCollectIt(eq2);
      //Active Equipment is returned dynamically
      heroEq = hero.GetActiveEquipment();
      var heroWeapoon = heroEq[CurrentEquipmentKind.Weapon];
      Assert.AreEqual(heroWeapoon, eq2);
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
      var heroAttack = heroStats.MeleeAttack;
      Assert.AreEqual(heroStats.Strength, heroAttack);
      var attack1 = hero.GetCurrentValue(EntityStatKind.MeleeAttack);
      Assert.AreEqual(attack1, heroAttack);

      var lg = game.GameManager.LootGenerator;
      var kinds = new[] { EquipmentKind.Weapon, EquipmentKind.Armor, EquipmentKind.Shield, EquipmentKind.Helmet, EquipmentKind.Ring, EquipmentKind.Amulet };
      foreach (var kind in kinds)
      {
        var eq = lg.GetRandomEquipment(kind, 1);
        Assert.Greater(eq.PrimaryStatValue, 0);

        var statBefore = heroStats.GetTotalValue(eq.PrimaryStatKind);
        PutEqOnLevelAndCollectIt(eq);
        var active = hero.CurrentEquipment.GetActiveEquipment();
        var eqOn = active[Equipment.FromEquipmentKind(kind, kind == EquipmentKind.Ring ? AdvancedLivingEntity.DefaultCurrentEquipmentPosition : CurrentEquipmentPosition.Unset)];
        Assert.AreEqual(eqOn, eq);
        Assert.AreEqual(heroStats.GetTotalValue(eq.PrimaryStatKind), statBefore + eq.PrimaryStatValue);
      }
    }

    [Test]
    public void TorchPutOnHero()
    {
      var game = CreateGame();
      var hero = game.Hero;
      Assert.AreEqual(hero.Inventory.ItemsCount, 0);
      var heroStats = hero.Stats;
      var lg = game.GameManager.LootGenerator;
      var torch = lg.GetLootByAsset("ThrowingTorch") as ProjectileFightItem;
      torch.Count = 5;
      PutEqOnLevelAndCollectIt(torch);
      Assert.AreEqual(hero.Inventory.ItemsCount, 0);//it's equipped
      Assert.False(hero.Inventory.Contains(torch));
      Assert.AreEqual(torch.Count, 5);
      //Assert.True(hero.MoveEquipmentInv2Current(torch, CurrentEquipmentKind.Shield));
      var sh = hero.CurrentEquipment.GetActiveEquipment()[CurrentEquipmentKind.Shield];
      Assert.AreEqual(sh, torch);
      Assert.AreEqual(sh.Count, 5);
      Assert.False(hero.Inventory.Contains(torch));

      Assert.True(hero.MoveEquipmentCurrent2Inv(sh, CurrentEquipmentKind.Shield));
      Assert.True(hero.Inventory.Contains(torch));
      Assert.AreEqual(torch.Count, 5);
    }
  }
}
