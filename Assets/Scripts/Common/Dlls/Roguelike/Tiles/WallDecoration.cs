using Dungeons.Tiles;
using Roguelike.Tiles.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public enum WallDecorationKind
  { 
    Unset, Candle, Torch, Gargoyle 
  }

  public interface IWallDecoration
  {
        public WallDecorationKind DecorationKind { get; set; }
    }

  public class WallDecoration : Tile, IWallDecoration
  {
    WallDecorationKind decorationKind;

    public WallDecoration() : base('#')
    {
      tag1 = "WallDecoration";
      Name = "WallDecoration";
      DecorationKind = WallDecorationKind.Gargoyle;

    }

    public WallDecorationKind DecorationKind
    { 
      get => decorationKind; 
      set {
        decorationKind = value;
        if (decorationKind == WallDecorationKind.Gargoyle)
        {
          tag1 = "gargoyle1";
        }
      }
    }
  }
}
