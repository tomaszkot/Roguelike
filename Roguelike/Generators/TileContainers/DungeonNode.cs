using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.Generators.TileContainers
{
  //a single room, can have size like 10x10, used only at the time of generation, then copied to the destination container
  public class DungeonNode : Dungeons.TileContainers.DungeonNode
  {
    public DungeonNode() : base(null)
    {
    }

    public DungeonNode(Container c) : base(c)
    {
    }

    

    public override bool SetTile(Tile tile, Point point, bool resetOldTile = true,
      bool revealReseted = true, bool autoSetTileDungeonIndex = true, bool reportError = true)
    {
      try
      {
        var atPos = tiles[point.Y, point.X];
        if (tile != null && !tile.IsEmpty && atPos != null && !atPos.IsEmpty)
        {
          var allowed = (tile is IDoor && atPos is Wall) || (tile is Wall && atPos is IDoor);
          if (!allowed)
          {
            allowed = tile is Wall && atPos is Wall;
            if (!allowed)
            {
              if (reportError)
                Container.GetInstance<ILogger>().LogError("atPos != null: " + atPos + ", while setting " + tile);
              return false;
            }
          }
        }
        return base.SetTile(tile, point, resetOldTile, revealReseted, autoSetTileDungeonIndex);
      }
      catch (System.Exception ex)
      {
        Debug.WriteLine(ex.Message);
        throw;
      }
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
          reveal = (tiles[row, col] is Wall) && (tiles[row, col] as Wall).IsSide || (tiles[row, col] is Tiles.Interactive.Door);
        }
      }

      return reveal;
    }

    public override Tile SetTileAtRandomPosition(Tile tile, bool matchNodeIndex = true)
    {
      var tileSet = base.SetTileAtRandomPosition(tile, matchNodeIndex);
      if (tileSet != null)
      {
        var doors = GetNeighborTiles<Door>(tileSet);
        if (tileSet is Chest && doors.Any())//chest is blocking!
        {
          var emptyOnes = GetEmptyNeighborhoodTiles(tileSet);
          if (emptyOnes.Any())
          {
            SetTile(tileSet, emptyOnes.First().point);
          }
        }
      }
      return tileSet;
    }

    public T SetTileAtRandomPosition<T>(int levelIndex, bool matchNodeIndex = true) where T : Tile, new()
    {
      var tile = new T();
      var inter = tile as Roguelike.Tiles.Interactive.InteractiveTile;
      if (inter != null)
        inter.Level = levelIndex;
      return SetTileAtRandomPosition(tile, matchNodeIndex) as T;
    }
  }
}
