using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using SimpleInjector;

namespace Dungeons
{
  public interface IContainerConfigurator
  {
    Container Container { get; set; }
  }

  public class ContainerConfigurator : IContainerConfigurator
  {
    public Container Container { get; set; }

    public ContainerConfigurator()
    {
      var container = new Container();
      container.Options.ConstructorResolutionBehavior = new GreediestConstructorBehavior();
      container.Register<IDungeonGenerator, DungeonGenerator>();

      container.Register<DungeonNode, DungeonNode>();
      container.Register<DungeonLevel, DungeonLevel>();
      container.Register<Door, Door>();

      Container = container;

    }
  }
}
