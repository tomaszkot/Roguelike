using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.LootFactories
{
  public abstract class LootFactory
  {
    protected abstract void Create();
    public abstract Loot GetRandom(); //where T : Loot;
    public abstract Loot GetByName(string name);

    public LootFactory()
    {
      Create();
    }
  }


  public abstract class EquipmentTypeFactory : LootFactory
  {
    protected Dictionary<string, Func<string, Roguelike.Tiles.Equipment>> factory = new Dictionary<string, Func<string, Roguelike.Tiles.Equipment>>();

    public override Loot GetByName(string name)
    {
      var tile = factory.FirstOrDefault(i => i.Key == name);
      if (tile.Key != null)
        return tile.Value(name);

      return null;
    }

    public override Loot GetRandom()
    {
      return null;
    }
  };
}
