using System;
using NUnit.Framework;
using Roguelike;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using SimpleInjector;

namespace RoguelikeUnitTests
{
  [TestFixture]
  public class TestBase
  {
    protected Game game;

    public Game Game { get => game; protected set => game = value; }

    [SetUp]
    public void Init()
    {
      CreateGame();
    }

    public virtual Game CreateGame(bool autoLoadLevel = true, bool autoHandleStairs = true)
    {
      Game = new Game(new ContainerConfigurator().Container);
      if (autoLoadLevel)
        Game.GenerateLevel< DungeonLevel>(0);

      Game.SetAutoHandleStairs(autoHandleStairs);
      return Game;
    }

    [TearDown]
    public void Cleanup()
    { 
    }
  }
}
