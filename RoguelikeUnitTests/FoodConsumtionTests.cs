using NUnit.Framework;
using Roguelike.Tiles;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class FoodConsumtionTests : TestBase
  {
    [Test]
    public void TestConsumeRoasted()
    {
      TestConsume(true);
    }

    [Test]
    public void TestConsumeRaw()
    {
      TestConsume(false);
    }

    public void TestConsume(bool roasted)
    {
      var game = CreateGame();
      var hero = game.Hero;
      var expectedHealthRestore = roasted ? hero.Stats.Health / 2 : hero.Stats.Health / 4;

      var enemy = game.GameManager.CurrentNode.SpawnEnemy(1);
      //Assert.Greater(ActiveEnemies.Count, 0);//enemies on level can be poisonous, failing this test, so let's use skeleton
      var heroHealth = hero.Stats.Health;
      while (hero.Stats.Health > 5)
        hero.OnPhysicalHitBy(enemy);

      Assert.Greater(heroHealth, hero.Stats.Health);
      heroHealth = hero.Stats.Health;
      var heroHurtHealth = heroHealth;

      var food = Helper.AddTile<Food>();
      if(roasted)
        food.MakeRoasted();
      AddItemToInv(food);

      var turnOwner = game.GameManager.Context.TurnOwner;
      Assert.AreEqual(turnOwner, Roguelike.TurnOwner.Hero);
      hero.Consume(food);
      Assert.Greater(hero.Stats.Health, heroHealth);
      var le = hero.LastingEffects.Single();
      Assert.NotNull(le);
      Assert.AreEqual(le.PendingTurns, food.ConsumptionSteps - 1);

      for (int i = 0; i < food.ConsumptionSteps - 1; i++)
      {
        Assert.Greater(le.PendingTurns, 0);
        Assert.AreEqual(game.GameManager.Context.TurnOwner, Roguelike.TurnOwner.Allies);
        heroHealth = hero.Stats.Health;

        var leBeforeStep = hero.LastingEffects.SingleOrDefault();
        GotoNextHeroTurn(game);//food is working gradually
        var leAfterStep = hero.LastingEffects.SingleOrDefault();
        Assert.Greater(hero.Stats.Health, heroHealth);
        heroHealth = hero.Stats.Health;
        Game.GameManager.SkipHeroTurn();
      }
      var diff = heroHealth - heroHurtHealth;
      Assert.AreEqual(diff, expectedHealthRestore);
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
    //  Assert.IsTrue(Hero.LastingEffects.Where(i => i.Type == Roguelike.Tiles.LivingEntities.LivingEntity.EffectType.Hooch).Any());

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
    //  Assert.IsFalse(Hero.LastingEffects.Where(i => i.Type == Roguelike.Tiles.LivingEntities.LivingEntity.EffectType.Hooch).Any());
    //  hoochAttack = Hero.Stats.Stats[EntityStatKind.Strength];
    //  hoochChanceToHit = Hero.Stats.Stats[EntityStatKind.ChanceToHit];
    //  Assert.AreEqual(hoochAttack.CurrentValue, attack);
    //  Assert.AreEqual(hoochChanceToHit.CurrentValue, chanceToHit);
    //}
  }
}
