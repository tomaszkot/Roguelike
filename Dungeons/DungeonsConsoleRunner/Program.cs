using Dungeons;
using Dungeons.ASCIIDisplay;
using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using DungeonsConsoleRunner;
using SimpleInjector;

namespace ConsoleDungeonsRunner
{
  
  class Program
  {
    static void Main(string[] args)
    {
      var container = new ContainerConfigurator().Container;
      container.Register<GameController, GameController>();
      container.Register<IDrawingEngine, ConsoleDrawingEngine>();
      container.Register<ILogger, Logger>();
      container.Register<Screen, Screen>();
      //container.Verify();

      var controller = container.GetInstance<GameController>();
      controller.Run();
    }
  }
}
