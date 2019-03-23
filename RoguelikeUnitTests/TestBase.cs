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
    //public GameNode GameNode { get; set; }
    public Container Container { get; set; }
    int levelIndex;
    public Hero Hero { get { return GameManager.Hero; } }

    protected virtual IContainerConfigurator CreateContainerConfigurator()
    {
      return new ContainerConfigurator();
    }

    [SetUp]
    public void Init()
    {
      Container =  CreateContainerConfigurator().Container;
      GameManager = Container.GetInstance<GameManager>();
    }

    protected DungeonLevel CreateNewDungeon() 
    {
      return CreateNewDungeon<DungeonLevel>() ;
    }

    protected Dungeon CreateNewDungeon<Dungeon>() where Dungeon : GameNode
    {
      var gameNode = Container.GetInstance<IGameGenerator>().Generate(levelIndex) as Dungeon;
      GameManager.SetContext(gameNode, AddHero(gameNode), Roguelike.GameContextSwitchKind.NewGame);

      return gameNode;
    }

    protected Hero AddHero(GameNode node)
    {
      var hero = Container.GetInstance<Hero>();
      node.SetTile(hero, node.GetFirstEmptyPoint().Value);
      return hero;
    }

    [TearDown]
    public void Cleanup()
    { 
    }

  }
}
