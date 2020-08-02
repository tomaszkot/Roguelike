using Dungeons.Core;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.LootFactories
{
  public abstract class AbstractLootFactory
  {
    protected T GetRandom<T>(Dictionary<string, Func<string, T>> factory)
    {
      var index = RandHelper.GetRandomInt(factory.Count);
      var lootCreator = factory.ElementAt(index);
      return lootCreator.Value(lootCreator.Key);
    }

    protected abstract void Create();
    public abstract Loot GetRandom();// where T : Loot;
    public abstract Loot GetByName(string name);
    public abstract Loot GetByTag(string tagPart);
    protected Container container;

    public AbstractLootFactory(Container container)
    {
      this.container = container;
      Create();
    }

    
  }
  
  public abstract class EquipmentTypeFactory : AbstractLootFactory
  {
    protected Dictionary<string, Func<string, Equipment>> factory = new Dictionary<string, Func<string, Equipment>>();

    public EquipmentTypeFactory(Container container) : base(container)
    {
    }

    public override Loot GetRandom()
    {
      var index = RandHelper.GetRandomInt(factory.Count);
      var lootCreator = factory.ElementAt(index);
      return lootCreator.Value(lootCreator.Key);
    }

    public override Loot GetByName(string name)
    {
      return GetByTag(name);
    }

    public override Loot GetByTag(string tagPart)
    {
      var tile = factory.FirstOrDefault(i => i.Key == tagPart);
      if (tile.Key != null)
        return tile.Value(tagPart);

      return null;
    }

    
  };
}
