using NUnit.Framework;
using Roguelike.Tiles;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FoodConsumtionTests : TestBase
  {
    [Test]
    public void TestConsume()
    {
      var game = CreateGame();
      var hero = game.Hero;

      Assert.Greater(game.GameManager.EnemiesManager.Enemies.Count, 0);
      var heroHealth = hero.Stats.Health;
      hero.OnPhysicalHit(game.GameManager.EnemiesManager.Enemies.First());
      Assert.Greater(heroHealth, hero.Stats.Health);
      heroHealth = hero.Stats.Health;

      var food = Helper.AddTile<Food>();
      AddItemToInv(food);

      hero.Consume(food);
      Assert.Greater(hero.Stats.Health, heroHealth);
    }
  }
}
