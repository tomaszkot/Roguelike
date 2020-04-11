using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class LootingTests : TestBase
  {
    [Test]
    public void KilledEnemyForEquipment()
    {
      var game = PerepareForEnemyLooting();
      game.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1);
      AssertLootKind(new[] { LootKind.Equipment });
    }

    [Test]
    public void KilledEnemyForGold()
    {
      var game = PerepareForEnemyLooting();
      game.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, 1);
      AssertLootKind(new[] { LootKind.Gold });
    }

    [Test]
    public void KilledEnemyForEqipAndGold()
    {
      var game = PerepareForEnemyLooting(25);
      game.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, .5f);
      game.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, .5f);
      var loots = AssertLootKind(new[] { LootKind.Gold, LootKind.Equipment });
      Assert.AreEqual(loots.GroupBy(i=>i).Count(), 2);
    }

    [Test]
    public void KilledEnemyForEqipAndGoldMoreEq()
    {
      var game = PerepareForEnemyLooting(50);
      game.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, .8f);
      game.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, .2f);
      var loots = AssertLootKind(new[] { LootKind.Gold, LootKind.Equipment });
      Assert.AreEqual(loots.GroupBy(i => i).Count(), 2);
      var goldC = loots.Where(i => i == LootKind.Gold).Count();
      var eqC = loots.Where(i => i == LootKind.Equipment).Count();
      Assert.Greater(eqC, goldC);
    }

    private RoguelikeGame PerepareForEnemyLooting(int numEnemies = 10)
    {
      var game = CreateGame(false);
      var gi = new GenerationInfo();
      
      gi.MinNodeSize = new System.Drawing.Size(30, 30);
      gi.MaxNodeSize = gi.MinNodeSize;
      gi.ForcedNumberOfEnemiesInRoom = numEnemies;
      game.GenerateLevel(0, gi);
      game.Hero.Stats[Roguelike.Attributes.EntityStatKind.Attack].Factor += 30;
      return game;
    }

    private List<LootKind> AssertLootKind(LootKind[] kinds )
    {
      List<LootKind> res = new List<LootKind>();
      var enemies = game.GameManager.EnemiesManager.Enemies;
      Assert.GreaterOrEqual(enemies.Count, 5);
      for (int i=0;i< enemies.Count;i++)
      {
        var en = enemies[i];
        while (en.Alive)
          en.OnPhysicalHit(game.Hero);

        var loot = game.GameManager.CurrentNode.GetTile(en.Point) as Loot;
        //Assert.NotNull(loot);
        if (loot != null)
        {
          Assert.True(kinds.Contains(loot.LootKind));
          res.Add(loot.LootKind);
        }
      }

      return res;
    }

    [Test]
    public void Barrels()
    {
    }

    [Test]
    public void PlainChests()
    {
    }

    [Test]
    public void GoldChests()
    {
      var game = CreateGame();
      var chest = AddTile<Chest>();
      chest.ChestKind = ChestKind.Gold;
      var lootPrev = game.Level.GetTiles<Loot>();
      game.GameManager.InteractHeroWith(chest);
      var lootAfter = game.Level.GetTiles<Loot>();
      Assert.Greater(lootAfter.Count, lootPrev.Count);
      var newLootItems = lootAfter.Except(lootPrev);

      var eq = newLootItems.Where(i => i is Equipment).Cast<Equipment>().ToList();
      Assert.AreEqual(eq.First().Class, EquipmentClass.Unique);
    }

    
  }
}