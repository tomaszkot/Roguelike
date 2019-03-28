using Dungeons;
using Dungeons.ASCIIDisplay;
using DungeonsConsoleRunner;
using SimpleInjector;
using System;
using System.Diagnostics;

namespace ConsoleDungeonsRunner
{
  class Program
  {
    static void Main(string[] args)
    {
      var container = new ContainerConfigurator().Container;
      container.Register<GameController, GameController>();
      container.Register<IDrawingEngine, ConsoleDrawingEngine>();
      container.Register<Screen, Screen>();
      container.Verify();

      var controller = container.GetInstance<GameController>();
      controller.Run();
    }
  }
}
