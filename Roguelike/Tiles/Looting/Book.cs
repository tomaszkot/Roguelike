using Roguelike.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public class Book : SpellSource
  {
    public Book() : this(SpellKind.Unset)
    {

    }

    public Book(SpellKind kind = SpellKind.Unset) : base(kind)
    {
      LootKind = LootKind.Food;
      Price *= 10;
    }

    public static SpellKind DiscoverKindFromName(string name)//->name fire_ball -> FireBall
    {
      return DiscoverKindFromName(name, false);
    }
  }
}
