using NUnit.Framework;
using Roguelike.Tiles;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class InventoryTests : TestBase
  {
    [Test]
    public void HeroOn()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var wpn = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon);
      //hero.Inventory.Add(wpn);
     //TODO
      //var ca = hero.GetCurrentValue(EntityStatKind.Attack);
      //var ta = hero.GetTotalValue(EntityStatKind.Attack);
      //Assert.AreEqual(hero.Stats.Attack, hero.Stats.Strength);
    }

    [Test]
    public void SaveLoadMushrooms()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var mush1 = new Mushroom();
      mush1.SetKind(MushroomKind.Boletus);
      hero.Inventory.Add(mush1);

      var mush2 = new Mushroom();
      mush2.SetKind(MushroomKind.RedToadstool);
      hero.Inventory.Add(mush2);
      Assert.AreEqual(hero.Inventory.Items.Count, 2);

      var mush3 = new Mushroom();
      mush3.SetKind(MushroomKind.RedToadstool);
      hero.Inventory.Add(mush3);
      Assert.AreEqual(hero.Inventory.Items.Count, 2);

      Assert.AreEqual(hero.Inventory.GetStackedCount(mush1), 1);
      Assert.AreEqual(hero.Inventory.GetStackedCount(mush2), 2);

      game.GameManager.Save();
      game.GameManager.Load();

      var heroLoaded = game.GameManager.Hero;
      Assert.AreEqual(heroLoaded.Inventory.GetStackedCount(mush1), 1);
      Assert.AreEqual(heroLoaded.Inventory.GetStackedCount(mush2), 2);
    }

    [Test]
    public void ScrollTests()
    {
      var game = CreateGame();
      var hero = game.Hero;

      var loot = game.GameManager.LootGenerator.GetRandomLoot(LootKind.Scroll);
      Assert.NotNull(loot.tag1);
      Assert.True(hero.Inventory.Add(loot));
    }
  }
}