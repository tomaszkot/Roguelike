using Dungeons.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  [TestFixture]
  class CharacterTests : TestBase
  {
    string GetFormattedHealth()
    {
      return game.Hero.GetFormattedStatValue(Roguelike.Attributes.EntityStatKind.Health);
    }

    [Test]
    public void TestLevelUp()
    {
      var tt = new TimeTracker();
      var game = CreateGame();
      var health = game.Hero.Stats.Health;
      bool leveledUpDone = false;
      game.Hero.LeveledUp += (object sender, EventArgs e)=>
      {
        Assert.AreEqual(health, game.Hero.Stats.Health);//Health restored
        leveledUpDone = true;
      };
           
      var enemy = game.GameManager.EnemiesManager.AllEntities.First();

      game.Hero.OnPhysicalHit(enemy);
      Assert.Greater(health, game.Hero.Stats.Health);
      for (int i = 0; i < 100; i++)
      {
        game.Hero.IncreaseExp(i);
        if (leveledUpDone)
          break;
      }
      var time = tt.TotalSeconds;
      Assert.True(leveledUpDone);
      
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
      Assert.AreEqual(health,  game.Hero.Stats.Health+1);

      float reduce = 1.3f;
      game.Hero.Stats.SetFactor(Roguelike.Attributes.EntityStatKind.Health, -reduce);
      Assert.AreEqual(health, game.Hero.Stats.Health + reduce);
      var fh = GetFormattedHealth();
      Assert.AreEqual(fh, ((int)(health-1)).ToString());


      reduce = 1.6f;
      game.Hero.Stats.SetFactor(Roguelike.Attributes.EntityStatKind.Health, -reduce);
      Assert.AreEqual(health, game.Hero.Stats.Health + reduce);
      fh = GetFormattedHealth();
      Assert.AreEqual(fh, ((int)(health - 2)).ToString());
    }
  }
}
