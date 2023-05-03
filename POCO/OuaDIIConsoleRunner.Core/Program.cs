using Dungeons.ASCIIDisplay;
using OuaDII.Discussions;
using OuaDII.Generators;
using Roguelike.Abstract.Multimedia;
using Roguelike.Abstract.Projectiles;
using Roguelike.Abstract.Spells;
using Roguelike.Multimedia;
using Roguelike.Strategy;
//using Roguelike.Managers;

namespace OuaDIIConsoleRunner
{
  class Program
  {
    static void Main(string[] args)
    {
      var container = new OuaDII.ContainerConfigurator().Container;

      container.Register<IDrawingEngine, ConsoleDrawingEngine>();
      container.Register<ISoundPlayer, BasicSoundPlayer>();
      container.Register<IQuestRoomCreator, ProceduralQuestRoomCreator>();

      container.Register<GameController, GameController>();
      container.Register<ITilesAtPathProvider, TilesAtPathProvider>();
      container.Register<IProjectilesFactory, Roguelike.Generators.ProjectilesFactory>();
      container.Register<IStaticSpellFactory, Roguelike.Generators.StaticSpellFactory>();
      container.Register<OuadIIGame, OuadIIGame>();
      container.Register<WorldGenerator, WorldGenerator>();
      //container.Register<Roguelike.Discussions.DiscussionTopic, DiscussionTopic>();
      //container.Verify();

      //var game = new OuadIIGame(container);

      var controller = container.GetInstance<GameController>();
      //var controller = new GameController(new OuadIIGame(container), new WorldGenerator(container));
      controller.Run();
    }
  }
}
