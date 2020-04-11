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
      foreach (var lootSourceKind in lootSourceKinds)
      {
        var equipmentClassChances = lootingProbab.EquipmentClassChances[lootSourceKind];
        var sum = equipmentClassChances.Values().Sum();
        Assert.Greater(sum, 0);

        var lootKindChances = lootingProbab.LootKindChances[lootSourceKind];
        var sum1 = lootKindChances.Values().Sum();
        Assert.Greater(sum1, 0);
      }
    }
  }
}
