using Dungeons.ASCIIDisplay;
using Roguelike;

namespace RoguelikeConsoleRunner
{
  class Program
  {
    static void Main(string[] args)
    {
      var container = new Roguelike.ContainerConfigurator().Container;

      container.Register<GameController, GameController>();
      container.Register<IDrawingEngine, ConsoleDrawingEngine>();
      container.Register<IGame, RoguelikeGame>();
      //container.Verify();

      var controller = container.GetInstance<GameController>();
      //controller.Game.SetAutoHandleStairs(true);
      controller.Run();
    }
  }
}
