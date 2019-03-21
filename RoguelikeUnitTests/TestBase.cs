using Dungeons;
using Dungeons.Core;
using NUnit.Framework;
using Roguelike;
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
