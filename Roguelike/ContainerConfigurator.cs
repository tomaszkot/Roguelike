using Dungeons.Core;
using Roguelike.Abstract.Projectiles;
using Roguelike.Crafting;
using Roguelike.Generators;
using Roguelike.LootContainers;
using Roguelike.LootFactories;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Serialization;
using Roguelike.State;
using Roguelike.Strategy;
using Roguelike.TileContainers;
using Roguelike.Tiles.LivingEntities;
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

      container.Register<Merchant, Merchant>();
      container.Register<Hero, Hero>();
      container.Register<Inventory, Inventory>();
      container.Register<LootContainers.Crafting, LootContainers.Crafting>();
      container.Register<Dungeons.IDungeonGenerator, LevelGenerator>();
      container.Register<IPersister, JSONPersister>();
      container.Register<GameManager, GameManager>();
      container.Register<Dungeons.TileContainers.DungeonNode, Roguelike.Generators.TileContainers.DungeonNode>();
      container.Register<Dungeons.Tiles.IDoor, Roguelike.Tiles.Interactive.Door>();

      //container.Register< Dungeons.TileContainers.DungeonLevel, GameLevel>();
      container.Register<Dungeons.TileContainers.DungeonLevel>(() => new GameLevel(container));
      RegisterLogger(container);
      container.Register<LootGenerator, LootGenerator>(Lifestyle.Singleton);
      container.Register<EventsManager, EventsManager>(Lifestyle.Singleton);
      container.Register<Enemy, Enemy>();
      container.Register<RoomContentGenerator, RoomContentGenerator>();
      container.Register<AbstractLootFactory, LootFactory>();
      container.Register<SpellCastPolicy, SpellCastPolicy>();

      container.Register<LootCrafterBase, LootCrafter>();
      container.Register<GameState, GameState>();
      container.Register<LootManager, LootManager>();
      container.Register<NPC, NPC>();

      container.Register<ITilesAtPathProvider, TilesAtPathProvider>();
      container.Register<IProjectilesFactory, ProjectilesFactory>();

      Container = container;
    }

    protected virtual void RegisterLogger(Container container)
    {
      container.Register<ILogger, Logger>();
    }
  }
}
