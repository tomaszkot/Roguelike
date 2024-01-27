using Dungeons.Core;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.LootFactories
{
  public abstract class AbstractLootFactory
  {
    protected static T PrepareLoot<T>(string tagPart, T loot) where T : Loot
    {
      if (string.IsNullOrEmpty(loot.tag1))
        loot.tag1 = tagPart;

      return loot;
    }

    protected T GetRandom<T>(Dictionary<string, Func<string, T>> factory) where T : Loot
    {
      var index = RandHelper.GetRandomInt(factory.Count);
      var lootCreator = factory.ElementAt(index);
      var loot = lootCreator.Value(lootCreator.Key);
      return PrepareLoot(lootCreator.Key, loot);
    }
    protected T GetByAsset<T>(Dictionary<string, Func<string, T>> factory, string tag) where T : Loot
    {
      return GetByTag<T>(factory, tag);
    }

    protected T GetByTag<T>(Dictionary<string, Func<string, T>> factory, string tag) where T : Loot
    {
      var tile = factory.FirstOrDefault(i => i.Key.ToLower() == tag.ToLower());
      if (tile.Key != null)
      {
        T loot = tile.Value(tag);
        PrepareLoot(tag, loot);
        return loot;
      }
      return null;
    }

    protected int GetStackableDefaultCount()
    {
      var rand = Enumerable.Range(2, 5).ToList().GetRandomElem();
      return rand;
    }

    protected abstract void Create();
    public abstract Loot GetRandom(int level);// where T : Loot;
    //public abstract Loot GetByName(string name);
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

  public class PrototypeValue
  {
    public int Level { get; set; }
    public bool IsMaterialAware { get; set; }
    public EquipmentMaterial Material { get; set; }

    public string Tag { get; set; }

    public EquipmentKind Kind { get; set; }

    public Weapon.WeaponKind WeaponKind { get; set; }
  }

  
}
