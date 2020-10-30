
using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Events;
using Roguelike.Generators;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.Managers
{
  class LootManager
  {
    List<Loot> extraLoot = new List<Loot>();
    public GameManager GameManager { get; set; }
    public LootGenerator LootGenerator => GameManager.LootGenerator;

    public LootManager() { }

    public LootManager(GameManager mgr)
    {
      this.GameManager = mgr;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lootSource"></param>
    /// <returns>generated loot</returns>
    public List<Loot> TryAddForLootSource(ILootSource lootSource)//Barrel, Chest from attackPolicy.Victim
    {
      var lootItems = new List<Loot>();

      Loot debugLoot = null;// LootGenerator.GetLootByAsset("fire_ball_scroll"); 
      if(debugLoot != null)
        lootItems.Add(debugLoot);

      if (!lootItems.Any())
      {
        if (lootSource is Enemy)
        {
          var loot = TryAddLootForDeadEnemy(lootSource as Enemy);
          if (loot != null)
            lootItems.Add(loot);
        }
        else
        {
          lootItems = TryAddForNonEnemy(lootSource);
        }
      }
      foreach (var loot in lootItems)
      {
        if (loot is Equipment)
        {
          var eq = loot as Equipment;
          if (eq.Class == EquipmentClass.Plain && !eq.Enchantable)
          {
            if(RandHelper.GetRandomDouble() > 0.2)//TODO
              eq.MakeEnchantable();
          }
        }
      }

      return lootItems;
    }

    private List<Loot> TryAddForNonEnemy(ILootSource lootSource)
    {
      var lootItems = new List<Loot>();
      var inter = lootSource as Roguelike.Tiles.InteractiveTile;
      var lsk = LootSourceKind.Barrel;
      Chest chest = null;
      if (lootSource is Chest)
      {
        chest = (lootSource as Chest);
        if (!chest.Closed)
          return lootItems;
        lsk = chest.LootSourceKind;
      }

      if (lootSource is Barrel && RandHelper.GetRandomDouble() < GenerationInfo.ChanceToGenerateEnemyFromBarrel)
      {
        var enemy = GameManager.CurrentNode.SpawnEnemy(lootSource);
        GameManager.EnemiesManager.AddEntity(enemy);
        GameManager.ReplaceTile<Enemy>(enemy, lootSource.GetPoint(), false, lootSource as Tile);
        return lootItems;
      }

      var loot = GameManager.TryGetRandomLootByDiceRoll(lsk, inter.Level);
      if(loot!=null)
        lootItems.Add(loot);
      if (lootSource is Barrel)
      {
        bool repl = GameManager.ReplaceTile<Loot>(loot, lootSource.GetPoint(), false, lootSource as Tile);
        GameManager.Assert(repl, "ReplaceTileByLoot " + loot);
        GameManager.Logger.LogInfo("ReplaceTileByLoot " + loot + " " + repl);
      }
      else
      {
        if (!chest.Open())
          return lootItems;
        GameManager.AppendAction<InteractiveTileAction>((InteractiveTileAction ac) => { ac.InvolvedTile = chest; ac.InteractiveKind = InteractiveActionKind.ChestOpened; });
        GameManager.AddLootReward(loot, lootSource, true);//add loot at closest empty
        //if (chest.ChestKind == ChestKind.GoldDeluxe ||
        //  chest.ChestKind == ChestKind.Gold)
        {
          var lootEx1 = GetExtraLoot(lootSource, false, loot.LootKind);
          if(lootEx1!=null)
            lootItems.Add(lootEx1);
          GameManager.AddLootReward(lootEx1, lootSource, true);

          if (chest.ChestKind == ChestKind.GoldDeluxe)
          {
            var lootEx2 = GetExtraLoot(lootSource, true, loot.LootKind);
            if (lootEx2 != null)
              lootItems.Add(lootEx2);
            GameManager.AddLootReward(lootEx2, lootSource, true);
          }
        }
      }

      return lootItems;
    }

    Loot TryAddLootForDeadEnemy(Enemy enemy)
    {
      Loot loot = null;
      if(enemy.PowerKind == EnemyPowerKind.Champion ||
          enemy.PowerKind == EnemyPowerKind.Boss)
      {
        loot = GameManager.LootGenerator.GetBestLoot(enemy.PowerKind, enemy.Level);
      }
      else
        loot = GameManager.TryGetRandomLootByDiceRoll(LootSourceKind.Enemy, enemy.Level);
      if (loot != null)
      {
        GameManager.AddLootReward(loot, enemy, false);
      }
      var extraLootItems = GetExtraLoot(enemy, loot);
      foreach (var extraLoot in extraLootItems)
      {
        GameManager.AddLootReward(extraLoot, enemy, true);
      }

      return loot;
    }

    private Loot GetExtraLoot(ILootSource victim, bool nonEquipment, LootKind skip)
    {
      if (victim is Chest)
      {
        var chest = victim as Chest;
        if (
          chest.ChestKind == ChestKind.Gold ||
          chest.ChestKind == ChestKind.GoldDeluxe
          )
        {
          if (nonEquipment)
          {
            return LootGenerator.GetRandomLoot(chest.Level, skip);//TODO Equipment might happen
          }
          else
          {
            var eq = LootGenerator.GetRandomEquipment(GameManager.Hero.Level);
            if (eq.IsPlain())
            {
              eq.MakeEnchantable();
              eq.MakeMagic(true);
            }

            return eq;
          }
        }
      }

      return LootGenerator.GetRandomLoot(victim.Level, skip);
    }

    List<Loot> GetExtraLoot(ILootSource lootSource, Loot primaryLoot)
    {
      extraLoot.Clear();
      if (GenerationInfo.DebugInfo.EachEnemyGivesPotion)
      {
        var potion = LootGenerator.GetRandomLoot(LootKind.Potion, lootSource.Level);
        extraLoot.Add(potion);
      }
      if (GenerationInfo.DebugInfo.EachEnemyGivesJewellery)
      {
        var loot = LootGenerator.GetRandomRing();
        extraLoot.Add(loot);
      }
      if (primaryLoot is Gold)
      {
        var loot = LootGenerator.TryGetRandomLootByDiceRoll(LootSourceKind.Enemy, lootSource.Level);
        if (!(loot is Gold))
          extraLoot.Add(loot);
      }
      return extraLoot;
    }
  }
}
