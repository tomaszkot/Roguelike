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
    SampleGame sampleGame;

    [SetUp]
    public void Init()
    {
      CreateGame();
    }

    internal SampleGame CreateGame(bool autoLoadLevel = true)
    {
      sampleGame = new SampleGame();
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
