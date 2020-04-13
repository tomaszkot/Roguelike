using NUnit.Framework;
using Roguelike;
using Roguelike.Managers;
using Roguelike.Tiles;
using RoguelikeUnitTests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  class TestBaseTyped<T> : TestBase
    where T : BaseHelper, new()
  {
    T helper;

    public T Helper { get => helper; set => helper = value; }

    protected override void OnInit()
    {
      base.OnInit();
      //helper = new T();
    }

    public T CreateTestEnv(bool autoLoadLevel = true, int numEnemies = 10)
    {
      CreateGame(autoLoadLevel, numEnemies);
      return helper;
    }

    //private RoguelikeGame PerepareForEnemyLooting(int numEnemies = 10)
    //{
    //  var game = CreateGame(false);
    //  var gi = new GenerationInfo();

    //  gi.MinNodeSize = new System.Drawing.Size(30, 30);
    //  gi.MaxNodeSize = gi.MinNodeSize;
    //  gi.ForcedNumberOfEnemiesInRoom = numEnemies;
    //  game.GenerateLevel(0, gi);
    //  game.Hero.Stats[Roguelike.Attributes.EntityStatKind.Attack].Factor += 30;
    //  return game;
    //}

    //private RoguelikeGame PerepareForLooting()
    //{
    //  //var game = CreateGame(false);

    //  game.GenerateLevel(0, gi);
    //  return game;
    //}

    public List<LootKind> AssertLootKind(LootKind[] expectedKinds)
    {
      List<LootKind> res = new List<LootKind>();
      var enemies = game.GameManager.EnemiesManager.Enemies;
      Assert.GreaterOrEqual(enemies.Count, 5);
      for (int i = 0; i < enemies.Count; i++)
      {
        var li = new LootInfo(game, null);
        var en = enemies[i];
        while (en.Alive)
          en.OnPhysicalHit(game.Hero);

        var lootItems = li.GetDiff(); //game.GameManager.CurrentNode.GetTile(en.Point) as Loot;
        //Assert.NotNull(loot);
        if (lootItems != null)
        {
          foreach (var loot in lootItems)
          {
            Assert.True(expectedKinds.Contains(loot.LootKind));
            res.Add(loot.LootKind);
          }
        }
      }

      return res;
    }

    public override RoguelikeGame CreateGame(bool autoLoadLevel = true, int numEnemies = 10) 
    {
      Game = new RoguelikeGame(Container);
      if (autoLoadLevel)
      {
        var gi = new GenerationInfo();

        gi.MinNodeSize = new System.Drawing.Size(30, 30);
        gi.MaxNodeSize = gi.MinNodeSize;
        gi.ForcedNumberOfEnemiesInRoom = numEnemies;
        Game.GenerateLevel(0, gi);
      }

      helper = Activator.CreateInstance(typeof(T), new object[] { this, Game }) as T;
      return Game;
    }
  }
}
