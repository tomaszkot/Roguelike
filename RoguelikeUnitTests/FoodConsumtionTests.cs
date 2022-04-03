﻿using NUnit.Framework;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
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
        hero.OnMeleeHitBy(enemy);

      Assert.Greater(heroHealth, hero.Stats.Health);
      heroHealth = hero.Stats.Health;
      var heroHurtHealth = heroHealth;

      var food = Helper.AddTile<Food>();
      if (roasted)
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

    [Test]
    public void TestApple()
    {
      var food = new Food(FoodKind.Apple);
      Assert.AreEqual(food.PercentageStatIncrease, true);
      food.SetPoisoned();
      TestPoisonous(food);
    }

    private void TestPoisonous(Food food)
    {
      var game = CreateGame();
      var hero = game.Hero;
      
      Assert.AreEqual(food.PercentageStatIncrease, false);
      Assert.AreEqual(food.StatKind, EntityStatKind.PoisonAttack);
      Assert.Greater(food.StatKindEffective.Value, 0);
      var lsi = food.GetLootStatInfo(hero);
      Assert.AreEqual(lsi[0].Desc, "Poison Damage: " + Math.Abs(food.StatKindEffective.Value));
    }

    [TestCase(MushroomKind.BlueToadstool)]
    [TestCase(MushroomKind.RedToadstool)]
    public void TestMash(MushroomKind kind)
    {
      var food = new Mushroom(kind);
      Assert.AreEqual(food.EffectType, EffectType.Poisoned);
      TestPoisonous(food);
    }

    [Test]
    public void TestHooch()
    {
      var game = CreateGame();
      var Hero = game.Hero;
      var attack = Hero.Stats[EntityStatKind.Strength].CurrentValue;
      var chanceToHit = Hero.Stats[EntityStatKind.ChanceToMeleeHit].CurrentValue;

      var hooch = new Hooch();
      //Assert.AreEqual(hooch.PrimaryStatDescription, "");
      var added = Hero.Inventory.Add(hooch);

      Hero.Consume(hooch);
      Assert.IsTrue(Hero.LastingEffects.Any());
      var le = Hero.LastingEffects.Where(i => i.Type == Roguelike.Effects.EffectType.Hooch);
      Assert.IsTrue(le.Any());

      Action assertGreater = () =>
      {
        var hoochAttack = Hero.Stats[EntityStatKind.Strength];
        var hoochChanceToHit = Hero.Stats[EntityStatKind.ChanceToMeleeHit];
        Assert.Greater(hoochAttack.CurrentValue, attack);
        Assert.Less(hoochChanceToHit.CurrentValue, chanceToHit);
      };

      assertGreater();

      SkipTurns(1);

      //still on
      assertGreater();

      SkipTurns(6);

      //now shall be off
      Assert.IsFalse(le.Any());
      var hoochAttackAfter = Hero.Stats[EntityStatKind.Strength];
      var hoochChanceToHitAfter = Hero.Stats[EntityStatKind.ChanceToMeleeHit];
      Assert.AreEqual(hoochAttackAfter.CurrentValue, attack);
      Assert.AreEqual(hoochChanceToHitAfter.CurrentValue, chanceToHit);
    }

    [Test]
    public void TestHoochDrunkTwice()
    {
      var game = CreateGame();
      var Hero = game.Hero;
      var attack = Hero.Stats[EntityStatKind.Strength].CurrentValue;
      var chanceToHit = Hero.Stats[EntityStatKind.ChanceToMeleeHit].CurrentValue;

      var hooch = new Hooch();
      var added = Hero.Inventory.Add(hooch);

      Hero.Consume(hooch);
      Assert.IsTrue(Hero.LastingEffects.Any());
      var le = Hero.LastingEffects.Where(i => i.Type == Roguelike.Effects.EffectType.Hooch);
      Assert.IsTrue(le.Any());
      float strengthWithLE = 0;
      float chanceToHitWithLE = 0;

      Action assertGreater = () =>
      {
        var hoochAttack = Hero.Stats[EntityStatKind.Strength];
        var hoochChanceToHit = Hero.Stats[EntityStatKind.ChanceToMeleeHit];
        Assert.Greater(hoochAttack.CurrentValue, attack);
        Assert.Less(hoochChanceToHit.CurrentValue, chanceToHit);
        if (strengthWithLE == 0)
        {
          strengthWithLE = hoochAttack.CurrentValue;
          chanceToHitWithLE = hoochChanceToHit.CurrentValue;
        }
      };

      assertGreater();
            
      SkipTurns(1);

      //still on
      assertGreater();

      hooch = new Hooch();
      added = Hero.Inventory.Add(hooch);
      Hero.Consume(hooch);

      //shall be same
      var str = Hero.Stats[EntityStatKind.Strength].CurrentValue;
      Assert.AreEqual(str, strengthWithLE);
      Assert.AreEqual(Hero.Stats[EntityStatKind.ChanceToMeleeHit].CurrentValue, chanceToHitWithLE);

      SkipTurns(6);

      //now shall be on - prolonged
      Assert.IsTrue(le.Any());
      SkipTurns(1);
      //now shall be off
      Assert.IsFalse(le.Any());
      var hoochAttackAfter = Hero.Stats[EntityStatKind.Strength];
      var hoochChanceToHitAfter = Hero.Stats[EntityStatKind.ChanceToMeleeHit];
      Assert.AreEqual(hoochAttackAfter.CurrentValue, attack);
      Assert.AreEqual(hoochChanceToHitAfter.CurrentValue, chanceToHit);
    }
  }
}
