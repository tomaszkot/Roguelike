using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.LootFactories
{
  public class ScrollsFactory : AbstractLootFactory
  {
    protected Dictionary<string, Func<string, Scroll>> factory = 
      new Dictionary<string, Func<string, Scroll>>();

    public ScrollsFactory(Container container) : base(container)
    {
    }

    protected override void Create()
    {
      Func<string, Scroll> createScroll = (string tag) =>
      {
        var scroll = new Scroll();
        scroll.tag1 = tag;
        scroll.Kind = Scroll.DiscoverKindFromName(tag);
        return scroll;
      };
      var names = new[] { "FireBallScroll" , "IdentifyScroll", "TeleportScroll", "PortalScroll" };
      foreach(var name in names)
        factory[name] = createScroll;
    }

    public override Loot GetRandom()
    {
      return GetRandom<Scroll>(factory);
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

    //public override Loot GetByTag(string tagPart)
    //{
    //  var sc = scrolls.Where(i => i.tag1  == tagPart).SingleOrDefault();
    //  return sc.CloneAsScroll();
    //}

    public Loot GetByKind(Spells.SpellKind kind)
    {
      var tile = factory.FirstOrDefault(i => Scroll.DiscoverKindFromName(i.Key) == kind);
      if (tile.Key != null)
        return tile.Value(tile.Key);

      return null;
    }

  }
}
