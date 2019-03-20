using Dungeons;
using NUnit.Framework;
using Roguelike.Abstract;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using SimpleInjector;
using SimpleInjector.Advanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests
{
  public class GreediestConstructorBehavior : IConstructorResolutionBehavior
  {
    public ConstructorInfo GetConstructor(Type implementationType) => (
        from ctor in implementationType.GetConstructors()
        orderby ctor.GetParameters().Length //descending
        select ctor)
        .First();
  }

  [TestFixture]
  class TestBase
  {
    Container container;
    public GameManager GameManager { get; private set; }
    public GameNode GameNode { get; set; }

    [SetUp]
    public void Init()
    {
      container = new Container();
      container.Options.ConstructorResolutionBehavior = new GreediestConstructorBehavior();

      container.Register<IGameGenerator, LevelGenerator>();
      container.Register<GameManager, GameManager>();
      
      container.Register<ILogger, Roguelike.Utils.Logger>();

      GameManager = container.GetInstance<GameManager>();

      GameNode = container.GetInstance<IGameGenerator>().Generate() as GameNode;
    }

    protected Hero AddHero()
    {
      var hero = new Hero();
      GameNode.SetTile(hero, GameNode.GetFirstEmptyPoint().Value);
      return hero;
    }

    [TearDown]
    public void Cleanup()
    { 
      /* ... */
    }

  }
}
