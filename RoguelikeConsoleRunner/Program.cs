using Dungeons;
using Dungeons.ASCIIDisplay;
using Roguelike.Abstract;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.TileContainers;
using SimpleInjector;
using SimpleInjector.Advanced;
using System;
using System.Linq;
using System.Reflection;

namespace RoguelikeConsoleRunner
{
  class Program
  {
    // Custom constructor resolution behavior
    public class GreediestConstructorBehavior : IConstructorResolutionBehavior
    {
      public ConstructorInfo GetConstructor(Type implementationType) => (
          from ctor in implementationType.GetConstructors()
          orderby ctor.GetParameters().Length //descending
          select ctor)
          .First();
    }
      

    static void Main(string[] args)
    {
      var container = new Container();
      container.Options.ConstructorResolutionBehavior = new GreediestConstructorBehavior();

      container.Register<GameController, GameController>();
      
      container.Register<IGameGenerator, LevelGenerator>();
      container.Register<IDrawingEngine, ConsoleDrawingEngine>();
      container.Register<GameManager, GameManager>();
      container.Register<ILogger, Roguelike.Utils.Logger>();
      //container.Register<World, World>();
      //container.Register<Screen, RoguelikeConsoleRunner.ASCIIDisplay.Screen>();
      //container.Verify();

      var controller = container.GetInstance<GameController>();
      controller.Run();
    }
  }
}
