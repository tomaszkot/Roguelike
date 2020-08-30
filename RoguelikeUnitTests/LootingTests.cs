using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Helpers;
using System;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class LootingTests : TestBaseTyped<LootingTestsHelper>
  {
    [Test]
    public void LotsOfPotionsTest()
    {
      var env = CreateTestEnv();
      //GenerationInfo.DebugInfo.EachEnemyGivesPotion = true;
      try
      {
        var lootInfo = new LootInfo(game, null);
        for (int i = 0; i < 10; i++)
        {
          var pot = env.LootGenerator.GetRandomLoot(LootKind.Potion);
          var added = game.GameManager.AddLootReward(pot, env.Game.Hero, false);
          Assert.True(added);
          var dist = pot.DistanceFrom(env.Game.Hero);
          Assert.Less(dist, 5);
          Assert.True(dist < 4 || i > 5);
        }
        var newLootItems = lootInfo.GetDiff();
        Assert.AreEqual(newLootItems.Count, 10);
      }
      catch (System.Exception)
      {
        //GenerationInfo.DebugInfo.EachEnemyGivesPotion = false;
      }
    }

    [Test]
    public void LotsOfEqTest()
    {
      var env = CreateTestEnv();
      var lootInfo = new LootInfo(game, null);
      for (int i = 0; i < 10; i++)
      {
        var pot = env.LootGenerator.GetRandomLoot(LootKind.Equipment);
        var closeEmp = env.Game.Level.GetClosestEmpty(env.Game.Hero, true);
        var set = env.Game.Level.SetTile(pot, closeEmp.Point);
        Assert.True(set);
      }
      var newLootItems = lootInfo.GetDiff();
      Assert.AreEqual(newLootItems.Count, 10);
    }

    [Test]
    public void IdentifiedPlainClassTest()
    {
      var env = CreateTestEnv();
      var wpn = env.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.AreEqual(wpn.Class, EquipmentClass.Plain);
      var price = wpn.Price;
      var damage = wpn.GetStats().GetTotalValue(EntityStatKind.Attack);

      Assert.Greater(price, 0);
      Assert.Greater(damage, 0);
      Assert.AreEqual(wpn.IsIdentified, true);

      int extraAttack = 2;
      wpn.MakeMagic(EntityStatKind.Attack, extraAttack);
      Assert.AreEqual(wpn.IsIdentified, false);
      Assert.AreEqual(wpn.Class, EquipmentClass.Magic);
      Assert.AreEqual(wpn.GetStats().GetTotalValue(EntityStatKind.Attack), damage);
      Assert.Greater(wpn.Price, price);//shall be bit bigger
      price = wpn.Price;

      wpn.Identify();
      Assert.AreEqual(wpn.IsIdentified, true);
      Assert.AreEqual(wpn.GetStats().GetTotalValue(EntityStatKind.Attack), damage + extraAttack);
      Assert.Greater(wpn.Price, price);//shall be bit bigger
    }

    [Test]
    public void KilledEnemyForEquipment()
    {
      var env = CreateTestEnv(numEnemies: 25);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1);
      env.AssertLootKindFromEnemies(new[] { LootKind.Equipment });
    }

    [Test]
    public void KilledEnemyForPotion()
    {
      var env = CreateTestEnv(numEnemies: 25);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Potion, 1);
      env.AssertLootKindFromEnemies(new[] { LootKind.Potion });
    }

    [Test]
    public void KilledEnemyForGold()
    {
      var env = CreateTestEnv();
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, 1);
      env.AssertLootKindFromEnemies(new[] { LootKind.Gold });
    }

    [Test]
    public void KilledEnemyForEqipAndGold()
    {
      var env = CreateTestEnv(numEnemies: 25);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, .5f);
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, .5f);
      var loots = env.AssertLootKindFromEnemies(new[] { LootKind.Gold, LootKind.Equipment });
      Assert.AreEqual(loots.GroupBy(i => i).Count(), 2);
    }

    [Test]
    public void KilledEnemyAtSamePlace()
    {
      var env = CreateTestEnv(numEnemies: 5);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1f);

      var enemies = game.GameManager.EnemiesManager.Enemies;
      env.KillEnemy(enemies[0]);
      var loot = env.Game.Level.GetTile(enemies[0].Point);
      Assert.NotNull(loot);
      Assert.True(env.Game.Level.SetTile(enemies[1], enemies[0].Point));

      var li = new LootInfo(game, null);
      env.KillEnemy(enemies[1]);
      var lootItems = li.GetDiff();
      Assert.AreEqual(lootItems.Count, 1);
      Assert.True(lootItems[0].DistanceFrom(loot) < 2);
    }

    [Test]
    public void KilledEnemyForEqipAndGoldMoreEq()
    {
      var env = CreateTestEnv(numEnemies: 30);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, .75f);
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, .25f);
      var loots = env.AssertLootKindFromEnemies(new[] { LootKind.Gold, LootKind.Equipment });
      Assert.AreEqual(loots.GroupBy(i => i).Count(), 2);
      var goldC = loots.Where(i => i == LootKind.Gold).Count();
      var eqC = loots.Where(i => i == LootKind.Equipment).Count();
      Assert.Greater(eqC, goldC);
    }

    [Test]
    public void Barrels()
    {
      var env = CreateTestEnv();
      var numberOfInterTiles = 100;
      var enemiesBefore = env.Game.Level.GetTiles<Enemy>();
      var newLootItems = env.TestInteractive<Barrel>(
         (InteractiveTile barrel) =>
         {
         }, numberOfInterTiles, 50, 1
        );
      var enemiesAfter = env.Game.Level.GetTiles<Enemy>();
      Assert.Greater(enemiesAfter.Count, enemiesBefore.Count);
      Assert.Greater(enemiesAfter.Count - enemiesBefore.Count, 5);
      Assert.AreEqual(enemiesAfter.Count, Game.GameManager.EnemiesManager.Enemies.Count);
    }

    [Test]
    public void PlainChests()
    {
      int mult = 1;
      var env = CreateTestEnv();
      int tilesToCreateCount = 100 * mult;
      int maxExpectedLootCount = 100;// 32 * mult;

      var lootInfo = env.TestInteractive<Chest>(
         (InteractiveTile chest) =>
         {
           (chest as Chest).ChestKind = ChestKind.Plain;
         }, tilesToCreateCount, maxExpectedLootCount

        );

      var potions = lootInfo.Get<Potion>();
      Assert.Greater(potions.Count, 8);
      Assert.Less(potions.Count, 40);

      var mushes = lootInfo.Get<Mushroom>();
      Assert.Greater(mushes.Count, 1);
      Assert.Less(potions.Count, 20);
    }

    [Test]
    public void GoldChests()
    {
      TestValuableChests(false);
    }

    [Test]
    public void GoldDeluxeChests()
    {
      TestValuableChests(true);
    }

    private void TestValuableChests(bool deluxe)
    {
      int numberOfChests = 20;
      var env = CreateTestEnv();
      var lootInfo = env.TestInteractive<Chest>(
         (InteractiveTile chest) =>
         {
           (chest as Chest).ChestKind = deluxe ? ChestKind.GoldDeluxe : ChestKind.Gold;
         }, numberOfChests, numberOfChests * 3, (int)(numberOfChests * 1.5f)

        );

      var lootItems = lootInfo.Get<Loot>();
      Assert.GreaterOrEqual(lootItems.Count, deluxe ? 60 : 40);
      //lootItems.ForEach(i=> Assert.AreEqual(i.Class, EquipmentClass.Unique));
      var eqItems = lootInfo.Get<Equipment>();
      Assert.GreaterOrEqual(eqItems.Where(i => i.Class == EquipmentClass.Unique).Count(), numberOfChests);
      var magicItems = eqItems.Where(i => i.Class == EquipmentClass.Magic).ToList();
      Assert.GreaterOrEqual(lootItems.Count, 40);
    }

    [Test]
    public void FoodTests()
    {
      var env = CreateTestEnv();
      var gm = env.Game.GameManager;

      //TODO test names
      {
        var food = new Roguelike.Tiles.Mushroom();
        food.SetMushroomKind(MushroomKind.BlueToadstool);
        Assert.AreEqual(food.tag1, "mash_BlueToadstool1");
        Assert.AreEqual(food.Kind, FoodKind.Mushroom);
      }
      {
        var food = new Roguelike.Tiles.Food();
        food.SetKind(FoodKind.Plum);
        Assert.AreEqual(food.tag1, "plum_mirabelka");
        Assert.AreEqual(food.Kind, FoodKind.Plum);
      }
      {
        var food = new Roguelike.Tiles.Food();
        food.SetKind(FoodKind.Meat);
        food.MakeRoasted();
        Assert.AreEqual(food.Name, "Roasted Meat");
        Assert.AreEqual(food.tag1, "meat_roasted");
        Assert.AreEqual(food.Kind, FoodKind.Meat);
      }

      {
        var food = new Roguelike.Tiles.Food();
        food.SetKind(FoodKind.Meat);
        Assert.AreEqual(food.Name, "Raw Meat");
        Assert.AreEqual(food.tag1, "meat_raw");
        Assert.AreEqual(food.Kind, FoodKind.Meat);
      }

      {
        var food = new Roguelike.Tiles.Food();
        food.SetKind(FoodKind.Fish);
        food.MakeRoasted();
        Assert.AreEqual(food.Name, "Roasted Fish");
        Assert.AreEqual(food.tag1, "fish_roasted");
        Assert.AreEqual(food.Kind, FoodKind.Fish);
      }

      {
        var food = new Roguelike.Tiles.Food();
        food.SetKind(FoodKind.Fish);
        Assert.AreEqual(food.Name, "Raw Fish");
        Assert.AreEqual(food.tag1, "fish_raw");
        Assert.AreEqual(food.Kind, FoodKind.Fish);
      }
      //gm.CurrentNode.set
    }

    int loopCount = 1;
    [Test]
    public void KilledEnemyGivesPotionFromTimeToTime()
    {
      for (int i = 0; i < loopCount; i++)
      {
        var env = CreateTestEnv(numEnemies: 100);
        var li = new LootInfo(game, null);
        env.KillAllEnemies();
        var lootItems = li.GetDiff();
        Assert.Greater(lootItems.Count, 0);
        var potions = lootItems.Where(j => j.LootKind == LootKind.Potion).ToList();
        Assert.Greater(potions.Count, 4);
        Assert.Less(potions.Count, 35);
      }
    }

    [Test]
    public void KilledEnemyGivesFoodFromTimeToTime()
    {
      for (int ind = 0; ind < loopCount; ind++)
      {
        var env = CreateTestEnv(numEnemies: 100);
        var enemies = env.Enemies;
        Assert.AreEqual(enemies.Count, 100);

        var li = new LootInfo(game, null);
        env.KillAllEnemies();
        var lootItems = li.GetDiff();
        Assert.Greater(lootItems.Count, 0);
        var food = lootItems.Where(j => j.LootKind == LootKind.Food).ToList();
        Assert.Greater(food.Count, 5);
        Assert.Less(food.Count, 36);

        var foodCasted = food.Cast<Food>().ToList();
        var foodTypes = foodCasted.GroupBy(f => f.Kind).ToList();
        var allKinds = Enum.GetValues(typeof(FoodKind)).Cast<FoodKind>().Where(i => i != FoodKind.Unset).ToList();
        //Assert.AreEqual(foodTypes.Count, allKinds.Count);
        foreach (var gr in foodTypes)
        {
          var count = gr.Count();
          Assert.Less(count, 10);
        }
      }
    }
        
  }
}