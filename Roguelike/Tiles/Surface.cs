using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public enum SurfaceKind { Unset, Empty, ShallowWater, DeepWater, Lava }

  public class Surface : Tile
  {
    SurfaceKind kind;

    public Surface() : base(Constants.SymbolBackground)
    {
#if ASCII_BUILD
      color = ConsoleColor.Green;
#endif
    }

    public SurfaceKind Kind { get => kind; set => kind = value; }
  }
}
