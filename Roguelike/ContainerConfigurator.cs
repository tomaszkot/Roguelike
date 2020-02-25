using Dungeons.Core;
using Roguelike.Abstract;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Serialization;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using SimpleInjector;

namespace Roguelike
{
  public class ContainerConfigurator : Dungeons.IContainerConfigurator
  {
    public Container Container { get; set; }

    public ContainerConfigurator()
    {
      var container = new Container();
      container.Options.ConstructorResolutionBehavior = new GreediestConstructorBehavior();
      container.Register<Dungeons.IDungeonGenerator, LevelGenerator>();
      container.Register<JSONPersister, JSONPersister>();
      container.Register<GameManager, GameManager>();
      container.Register< Dungeons.TileContainers.DungeonNode, Roguelike.Generators.TileContainers.DungeonNode>();
      container.Register<Dungeons.Tiles.Door, Door>();
      container.Register< Dungeons.TileContainers.DungeonLevel, GameLevel>();
      container.Register<ILogger, Logger>();
      container.Register<LootGenerator, LootGenerator>(Lifestyle.Singleton);
      Container = container;

    }
  }
}
