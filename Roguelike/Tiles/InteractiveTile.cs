using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public enum InteractiveTileKind
  {
    Unknown, Stairs, Doors, Barrel, TreasureChest,
    Trap, Lever
  }

  public class InteractiveTile : Dungeons.Tiles.Tile
  {
    public InteractiveTile(char symbol) : base(symbol)
    {

    }

    public InteractiveTileKind Kind { get; set; } = InteractiveTileKind.Unknown;
  }
}
