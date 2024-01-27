using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
//using static UnityEngine.UIElements.UxmlAttributeDescription;
//using UnityEditor.PackageManager;

namespace Roguelike.Generators.TileContainers
{
  //a single room, can have size like 10x10, used only at the time of generation, then copied to the destination container
  public class DungeonNode : Dungeons.TileContainers.DungeonNode
  {
    //Assets\Scripts\UnityOuaDII\Grids\DungeonRoomGridPredefinied.cs(14,52): error CS0310: 'DungeonNode' must be a non-abstract type with a public parameterless constructor in order to use it as parameter 'T' in the generic type
    public DungeonNode() : base(null)
    {
    }
    public DungeonNode(Container c) : base(c)
    {
    }

    //in the mill they were outside the room
    protected override bool ShallEnsureCorrectY(Dungeons.Tiles.Tile tile)
    {
      return  tile.IsDynamic();
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
              if (tile.GetType() != atPos.GetType())
              {
                if (reportError)
                  Container.GetInstance<ILogger>().LogError("atPos != null: " + atPos + ", while setting " + tile);
                return false;
              }
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

    public override Tile SetTileAtRandomPosition
    (
      Tile tile, 
      bool matchNodeIndex = true,
      EmptyCheckContext emptyCheckContext = EmptyCheckContext.Unset
    )
    {
      if (tile is Loot)
        emptyCheckContext = EmptyCheckContext.DropLoot;

      var tileSet = base.SetTileAtRandomPosition(tile, matchNodeIndex, emptyCheckContext);
      if (tileSet != null)
      {
        var doors = GetNeighborTiles<Tiles.Interactive.Door>(tileSet);
        if (tileSet is Chest && doors.Any())//chest is blocking!
        {
          var emptyOnes = GetEmptyNeighborhoodTiles(tileSet);
          if (emptyOnes.Any())
          {
            SetTile(tileSet, emptyOnes.First().point);
            var ets = GetEmptyTiles().Where(i=> !GetNeighborTiles<Tiles.Interactive.Door>(i).Any());
            if(ets.Any())
              SetTile(tileSet, ets.First().point);
          }
        }
      }
      if (tileSet == null)
      {
        //int k = 0;
        //k++;
      }
      return tileSet;
    }

    public T SetTileAtRandomPosition<T>(int levelIndex, T tile, bool matchNodeIndex = true) where T : Tile
    {
      return InitTile(levelIndex, matchNodeIndex, tile);
    }

    //public T SetTileAtRandomPosition<T>(int levelIndex, T tile, Container container, bool matchNodeIndex = true) where T : Tile
    //{
    //  return InitTile(levelIndex, matchNodeIndex, tile);
    //}

    public T SetTileAtRandomPosition<T>(int levelIndex, Container container, bool matchNodeIndex = true) where T : Enemy
    {
      var tile = container.GetInstance<T>();
      return InitTile(levelIndex, matchNodeIndex, tile);
    }

    public T SetTileAtRandomPosition<T>(int levelIndex, bool matchNodeIndex = true) where T : Tile, new()
    {
      var tile = new T();
      return SetTileAtRandomPosition(levelIndex, tile, matchNodeIndex);
    }

    private T InitTile<T>(int levelIndex, bool matchNodeIndex, T tile) where T : Tile
    {
      var inter = tile as Roguelike.Tiles.Interactive.InteractiveTile;
      if (inter != null)
        inter.Level = levelIndex;
      return SetTileAtRandomPosition(tile, matchNodeIndex) as T;
    }

    public override void InteriorShadowed(Point pt)
    {
      if (GetTile(pt) is Wall wall)
      {
        base.InteriorShadowed(pt);
        TryAddDecor(wall);
      }
    }

    public void TryAddDecor(Wall wall)
    {
      if (RandHelper.GetRandomDouble() > 0.8)
      {
        wall.Child = new Candle(Container);
        wall.Child.point = wall.point;
      }
      else if (RandHelper.GetRandomDouble() > 0.8)
      {
        wall.Child = new TorchSlot(Container);
        wall.Child.point = wall.point;
      }
      else if (RandHelper.GetRandomDouble() > 0.85)
      {
        wall.Child = new WallDecoration();
        wall.Child.point = wall.point;
      }
    }
  }
}
