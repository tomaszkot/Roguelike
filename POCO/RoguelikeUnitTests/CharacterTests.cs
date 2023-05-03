using Dungeons.Core;
using NUnit.Framework;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Linq;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class CharacterTests : TestBase
  {
    string GetFormattedHealth()
    {
      return game.Hero.GetFormattedStatValue(Roguelike.Attributes.EntityStatKind.Health, false);
    }

    [Test]
    public void TestExpSaveLoad()
    {
      var tt = new TimeTracker();
      var game = CreateGame();

      var enemy = game.GameManager.EnemiesManager.AllEntities.First();
      for (int i = 0; i < 10; i++)
      {
        game.Hero.IncreaseExp(i);
      }
      var exp = game.Hero.Experience;
      Assert.Greater(exp, 0);
      game.GameManager.Save();
      game.GameManager.Load(game.Hero.Name);
      Assert.AreEqual(game.Hero.Experience, exp);
    }

    [Test]
    public void TestHeroMana()
    {
      var game = CreateGame();
      var mana = game.Hero.Stats.Mana;
      var magic = game.Hero.Stats.Magic;
      DoLevelUp(game.Hero);
      game.Hero.IncreaseStatByLevelUpPoint(Roguelike.Attributes.EntityStatKind.Magic);
      Assert.AreEqual(game.Hero.Stats.Magic, magic+1);
      Assert.AreEqual(game.Hero.Stats.Mana, mana+1);

    }

    [Test]
    public void TestLevelUp()
    {
      var tt = new TimeTracker();
      var game = CreateGame();
      var health = game.Hero.Stats.Health;
      bool leveledUpDone = false;
      game.Hero.LeveledUp += (object sender, EventArgs e) =>
      {
        Assert.AreEqual(health, game.Hero.Stats.Health);//Health restored
        leveledUpDone = true;
      };

      var enemy = game.GameManager.EnemiesManager.AllEntities.First();

      game.Hero.OnMeleeHitBy(enemy);
      Assert.Greater(health, game.Hero.Stats.Health);
      DoLevelUp(game.Hero);
      var time = tt.TotalSeconds;
      Assert.True(leveledUpDone);
    }

    private static bool DoLevelUp(Hero hero)
    {
      bool leveledUpDone = false;
      for (int i = 0; i < 100; i++)
      {
        hero.IncreaseExp(i);
        if (leveledUpDone)
          break;
      }

      return leveledUpDone;
    }

    [Test]
    public void TestStatsFormatting()
    {
      //var v1 = Math.Round(1.2);
      //var v2 = Math.Round(1.2, MidpointRounding.ToEven);

      var game = CreateGame();
      var health = game.Hero.Stats.Health;
      Assert.AreEqual(GetFormattedHealth(), ((int)health).ToString());
      game.Hero.Stats.SetFactor(Roguelike.Attributes.EntityStatKind.Health, -1);
      Assert.AreEqual(health, game.Hero.Stats.Health + 1);

      float reduce = 1.3f;
      game.Hero.Stats.SetFactor(Roguelike.Attributes.EntityStatKind.Health, -reduce);
      Assert.AreEqual(health, game.Hero.Stats.Health + reduce);
      var fh = GetFormattedHealth();
      Assert.AreEqual(fh, ((int)(health - 1)).ToString());


      reduce = 1.6f;
      game.Hero.Stats.SetFactor(Roguelike.Attributes.EntityStatKind.Health, -reduce);
      Assert.AreEqual(health, game.Hero.Stats.Health + reduce);
      fh = GetFormattedHealth();
      Assert.AreEqual(fh, ((int)(health - 2)).ToString());
    }
  }
}
