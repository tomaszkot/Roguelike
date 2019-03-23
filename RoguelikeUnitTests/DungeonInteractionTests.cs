using NUnit.Framework;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class DungeonInteractionTests : TestBase
  {
    [Test]
    public void LootCollect()
    {
      var gameNode = CreateNewDungeon();
      var loot = new Loot();

      var freeTile = gameNode.GetFirstEmptyPoint().Value;
      Assert.True(gameNode.SetTile(loot, freeTile));

      Assert.True(gameNode.SetTile(Hero, freeTile));

      Assert.True(GameManager.CollectLootOnHeroPosition());
      Assert.True(Hero.Inventory.Contains(loot));
    }
  }
}
