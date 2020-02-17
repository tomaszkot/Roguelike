using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class EquipmentTests : TestBase
  {
    [Test]
    public void HeroNoEquipment()
    {
      var game = CreateGame();
      var hero = game.Hero;

      //var ca = hero.GetCurrentValue(EntityStatKind.Attack);
      //var ta = hero.GetTotalValue(EntityStatKind.Attack);
      Assert.AreEqual(hero.Stats.Attack, hero.Stats.Strength);
    }

    [Test]
    public void JewelleryTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      float ringLeft = 0;
      float origHeroDef = game.Hero.GetTotalValue(EntityStatKind.Defence);
      {
        var juw = game.GameManager.LootGenerator.GetRandomJewellery(EntityStatKind.Defence);
        Assert.AreEqual(juw.PrimaryStatKind, EntityStatKind.Defence);
        Assert.IsTrue(juw.PrimaryStatValue > 0);
  
        game.Hero.SetEquipment(EquipmentKind.RingLeft, juw);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + juw.PrimaryStatValue);
        ringLeft = juw.PrimaryStatValue;
      }
      {
        var juw1 = game.GameManager.LootGenerator.GetRandomJewellery(EntityStatKind.Defence);
        Assert.AreEqual(juw1.PrimaryStatKind, EntityStatKind.Defence);
        Assert.IsTrue(juw1.PrimaryStatValue > 0);
        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defence);
        game.Hero.SetEquipment(EquipmentKind.RingRight, juw1);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + ringLeft + juw1.PrimaryStatValue);
      }

      float ringRight = 0;
      {
        var juw2 = game.GameManager.LootGenerator.GetRandomJewellery(EntityStatKind.Defence);
        Assert.AreEqual(juw2.PrimaryStatKind, EntityStatKind.Defence);
        Assert.IsTrue(juw2.PrimaryStatValue > 0);
        juw2.PrimaryStatValue *= 5;
        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defence);
        game.Hero.SetEquipment(EquipmentKind.RingRight, juw2);
        ringRight = juw2.PrimaryStatValue;
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + ringLeft + juw2.PrimaryStatValue);
      }

      {
        var juw3 = game.GameManager.LootGenerator.GetRandomJewellery(EntityStatKind.Defence);
        Assert.AreEqual(juw3.PrimaryStatKind, EntityStatKind.Defence);
        Assert.IsTrue(juw3.PrimaryStatValue > 0);
        //juw2.PrimaryStatValue;
        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defence);
        var set = game.Hero.SetEquipment(EquipmentKind.Amulet, juw3);
        Assert.False(set);
        juw3 = game.GameManager.LootGenerator.GetRandomJewellery(EntityStatKind.Defence, EquipmentKind.Amulet);
        set = game.Hero.SetEquipment(EquipmentKind.Amulet, juw3);
        Assert.True(set);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + ringLeft + ringRight + juw3.PrimaryStatValue);
      }

      game.Hero.SetEquipment(EquipmentKind.RingRight, null);
      game.Hero.SetEquipment(EquipmentKind.RingLeft, null);
      game.Hero.SetEquipment(EquipmentKind.Amulet, null);
      Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef);
    }


    [Test]
    public void EquipmentProps()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Equipment wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      Assert.AreEqual(wpn.PrimaryStatKind, EntityStatKind.Attack);
      Assert.Greater(wpn.PrimaryStatValue, 0);
      var stats = wpn.GetStats();
      Assert.AreEqual(stats.Attack, wpn.PrimaryStatValue);

      var att = wpn.PrimaryStatValue;
      wpn.MakeMagic(EntityStatKind.Attack, false, 4);
      Assert.AreEqual(att+4, wpn.GetStats().GetTotalValue(EntityStatKind.Attack));
    }

    [Test]
    public void EquipmentPutOnHero()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var heroAttack = hero.Stats.Attack;

      Equipment wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      Assert.AreEqual(wpn.PrimaryStatKind, EntityStatKind.Attack);
      Assert.Greater(wpn.PrimaryStatValue, 0);
      hero.SetEquipment(EquipmentKind.Weapon, wpn);

      Assert.Greater(hero.Stats.Attack, heroAttack);
    }
  }
}
