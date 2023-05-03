using Dungeons.Core;
using Roguelike.Abstract.Projectiles;
using Roguelike.Abstract.Spells;
using Roguelike.Core.Managers;
using Roguelike.Policies;
using Roguelike.Crafting;
using Roguelike.Discussions;
using Roguelike.Generators;
using Roguelike.LootContainers;
using Roguelike.LootFactories;
using Roguelike.Managers;
using Roguelike.Serialization;
using Roguelike.State;
using Roguelike.Strategy;
using Roguelike.TileContainers;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;

namespace Roguelike
{
  public static class ContainerConfiguratorExt
  {
    public static bool TryGetInstance<TService>(
      this Container container, out TService instance)
      where TService : class
    {
      IServiceProvider provider = container;
      instance = (TService)provider.GetService(typeof(TService));
      return instance != null;
    }
  }
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
      container.Register<GameManager, GameManager>(Lifestyle.Singleton);
      container.Register<Dungeons.TileContainers.DungeonNode, Roguelike.Generators.TileContainers.DungeonNode>();
      container.Register<Dungeons.Tiles.IDoor, Roguelike.Tiles.Interactive.Door>();

      //container.Register< Dungeons.TileContainers.DungeonLevel, GameLevel>();
      container.Register<Dungeons.TileContainers.DungeonLevel>(() => new GameLevel(container));
      RegisterLogger(container);
      container.Register<LootGenerator, LootGenerator>(Lifestyle.Singleton);
      container.Register<HeroInventoryManager, HeroInventoryManager>();
      container.Register<EventsManager, EventsManager>(Lifestyle.Singleton);
      container.Register<Enemy, Enemy>();
      container.Register<RoomContentGenerator, RoomContentGenerator>();
      container.Register<AbstractLootFactory, LootFactory>();
      container.Register<ProjectileCastPolicy, ProjectileCastPolicy>();
      container.Register<StaticSpellCastPolicy, StaticSpellCastPolicy>();

      container.Register<LootCrafterBase, LootCrafter>();
      container.Register<GameState, GameState>();
      container.Register<LootManager, LootManager>();
      container.Register<NPC, NPC>();

      container.Register<ITilesAtPathProvider, TilesAtPathProvider>();
      container.Register<IProjectilesFactory, ProjectilesFactory>();
      container.Register<IStaticSpellFactory, StaticSpellFactory>();
      container.Register<Dungeons.GenerationInfo, GenerationInfo>();
      container.Register<EquipmentFactory, EquipmentFactory>();

      container.Register<ScrollsFactory, ScrollsFactory>();
      container.Register<MiscLootFactory, MiscLootFactory>();
      container.Register<BooksFactory, BooksFactory>();
      container.Register<Dungeons.DungeonGenerator, LevelGenerator>();
      container.Register<GameContext, GameContext>();
      container.Register<Discussion, Discussion>();
      container.Register<DiscussionTopic, DiscussionTopic>();
      container.Register<CurrentEquipment, CurrentEquipment>();
      container.Register<MovePolicy, MovePolicy>();
      container.Register<MeleeAttackPolicy, MeleeAttackPolicy>();
      container.Register<AlliedEnemy, AlliedEnemy>();

      Container = container;
    }

    protected virtual void RegisterLogger(Container container)
    {
      container.Register<ILogger, Logger>(Lifestyle.Singleton);
    }
  }
}
