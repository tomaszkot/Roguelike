using Dungeons.ASCIIDisplay;
using Roguelike;
using Roguelike.Managers;
using System;
using System.IO;
using System.Media;

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
      container.Register<ISoundPlayer, BasicSoundPlayer>();
      //container.Verify();

      var controller = container.GetInstance<GameController>();
      //controller.Game.SetAutoHandleStairs(true);
      controller.Run();
    }
  }
}
