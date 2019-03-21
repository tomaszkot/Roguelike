using Dungeons;
using Dungeons.ASCIIDisplay;
using Roguelike;
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
    static void Main(string[] args)
    {
      var container = new ContainerConfigurator().Container;

      container.Register<GameController, GameController>();
      container.Register<IDrawingEngine, ConsoleDrawingEngine>();
      //container.Verify();

      var controller = container.GetInstance<GameController>();
      controller.Run();
    }
  }
}
