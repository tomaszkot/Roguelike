using Dungeons.Core;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public abstract Loot GetRandom(int level);// where T : Loot;
    public abstract Loot GetByName(string name);
    public abstract Loot GetByAsset(string tagPart);
    protected Container container;

    public AbstractLootFactory(Container container)
    {
      this.container = container;
      Create();
    }

    public virtual IEnumerable<Loot> GetAll()
    {
      return new List<Loot>();
    }
  }
  
  public abstract class EquipmentTypeFactory : AbstractLootFactory
  {
    protected Dictionary<string, Func<string, Equipment>> factory = new Dictionary<string, Func<string, Equipment>>();
    //public List<string> UniqueItemTags { get; protected set; } = new List<string>();
    protected Dictionary<string, Roguelike.Tiles.Equipment> prototypes = new Dictionary<string, Roguelike.Tiles.Equipment>();

    public EquipmentTypeFactory(Container container) : base(container)
    {
    }

    public List<Roguelike.Tiles.Equipment> GetUniqueItems(int level)
    {
      return prototypes.Where(i => i.Value.Class == Roguelike.Tiles.EquipmentClass.Unique && i.Value.LevelIndex <= level).Select(i => i.Value).ToList();
    }

    public override Loot GetRandom(int level)
    {
      var lootProto = GetRandromFromPrototype(level);
      if (lootProto != null)
        return lootProto;

      Equipment loot = GetRandomFromAll();

      return loot;
    }

    protected Equipment GetRandomFromAll()
    {
      var index = RandHelper.GetRandomInt(factory.Count);
      var lootCreator = factory.ElementAt(index);
      var loot = lootCreator.Value(lootCreator.Key);
      return loot;
    }

    protected virtual Loot GetRandromFromPrototype(int level)
    {
      if (prototypes.Any())
      {
        var plains = prototypes.Values.Where(i => i.Class == EquipmentClass.Plain).ToList();

        var eqsAtLevel = plains.Where(i => i.LevelIndex == level).ToList();
        if (!eqsAtLevel.Any())//TODO!
        {
          var maxLevel = plains.Max(i => i.LevelIndex);
          eqsAtLevel = plains.Where(i => i.LevelIndex == maxLevel).ToList();
        }
        var eq = RandHelper.GetRandomElem<Equipment>(eqsAtLevel);
        if (eq != null)
        {
          var eqCreator = factory[eq.tag1];
          return eqCreator(eq.tag1);
        }
        Debug.Assert(false);
      }
      return null;
    }

    public override Loot GetByName(string name)
    {
      return GetByAsset(name);
    }

    public override Loot GetByAsset(string tagPart)
    {
      var tile = factory.FirstOrDefault(i => i.Key == tagPart);
      if (tile.Key != null)
        return tile.Value(tagPart);

      return null;
    }

    public override IEnumerable<Loot> GetAll()
    {
      List<Loot> loot = new List<Loot>();
      foreach (var lc in factory)
        loot.Add(lc.Value(lc.Key));

      return loot;
    }

  };
}
