﻿using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeUnitTests.Helpers
{
  public class LootingTestsHelper : BaseHelper
  {
    public LootingTestsHelper() : base(null)
    {
    }
    public LootingTestsHelper(TestBase test, RoguelikeGame game) : base(test, game)
    {
    }

    public List<LootKind> AssertLootKindFromEnemies(LootKind[] expectedKinds, bool lootKindMustMach = true)
    {
      var res = AssertLootFromEnemies(expectedKinds, lootKindMustMach);

      return res.Select(i => i.LootKind).ToList();
    }

    public List<Loot> AssertLootFromEnemies(LootKind[] expectedKinds, bool lootKindMustMach = true)
    {
      var res = new List<Loot>();
      var enemies = game.GameManager.EnemiesManager.AllEntities;
      Assert.GreaterOrEqual(enemies.Count, 5);
      var li = new LootInfo(game, null);
      KillAllEnemies();

      var lootItems = li.GetDiff();
      int expectedKindsCounter = 0;
      
      foreach (var loot in lootItems)
      {
        var exp = expectedKinds.Contains(loot.LootKind);
        //Assert.True(exp || loot is Equipment);//Bosses and Chemp throws Equipment -> fixed by ForEach(i => i.PowerKind = EnemyPowerKind.Plain)
        if (exp)
        {
          expectedKindsCounter++;
          res.Add(loot);
        }
        else if(lootKindMustMach && loot.LootKind != LootKind.Food)//enemies might thow meat
        {
          var en = loot.Source as Enemy;
          Assert.True(en.PowerKind != EnemyPowerKind.Plain);
        }
        Assert.True(!string.IsNullOrEmpty(loot.tag1));
      }
      
      Assert.Greater(expectedKindsCounter, 0);

      return res;
    }

    public void KillAllEnemies()
    {
      var enemies = game.GameManager.EnemiesManager.AllEntities;
      for (int i = 0; i < enemies.Count; i++)
      {
        var en = enemies[i];
        KillEnemy(en);
      }
    }

    public void KillEnemy(LivingEntity en)
    {
      Assert.True(en.Alive);
      while (en.Alive)
        en.OnMeleeHitBy(game.Hero);

      game.GameManager.EnemiesManager.RemoveDead();
      game.GameManager.AnimalsManager.RemoveDead();
    }

    public LootInfo TestInteractive<T>(Action<Roguelike.Tiles.Interactive.InteractiveTile> init,
      int tilesToCreateCount = 50,
      int maxExpectedLootCount = 15,
      int maxExpectedUniqCount = 2)
      where T : Roguelike.Tiles.Interactive.InteractiveTile, new()
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
      Action<Roguelike.Tiles.Interactive.InteractiveTile> init = null
    )
    where T : Roguelike.Tiles.Interactive.InteractiveTile, new()
    {
      var createdTiles = new List<Tile>();
      for (int i = 0; i < numberOfTilesToTest; i++)
      {
        var tile = AddTile<T>();
        if (init != null)
          init(tile);

        createdTiles.Add(tile);
      }

      for (int i = 0; i < createdTiles.Count; i++)
      {
        var tile = createdTiles[i];
        var to = game.GameManager.Context.TurnOwner;
        var tac = game.GameManager.Context.TurnActionsCount;
        var ni = Enemies.Where(e => e.State != EntityState.Idle).ToList();

        Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
        test.InteractHeroWith(tile as Roguelike.Tiles.Interactive.InteractiveTile);
      }
    }
  }
}
