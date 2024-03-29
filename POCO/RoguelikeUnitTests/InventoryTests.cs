﻿using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.LootContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class InventoryTests : TestBase
  {
    
    //[Test]
    //public void TransferSulfur()
    //{
    //  var game = CreateGame();
    //  var hero = game.Hero;
    //  var sulf = new Sulfur();
    //  hero.Inventory.Add(sulf);
    //  Assert.AreEqual(hero.Inventory.Items.Count, 1);

    //  sulf = new Sulfur();
    //  hero.Inventory.Add(sulf);
    //  Assert.AreEqual(hero.Inventory.Items.Count, 1);
    //  Assert.AreEqual(hero.Inventory.GetStackedCount(sulf), 2);
    //}

    [Test]
    public void TransferStacked()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var gold = new Gold(5);

      hero.Inventory.Add(gold);
      Assert.AreEqual(hero.Inventory.Items.Count, 1);
      var goldInv = hero.Inventory.Items[0] as Gold;
      Assert.AreEqual(goldInv.Count, 5);

      var gold1 = new Gold(15);
      hero.Inventory.Add(gold1);
      Assert.AreEqual(hero.Inventory.Items.Count, 1);
      Assert.AreEqual(goldInv.Count, 20);

      game.GameManager.Save();
      game.GameManager.Load(hero.Name);

      var heroLoaded = game.GameManager.Hero;
      Assert.AreEqual(heroLoaded.Inventory.Items.Count, 1);
      goldInv = heroLoaded.Inventory.Items[0] as Gold;
      Assert.AreEqual(goldInv.Count, 20);
    }

    [Test]
    public void SaveLoadNonStacked()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var wpn = game.GameManager.LootGenerator.GetLootByAsset("rusty_sword");
      Assert.NotNull(wpn);
      hero.Inventory.Add(wpn);
      Assert.AreEqual(hero.Inventory.Items.Count, 1);

      var wpn1 = game.GameManager.LootGenerator.GetLootByAsset("rusty_sword");
      hero.Inventory.Add(wpn1);
      Assert.AreEqual(hero.Inventory.Items.Count, 2);
      Assert.AreEqual(hero.Inventory.Items[0].tag1, "rusty_sword");

      game.GameManager.Save();
      game.GameManager.Load(hero.Name);

      var heroLoaded = game.GameManager.Hero;
      Assert.AreEqual(heroLoaded.Inventory.Items.Count, 2);
      Assert.AreEqual(heroLoaded.Inventory.Items[0].tag1, "rusty_sword");
    }

    [Test]
    public void SaveLoadStacked()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var mush1 = new Mushroom();
      mush1.MushroomKind = MushroomKind.Boletus;
      hero.Inventory.Add(mush1);

      var mush2 = new Mushroom();
      mush2.MushroomKind = MushroomKind.RedToadstool;
      hero.Inventory.Add(mush2);
      Assert.AreEqual(hero.Inventory.Items.Count, 2);

      var mush3 = new Mushroom();
      mush3.MushroomKind = MushroomKind.RedToadstool;
      hero.Inventory.Add(mush3);
      Assert.AreEqual(hero.Inventory.Items.Count, 2);

      Assert.AreEqual(hero.Inventory.GetStackedCount(mush1), 1);
      Assert.AreEqual(hero.Inventory.GetStackedCount(mush2), 2);

      var plant1 = new Plant();
      plant1.SetKind(PlantKind.Thistle);
      hero.Inventory.Add(plant1);
      Assert.AreEqual(hero.Inventory.Items.Count, 3);

      var plant2 = new Plant();
      plant2.SetKind(PlantKind.Thistle);
      hero.Inventory.Add(plant2);
      Assert.AreEqual(hero.Inventory.Items.Count, 3);
      Assert.AreEqual(hero.Inventory.GetStackedCount(plant2), 2);

      game.GameManager.Save();
      game.GameManager.Load(hero.Name);

      var heroLoaded = game.GameManager.Hero;
      Assert.AreEqual(heroLoaded.Inventory.GetStackedCount(mush1), 1);
      Assert.AreEqual(heroLoaded.Inventory.GetStackedCount(mush2), 2);
      Assert.AreEqual(heroLoaded.Inventory.GetStackedCount(plant2), 2);
    }

    [Test]
    public void ScrollIdentifyTest()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var scroll = game.GameManager.LootGenerator.LootFactory.ScrollsFactory.GetByKind(Roguelike.Spells.SpellKind.Identify) as Scroll;
      Assert.NotNull(scroll);
      Assert.True(hero.Inventory.Add(scroll));
      Assert.True(hero.Inventory.Contains(scroll));
      int count = scroll.Count;

      var wpn = game.GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Weapon, 1, null);
      Assert.IsFalse(wpn.GetMagicStats().Any());
      wpn.MakeMagic();
      Assert.False(wpn.GetMagicStats().Any());

      //identify
      hero.Identify(wpn, game.GameManager.SpellManager);
      Assert.AreEqual(scroll.Count, count - 1);
      Assert.True(wpn.GetMagicStats().Any());

    }

    [Test]
    public void ScrollTests()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var loot = game.GameManager.LootGenerator.GetRandomLoot(LootKind.Scroll, 1);
      Assert.NotNull(loot.tag1);
      Assert.True(hero.Inventory.Add(loot));
    }

    [Test]
    public void PriceTests()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var loot = game.GameManager.LootGenerator.GetRandomLoot(LootKind.Scroll, 1);
      Assert.True(hero.Inventory.Add(loot));
      var priceInHeroInv = hero.GetPrice(loot);
      Assert.AreEqual(priceInHeroInv, loot.Price);
      var merch = game.GameManager.CurrentNode.GetTiles<Merchant>().First();

      var merchGold = merch.Gold;
      Assert.NotNull(game.GameManager.SellItem(loot, hero, merch));
      Assert.True(merch.Inventory.Contains(loot));
      Assert.Less(merch.Gold, merchGold);

      var priceInMerchInv = merch.GetPrice(loot);
      Assert.Greater(priceInMerchInv, priceInHeroInv);
    }

    [Test]
    public void CountOfStackedRecipe()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.AreEqual(hero.Inventory.ItemsCount, 0);

      var loot1 = game.GameManager.LootGenerator.GetLootByAsset("craft_one_eq");
      PutEqOnLevelAndCollectIt(loot1);
      Assert.AreEqual(hero.Crafting.Recipes.Inventory.ItemsCount, 1);

      var loot2 = game.GameManager.LootGenerator.GetLootByAsset("craft_one_eq");
      PutEqOnLevelAndCollectIt(loot2);
      Assert.AreEqual(hero.Crafting.Recipes.Inventory.ItemsCount, 1);

      Assert.AreEqual(hero.Crafting.Recipes.Inventory.GetStackedCount(loot2 as StackedLoot), 2);

      var loot3 = game.GameManager.LootGenerator.GetLootByAsset("craft_three_gems");
      PutEqOnLevelAndCollectIt(loot3);
      Assert.AreEqual(hero.Crafting.Recipes.Inventory.ItemsCount, 2);
      Assert.AreEqual(hero.Crafting.Recipes.Inventory.GetStackedCount(loot2 as StackedLoot), 2);
      Assert.AreEqual(hero.Crafting.Recipes.Inventory.GetStackedCount(loot3 as StackedLoot), 1);
    }

    [Test]
    public void CountOfStackedTrophies()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.AreEqual(hero.Inventory.ItemsCount, 0);

      var loot1 = game.GameManager.LootGenerator.GetLootByAsset("big_claw") as StackedLoot;
      Assert.NotNull(loot1);
      PutEqOnLevelAndCollectIt(loot1);
      Assert.AreEqual(hero.Inventory.ItemsCount, 1);

      var loot2 = game.GameManager.LootGenerator.GetLootByAsset("big_claw");
      PutEqOnLevelAndCollectIt(loot2);
      Assert.AreEqual(hero.Inventory.ItemsCount, 1);
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot2 as StackedLoot), 2);

      var loot3 = game.GameManager.LootGenerator.GetLootByAsset("big_fang");
      PutEqOnLevelAndCollectIt(loot3);
      Assert.AreEqual(hero.Inventory.ItemsCount, 2);
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot2 as StackedLoot), 2);
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot3 as StackedLoot), 1);
    }

    [Test]
    public void CountOfStackedGems()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.AreEqual(hero.Inventory.ItemsCount, 0);

      var lootBase = game.GameManager.LootGenerator.GetLootByAsset("diamond_big");
      Assert.NotNull(lootBase);
      var loot1 = lootBase as StackedLoot;
      Assert.NotNull(loot1);
      PutEqOnLevelAndCollectIt(loot1);
      Assert.AreEqual(hero.Inventory.ItemsCount, 1);

      var loot2 = game.GameManager.LootGenerator.GetLootByAsset("diamond_big");
      PutEqOnLevelAndCollectIt(loot2);
      Assert.AreEqual(hero.Inventory.ItemsCount, 1);
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot2 as StackedLoot), 2);

      var loot3 = game.GameManager.LootGenerator.GetLootByAsset("emerald_medium");
      PutEqOnLevelAndCollectIt(loot3);
      Assert.AreEqual(hero.Inventory.ItemsCount, 2);
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot2 as StackedLoot), 2);
      Assert.AreEqual(hero.Inventory.GetStackedCount(loot3 as StackedLoot), 1);
    }

    [Test]
    public void EquipmentPutOnHero()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.Level = 3;
      hero.Stats[EntityStatKind.Strength].Nominal = 20;

      var wpn1 = game.GameManager.LootGenerator.GetLootByAsset("rusty_sword") as Weapon;
      PutEqOnLevelAndCollectIt(wpn1);

      var heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], wpn1);

      Assert.False(hero.Inventory.Contains(wpn1));

      var wpn2 = game.GameManager.LootGenerator.GetLootByAsset("axe") as Weapon;
      Assert.True(wpn2.IsBetter(wpn1));
      PutEqOnLevelAndCollectIt(wpn2);

      heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], wpn2);
      Assert.False(hero.Inventory.Contains(wpn2));

      Assert.True(hero.Inventory.Contains(wpn1));
    }

    [Test]
    public void EquipmentPrimaryForCap()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.Level = 3;
      hero.Stats[EntityStatKind.Strength].Nominal = 20;

      var eq1 = game.GameManager.LootGenerator.GetLootByAsset("cap") as Equipment;
      PutEqOnLevelAndCollectIt(eq1);

      var heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Helmet], eq1);
      Assert.AreEqual(hero.CurrentEquipment.PrimaryEquipment[CurrentEquipmentKind.Helmet], eq1);
            
      Assert.True(Game.GameManager.Hero.MoveEquipmentCurrent2Inv(eq1, eq1.EquipmentKind, CurrentEquipmentPosition.Unset));
      
      Game.GameManager.Hero.CurrentEquipment.SwapActiveWeaponSet();
      
      //try to chaet
      Game.GameManager.Hero.CurrentEquipment.SetEquipment(eq1, CurrentEquipmentKind.Unset, false);

      Assert.AreEqual(heroEq[CurrentEquipmentKind.Helmet], eq1);
      Assert.AreEqual(hero.CurrentEquipment.PrimaryEquipment[CurrentEquipmentKind.Helmet], eq1);

    }

    [Test]
    public void EquipmentPutOnHeroRevert()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var wpn1 = game.GameManager.LootGenerator.GetLootByAsset("rusty_sword") as Weapon;
      PutEqOnLevelAndCollectIt(wpn1);
      var heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], wpn1);

      var wpn2 = game.GameManager.LootGenerator.GetLootByAsset("sickle") as Weapon;
      PutEqOnLevelAndCollectIt(wpn2);
      Assert.True(hero.Inventory.Contains(wpn2));

      wpn2.RequiredLevel = 100;

      //shall fail  - level
      Assert.Null(Game.GameManager.SellItem(wpn2, hero, hero.Inventory, hero, hero.CurrentEquipment));
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], wpn1);
      Assert.True(hero.Inventory.Contains(wpn2));
    }

    [Test]
    public void EquipmentFromCurrentEqToFullInv()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var wpn1 = game.GameManager.LootGenerator.GetLootByAsset("rusty_sword") as Weapon;
      PutEqOnLevelAndCollectIt(wpn1);
      var heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], wpn1);

      for (int i = 0; i < hero.Inventory.Capacity; i++)
      {
        var wpn2 = game.GameManager.LootGenerator.GetLootByAsset("sickle") as Weapon;
        PutEqOnLevelAndCollectIt(wpn2);
        Assert.True(hero.Inventory.Contains(wpn2));
      }

      var wpnFail = game.GameManager.LootGenerator.GetLootByAsset("sickle") as Weapon;
      PutEqOnLevelAndCollectIt(wpnFail);
      Assert.False(hero.Inventory.Contains(wpnFail));

      //shall fail = not supported so far
      Assert.Null(Game.GameManager.SellItem(wpn1, hero, hero.CurrentEquipment, hero, hero.Inventory));
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], wpn1);
      //Assert.True(hero.Inventory.Contains(wpn2));
    }

    [Test]
    public void SpareEquipmentTest()
    {
      var game = CreateGame();
      var hero = game.Hero;
      hero.Level = 2;

      Assert.AreEqual(hero.CurrentEquipment.GetActiveWeaponSet(), ActiveWeaponSet.Primary);

      var wpn1 = game.GameManager.LootGenerator.GetLootByAsset("rusty_sword") as Weapon;
      PutEqOnLevelAndCollectIt(wpn1);
      var heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], wpn1);

      hero.CurrentEquipment.SwapActiveWeaponSet();

      heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], null);

      var wpn2 = game.GameManager.LootGenerator.GetLootByAsset("sickle") as Weapon;
      Assert.True(hero.CanUseEquipment(wpn2, true));
      PutEqOnLevelAndCollectIt(wpn2);
      heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], wpn2);
      Assert.AreEqual(hero.CurrentEquipment.GetActiveWeaponSet(), ActiveWeaponSet.Secondary);

      Game.GameManager.Save();
      Game.GameManager.Load(hero.Name);
      hero = game.Hero;
      Assert.AreEqual(hero.CurrentEquipment.GetActiveWeaponSet(), ActiveWeaponSet.Secondary);
      heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon].Name, wpn2.name);
    }

  }
}