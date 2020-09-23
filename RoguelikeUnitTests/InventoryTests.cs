using NUnit.Framework;
using Roguelike.LootContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class InventoryTests : TestBase
  {
    //[Test]
    //public void HeroOn()
    //{
    //  var game = CreateGame();
    //  var hero = game.Hero;

    //  var wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
    //  //hero.Inventory.Add(wpn);
    //  //TODO
    //  //var ca = hero.GetCurrentValue(EntityStatKind.Attack);
    //  //var ta = hero.GetTotalValue(EntityStatKind.Attack);
    //  //Assert.AreEqual(hero.Stats.Attack, hero.Stats.Strength);
    //}

    [Test]
    public void TransferSulfur()
    {
      var game = CreateGame();
      var hero = game.Hero;
      var sulf = new Sulfur();
      hero.Inventory.Add(sulf);
      Assert.AreEqual(hero.Inventory.Items.Count, 1);

      sulf = new Sulfur();
      hero.Inventory.Add(sulf);
      Assert.AreEqual(hero.Inventory.Items.Count, 1);
      Assert.AreEqual(hero.Inventory.GetStackedCount(sulf), 2);
    }

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
      mush1.SetMushroomKind(MushroomKind.Boletus);
      hero.Inventory.Add(mush1);

      var mush2 = new Mushroom();
      mush2.SetMushroomKind(MushroomKind.RedToadstool);
      hero.Inventory.Add(mush2);
      Assert.AreEqual(hero.Inventory.Items.Count, 2);

      var mush3 = new Mushroom();
      mush3.SetMushroomKind(MushroomKind.RedToadstool);
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

      var scroll = game.GameManager.LootGenerator.LootFactory.ScrollsFactory.GetByKind(Roguelike.Spells.SpellKind.Identify);
      Assert.NotNull(scroll);
      Assert.True(hero.Inventory.Add(scroll));
      Assert.True(hero.Inventory.Contains(scroll));

      var wpn = game.GameManager.LootGenerator.GetRandomEquipment(EquipmentKind.Weapon, 1);
      Assert.IsFalse(wpn.GetMagicStats().Any());
      wpn.MakeMagic();
      Assert.False(wpn.GetMagicStats().Any());

      //identify
      hero.Identify(wpn);
      Assert.False(hero.Inventory.Contains(scroll));
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
      Assert.NotNull(game.GameManager.SellItem(loot, hero, hero.Inventory, merch, merch.Inventory));
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
      Assert.AreEqual(hero.Crafting.Recipes.ItemsCount, 1);

      var loot2 = game.GameManager.LootGenerator.GetLootByAsset("craft_one_eq");
      PutEqOnLevelAndCollectIt(loot2);
      Assert.AreEqual(hero.Crafting.Recipes.ItemsCount, 1);

      Assert.AreEqual(hero.Crafting.Recipes.GetStackedCount(loot2 as StackedLoot), 2);

      var loot3 = game.GameManager.LootGenerator.GetLootByAsset("craft_three_gems");
      PutEqOnLevelAndCollectIt(loot3);
      Assert.AreEqual(hero.Crafting.Recipes.ItemsCount, 2);
      Assert.AreEqual(hero.Crafting.Recipes.GetStackedCount(loot2 as StackedLoot), 2);
      Assert.AreEqual(hero.Crafting.Recipes.GetStackedCount(loot3 as StackedLoot), 1);
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

      var wpn1 = game.GameManager.LootGenerator.GetLootByAsset("rusty_sword") as Weapon;
      PutEqOnLevelAndCollectIt(wpn1);

      var heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], wpn1);

      Assert.False(hero.Inventory.Contains(wpn1));

      var wpn2 = game.GameManager.LootGenerator.GetLootByAsset("gladius") as Weapon;
      PutEqOnLevelAndCollectIt(wpn2);

      heroEq = hero.GetActiveEquipment();
      Assert.AreEqual(heroEq[CurrentEquipmentKind.Weapon], wpn2);
      Assert.False(hero.Inventory.Contains(wpn2));

      Assert.True(hero.Inventory.Contains(wpn1));
    }
  }
}