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
    Game sampleGame;

    [SetUp]
    public void Init()
    {
      CreateGame();
    }

    internal Game CreateGame(bool autoLoadLevel = true)
    {
      sampleGame = new Game(new ContainerConfigurator().Container);
      if(autoLoadLevel)
        sampleGame.GenerateLevel(0);
      return sampleGame;
    }

    [TearDown]
    public void Cleanup()
    { 
    }
  }
}
