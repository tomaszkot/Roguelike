using NUnit.Framework;
using Roguelike;
using Roguelike.Tiles;
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
    public void KilledEnemyForPotions()
    {
      var game = PerepareForEnemyLooting();
      game.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, 1);
      AssertLootKind(new[] { LootKind.Gold });
    }

    [Test]
    public void KilledEnemyForEqipAndPotions()
    {
      var game = PerepareForEnemyLooting(25);
      game.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, .5f);
      game.GameManager.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, .5f);
      var loots = AssertLootKind(new[] { LootKind.Gold, LootKind.Equipment });
      Assert.AreEqual(loots.GroupBy(i=>i).Count(), 2);
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
    }
  }
}