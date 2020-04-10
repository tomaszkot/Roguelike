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
  }
}