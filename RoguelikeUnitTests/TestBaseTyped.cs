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

    public T CreateTestEnv(bool autoLoadLevel = true, int numEnemies = 10)
    {
      var game = CreateGame(autoLoadLevel, numEnemies);
      helper = new T();
      helper.Game = game;
      return helper as T;
    }
  }
}
