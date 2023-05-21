using Dungeons.Core;
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
        if (tag == "cracked_stone_scroll")
        {
          int k = 0; 
          k++;

        }

        var scroll = tag == "swiatowit_scroll" ? new SwiatowitScroll() :  new Scroll();
        scroll.tag1 = tag;
        scroll.Kind = Scroll.DiscoverKindFromName(tag);
        scroll.Count = Enumerable.Range(1, 3).ToList().GetRandomElem();
        return scroll;
      };
      var names = new[] { "fire_ball_scroll" , "ice_ball_scroll", "poison_ball_scroll",
        "identify_scroll", "teleport_scroll", "portal_scroll", "transform_scroll", "mana_shield_scroll",
        "skeleton_scroll", "dziewanna_scroll", "swarog_scroll", "swiatowit_scroll", "swap_positions_scroll",
        "cracked_stone_scroll", "perun_scroll", "frighten_scroll"
      };
      foreach (var name in names)
        factory[name] = createScroll;
    }

    public override Loot GetRandom(int level)
    {
      return GetRandom<Scroll>(factory);
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

    public Loot GetByKind(Spells.SpellKind kind)
    {
      var tile = factory.FirstOrDefault(i => Scroll.DiscoverKindFromName(i.Key) == kind);
      if (tile.Key != null)
        return tile.Value(tile.Key);

      return null;
    }

  }
}
