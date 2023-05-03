using OuaDII.Generators;
using OuaDII.Quests;
using OuaDII.TileContainers;
using OuaDII.Tiles.LivingEntities;
using Roguelike;
using Roguelike.Abstract;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System.Linq;

namespace OuaDIIConsoleRunner
{
  class OuadIIGame : Game
  {
    Container container;
    public OuadIIGame(Container container) : base(container)
    {
      this.container = container;
    }

    public override Dungeons.TileContainers.DungeonNode GenerateDungeon()
    {
      var generator = new WorldGenerator(container);
      var gi = new OuaDII.Generators.GenerationInfo();
      var size = 25;
      gi.MinNodeSize = new System.Drawing.Size(size, size);
      //gi.allowSmallWordSize = true;
      gi.Counts.WorldEnemiesCount = 2;

      var world = generator.Generate(0, gi);
      // var hero =  world.GetTiles<Hero>().Single() as Hero;
      //hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Health, 150);
      return world;
    }
  }

  class GameController : RoguelikeConsoleRunner.GameController
  {
    OuadIIGame game;

    public GameController(OuadIIGame game, WorldGenerator generator) : base(game, generator)
    {
      this.game = game;
    }

    public override Dungeons.TileContainers.DungeonNode GenerateDungeon()
    {
      game.GameManager.Context.Hero = null;

      World world = base.GenerateDungeon() as World;
      var hero = Container.GetInstance<Hero>();
      //world.SetTile(hero, world.GetFirstEmptyPoint().Value);
      GameManager.SetContext(world, hero, GameContextSwitchKind.NewGame);

      //hack
      hero.Stats.SetNominal(Roguelike.Attributes.EntityStatKind.Health, 150);
      hero.Name = "HeroConsole";

      return world;
    }

    protected override void RevealAll()
    {
      base.RevealAll();
    }

    protected override bool HandleNextLevelDown()
    {
      bool down = base.HandleNextLevelDown();
      if (!down)
      {
        var st = GameManager.CurrentNode.GetAllStairs(StairsKind.PitDown).Where(i => DungeonPit.GetQuestKind(i.PitName) == QuestKind.Unset).FirstOrDefault();
        if (st != null)
        {
          GameManager.InteractHeroWith(st);
          return true;
        }
      }
      return down;
    }
  }
}
