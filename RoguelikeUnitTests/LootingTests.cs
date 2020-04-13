using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
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
    public void KilledEnemyForEquipment()
    {
      var env = CreateTestEnv(numEnemies: 25);
      env.GameManager.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1);
      AssertLootKind(new[] { LootKind.Equipment });
    }

    [Test]
    public void KilledEnemyForGold()
    {
      var env = CreateTestEnv();
      env.GameManager.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, 1);
      AssertLootKind(new[] { LootKind.Gold });
    }

    [Test]
    public void KilledEnemyForEqipAndGold()
    {
      var env = CreateTestEnv(numEnemies: 25);
      env.GameManager.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, .5f);
      env.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, .5f);
      var loots = AssertLootKind(new[] { LootKind.Gold, LootKind.Equipment });
      Assert.AreEqual(loots.GroupBy(i=>i).Count(), 2);
    }

    [Test]
    public void KilledEnemyForEqipAndGoldMoreEq()
    {
      var env = CreateTestEnv();
      env.GameManager.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, .8f);
      env.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, .2f);
      var loots = AssertLootKind(new[] { LootKind.Gold, LootKind.Equipment });
      Assert.AreEqual(loots.GroupBy(i => i).Count(), 2);
      var goldC = loots.Where(i => i == LootKind.Gold).Count();
      var eqC = loots.Where(i => i == LootKind.Equipment).Count();
      Assert.Greater(eqC, goldC);
    }

    [Test]
    public void Barrels()
    {
    }

    [Test]
    public void PlainChests()
    {
      var env = CreateTestEnv();
      var lootPrev = game.Level.GetTiles<Loot>();

      Helper.AddThenDestroyInteractive<Chest>(init : (InteractiveTile chest) => {
        (chest as Chest).ChestKind = ChestKind.Plain;
      });

      var lootAfter = game.Level.GetTiles<Loot>();
      Assert.Greater(lootAfter.Count, lootPrev.Count);
      var newLootItems = lootAfter.Except(lootPrev).ToList();

      Assert.Greater(newLootItems.Count, 0);
      Assert.Less(newLootItems.Count, 15);
      var eqs = newLootItems.Where(i => i is Equipment).Cast<Equipment>().ToList();

      var uniq = eqs.Where(i => i.Class == EquipmentClass.Unique).ToList();
      Assert.Less(uniq.Count, 2);
      //Assert.AreEqual(eq.First().Class, EquipmentClass.Unique);
    }

    [Test]
    public void GoldChests()
    {
      var env = CreateTestEnv();
      var chest = env.AddTile<Chest>(game);
      chest.ChestKind = ChestKind.Gold;
      var lootInfo = new LootInfo(game, chest);

      Assert.NotNull(lootInfo.newLoot);

      var eq = lootInfo.newLoot.Where(i => i is Equipment).Cast<Equipment>().ToList();
      Assert.AreEqual(eq.First().Class, EquipmentClass.Unique);
    }

    
  }
}