using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
      container.Register<Door, Door>();
      Container = container;

    }
  }
}
