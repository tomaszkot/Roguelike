using NUnit.Framework;
using Roguelike;

namespace RoguelikeUnitTests
{
  [TestFixture]
  public class TestBase
  {
    protected RoguelikeGame game;

    public RoguelikeGame Game { get => game; protected set => game = value; }

    [SetUp]
    public void Init()
    {
      CreateGame();
    }

    public virtual RoguelikeGame CreateGame(bool autoLoadLevel = true, bool autoHandleStairs = true)
    {
      Game = new RoguelikeGame(new ContainerConfigurator().Container);
      if (autoLoadLevel)
        Game.GenerateLevel(0);

      Game.SetAutoHandleStairs(autoHandleStairs);
      return Game;
    }

    [TearDown]
    public void Cleanup()
    { 
    }
  }
}
