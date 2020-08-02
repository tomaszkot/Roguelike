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

    //[Test]
    //public void TestHooch()
    //{
    //  var attack = Hero.Stats.Stats[EntityStatKind.Strength].CurrentValue;
    //  var chanceToHit = Hero.Stats.Stats[EntityStatKind.ChanceToHit].CurrentValue;

    //  var hooch = new Hooch();
    //  var added = Hero.Inventory.Add(hooch);

    //  Hero.Consume(hooch);
    //  Assert.IsTrue(Hero.LastingEffects.Any());
    //  Assert.IsTrue(Hero.LastingEffects.Where(i => i.Type == Roguelike.Tiles.LivingEntity.EffectType.Hooch).Any());

    //  var hoochAttack = Hero.Stats.Stats[EntityStatKind.Strength];
    //  var hoochChanceToHit = Hero.Stats.Stats[EntityStatKind.ChanceToHit];
    //  AssertGreater(hoochAttack.CurrentValue, attack);
    //  AssertLess(hoochChanceToHit.CurrentValue, chanceToHit);

    //  SkipTurns(1);

    //  //still on
    //  hoochAttack = Hero.Stats.Stats[EntityStatKind.Strength];
    //  hoochChanceToHit = Hero.Stats.Stats[EntityStatKind.ChanceToHit];
    //  AssertGreater(hoochAttack.CurrentValue, attack);
    //  AssertLess(hoochChanceToHit.CurrentValue, chanceToHit);

    //  SkipTurns(6);

    //  //now shall be off
    //  Assert.IsFalse(Hero.LastingEffects.Where(i => i.Type == Roguelike.Tiles.LivingEntity.EffectType.Hooch).Any());
    //  hoochAttack = Hero.Stats.Stats[EntityStatKind.Strength];
    //  hoochChanceToHit = Hero.Stats.Stats[EntityStatKind.ChanceToHit];
    //  Assert.AreEqual(hoochAttack.CurrentValue, attack);
    //  Assert.AreEqual(hoochChanceToHit.CurrentValue, chanceToHit);
    //}
  }
}
