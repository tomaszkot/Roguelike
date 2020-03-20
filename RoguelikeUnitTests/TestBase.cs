using NUnit.Framework;
using Roguelike;
using Roguelike.Managers;
using SimpleInjector;

namespace RoguelikeUnitTests
{
  [TestFixture]
  public class TestBase
  {
    protected RoguelikeGame game;

    public RoguelikeGame Game { get => game; protected set => game = value; }
    public Container Container { get; set; }

    [SetUp]
    public void Init()
    {
      Container = new Roguelike.ContainerConfigurator().Container;
      Container.Register<ISoundPlayer, BasicSoundPlayer>();
      //CreateGame();
    }

    public virtual RoguelikeGame CreateGame(bool autoLoadLevel = true)
    {
      Game = new RoguelikeGame(Container);
      if (autoLoadLevel)
        Game.GenerateLevel(0);
      return Game;
    }

    [TearDown]
    public void Cleanup()
    { 
    }
  }
}
