using Dungeons.TileContainers;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System;
using System.Drawing;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class LootingProbabilityTests : TestBase
  {
    [Test]
    public void MainTest()
    {
      var game = CreateGame(false);
      var lootingProbab = game.GameManager.LootGenerator.Probability;

      var lootSourceKinds = Enum.GetValues(typeof(LootSourceKind)).Cast<LootSourceKind>();
      //iterate chances for: Enemy, Barrel, GoldChest...
      foreach (var lootSourceKind in lootSourceKinds)
      {
        //1 check Equipment chances
        var equipmentClassChances = lootingProbab.EquipmentClassChances[lootSourceKind];
        var sum = equipmentClassChances.ValuesCopy().Values.Sum();
        Assert.Greater(sum, 0);
        //foreach (var eqc in equipmentClassChances.ValuesCopy())
        //{
        //  if (eqc.Key == EquipmentClass.Unset)
        //    continue;
        //  Assert.Greater(eqc.Value, 0);
        //}


        //2 check Loot Kind chances
        var lootKindChances = lootingProbab.LootKindChances[lootSourceKind];
        foreach (var eqc in lootKindChances.ValuesCopy())
        {
          if (eqc.Key == LootKind.Unset)
            continue;
          Assert.Greater(eqc.Value, 0);
        }
        //var sum1 = lootKindChances.Values().Sum();
        //Assert.Greater(sum1, 0);
      }
    }

    [Test]
    public void FoodTest()
    {
      //var game = CreateGame(true, 50);
      //game.GameManager.Get
      //var lootingProbab = game.GameManager.LootGenerator.Probability;

      //var lootSourceKinds = Enum.GetValues(typeof(LootSourceKind)).Cast<LootSourceKind>();
      ////iterate chances for: Enemy, Barrel, GoldChest...
      //foreach (var lootSourceKind in lootSourceKinds)
      //{
      //}
    }
  }
}
