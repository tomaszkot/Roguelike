using System.Drawing;
using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Tiles.Interactive;
using SimpleInjector;

namespace Roguelike.Generators.TileContainers
{
  //a single room, can have size like 10x10, used only at the time of generation, then copied to the destination container
  public class DungeonNode : Dungeons.TileContainers.DungeonNode
  {
    public DungeonNode(Container c) : base(c)
    {
    }

    protected override Dungeons.Tiles.Door CreateDoorInstance()
    {
      return new Tiles.Door();
    }

    public override bool SetTile(Tile tile, Point point, bool resetOldTile = true, bool revealReseted = true, bool autoSetTileDungeonIndex = true)
    {
      var atPos = tiles[point.Y, point.X];
      if (tile != null && !tile.IsEmpty && atPos != null && !atPos.IsEmpty)
      {
        var allowed = (tile is Door && atPos is Wall) || (tile is Wall && atPos is Door)
           //|| (tile is Door && atPos is Door)
           ;
        if (!allowed)
        {
          allowed = tile is Wall && atPos is Wall;
          if (!allowed)
          {
            Container.GetInstance<ILogger>().LogError("atPos != null: " + atPos + ", while setting " + tile);
            return false;
          }
        }
      }
      return base.SetTile(tile, point, resetOldTile, revealReseted, autoSetTileDungeonIndex);
    }

    protected override bool ShallReveal(int row, int col)
    {
      var reveal = tiles[row, col].DungeonNodeIndex == NodeIndex; 
      if (!reveal)
      {
        if (tiles[row, col].IsFromChildIsland)
        {
          //if (tiles[row, col] is Tiles.Door)
          //{
          //  int k = 0;
          //}
          reveal = (tiles[row, col] is Wall) && (tiles[row, col] as Wall).IsSide || (tiles[row, col] is Dungeons.Tiles.Door);
        }
      }

      return reveal;
    }
  }
}
