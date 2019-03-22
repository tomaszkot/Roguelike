using Dungeons;
using Dungeons.Core;
using Roguelike.Abstract;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Serialization;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  public class ContainerConfigurator
  {
    public Container Container { get; set; }

    public ContainerConfigurator()
    {
      var container = new Container();
      container.Options.ConstructorResolutionBehavior = new GreediestConstructorBehavior();
      container.Register<IGameGenerator, LevelGenerator>();
      container.Register<JSONPersister, JSONPersister>();
      container.Register<GameManager, GameManager>();
      container.Register<ILogger, Utils.Logger>();
      Container = container;

    }
  }
}
