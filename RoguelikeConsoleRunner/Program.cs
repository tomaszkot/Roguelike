using Dungeons.ASCIIDisplay;
using Roguelike;
using Roguelike.Abstract;
using Roguelike.Abstract.Multimedia;
using Roguelike.Abstract.Projectiles;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Multimedia;
using Roguelike.Strategy;
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
