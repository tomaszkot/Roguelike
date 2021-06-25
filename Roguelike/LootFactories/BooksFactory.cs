using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.LootFactories
{
  public class BooksFactory : AbstractLootFactory
  {
    protected Dictionary<string, Func<string, Book>> factory =
      new Dictionary<string, Func<string, Book>>();

    public BooksFactory(Container container) : base(container)
    {
    }

    protected override void Create()
    {
      Func<string, Book> createBook = (string tag) =>
      {
        var book = new Book();
        book.tag1 = tag;
        book.Kind = Scroll.DiscoverKindFromName(tag);
        //scroll.Count = Enumerable.Range(1, 3).ToList().GetRandomElem();
        return book;
      };
      var names = new[] { "fire_ball_book", "ice_ball_book", "poison_ball_book",
        "identify_book", "portal_book", "mana_shield_book", "skeleton_book"
        //"teleport_scroll" "transform_scroll"
        };
      foreach (var name in names)
        factory[name] = createBook;
    }

    public override Loot GetRandom(int level)
    {
      return GetRandom<Book>(factory);
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
