using System;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;

namespace Roguelike.LootFactories
{
  public class MiscLootFactory : AbstractLootFactory
  {
    protected Dictionary<string, Func<string, Loot>> factory =
     new Dictionary<string, Func<string, Loot>>();

    public MiscLootFactory(Container container) : base(container)
    {
    }

    protected override void Create()
    {
      Func<string, Loot> createPotion = (string tag) =>
      {
        var loot = new Potion();
        loot.tag1 = tag;
        if (tag == "poison_potion")
          loot.SetKind(PotionKind.Poison);
        else if (tag == "health_potion")
          loot.SetKind(PotionKind.Health);
        else if (tag == "mana_potion")
          loot.SetKind(PotionKind.Mana);
        return loot;
      };
      var names = new[] { "poison_potion", "health_potion", "mana_potion" };
      foreach (var name in names)
      {
        factory[name] = createPotion;
      }
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

    public override Loot GetRandom()
    {
      return GetRandom<Loot>(factory);
    }
  }
}
