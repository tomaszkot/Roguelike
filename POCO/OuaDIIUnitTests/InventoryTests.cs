using NUnit.Framework;
using OuaDII.Tiles.LivingEntities;
using OuaDII.Tiles.Looting;
using Roguelike.Attributes;
using Roguelike.LootContainers;
using Roguelike.Managers;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace OuaDIIUnitTests
{
  [TestFixture]
  class InventoryTests : TestBase
  {
    string portalScrollAsset = "portal_scroll";

    [Test]
    public void GodStatueBasicTests()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.False(GameManager.AlliesManager.AllEntities.Any());
      var god = new GodStatue();
      god.GodKind = OuaDII.Tiles.GodKind.Perun;
      Assert.True(god.GetMagicStats().Any());
      Assert.Greater(god.GetStats().GetTotalValue(Roguelike.Attributes.EntityStatKind.LightingAttack), 0);
      Assert.True(hero.Inventory.Add(god));
      Assert.True(hero.Inventory.Contains(god));

      Assert.True(hero.MoveEquipmentInv2Current(god, CurrentEquipmentKind.God));

      Assert.False(hero.Inventory.Contains(god));
      //Assert.True(GameManager.AlliesManager.AllEntities.Any());

      //Assert.True(hero.MoveEquipmentCurrent2Inv(god, CurrentEquipmentKind.God));
      //Assert.False(GameManager.AlliesManager.AllEntities.Any());
      //Assert.True(hero.Inventory.Contains(god));
    }

    [Test]
    public void TradeHero2Merchant()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.AreEqual(hero.Inventory.ItemsCount, 0);
      Assert.NotNull(hero);

      var merchant = GameManager.World.GetTiles<Merchant>().FirstOrDefault();
      Assert.NotNull(merchant);

      var loot = GameManager.LootGenerator.GetRandomLoot(LootKind.Scroll, 1);
      Assert.True(hero.Inventory.Add(loot));
      Assert.True(hero.Inventory.Items.Any());
      var heroGold = hero.Gold;

      var merchantGold = merchant.Gold;
      Assert.Greater(merchantGold, loot.Price);
      var sold = GameManager.SellItem(loot, hero, merchant);
      Assert.NotNull(sold);
      Assert.Greater(hero.Gold, heroGold);
      Assert.Less(merchant.Gold, merchantGold);
    }

    [Test]
    public void TradeMerchant2Hero()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.AreEqual(hero.Inventory.ItemsCount, 0);
      Assert.NotNull(hero);

      var merchant = GameManager.World.GetTiles<Merchant>().FirstOrDefault();
      Assert.NotNull(merchant);

      var loot = GameManager.LootGenerator.GetRandomLoot(LootKind.Scroll, 1);
      Assert.True(merchant.Inventory.Add(loot));
      Assert.True(merchant.Inventory.Items.Any());
      hero.Gold = 1000;
      var heroGold = hero.Gold;

      var merchantGold = merchant.Gold;
      Assert.Greater(heroGold, loot.Price);
      var sold = GameManager.SellItem(loot, merchant, hero);
      Assert.NotNull(sold);
      Assert.Greater(merchant.Gold, merchantGold);
      Assert.Less(hero.Gold, heroGold);
    }

    [Test]
    public void TradeWithMerchantNotPossibleIfCapacityReached()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.AreEqual(hero.Inventory.ItemsCount, 0);
      Assert.NotNull(hero);
      hero.Gold = 2000;

      var merchant = GameManager.World.GetTiles<Merchant>().FirstOrDefault();
      Assert.NotNull(merchant);

      var capacity = hero.Inventory.Capacity;
      //make hero inv full
      while (hero.Inventory.ItemsCount < capacity)
      {
        var loot = GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Weapon, hero.Level);
        Assert.True(hero.Inventory.Add(loot));
      }

      var lootExtra = GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Weapon, hero.Level);
      Assert.False(hero.Inventory.Add(lootExtra));

      var heroGold = hero.Gold;
      var merchantGold = merchant.Gold;
      Assert.True(merchant.Inventory.Add(lootExtra));
      var sold = GameManager.SellItem(lootExtra, merchant, hero);
      Assert.Null(sold);
      Assert.True(merchant.Inventory.Contains(lootExtra));
      Assert.AreEqual(hero.Gold, heroGold);
      Assert.AreEqual(merchant.Gold, merchantGold);
    }

    [Test]
    public void TradeWithMerchantPossibleIfCapacityReachedButStackableAlreadyInBasket()
    {
      CreateWorld();

      //init hero
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.NotNull(hero);
      Assert.AreEqual(hero.Inventory.ItemsCount, 0);
      hero.Inventory.Capacity = 1;

      //init merchant
      var merchant = GameManager.World.GetTiles<Merchant>().FirstOrDefault();
      Assert.NotNull(merchant);

      //make inv full, because Capacity == 1
      var loot = GameManager.LootGenerator.GetLootByAsset(portalScrollAsset) as Scroll;
      Assert.True(loot.StackedInInventory);
      loot.Count = 1;
      Assert.True(hero.Inventory.Add(loot));

      var lootExtra = GameManager.LootGenerator.GetLootByAsset("identify_scroll") as StackedLoot;
      //out of space, because Capacity == 1 
      Assert.False(hero.Inventory.Add(lootExtra));

      Assert.AreEqual(hero.Inventory.GetStackedCount(loot as StackedLoot), 1);

      lootExtra = GameManager.LootGenerator.GetLootByAsset(portalScrollAsset) as StackedLoot;
      lootExtra.Count = 5;
      hero.Gold = 1000;
      Assert.True(merchant.Inventory.Add(lootExtra));
      Assert.AreEqual(merchant.Inventory.GetStackedCount(lootExtra), 5);

      //sell to hero
      var sellItemArg = new RemoveItemArg();
      sellItemArg.StackedCount = 5;
      var sold = GameManager.SellItem(lootExtra, merchant, hero, sellItemArg);
      Assert.NotNull(sold);
      Assert.False(merchant.Inventory.Contains(lootExtra));
      Assert.True(hero.Inventory.Contains(lootExtra));
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot as StackedLoot), 1 + 5);
    }

    [Test]
    public void TradeWithMerchantCheckPricesForStacked()
    {
      CreateWorld();

      //hero
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero.Inventory.Capacity = 16;
      hero.Gold = 20;
      Assert.AreEqual(hero.Inventory.ItemsCount, 0);

      //mechant
      var merchant = GameManager.World.GetTiles<Merchant>().FirstOrDefault();
      Assert.NotNull(merchant);
      var merchantGold = merchant.Gold;

      merchant.Inventory.Items.Clear();
      var lootExtra = GameManager.LootGenerator.GetLootByAsset(portalScrollAsset) as StackedLoot;
      lootExtra.Count = 20;
      Assert.True(merchant.Inventory.Add(lootExtra));

      var heroGold = hero.Gold;
      //sell to hero
      var args = new RemoveItemArg();
      args.StackedCount = 5;
      var sold = GameManager.SellItem(lootExtra, merchant, hero, args);
      Assert.Null(sold);//not enough money
      Assert.AreEqual(hero.Inventory.ItemsCount, 0);
    }

      [Test]
    public void TradeWithMerchantCheckPrices()
    {
      CreateWorld();

      //hero
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero.Inventory.Capacity = 16;
      hero.Gold = 1000;

      Assert.AreEqual(hero.Inventory.ItemsCount, 0);

      //mechant
      var merchant = GameManager.World.GetTiles<Merchant>().FirstOrDefault();
      Assert.NotNull(merchant);
      var merchantGold = merchant.Gold;

      merchant.Inventory.Items.Clear();

      var lootExtra = GameManager.LootGenerator.GetLootByAsset(portalScrollAsset) as StackedLoot;
      lootExtra.Count = 1;
      Assert.True(merchant.Inventory.Add(lootExtra));

      var priceInMerchant = merchant.GetPrice(lootExtra);
      Assert.Greater(priceInMerchant, lootExtra.Price);
      Assert.AreEqual(merchant.Inventory.GetStackedCount(lootExtra), 1);
      lootExtra.Count = 5;
      //Assert.Greater(merchant.GetPrice(lootExtra), priceInMerchant);
      //Assert.AreEqual(priceInMerchant* lootExtra.Count, merchant.GetPrice(lootExtra));

      var heroGold = hero.Gold;
      //sell to hero
      var args = new RemoveItemArg();
      var sold = GameManager.SellItem(lootExtra, merchant, hero, args);
      Assert.NotNull(sold);
      Assert.True(merchant.Inventory.Contains(lootExtra));

      Assert.AreEqual(merchant.Inventory.GetStackedCount(lootExtra), 4);
      Assert.True(hero.Inventory.Contains(lootExtra));
      Assert.Less(hero.Gold, heroGold);
      Assert.Greater(merchant.Gold, merchantGold);
      Assert.AreEqual(hero.Gold, heroGold - priceInMerchant);

      args.StackedCount = 4;
      sold = GameManager.SellItem(lootExtra, merchant, hero, args);
      Assert.NotNull(sold);
      Assert.True(hero.Inventory.Contains(lootExtra));

      Assert.AreEqual(hero.Inventory.GetStackedCount(lootExtra), 5);
      Assert.False(merchant.Inventory.Contains(lootExtra));
      Assert.AreEqual(hero.Gold, heroGold - priceInMerchant * 5);
    }

   

    [Test]
    public void HeroChest()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.AreEqual(hero.Chest.Inventory.ItemsCount, 0);
      var loot = GameManager.LootGenerator.GetRandomLoot(LootKind.Scroll, 1) as Scroll;
      loot.Count = 1;
      hero.Chest.Inventory.Add(loot);
      Assert.AreEqual(hero.Chest.Inventory.ItemsCount, 1);

      var heroGold = hero.Gold;
      var sold = GameManager.SellItem(loot, hero.Chest, hero);
      Assert.NotNull(sold);
      Assert.AreEqual(heroGold, hero.Gold);
      Assert.AreEqual(hero.Chest.Inventory.ItemsCount, 0);

      sold = GameManager.SellItem(loot, hero, hero.Chest);
      Assert.NotNull(sold);
      Assert.AreEqual(heroGold, hero.Gold);
      Assert.AreEqual(hero.Chest.Inventory.ItemsCount, 1);

      GameManager.Save();
      GameManager.Load(hero.Name);
      hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      Assert.AreEqual(hero.Chest.Inventory.ItemsCount, 1);
    }

    [Test]
    public void MoveEqToCurrentEquipmentAndBack()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero.Gold = 0;
      Assert.AreEqual(hero.Inventory.ItemsCount, 0);

      //var merchant = GameManager.World.GetTiles<Merchant>().FirstOrDefault();
      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");

      Assert.True(hero.Inventory.Add(rusty));
      Assert.True(hero.Inventory.Contains(rusty));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), null);
      Assert.NotNull(GameManager.SellItem(rusty, hero, hero.Inventory, hero, hero.CurrentEquipment));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), rusty);
      Assert.False(hero.Inventory.Contains(rusty));

      Assert.NotNull(GameManager.SellItem(rusty, hero, hero.CurrentEquipment, hero, hero.Inventory));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), null);
      Assert.True(hero.Inventory.Contains(rusty));
    }

    [Test]
    public void SwapEq()
    {
      CreateWorld();

      var hero = GameManager.Hero as Hero;
      hero.Stats.SetNominal(EntityStatKind.Strength, 100);
      //add one eq
      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.True(hero.Inventory.Add(rusty));
      Assert.NotNull(GameManager.SellItem(rusty, hero, hero.Inventory, hero, hero.CurrentEquipment));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), rusty);
      Assert.False(hero.Inventory.Contains(rusty));

      var axe = GameManager.LootGenerator.GetLootByTileName<Weapon>("axe");
      axe.RequiredLevel = 1;
      Assert.True(hero.Inventory.Add(axe));

      Assert.NotNull(GameManager.SellItem(axe, hero, hero.Inventory, hero, hero.CurrentEquipment));//, null,
                                                                                                   //new CurrentEquipmentAddItemArg() { cek = CurrentEquipmentKind.Weapon }));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), axe);
      Assert.True(hero.Inventory.Contains(rusty));
      Assert.False(hero.Inventory.Contains(axe));
    }

    [Test]
    public void MoveCurrentEquipmentToMerchantAndBack()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero.Gold = 1000;

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.True(hero.Inventory.Add(rusty));
      Assert.NotNull(GameManager.SellItem(rusty, hero, hero.Inventory, hero, hero.CurrentEquipment));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), rusty);

      var merchant = GameManager.World.GetTiles<Merchant>().FirstOrDefault();
      Assert.NotNull(GameManager.SellItem(rusty, hero, hero.CurrentEquipment, merchant, merchant.Inventory));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), null);
      Assert.True(merchant.Inventory.Contains(rusty));

      //sell back to hero
      Assert.NotNull(GameManager.SellItem(rusty, merchant, merchant.Inventory, hero, hero.CurrentEquipment));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), rusty);
      Assert.False(merchant.Inventory.Contains(rusty));
      Assert.False(hero.Inventory.Contains(rusty));
    }

    [Test]
    public void MoveEquipment2AllyCurrentAndBack()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero.Gold = 0;

      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.True(hero.Inventory.Add(rusty));
      Assert.NotNull(GameManager.SellItem(rusty, hero, hero.Inventory, hero, hero.CurrentEquipment));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), rusty);

      var ally = new SkeletonSpell(hero).Ally;
      GameManager.AlliesManager.AddEntity(ally);

      Assert.NotNull(GameManager.SellItem(rusty, hero, hero.CurrentEquipment, ally, ally.CurrentEquipment));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), null);
      Assert.AreEqual(ally.CurrentEquipment.GetWeapon(), rusty);

      //sell back to hero
      Assert.NotNull(GameManager.SellItem(rusty, ally, ally.CurrentEquipment, hero, hero.CurrentEquipment));
      Assert.AreEqual(hero.CurrentEquipment.GetWeapon(), rusty);
      Assert.AreEqual(ally.CurrentEquipment.GetWeapon(), null);
      Assert.False(ally.Inventory.Contains(rusty));
      Assert.False(hero.Inventory.Contains(rusty));
    }

    [Test]
    public void MoveStackedEquipment2Ally()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var ally = new SkeletonSpell(hero).Ally;
      GameManager.AlliesManager.AddEntity(ally);
      Assert.False(ally.Inventory.Items.Any());
      ally.Inventory.Add(new Potion() { Kind = PotionKind.Health });
      ally.Inventory.Add(new Potion() { Kind = PotionKind.Health });
      ally.Inventory.Add(new Potion() { Kind = PotionKind.Health });
      Assert.AreEqual(ally.Inventory.ItemsCount, 1);

      var pot = new Potion() { Kind = PotionKind.Health };
      Assert.AreEqual(ally.Inventory.GetStackedCount(pot.Name), 3);
    }

    [Test]
    public void MoveEquipment2AllySaveMoveEqAgain()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      {
        var loot = GameManager.LootGenerator.GetLootByTileName<Armor>("cap");
        Assert.True(hero.Inventory.Add(loot));

        var ally = new SkeletonSpell(hero).Ally;
        GameManager.AlliesManager.AddEntity(ally);
        Assert.False(ally.Inventory.Items.Any());

        Assert.NotNull(GameManager.SellItem(loot, hero, hero.Inventory, ally, ally.Inventory));
        Assert.True(ally.Inventory.Items.Any());
        Assert.NotNull(ally.Inventory.Owner);
      }
      GameManager.Save();
      GameManager.Load(hero.Name);
      var allyLoaded = GameManager.AlliesManager.AllAllies.ElementAt(0) as Roguelike.Tiles.LivingEntities.Ally;
      Assert.True(allyLoaded.Inventory.Items.Any());

      var axe = GameManager.LootGenerator.GetLootByTileName<Weapon>("axe");
      Assert.True(hero.Inventory.Add(axe));
      Assert.NotNull(GameManager.SellItem(axe, hero, hero.Inventory, allyLoaded, allyLoaded.Inventory));
      Assert.NotNull(allyLoaded.Inventory.Owner);
    }

    [Test]
    public void MoveEquipment2AllyCurrentSaveMoveEqAgain()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      {
        var arm = GameManager.LootGenerator.GetLootByTileName<Armor>("cap");
        Assert.True(hero.Inventory.Add(arm));

        var ally = new SkeletonSpell(hero).Ally;
        Assert.AreEqual(ally.Inventory.InvBasketKind, InvBasketKind.Ally);
        GameManager.AlliesManager.AddEntity(ally);
        Assert.False(ally.Inventory.Items.Any());

        Assert.NotNull(GameManager.SellItem(arm, hero, hero.Inventory, ally, ally.CurrentEquipment));
        Assert.NotNull(ally.CurrentEquipment.GetHelmet());
        Assert.NotNull(ally.CurrentEquipment.Owner);
      }
      GameManager.Save();
      GameManager.Load(hero.Name);
      var allyLoaded = GameManager.AlliesManager.AllAllies.ElementAt(0) as Roguelike.Tiles.LivingEntities.Ally;
      Assert.NotNull(allyLoaded.CurrentEquipment.GetHelmet());
      Assert.NotNull(allyLoaded.CurrentEquipment.Owner);
      Assert.AreEqual(allyLoaded.CurrentEquipment.Owner, allyLoaded);

      var axe = GameManager.LootGenerator.GetLootByTileName<Weapon>("axe");
      Assert.True(hero.Inventory.Add(axe));
      axe.RequiredLevel = 1;
      //Assert.NotNull(GameManager.SellItem(axe, hero, hero.Inventory, allyLoaded, allyLoaded.CurrentEquipment));

    }



    [Test]
    public void EquipmentEhancesAlly()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      var rusty = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.True(hero.Inventory.Add(rusty));

      var ally = new SkeletonSpell(hero).Ally;
      GameManager.AlliesManager.AddEntity(ally);
      var oldAttack = ally.Stats.MeleeAttack;

      Assert.NotNull(GameManager.SellItem(rusty, hero, hero.Inventory, ally, ally.CurrentEquipment, null,
        new CurrentEquipmentAddItemArg() { cek = CurrentEquipmentKind.Weapon }));
      Assert.AreEqual(ally.CurrentEquipment.GetWeapon(), rusty);
      var newAttack = ally.Stats.MeleeAttack;
      Assert.Greater(newAttack, oldAttack);
    }

    [Test]
    public void MoveRingToAllyCurrent()
    {
      CreateWorld();

      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;
      hero.Gold = 0;

      var ring = GameManager.LootGenerator.GetLootByTileName<Jewellery>("ring_magic");
      Assert.NotNull(ring);
      Assert.True(hero.Inventory.Add(ring));
      Assert.NotNull(GameManager.SellItem(ring, hero, hero.Inventory, hero, hero.CurrentEquipment, null,
         new CurrentEquipmentAddItemArg() { cek = CurrentEquipmentKind.RingLeft }));
      Assert.AreEqual(hero.CurrentEquipment.PrimaryEquipment[CurrentEquipmentKind.RingLeft], ring);

      var ally = new SkeletonSpell(hero).Ally;
      GameManager.AlliesManager.AddEntity(ally);
      Assert.NotNull(GameManager.SellItem(ring, hero, hero.CurrentEquipment, ally, ally.CurrentEquipment,
        null, new CurrentEquipmentAddItemArg() { cek = CurrentEquipmentKind.RingRight }));
      Assert.AreEqual(hero.CurrentEquipment.PrimaryEquipment[CurrentEquipmentKind.RingLeft], null);
      Assert.AreEqual(ally.CurrentEquipment.PrimaryEquipment[CurrentEquipmentKind.RingRight], ring);
    }

    [Test]
    public void WeaponCanNotBeSetInRingSlot()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.True(hero.Inventory.Add(wpn));
      Assert.Null(GameManager.SellItem(wpn, hero, hero.Inventory, hero, hero.CurrentEquipment, null,
        new CurrentEquipmentAddItemArg() { cek = CurrentEquipmentKind.RingLeft }));
      Assert.True(hero.Inventory.Contains(wpn));
    }

    [Test]
    public void WeaponCanNotBeSetInWeaponSlot()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var wpn = GameManager.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.True(hero.Inventory.Add(wpn));
      Assert.NotNull(GameManager.SellItem(wpn, hero, hero.Inventory, hero, hero.CurrentEquipment, null,
        new CurrentEquipmentAddItemArg() { cek = CurrentEquipmentKind.Weapon }));
      Assert.False(hero.Inventory.Contains(wpn));
    }


    [Test]
    public void RingFromCurrentEqToFullInv()
    {
      CreateWorld();
      var hero = GameManager.Hero as OuaDII.Tiles.LivingEntities.Hero;

      var ring = GameManager.LootGenerator.GetLootByTileName<Jewellery>("ring_magic");
      Assert.NotNull(ring);
      Assert.True(hero.Inventory.Add(ring));
      Assert.NotNull(GameManager.SellItem(ring, hero, hero.Inventory, hero, hero.CurrentEquipment,
        null, new CurrentEquipmentAddItemArg() { cek = CurrentEquipmentKind.RingRight }));
      Assert.AreEqual(hero.CurrentEquipment.PrimaryEquipment[CurrentEquipmentKind.RingRight], ring);


      for (int i = 0; i < hero.Inventory.Capacity; i++)
      {
        var wpn2 = GameManager.LootGenerator.GetLootByAsset("axe");
        Assert.True(hero.Inventory.Add(wpn2));
        Assert.True(hero.Inventory.Contains(wpn2));
      }

      //no room
      
      Assert.Null(GameManager.SellItem(ring, hero, hero.CurrentEquipment, hero, hero.Inventory));
      Assert.AreEqual(hero.CurrentEquipment.PrimaryEquipment[CurrentEquipmentKind.RingRight], ring);
    }
  }
}