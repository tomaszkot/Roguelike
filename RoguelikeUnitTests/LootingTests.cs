using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeUnitTests
{

  [TestFixture]
  class LootingTests : TestBaseTyped<LootingTestsHelper>
  {

    [Test]
    public void GemsLevel()
    {
      var env = CreateTestEnv();
      try
      {
        var lootInfo = new LootInfo(game, null);
        ILootSource lootSrc = ActiveEnemies.First();
        for (int i = 0; i < 10; i++)
        {
          var loot = env.LootGenerator.GetRandomLoot(LootKind.Gem, 1) as Gem;
          Assert.AreEqual(loot.EnchanterSize, EnchanterSize.Small);
        }
      }
      catch (System.Exception)
      {
        //GenerationInfo.DebugInfo.EachEnemyGivesPotion = false;
      }
    }

    [Test]
    public void Sounds()
    {
      var barrel = new Barrel();
      Assert.AreEqual(barrel.DestroySound, "barrel_broken");

      barrel.BarrelKind = BarrelKind.PileOfSkulls;
      Assert.AreEqual(barrel.DestroySound, "bones_fall");

    }


    [Test]
    public void ScrollFromTagName()
    {
      var kind = Scroll.DiscoverKindFromName("fire_ball_scroll");
      Assert.AreEqual(kind, SpellKind.FireBall);
    }
    
    [Test]
    public void HunterTrophyProps()
    {
      var env = CreateTestEnv();
      var trophyBig = env.GameManager.LootGenerator.GetLootByAsset("big_claw") as HunterTrophy;
      var siBig = trophyBig.GetStatIncrease(EquipmentKind.Weapon);
      Assert.AreEqual(trophyBig.Name, "Big Claw");

      var trophySmall = env.GameManager.LootGenerator.GetLootByAsset("small_claw") as HunterTrophy;
      var siSmall = trophySmall.GetStatIncrease(EquipmentKind.Weapon);
      Assert.AreEqual(trophySmall.Name, "Small Claw");

      Assert.Greater(siBig, siSmall);
      Assert.Greater(trophyBig.Price, trophySmall.Price);
    }
    
    
    [Test]
    public void GemsStats()
    {
      //var env = CreateTestEnv();
      var gem = new Gem(GemKind.Ruby);
      var statInfo = gem.GetLootStatInfo(null);
      Assert.AreEqual(statInfo.Count(), 3);
      foreach (var stat in statInfo)
      {
        Assert.AreNotEqual(stat.Kind, LootStatKind.Unset);
      }

      gem = new Gem(GemKind.Amber);
      gem.EnchanterSize = EnchanterSize.Big;
      Assert.AreEqual(gem.Name, "Big Amber");
      statInfo = gem.GetLootStatInfo(null);
      Assert.AreEqual(statInfo.Count(), 3);
      foreach (var stat in statInfo)
      {
        Assert.AreNotEqual(stat.Kind, LootStatKind.Unset);
      }
    }

    [Test]
    public void LotsOfPotionsTest()
    {
      var des = EquipmentKind.Weapon.ToDescription();
      var env = CreateTestEnv();
      //GenerationInfo.DebugInfo.EachEnemyGivesPotion = true;
      try
      {
        var lootInfo = new LootInfo(game, null);
        ILootSource lootSrc = ActiveEnemies.First();//env.Game.Hero
        for (int i = 0; i < 10; i++)
        {
          var pot = env.LootGenerator.GetRandomLoot(LootKind.Potion, 1);
          var added = game.GameManager.AddLootReward(pot, lootSrc, false);
          Assert.True(added);
          var dist = pot.DistanceFrom(lootSrc.GetPoint());
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
        var pot = env.LootGenerator.GetRandomLoot(LootKind.Equipment, 1);
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

      var enemies = AllEnemies;
      enemies.ForEach(i => i.PowerKind = EnemyPowerKind.Plain);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Potion, 1);
      var lootItems = env.AssertLootFromEnemies(new[] { LootKind.Potion }).Cast<Potion>().ToList();
      Assert.True(lootItems.Any(i => i.Kind == PotionKind.Health));
      Assert.True(lootItems.Any(i => i.Kind == PotionKind.Mana));
      Assert.True(lootItems.Any(i => i.Kind == PotionKind.Poison));
      Assert.False(lootItems.Any(i => i.Kind == PotionKind.Special));//these are rare
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
    public void KilledEnemyForTinyTrophy()
    {
      var env = CreateTestEnv();
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.HunterTrophy, 1);
      env.AssertLootKindFromEnemies(new[] { LootKind.HunterTrophy });
    }

    [Test]
    public void KilledEnemyForGems()
    {
      var env = CreateTestEnv();
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gem, 1);
      env.AssertLootKindFromEnemies(new[] { LootKind.Gem });

    }

    [Test]
    public void KilledEnemyForScrolls()
    {
      var env = CreateTestEnv(true, 100);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Scroll, 1);
      var scrolls = env.AssertLootFromEnemies(new[] { LootKind.Scroll }).Cast<Scroll>().ToList();
      Assert.Less(scrolls.Count, 150);
      var typesGrouped = scrolls.GroupBy(f => f.Kind).ToList();
      Assert.GreaterOrEqual(typesGrouped.Count, 3);//TODO support more scrolls!
      var identCount = typesGrouped.Where(i => i.Key == SpellKind.Identify).First().Count();
      
      foreach (var gr in typesGrouped)
      {
        var count = gr.Count();
        Assert.Less(count, 90);
        int min = gr.Key == SpellKind.Portal ? 7 : 10;
        Assert.Greater(count, min);
        if (gr.Key != SpellKind.Identify)
        {
          Assert.Greater(identCount, count * 1.15);
        }
        if (gr.Key != SpellKind.Portal)
        {
          Assert.Less(identCount, count * 1.8);
        }
      }
            
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

      var enemies = game.GameManager.EnemiesManager.AllEntities;
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
      int multiplicator = 2;
      var numberOfInterTiles = 100 * multiplicator;
      var enemiesBefore = env.Game.Level.GetTiles<Enemy>();
      //var magicDusts = env.Game.Level.GetTiles<MagicDust>();
      var newLootItems = env.TestInteractive<Barrel>(
         (InteractiveTile barrel) =>
         {
         }, numberOfInterTiles, 50* multiplicator, 1
        );
      var enemiesAfter = env.Game.Level.GetTiles<Enemy>();
      Assert.Greater(enemiesAfter.Count, enemiesBefore.Count);
      Assert.Greater(enemiesAfter.Count - enemiesBefore.Count, 5* multiplicator);
      Assert.AreEqual(enemiesAfter.Count, game.GameManager.EnemiesManager.AllEntities.Count);

      //scrolls
      var scrolls = newLootItems.Get<Scroll>();
      Assert.Greater(scrolls.Count, 0);
      Assert.Less(scrolls.Count, 10);
      var typesGrouped = scrolls.GroupBy(f => f.Kind).ToList();
      var ident = typesGrouped.Where(i => i.Key == SpellKind.Identify).FirstOrDefault();
      Assert.NotNull(ident);
      var identCount = ident.Count();
      Assert.Less(identCount, 10);
      Assert.Greater(identCount, 0);

      var magicDusts = newLootItems.Get<MagicDust>();
      Assert.Greater(magicDusts.Count, 0);
      Assert.Less(magicDusts.Count, 10);
    }

    [Test]
    public void PlainChests()
    {
      int mult = 1;
      var env = CreateTestEnv();
      int tilesToCreateCount = 100 * mult;
      int maxExpectedLootCount = 200;//chest gives 2 items

      var lootInfo = env.TestInteractive<Chest>(
         (InteractiveTile chest) =>
         {
           (chest as Chest).ChestKind = ChestKind.Plain;
         }, tilesToCreateCount, maxExpectedLootCount

        );

      var potions = lootInfo.Get<Potion>();
      Assert.Greater(potions.Count, 8);
      Assert.Less(potions.Count, 43);

      var mushes = lootInfo.Get<Mushroom>();
      Assert.Greater(mushes.Count, 1);
      Assert.Less(mushes.Count, 20);

      //scrolls
      var scrolls = lootInfo.Get<Scroll>();
      Assert.Greater(scrolls.Count, 0);
      float maxScrolls = 35;
      Assert.Less(scrolls.Count, maxScrolls);
      var typesGrouped = scrolls.GroupBy(f => f.Kind).ToList();
      var ident = typesGrouped.Where(i => i.Key == SpellKind.Identify).FirstOrDefault();
      Assert.NotNull(ident);
      var identCount = ident.Count();
      Assert.Less(identCount, maxScrolls/2);
      Assert.Greater(identCount, maxScrolls/5);
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
        Assert.AreEqual(food.StatKind, EntityStatKind.Mana);
      }
      {
        var food = new Roguelike.Tiles.Mushroom();
        food.SetMushroomKind(MushroomKind.RedToadstool);
        Assert.AreEqual(food.Kind, FoodKind.Mushroom);
        Assert.AreEqual(food.StatKind, EntityStatKind.Health);
      }
      {
        var food = new Roguelike.Tiles.Mushroom();
        food.SetMushroomKind(MushroomKind.Boletus);
        Assert.AreEqual(food.Kind, FoodKind.Mushroom);
        Assert.AreEqual(food.StatKind, EntityStatKind.Health);
      }
      {
        var food = new Roguelike.Tiles.Food();
        food.SetKind(FoodKind.Plum);
        Assert.AreEqual(food.tag1, "plum_mirabelka");
        Assert.AreEqual(food.Kind, FoodKind.Plum);
        Assert.AreEqual(food.StatKind, EntityStatKind.Health);
      }
      {
        var food = new Roguelike.Tiles.Food();
        food.SetKind(FoodKind.Meat);
        food.MakeRoasted();
        Assert.AreEqual(food.Name, "Roasted Meat");
        Assert.AreEqual(food.tag1, "meat_roasted");
        Assert.AreEqual(food.Kind, FoodKind.Meat);
        Assert.AreEqual(food.StatKind, EntityStatKind.Health);
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
        Assert.Greater(lootItems.Count, 15);
        var potions = lootItems.Where(j => j.LootKind == LootKind.Potion).ToList();
        Assert.Greater(potions.Count, 4);
        Assert.Less(potions.Count, 20);
      }
    }

    [Test]
    public void KilledEnemyGivesFoodOfAllKinds()
    {
      var env = CreateTestEnv(numEnemies: 100);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Food, 1);

      var enemies = env.Enemies;
      Assert.AreEqual(enemies.Count, 100);

      var li = new LootInfo(game, null);
      env.KillAllEnemies();
      var lootItems = li.GetDiff();
      Assert.Greater(lootItems.Count, 0);
      var foods = lootItems.Where(j => j.LootKind == LootKind.Food).Cast<Food>().ToList();
      Assert.Greater(foods.Count, 80);
      var kinds = Enum.GetValues(typeof(FoodKind)).Cast<FoodKind>().Where(i=> i != FoodKind.Unset).ToList();
      foreach (var kind in kinds)
      {
        Assert.IsTrue(foods.Any(i=> i.Kind == kind));
      }
    }

    [Test]
    public void KilledEnemyGivesFoodFromTimeToTime()
    {
      for (int ind = 0; ind < loopCount; ind++)
      {
        var lootItems = KillEnemies(100);
        var food = lootItems.Get<Food>();
        Assert.Greater(food.Count, 0);
        Assert.Less(food.Count, 36);

        var foodCasted = food.Cast<Food>().ToList();
        var foodTypes = foodCasted.GroupBy(f => f.Kind).ToList();
        var allKinds = Enum.GetValues(typeof(FoodKind)).Cast<FoodKind>().Where(i => i != FoodKind.Unset).ToList();
        foreach (var gr in foodTypes)
        {
          var count = gr.Count();
          Assert.Less(count, 10);
        }
      }
    }

    private LootInfo KillEnemies(int enemiesCount)
    {
      var env = CreateTestEnv(numEnemies: enemiesCount);
      var enemies = env.Enemies;
      Assert.AreEqual(enemies.Count, enemiesCount);

      var li = new LootInfo(game, null);
      env.KillAllEnemies();
      var lootItems = li.GetDiff();
      Assert.Greater(lootItems.Count, 0);
      return li;
    }

    /////

    [Test]
    public void KilledEnemyGivesIdentifScroll()
    {
      for (int ind = 0; ind < loopCount; ind++)
      {
        var lootItems = KillEnemies(100);
        var scrolls = lootItems.Get<Scroll>();
        Assert.Greater(scrolls.Count, 0);
        Assert.Less(scrolls.Count, 10);
        var typesGrouped = scrolls.GroupBy(f => f.Kind).ToList();
        var identCount = typesGrouped.Where(i => i.Key == SpellKind.Identify).First().Count();
        Assert.Less(identCount, 10);
        Assert.Greater(identCount, 0);
      }
    }

    /////////////////////////////////////////////////////////
    [Test]
    public void KilledEnemyLevelAffectsTinyTrophy()
    {
      KilledEnemyLevelAffectsEnchanter(LootKind.HunterTrophy, 6, new[] { EnchanterSize.Medium, EnchanterSize.Big });
    }
    /////////////////////////////////////////////////////////
    [Test]
    [Repeat(2)]
    [TestCase(1)]
    [TestCase(6)]
    public void KilledEnemyLevelAffectsGem(int le)
    {
      EnchanterSize[] expectedSizes = new[] { EnchanterSize.Small };
      if (le == 6)
        expectedSizes = new[] { EnchanterSize.Medium, EnchanterSize.Big };
      KilledEnemyLevelAffectsEnchanter(LootKind.Gem, le, expectedSizes);
    }
    /////////////////////////////////////////////////////////
    public void KilledEnemyLevelAffectsEnchanter(LootKind kind, int enemyLevel, EnchanterSize[] expectedSizes)
    {
      var env = CreateTestEnv(true, 10, 2);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, kind, 1);

      var enemies = AllEnemies;
      Assert.GreaterOrEqual(enemies.Count, 5);
      enemies.ForEach(i => i.SetLevel(enemyLevel));
      var li = new LootInfo(game, null);
      env.KillAllEnemies();

      var res = new List<LootKind>();
      var lootItems = li.GetDiff();
      int expectedKindsCounter = 0;
      {
        foreach (var loot in lootItems)
        {
          var expected = kind == loot.LootKind;

          if (expected)
          {
            var ench = loot as Enchanter;
            Assert.True(expectedSizes.Contains(ench.EnchanterSize));
            expectedKindsCounter++;
          }

          res.Add(loot.LootKind);
        }
      }
      Assert.Greater(expectedKindsCounter, 0);
    }



  }
}