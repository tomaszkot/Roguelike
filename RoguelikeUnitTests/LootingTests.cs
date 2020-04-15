using NUnit.Framework;
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
    public void IdentifiedPlainClassTest()
    {
      var env = CreateTestEnv();
      var wpn = env.LootGenerator.GetLootByTileName<Weapon>("rusty_sword");
      Assert.AreEqual(wpn.Class, EquipmentClass.Plain);
      var price = wpn.Price;
      var damage = wpn.GetStats().GetTotalValue(EntityStatKind.Attack);

      Assert.Greater(price, 0);
      Assert.Greater(damage, 0);
      Assert.AreEqual(wpn.Identified, true);

      int extraAttack = 2;
      wpn.MakeMagic(EntityStatKind.Attack, extraAttack);
      Assert.AreEqual(wpn.Identified, false);
      Assert.AreEqual(wpn.Class, EquipmentClass.Magic);
      Assert.AreEqual(wpn.GetStats().GetTotalValue(EntityStatKind.Attack), damage);
      Assert.Greater(wpn.Price, price);//shall be bit bigger
      price = wpn.Price;

      wpn.Identify();
      Assert.AreEqual(wpn.Identified, true);
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
    public void KilledEnemyForEqipAndGoldMoreEq()
    {
      var env = CreateTestEnv();
      env.LootGenerator.Probability = new Roguelike.Probability.Looting();
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Equipment, .8f);
      env.LootGenerator.Probability.SetLootingChance(LootSourceKind.Enemy, LootKind.Gold, .2f);
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
      var newLootItems = env.TestInteractive<Barrel>(
         (InteractiveTile barrel) => {
         }
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
         }, 100 * mult, 30 * mult

        );

      var pots = lootInfo.Get<Potion>();
      Assert.Greater(pots.Count, 0);
      Assert.Less(pots.Count, 12);
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

    
  }
}