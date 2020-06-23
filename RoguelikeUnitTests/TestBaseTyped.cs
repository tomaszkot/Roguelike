﻿using RoguelikeUnitTests.Helpers;

namespace RoguelikeUnitTests
{
  class TestBaseTyped<T> : TestBase
    where T : BaseHelper, new()
  {
    protected override void OnInit()
    {
      base.OnInit();
    }

    public T CreateTestEnv(bool autoLoadLevel = true, int numEnemies = 10)
    {
      var numRooms = 10;
      //var numEn = numEnemies / numRooms;
      var game = CreateGame(autoLoadLevel, numEnemies, numRooms);
      helper = new T();

      helper.Enemies = this.GetLimitedEnemies();
      helper.Game = game;
      return helper as T;
    }
  }
}
