using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Helpers;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class LootingTests : TestBaseTyped<LootingTestsHelper>
  {
    [Test]
    public void LotsOfPotionsTest()
    {
      var env = CreateTestEnv();
      //GenerationInfo.DebugInfo.EachEnemyGivesPotion = true;
      try
      {
        var lootInfo = new LootInfo(game, null);
        for (int i = 0; i < 10; i++)
        {
          var pot = env.LootGenerator.GetRandomLoot(LootKind.Potion);
          var added = game.GameManager.AddLootReward(pot, env.Game.Hero, false);
          Assert.True(added);
          var dist = pot.DistanceFrom(env.Game.Hero);
          Assert.Less(dist, 5);
          Assert.True(dist < 4 || i>5);
        }
        var newLootItems = lootInfo.GetDiff();
        Assert.AreEqual(newLootItems.Count, 10);
      }
      catch (System.Exception)
      {
        //GenerationInfo.DebugInfo.EachEnemyGivesPotion = false;
      }
    }

    [Test]
    public void LotsOfEqTest()
    {
      var env = CreateTestEnv();
      var lootInfo = new LootInfo(game, null);
      for (int i = 0; i < 10; i++)
      {
        var pot = env.LootGenerator.GetRandomLoot(LootKind.Equipment);
        var closeEmp = env.Game.Level.GetClosestEmpty(env.Game.Hero, true);
        var set = env.Game.Level.SetTile(pot, closeEmp.Point);
        Assert.True(set);
      }
      var newLootItems = lootInfo.GetDiff();
      Assert.AreEqual(newLootItems.Count, 10);
    }

    [Test]
    public void IdentifiedPlainClassTest()
    {
      var env = CreateTestEnv();
      var wpn = env.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.AreEqual(wpn.Class, EquipmentClass.Plain);
      var price = wpn.Price;
      var damage = wpn.GetStats().GetTotalValue(EntityStatKind.Attack);

      Assert.Greater(price, 0);
      Assert.Greater(damage, 0);
      Assert.AreEqual(wpn.IsIdentified, true);

      int extraAttack = 2;
      wpn.MakeMagic(EntityStatKind.Attack, extraAttack);
      Assert.AreEqual(wpn.IsIdentified, false);
      Assert.AreEqual(wpn.Class, EquipmentClass.Magic);
      Assert.AreEqual(wpn.GetStats().GetTotalValue(EntityStatKind.Attack), damage);
      Assert.Greater(wpn.Price, price);//shall be bit bigger
      price = wpn.Price;

      wpn.Identify();
      Assert.AreEqual(wpn.IsIdentified, true);
      Assert.AreEqual(wpn.GetStats().GetTotalValue(EntityStatKind.Attack), damage + extraAttack);
      Assert.Greater(wpn.Price, price);//shall be bit bigger
    }

    [Test]
    public void KilledEnemyForEquipment()
    {
      var env = CreateTestEnv(numEnemies: 25);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1);
      env.AssertLootKindFromEnemies(new[] { LootKind.Equipment });
    }

    [Test]
    public void KilledEnemyForPotion()
    {
      var env = CreateTestEnv(numEnemies: 25);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Potion, 1);
      env.AssertLootKindFromEnemies(new[] { LootKind.Potion });
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
    public void KilledEnemyAtSamePlace()
    {
      var env = CreateTestEnv(numEnemies: 5);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, 1f);

      var enemies = game.GameManager.EnemiesManager.Enemies;
      env.KillEnemy(enemies[0]);
      var loot = env.Game.Level.GetTile(enemies[0].Point);
      Assert.NotNull(loot);
      Assert.True(env.Game.Level.SetTile(enemies[1], enemies[0].Point));

      var li = new LootInfo(game, null);
      env.KillEnemy(enemies[1]);
      var lootItems = li.GetDiff();
      Assert.AreEqual(lootItems.Count, 1);
      Assert.True(lootItems[0].DistanceFrom(loot) < 2);
    }

    [Test]
    public void KilledEnemyForEqipAndGoldMoreEq()
    {
      var env = CreateTestEnv(numEnemies:30);
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, .75f);
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, .25f);
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
      var max = 20;
      var newLootItems = env.TestInteractive<Barrel>(
         (InteractiveTile barrel) => {
         }, max
        );
    }

    [Test]
    public void PlainChests()
    {
      int mult = 3;
      var env = CreateTestEnv();
      var lootInfo = env.TestInteractive<Chest>(
         (InteractiveTile chest) => {
           (chest as Chest).ChestKind = ChestKind.Plain;
         }, 100 * mult, 32 * mult

        );

      var potions = lootInfo.Get<Potion>();
      Assert.Greater(potions.Count, 3);
      Assert.Less(potions.Count, 34);
    }
        
    [Test]
    public void GoldChests()
    {
      int mult = 1;
      var env = CreateTestEnv();
      var lootInfo = env.TestInteractive<Chest>(
         (InteractiveTile chest) => {
           (chest as Chest).ChestKind = ChestKind.Gold;
         }, 20 * mult, 20 * mult, 20 * mult

        );

      var lootItems = lootInfo.Get<Equipment>();
      Assert.AreEqual(lootItems.Count, 20);
      lootItems.ForEach(i=> Assert.AreEqual(i.Class, EquipmentClass.Unique));
    }

    [Test]
    public void KilledEnemyGivesPotionFromTimeToTime()
    {
      for (int i = 0; i < 2; i++)
      {
        var env = CreateTestEnv(numEnemies: 100);
        var li = new LootInfo(game, null);
        env.KillAllEnemies();
        var lootItems = li.GetDiff();
        Assert.Greater(lootItems.Count, 0);
        var potions = lootItems.Where(j => j.LootKind == LootKind.Potion).ToList();
        Assert.Greater(potions.Count, 4);
        Assert.Less(potions.Count, 35);
      }
    }

    [Test]
    public void KilledEnemyGivesFoodFromTimeToTime()
    {
      for (int i = 0; i < 2; i++)
      {
        var env = CreateTestEnv(numEnemies: 100);
        var li = new LootInfo(game, null);
        env.KillAllEnemies();
        var lootItems = li.GetDiff();
        Assert.Greater(lootItems.Count, 0);
        var food = lootItems.Where(j => j.LootKind == LootKind.Food).ToList();
        Assert.Greater(food.Count, 5);
        Assert.Less(food.Count, 36);
      }
    }
  }
}