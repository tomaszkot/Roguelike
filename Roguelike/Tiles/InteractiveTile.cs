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
    private InteractiveTileKind _kind = InteractiveTileKind.Unset;

    public InteractiveTile(char symbol) : base(symbol)
    {

    }

    public InteractiveTileKind Kind
    {
      get => _kind;
      set
      {
        _kind = value;
        if (_kind == InteractiveTileKind.TreasureChest)
        {
          
        }
      }
    }
    public bool CanBeHitBySpell()
    {
      return false;
    }

    public bool OnHitBy(ISpell damager)
    {
      return false;
    }

    public override string ToString()
    {
      var res = base.ToString();
      res += ", " + Kind;
      return res;
    }
  }
}
