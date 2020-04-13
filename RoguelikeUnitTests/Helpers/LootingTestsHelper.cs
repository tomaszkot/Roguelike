using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests.Helpers
{
  public class LootingTestsHelper : BaseHelper
  {
    public LootingTestsHelper():base(null) 
    {
    }

    public LootingTestsHelper(TestBase test, RoguelikeGame game) : base(test, game)
    {
    }

    public List<LootKind> AssertLootKindFromEnemies(LootKind[] expectedKinds)
    {
      var res = new List<LootKind>();
      var enemies = game.GameManager.EnemiesManager.Enemies;
      Assert.GreaterOrEqual(enemies.Count, 5);
      for (int i = 0; i < enemies.Count; i++)
      {
        var li = new LootInfo(game, null);
        var en = enemies[i];
        while (en.Alive)
          en.OnPhysicalHit(game.Hero);

        var lootItems = li.GetDiff(); //game.GameManager.CurrentNode.GetTile(en.Point) as Loot;
        if (lootItems != null)
        {
          foreach (var loot in lootItems)
          {
            Assert.True(expectedKinds.Contains(loot.LootKind));
            res.Add(loot.LootKind);
          }
        }
      }

      return res;
    }

    public List<Loot> TestInteractive<T>(Action<InteractiveTile> init, int maxExpectedLootCount = 15,
      int maxExpectedUniqCount = 2) 
      where T : InteractiveTile, new()
    {
      var lootInfo = new LootInfo(game, null);

      AddThenDestroyInteractive<T>(init: init);
      var newLootItems = lootInfo.GetDiff();
      Assert.Greater(newLootItems.Count, 0);

      Assert.Less(newLootItems.Count, maxExpectedLootCount);
      var eqs = newLootItems.Where(i => i is Equipment).Cast<Equipment>().ToList();

      var uniq = eqs.Where(i => i.Class == EquipmentClass.Unique).ToList();
      Assert.Less(uniq.Count, maxExpectedUniqCount);
      //Assert.AreEqual(eq.First().Class, EquipmentClass.Unique);

      return newLootItems;
    }

    public void AddThenDestroyInteractive<T>
    (
    int numberOfTilesToTest = 50,
    Action<InteractiveTile> init = null
    ) 
    where T : InteractiveTile, new()
    {
      for (int i = 0; i < numberOfTilesToTest; i++)
      {
        var tile = AddTile<T>(game);
        if (init != null)
          init(tile);
        game.GameManager.InteractHeroWith(tile);
      }
    }
  }
}
