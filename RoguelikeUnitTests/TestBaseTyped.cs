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
    }

    public T CreateTestEnv(bool autoLoadLevel = true, int numEnemies = 10)
    {
      var game = CreateGame(autoLoadLevel, numEnemies);
      helper = new T();
      helper.Game = game;
      return helper;
    }
  }
}
