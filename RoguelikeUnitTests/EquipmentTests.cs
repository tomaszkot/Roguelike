using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Generators;
using Roguelike.Tiles;

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

        game.Hero.Inventory.Add(juw);
        Assert.IsTrue(game.Hero.Inventory.Contains(juw));
        game.Hero.MoveEquipmentInv2Current(juw, EquipmentKind.RingLeft);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + juw.PrimaryStatValue);
        ringLeft = juw.PrimaryStatValue;
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw));
      }
      {
        var juw1 = game.GameManager.LootGenerator.GetRandomJewellery(EntityStatKind.Defence);
        game.Hero.Inventory.Add(juw1);
        Assert.IsTrue(game.Hero.Inventory.Contains(juw1));
        Assert.AreEqual(juw1.PrimaryStatKind, EntityStatKind.Defence);
        Assert.IsTrue(juw1.PrimaryStatValue > 0);
        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defence);
        game.Hero.MoveEquipmentInv2Current(juw1, EquipmentKind.RingRight);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + ringLeft + juw1.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw1));
      }

      float ringRight = 0;
      {
        var juw2 = game.GameManager.LootGenerator.GetRandomJewellery(EntityStatKind.Defence);
        game.Hero.Inventory.Add(juw2);
        Assert.IsTrue(game.Hero.Inventory.Contains(juw2));
        Assert.AreEqual(juw2.PrimaryStatKind, EntityStatKind.Defence);
        Assert.IsTrue(juw2.PrimaryStatValue > 0);
        juw2.PrimaryStatValue *= 5;
        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defence);
        //game.Hero.SetEquipment(EquipmentKind.RingRight, juw2);
        game.Hero.MoveEquipmentInv2Current(juw2, EquipmentKind.RingRight);
        ringRight = juw2.PrimaryStatValue;
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + ringLeft + juw2.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw2));
      }

      {
        var juw3 = game.GameManager.LootGenerator.GetRandomJewellery(EntityStatKind.Defence);
        game.Hero.Inventory.Add(juw3);
        Assert.IsTrue(game.Hero.Inventory.Contains(juw3));
        Assert.AreEqual(juw3.PrimaryStatKind, EntityStatKind.Defence);
        Assert.IsTrue(juw3.PrimaryStatValue > 0);
        //juw2.PrimaryStatValue;
        var defHero = game.Hero.GetTotalValue(EntityStatKind.Defence);
        var set = game.Hero.SetEquipment(EquipmentKind.Amulet, juw3);
        Assert.False(set);
        juw3 = game.GameManager.LootGenerator.GetRandomJewellery(EntityStatKind.Defence, EquipmentKind.Amulet);
        game.Hero.Inventory.Add(juw3);
        Assert.IsTrue(game.Hero.Inventory.Contains(juw3));
        set = game.Hero.MoveEquipmentInv2Current(juw3, EquipmentKind.Amulet);//game.Hero.SetEquipment(EquipmentKind.Amulet, juw3);
        Assert.True(set);
        Assert.AreEqual(game.Hero.GetTotalValue(EntityStatKind.Defence), origHeroDef + ringLeft + ringRight + juw3.PrimaryStatValue);
        Assert.IsTrue(!game.Hero.Inventory.Contains(juw3));
      }

      var on = game.Hero.CurrentEquipment.PutOnEquipment;
      Assert.True(game.Hero.MoveEquipmentCurrent2Inv(on[EquipmentKind.RingRight], EquipmentKind.RingRight));
      Assert.True(game.Hero.MoveEquipmentCurrent2Inv(on[EquipmentKind.RingLeft], EquipmentKind.RingLeft));
      Assert.True(game.Hero.MoveEquipmentCurrent2Inv(on[EquipmentKind.Amulet], EquipmentKind.Amulet));
      //game.Hero.SetEquipment(EquipmentKind.RingLeft, null);
      // game.Hero.SetEquipment(EquipmentKind.Amulet, null);
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

      var wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      Assert.AreEqual(wpn.PrimaryStatKind, EntityStatKind.Attack);
      Assert.Greater(wpn.PrimaryStatValue, 0);
      
      hero.SetEquipment(EquipmentKind.Weapon, wpn);

      Assert.Greater(hero.Stats.Attack, heroAttack);
      var heroDef = hero.Stats.Defence;
      var lg = new LootGenerator();
      {
        var hel = lg.GetRandomHelmet();
        Assert.AreEqual(hel.PrimaryStatKind, EntityStatKind.Defence);
        Assert.Greater(hel.PrimaryStatValue, 0);
        hero.SetEquipment(EquipmentKind.Helmet, hel);
        Assert.Greater(hero.Stats.Defence, heroDef);
      }
      {
        heroDef = hero.Stats.Defence;
        var sh = lg.GetRandomShield();
        Assert.AreEqual(sh.PrimaryStatKind, EntityStatKind.Defence);
        Assert.Greater(sh.PrimaryStatValue, 0);
        hero.SetEquipment(EquipmentKind.Shield, sh);
        Assert.Greater(hero.Stats.Defence, heroDef);
      }

      heroDef = hero.Stats.Defence;
      var gl = lg.GetRandomGloves();
      Assert.AreEqual(gl.PrimaryStatKind, EntityStatKind.Defence);
      Assert.Greater(gl.PrimaryStatValue, 0);
      hero.SetEquipment(EquipmentKind.Gloves, gl);
      Assert.Greater(hero.Stats.Defence, heroDef);
    }
  }
}
