using Dungeons.Core;
using Roguelike.Abstract;
using Roguelike.Crafting;
using Roguelike.Generators;
using Roguelike.LootFactories;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Serialization;
using Roguelike.State;
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
      container.Register<IPersister, JSONPersister>();
      container.Register<GameManager, GameManager>();
      container.Register<Dungeons.TileContainers.DungeonNode, Roguelike.Generators.TileContainers.DungeonNode>();
      container.Register<Dungeons.Tiles.IDoor, Roguelike.Tiles.Interactive.Door>();

      //container.Register< Dungeons.TileContainers.DungeonLevel, GameLevel>();
      container.Register<Dungeons.TileContainers.DungeonLevel>(() => new GameLevel(container));
      container.Register<ILogger, Logger>();
      container.Register<LootGenerator, LootGenerator>(Lifestyle.Singleton);
      container.Register<EventsManager, EventsManager>(Lifestyle.Singleton);
      container.Register<Enemy, Enemy>();
      container.Register<RoomContentGenerator, RoomContentGenerator>();
      container.Register<AbstractLootFactory, LootFactory>();
      container.Register<SpellCastPolicy, SpellCastPolicy>();
      container.Register<IProjectilesFactory, ProjectilesFactory>();
      container.Register<LootCrafterBase, LootCrafter>();
      container.Register<GameState, GameState>();
      container.Register<LootManager, LootManager>();
      //container.Register <MovePolicy, MovePolicy>();//move to exe
      Container = container;

    }
  }
}
