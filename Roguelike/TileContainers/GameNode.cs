using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Generators.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.TileContainers
{
  //a single room, can have size like 100x100 (DungeonLevel) or 1000x1000 (World)
  public class GameNode : Generators.TileContainers.DungeonNode
  {
    public static Tile EmptyTile = new Tile(symbol: Constants.SymbolBackground);
    Dictionary<Point, Loot> loot = new Dictionary<Point, Tiles.Loot>();
    public Dictionary<Point, Loot> Loot { get => loot; set => loot = value; }
    public ILogger Logger { get; set; }
    public virtual string Name { get; set; } = "";

    public GameNode() : this(10, 10)
    {
    }
    public GameNode(int width, int height) : this(width, height, null)
    {
    }
    
    public GameNode(int width = 10, int height = 10, Dungeons.GenerationInfo gi = null,
                     int nodeIndex = DefaultNodeIndex, Generators.TileContainers.DungeonNode parent = null)
   : base(width, height, gi, nodeIndex, parent)
    {

    }

    public override Dungeons.DungeonNode CreateChildIslandInstance(int w, int h, GenerationInfo gi, Dungeons.DungeonNode parent)
    {
      return new Generators.TileContainers.DungeonNode(w, h, gi, parent: this);
    }
    
    public override bool SetTile(Tile tile, Point point, bool resetOldTile = true, bool revealReseted = true)
    {
      if (tile is Enemy)
      {
        int k = 0;
        k++;
      }
      if (tile is Hero)
      {
        var tileAtPoint = GetTile(point);
        if (tileAtPoint == tile)
          return true;
        if (tileAtPoint is Stairs)
        {
          Debug.Assert(false);
        }
      }
      else if (tile is Roguelike.Tiles.Loot)
      {
        if (Loot.ContainsKey(point))// && loot[point] != null)
        {
          if (Logger != null)
            Logger.LogError("loot already at point: " + Loot[point] + ", trying to add: " + tile);
          return false;
        }
        tile.Point = point;
        Loot[point] = tile as Roguelike.Tiles.Loot;
        return true;
      }
      Point? prevPos = tile?.Point;
      var res =  base.SetTile(tile, point, resetOldTile, revealReseted);
      if (res && tile is LivingEntity && prevPos!=null)
      {
        (tile as LivingEntity).PrevPoint = prevPos.Value;
      }
      return res;
    }

    public Tile GetRandomEmptyTile(GenerationConstraints constraints = null, bool canBeNextToDoors = true)
    {
      List<Tile> emptyTiles = GetRandomEmptyTiles(constraints, canBeNextToDoors);

      if (emptyTiles.Any())
      {
        var emptyTileIndex = random.Next(emptyTiles.Count);
        return emptyTiles[emptyTileIndex];
      }

      return null;
    }

    private List<Tile> GetRandomEmptyTiles(GenerationConstraints constraints = null, bool canBeNextToDoors = true)
    {
      List<Tile> emptyTiles = GetEmptyTiles(constraints);
      if (constraints != null && constraints.Tiles != null)
      {
        emptyTiles = emptyTiles.Where(i => constraints.Tiles.Contains(i)).ToList();
      }
      if (!canBeNextToDoors)
      {
        emptyTiles = emptyTiles.Where(i => !GetNeighborTiles(i).Any(j => j is Dungeons.Tiles.Door)).ToList();
      }

      return emptyTiles;
    }

    public bool RemoveLoot(Point point)
    {
      if (Loot.ContainsKey(point))
      {
        Loot.Remove(point);
        return true;
      }

      return false;
    }

    [JsonIgnore]
    public List<Generators.TileContainers.DungeonNode> Nodes
    {
      get { return Parts[0].Parts.Cast<Generators.TileContainers.DungeonNode>().ToList(); }
    }

    Generators.TileContainers.DungeonNode GetNodeFromTile(Tile tile)
    {
      var parts = Parts;
      return Nodes.Where(i => i.NodeIndex == tile.DungeonNodeIndex).Single() as Generators.TileContainers.DungeonNode;
    }

    Generators.TileContainers.DungeonNode GetChildIslandFromTile(Tile tile)
    {
      foreach (var node in Nodes)
      {
        var isl = node.ChildIslands.FirstOrDefault(i => i.NodeIndex == tile.DungeonNodeIndex);
        if (isl != null)
          return isl as Generators.TileContainers.DungeonNode;
      }

      return null;
    }

    public virtual bool RevealRoom(Tiles.Door door, Roguelike.Tiles.Hero hero)
    {
      if (door.IsFromChildIsland)
      {
        // var neib = GetNeighborTiles(door).Where(i => i.DungeonNodeIndex != door.DungeonNodeIndex && i != hero).FirstOrDefault();
        var node = GetChildIslandFromTile(door);
        var parts = Parts;
        node.Reveal(true);
      }
      else
      {
        var neib = GetNeighborTiles(door).Where(i => i.DungeonNodeIndex != door.DungeonNodeIndex && i != hero).FirstOrDefault();
        if (neib != null)
        {
          var parts = Parts;
          GetNodeFromTile(neib).Reveal(true);
        }
      }
      door.Opened = true;
      return true;
    }

    public Tiles.Loot GetLootTile(Point point)
    {
      if (Loot.ContainsKey(point))
        return Loot[point];

      return null;
    }

    private byte[,] InitMatrixBeforePathSearch(Point from, Point end, bool forHeroAlly, bool canGoOverCrackedStone)
    {
      var findPathMatrix = new byte[Height, Width];
      int width = Width;
      int height = Height;
      for (int col = 0; col < width; col++)
      {
        for (int row = 0; row < height; row++)
        {
          byte value = 1;
          findPathMatrix[row, col] = value;

          var tile = Tiles[row, col];
          if (tile is Hero)
          {
            if (forHeroAlly)
              findPathMatrix[row, col] = 0;
            continue;
          }

          //if (!forHeroAlly && tile is Trap && (tile as Trap).SetUp)
          //{
          //  continue;
          //}

          if (tile is Dungeons.Tiles.IObstacle)
          {
            if (forHeroAlly && tile is LivingEntity)
            {
              int k = 0;
              k++;
            }
            //else if (!forHeroAlly && tile is CrackedStone)
            //{
            //  //let attack it
            //  value = canGoOverCrackedStone ? (byte)1 : (byte)0;
            //}
            else
            {
              value = 0;//0
            }
          }
          else if (tile is Wall)
            value = 0;//0
          else if (tile == null)
            value = 0;//0 mean can not move
          else
          {
            if (tile.Point.X == from.X && tile.Point.Y == from.Y)
            {
              continue;
            }
            if (tile.Point.X == end.X && tile.Point.Y == end.Y)
            {
              continue;
            }
            if (
               tile == null
              || tile is Loot
              || (tile is Dungeons.Tiles.Door  /*&& !EnemyCanPassDoors*/)

              )
              value = 0;
            if (tile is LivingEntity && !forHeroAlly)
            {
              value = 0;
            }
          }

          findPathMatrix[row, col] = value;

        }
      }

      return findPathMatrix;
    }

    public List<Algorithms.PathFinderNode> FindPath(Point from, Point endPoint, bool forHeroAlly, bool canGoOverCrackedStone)
    {
      //Commons.TimeTracker tr = new Commons.TimeTracker();

      var startPoint = new Algorithms.Point(from.Y, from.X);
      var findPathMatrix = InitMatrixBeforePathSearch(from, endPoint, forHeroAlly, canGoOverCrackedStone);

      var mPathFinder = new Algorithms.PathFinder(findPathMatrix);
      mPathFinder.Diagonals = false;

      var path = mPathFinder.FindPath(startPoint, new Algorithms.Point(endPoint.Y, endPoint.X));
      if (path != null)
      {
        //System.Diagnostics.//Debug.WriteLine("FindPathTest len = " + path.Count);
      }
      //if (worstPathFind < tr.TotalMiliseconds)
      //    worstPathFind = tr.TotalMiliseconds;
      //Log.AddInfo("FindPathTest end time : " + tr.TotalMiliseconds + ", worstPathFind = " + worstPathFind);
      return path;
    }

    public override string ToString()
    {
      return GetType().Name + " " + GetHashCode() + " " + base.ToString();
    }
  }
}
