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
      var enemies = Enemies;
      Assert.GreaterOrEqual(enemies.Count, 5);
      var li = new LootInfo(game, null);
      KillAllEnemies();

      var lootItems = li.GetDiff();
      int expectedKindsCounter = 0;
      {
        foreach (var loot in lootItems)
        {
          var exp = expectedKinds.Contains(loot.LootKind);
          //Assert.True(exp || loot is Equipment);//Bosses and Chemp throws Equipment
          if (exp)
            expectedKindsCounter++;

          res.Add(loot.LootKind);
        }
      }
      Assert.Greater(expectedKindsCounter, 0);

      return res;
    }

    public void KillAllEnemies()
    {
      var enemies = Enemies;
      for (int i = 0; i < enemies.Count; i++)
      {
        var en = enemies[i];
        KillEnemy(en);
      }
    }

    public void KillEnemy(LivingEntity en)
    {
      while (en.Alive)
        en.OnPhysicalHit(game.Hero);
    }

    public LootInfo TestInteractive<T>(Action<InteractiveTile> init,
      int tilesToCreateCount = 50,
      int maxExpectedLootCount = 15,
      int maxExpectedUniqCount = 2) 
      where T : InteractiveTile, new()
    {
      var lootInfo = new LootInfo(game, null);

      AddThenDestroyInteractive<T>(tilesToCreateCount, init);
      var newLootItems = lootInfo.GetDiff();
      Assert.Greater(newLootItems.Count, 0);

      Assert.LessOrEqual(newLootItems.Count, maxExpectedLootCount);
      var eqs = newLootItems.Where(i => i is Equipment).Cast<Equipment>().ToList();

      var uniq = eqs.Where(i => i.Class == EquipmentClass.Unique).ToList();
      Assert.LessOrEqual(uniq.Count, maxExpectedUniqCount);
      //Assert.AreEqual(eq.First().Class, EquipmentClass.Unique);

      return lootInfo;
    }

    public void AddThenDestroyInteractive<T>
    (
      int numberOfTilesToTest = 50,
      Action<InteractiveTile> init = null
    ) 
    where T : InteractiveTile, new()
    {
      var createdTiles = new List<Tile>();
      for (int i = 0; i < numberOfTilesToTest; i++)
      {
        var tile = AddTile<T>();
        if (init != null)
          init(tile);

        createdTiles.Add(tile);
      }

      for(int i=0;i<createdTiles.Count;i++)
      {
        var tile = createdTiles[i];
        var to = game.GameManager.Context.TurnOwner;
        var tac = game.GameManager.Context.TurnActionsCount;
        var ni = game.GameManager.EnemiesManager.GetEnemies().Where(e => e.State != EntityState.Idle).ToList();

        Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
        test.InteractHeroWith(tile as InteractiveTile);
      }
    }
  }
}
