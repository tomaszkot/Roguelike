using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.LootFactories
{
  public class LootFactory : AbstractLootFactory
  {
    public EquipmentFactory EquipmentFactory { get; set; }
    public ScrollsFactory ScrollsFactory { get; set; }
    List<AbstractLootFactory> factories = new List<AbstractLootFactory>();

    public LootFactory(Container container) : base(container)
    {
    }

    protected override void Create()
    {
      EquipmentFactory = container.GetInstance<EquipmentFactory>();
      ScrollsFactory = container.GetInstance<ScrollsFactory>();
      factories.Add(EquipmentFactory);
      factories.Add(ScrollsFactory);
    }

    public override Loot GetByTag(string tagPart)
    {
      foreach (var fac in factories)
      {
        var loot = fac.GetByTag(tagPart);
        if (loot != null)
          return loot;
      }

      return null;
    }

    public override Loot GetByName(string name)
    {
      foreach (var fac in factories)
      {
        var loot = fac.GetByName(name);
        if (loot != null)
          return loot;
      }
      
      return null;
    }

    public override Loot GetRandom()
    {
      return null;
    }

  }
}
