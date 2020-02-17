using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelike.Abstract;

namespace Roguelike.Tiles
{
  public enum InteractiveTileKind
  {
    Unset, Stairs, Doors, Barrel, TreasureChest,
    Trap, Lever
  }

  public class InteractiveTile : Dungeons.Tiles.Tile, IObstacle
  {
    public InteractiveTile(char symbol) : base(symbol)
    {

    }

    public InteractiveTileKind Kind { get; set; } = InteractiveTileKind.Unset;

    public bool CanBeHitBySpell()
    {
      return false;
    }

    public bool OnHitBy(IMovingDamager damager)
    {
      return false;
    }
  }
}
