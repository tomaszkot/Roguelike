using Dungeons.Core;
using OuaDII.Discussions;
using OuaDII.Generators;
using OuaDII.TileContainers;
//using Roguelike.Abstract.Projectiles;
//using Roguelike.Crafting;
//using Roguelike.LootFactories;
//using Roguelike.Managers;
//using Roguelike.Policies;
//using Roguelike.Strategy;
using SimpleInjector;

namespace OuaDII
{
  public class ContainerConfigurator : Dungeons.IContainerConfigurator
  {
    public Container Container { get; set; }
    public static Container LastOne { get; set; }

    public ContainerConfigurator()
    {
      var container = new Container();
      Configure(container);

      Container = container;
      LastOne = container;
    }

    protected virtual void Configure(Container container)
    {
      container.Options.ConstructorResolutionBehavior = new GreediestConstructorBehavior();
      container.Options.ResolveUnregisteredConcreteTypes = true;

      container.Register<IWorldGenerator, WorldGenerator>();
      container.Register<Roguelike.LootContainers.Inventory, OuaDII.LootContainers.Inventory>();
      container.Register<OuaDII.LootContainers.Inventory>();

      container.Register<Roguelike.Serialization.IPersister, OuaDII.Serialization.JSONPersister>();
      container.Register<OuaDII.Serialization.IPersister, OuaDII.Serialization.JSONPersister>();

      container.Register<Roguelike.Managers.GameManager, Managers.GameManager>(Lifestyle.Singleton);
      container.Register<Managers.GameManager>(Lifestyle.Singleton);

      container.Register<Roguelike.GameContext, OuaDII.GameContext>();
      container.Register<Roguelike.Tiles.LivingEntities.Hero, OuaDII.Tiles.LivingEntities.Hero>();
      container.Register<Dungeons.TileContainers.DungeonNode, Roguelike.Generators.TileContainers.DungeonNode>();
      container.Register<Roguelike.Generators.LootGenerator, LootGenerator>(Lifestyle.Singleton);
      container.Register<Roguelike.Managers.EventsManager>(Lifestyle.Singleton);

      container.Register<Dungeons.Tiles.IDoor, Roguelike.Tiles.Interactive.Door>();
      container.Register<Dungeons.TileContainers.DungeonLevel, Roguelike.TileContainers.GameLevel>();
      container.Register<Roguelike.Tiles.LivingEntities.Enemy, OuaDII.Tiles.LivingEntities.Enemy>();
      container.Register<Roguelike.Generators.RoomContentGenerator, OuaDII.Generators.RoomContentGenerator>();
      container.Register<Roguelike.LootFactories.AbstractLootFactory, Roguelike.LootFactories.LootFactory>();
      container.Register<Roguelike.LootFactories.EquipmentFactory, LootFactories.EquipmentFactory>();
      container.Register<Roguelike.Crafting.LootCrafterBase, Crafting.LootCrafter>();
      container.Register<Roguelike.State.GameState, OuaDII.State.GameState>();
      container.Register<Roguelike.Managers.LootManager, Managers.LootManager>();
      container.Register<Roguelike.Tiles.LivingEntities.NPC, OuaDII.Tiles.LivingEntities.NPC>();
      container.Register<Roguelike.Discussions.DiscussionTopic, OuaDII.Discussions.DiscussionTopic>();
      container.Register<Roguelike.Discussions.Discussion, OuaDII.Discussions.Discussion>();
      container.Register<Dungeons.GenerationInfo, GenerationInfo>();

      RegisterPathFinder(container);

      container.Register<World>();

      RegisterLogger(container);
    }

    protected virtual void RegisterPathFinder(Container container)
    {
      container.Register<Roguelike.Strategy.ITilesAtPathProvider, Roguelike.Strategy.TilesAtPathProvider>();
    }

    protected virtual void RegisterLogger(Container container)
    {
      container.Register<ILogger, Logger>();
    }
  }
}
