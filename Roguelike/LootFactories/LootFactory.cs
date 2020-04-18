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
    
    public LootFactory(Container container) : base(container)
    {
    }

    protected override void Create()
    {
      EquipmentFactory = container.GetInstance<EquipmentFactory>();
      ScrollsFactory = container.GetInstance<ScrollsFactory>();
    }

    public override Loot GetByName(string name)
    {
      //var tile = factory.FirstOrDefault(i => i.Key == name);
      //if (tile.Key != null)
      //  return tile.Value(name);

      return null;
    }

    public override Loot GetRandom()
    {
      return null;
    }

  }
}
