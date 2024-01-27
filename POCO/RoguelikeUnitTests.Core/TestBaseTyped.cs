using Dungeons.Core;
using RoguelikeUnitTests.Helpers;

namespace RoguelikeUnitTests
{
  class TestBaseTyped<T> : TestBase
    where T : BaseHelper, new()
  {
    protected override void OnInit()
    {
      base.OnInit();
    }

    public T CreateTestEnv(bool autoLoadLevel = true, int numEnemies = 10, int numRooms = 10)
    {
      var logLevel = Container.GetInstance<ILogger>().LogLevel;
      var game = CreateGame(autoLoadLevel, numEnemies, numRooms, logLevel : logLevel);
      helper = new T();

      helper.Test = this;
      helper.Enemies = this.GetLimitedEnemies();
      helper.Game = game;

      return helper as T;
    }
  }
}
