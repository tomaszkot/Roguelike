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
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1);
      env.AssertLootKindFromEnemies(new[] { LootKind.Equipment });
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
      Assert.AreEqual(loots.GroupBy(i=>i).Count(), 2);
    }

    [Test]
    public void KilledEnemyForEqipAndGoldMoreEq()
    {
      var env = CreateTestEnv();
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, .8f);
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, .2f);
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
      var newLootItems = env.TestInteractive<Barrel>(
         (InteractiveTile chest) => {
         }
        );
    }

    [Test]
    public void PlainChests()
    {
      var env = CreateTestEnv();
      var newLootItems = env.TestInteractive<Chest>(
         (InteractiveTile chest) => {
           (chest as Chest).ChestKind = ChestKind.Plain;
         }
        );
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