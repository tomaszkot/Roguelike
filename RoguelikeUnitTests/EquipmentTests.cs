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
        //game.Hero.SetEquipment(EquipmentKind.RingRight, juw2);
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

        var juw33 = game.GameManager.LootGenerator.GetRandomJewellery(EntityStatKind.Defence, EquipmentKind.Amulet);
        AddItemToInv(game, juw33);
        set = game.Hero.MoveEquipmentInv2Current(juw33, EquipmentKind.Amulet);//game.Hero.SetEquipment(EquipmentKind.Amulet, juw3);
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

    private static Jewellery AddJewelleryToInv(Roguelike.RoguelikeGame game, EntityStatKind statKind)
    {
      var juw = game.GameManager.LootGenerator.GetRandomJewellery(statKind);
      Assert.AreEqual(juw.PrimaryStatKind, EntityStatKind.Defence);
      Assert.IsTrue(juw.PrimaryStatValue > 0);

      AddItemToInv(game, juw);
      return juw;
    }

    private static void AddItemToInv(Roguelike.RoguelikeGame game, Jewellery juw)
    {
      game.Hero.Inventory.Add(juw);
      Assert.IsTrue(game.Hero.Inventory.Contains(juw));
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
      Assert.AreEqual(hero.Stats.Strength, heroAttack);
      var attack1 = hero.GetCurrentValue(EntityStatKind.Attack);
      Assert.AreEqual(attack1, heroAttack);

      var wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      Assert.AreEqual(wpn.PrimaryStatKind, EntityStatKind.Attack);
      Assert.Greater(wpn.PrimaryStatValue, 0);
      
      hero.SetEquipment(EquipmentKind.Weapon, wpn);

      Assert.Greater(hero.Stats.Attack, heroAttack);
      Assert.AreEqual(hero.GetCurrentValue(EntityStatKind.Attack), hero.Stats.Attack);
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
