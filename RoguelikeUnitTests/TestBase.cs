using Dungeons;
using NUnit.Framework;
using Roguelike;
using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using SimpleInjector;

namespace RoguelikeUnitTests
{
  [TestFixture]
  public class TestBase
  {
    public GameManager GameManager { get; private set; }
    public GameNode GameNode { get; set; }
    public Container Container { get; set; }

    [SetUp]
    public void Init()
    {
      Container = new ContainerConfigurator().Container;
      GameManager = Container.GetInstance<GameManager>();

      GameNode = Container.GetInstance<IGameGenerator>().Generate() as GameNode;
    }

    protected Hero AddHero()
    {
      var hero = Container.GetInstance<Hero>();
      GameNode.SetTile(hero, GameNode.GetFirstEmptyPoint().Value);
      return hero;
    }

    [TearDown]
    public void Cleanup()
    { 
    }

  }
}
