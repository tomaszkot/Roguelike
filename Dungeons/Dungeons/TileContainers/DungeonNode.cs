﻿using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

namespace Dungeons
{
  public class NodeRevealedParam
  {
    public IList<Tile> Tiles { get; set; }
    public int NodeIndex { get; set; }
  }

  public class PrintInfo
  {
    public bool PrintNodeIndexes = false;
    public int OriginX { get; set; }
    public int OriginY { get; set; }
  }


  public enum EntranceSide { Left, Right, Top, Bottom };
  public enum TileCorner { NorthWest, SouthWest, NorthEast, SouthEast }
  public enum TileNeighborhood { North, South, East, West }
  public enum Interior { T, L };//shape of the interior

  namespace TileContainers
  {

    public class ChildIslandCreationInfo
    {
      public DungeonNode Child { get; set; }
      public GenerationInfo GenerationInfoIsl { get; set; }
      public DungeonNode ParentDungeonNode { get; set; }
    }

    //a single room - typically size of 20x20 tiles
    [XmlRoot("Node", Namespace = "DungeonNode")]
    [XmlInclude(typeof(Wall))]
    public class DungeonNode
    {
      [XmlIgnore]
      //[JsonIgnore]
      protected Tile[,] tiles;
      protected GenerationInfo generationInfo;
      protected static Random random;

      [XmlIgnore]
      [JsonIgnore]
      List<DungeonNode> childIslands = new List<DungeonNode>();

      //sides are borders of the dungeon node
      [XmlIgnore]
      [JsonIgnore]
      Dictionary<EntranceSide, List<Wall>> sides = new Dictionary<EntranceSide, List<Wall>>();

      /// <summary>
      /// Dungeons appended as child of this one.
      /// </summary>
      private List<DungeonNode> parts = new List<DungeonNode>();

      List<Door> doors = new List<Door>();

      [XmlIgnore]
      //[JsonIgnore]
      public Tile[,] Tiles
      {
        get { return tiles; }
        set { tiles = value; }
      }
      public int Width { get { return tiles.GetLength(1); } }
      public int Height { get { return tiles.GetLength(0); } }
      [XmlIgnore]
      [JsonIgnore]
      internal Dictionary<EntranceSide, List<Wall>> Sides { get { return sides; } }
      protected List<TileNeighborhood> allNeighborhoods = new List<TileNeighborhood> { TileNeighborhood.East, TileNeighborhood.West, TileNeighborhood.North, TileNeighborhood.South };

      //it's assummed, level has there are less rooms that 999. 
      public const int DefaultNodeIndex = 999;
      //public const int ChildIslandNodeIndex1 = 998;
      //public const int ChildIslandNodeIndex2 = 997;

      public const int ChildIslandNodeIndex = -1;
      public static int NextChildIslandId = ChildIslandNodeIndex;
      int nodeIndex;
      DungeonNode parent;

      public event EventHandler<GenericEventArgs<Tile>> OnTileRevealed;
      public event EventHandler<ChildIslandCreationInfo> ChildIslandCreated;
      NodeInteriorGenerator interiorGenerator;
      bool revealed;

      [JsonIgnore]
      public Container Container { get; set; }

      //ctors
      static DungeonNode()
      {
        random = new Random();
      }

      public DungeonNode(Container container)
      {
        this.Container = container;
      }

      public void Create(int width = 10, int height = 10, GenerationInfo info = null,
                         int nodeIndex = DefaultNodeIndex, DungeonNode parent = null, bool generateContent = true)
      {
        Create(null, info, nodeIndex, parent);
        tiles = new Tile[height, width];

        if (generateContent)
        {
          GenerateContent();
        }
      }

      public void Create(Tile[,] tiles, GenerationInfo gi = null, int nodeIndex = DefaultNodeIndex, DungeonNode parent = null)
      {
        this.Parent = parent;
        this.NodeIndex = nodeIndex;

        this.generationInfo = gi;
        this.interiorGenerator = new NodeInteriorGenerator(this, generationInfo);
        interiorGenerator.ChildIslandCreated += (s,e) => 
        {
          ChildIslandCreated?.Invoke(s, e);
        };
        this.tiles = tiles;
      }

      public virtual string Description
      {
        get { return GetType().Name; }
      }

      public Point? AppendMazeStartPoint { get; set; }

      public int NodeIndex
      {
        get
        {
          return nodeIndex;
        }

        set
        {
          nodeIndex = value;
        }
      }

      [XmlIgnore]
      [JsonIgnore]
      public DungeonNode Parent
      {
        get
        {
          return parent;
        }

        set
        {
          parent = value;
        }
      }

      public virtual List<DungeonNode> Parts
      {
        get
        {
          return parts;
        }
        set { parts = value; }
      }

      public virtual List<DungeonNode> ChildIslands
      {
        get
        {
          return childIslands;
        }
      }

      //methods

      internal void GenerateLayoutDoors(EntranceSide side, DungeonNode nextNode)
      {
        List<Wall> wall = sides[side];
        for (int i = 0; i < wall.Count; i++)
        {
          if (i > 0 && i % 2 == 0)// && (side != EntranceSide.Bottom || i < nextNode.Width))
            CreateDoor(wall[i]);
        }
      }

      protected virtual void GenerateContent()
      {
        if (generationInfo == null)
          return;

        Sides.Add(EntranceSide.Top, new List<Wall>());
        Sides.Add(EntranceSide.Bottom, new List<Wall>());
        Sides.Add(EntranceSide.Left, new List<Wall>());
        Sides.Add(EntranceSide.Right, new List<Wall>());


        if (generationInfo.GenerateEmptyTiles)
          PlaceEmptyTiles();
        if (generationInfo.GenerateOuterWalls)
          GenerateOuterWalls();

        if (generationInfo.GenerateRandomInterior)
        {
          interiorGenerator.GenerateRandomInterior();

          GenerateRandomStonesBlocks();
        }

        Reveal(generationInfo.RevealTiles);
      }

      protected void GenerateRandomStonesBlocks()
      {
        interiorGenerator.GenerateRandomStonesBlocks();
      }

      public bool IsCornerWall(Wall wall)
      {
        var neibs = GetNeighborTiles(wall).Where(i => i is Wall).ToList();
        if (neibs.Count >= 3)
          return true;
        if (neibs.Count != 2)
          return false;
        if (neibs.Count(i => i.Point.X == wall.Point.X) == 2 ||
          neibs.Count(i => i.Point.Y == wall.Point.Y) == 2)
          return false;
        return true;

      }

      public List<T> GetNeighborTiles<T>(Tile tile) where T : Tile
      {
        return GetNeighborTiles(tile).Where(i => i != null && i.GetType() == typeof(T)).Cast<T>().ToList();
      }

      public List<Tile> GetNeighborTiles(Tile tile, bool incDiagonal = false)
      {
        var neibs = new List<Tile>();
        foreach (var i in allNeighborhoods)
        {
          var neib = GetNeighborTile(tile, i);
          neibs.Add(neib);
          if (incDiagonal && neib != null)
          {
            if (i == TileNeighborhood.North || i == TileNeighborhood.South)
            {
              neibs.Add(GetNeighborTile(neib, TileNeighborhood.East));
              neibs.Add(GetNeighborTile(neib, TileNeighborhood.West));
            }
            else
            {
              neibs.Add(GetNeighborTile(neib, TileNeighborhood.North));
              neibs.Add(GetNeighborTile(neib, TileNeighborhood.South));
            }
          }
        }


        return neibs;
      }


      public override string ToString()
      {
        return NodeIndex + " [" + Width + "," + Height + "]";
      }

      public void SetTilesNodeIndex()
      {
        //GameManager.Instance.Assert(NodeIndex >= 0); this crashes win 10 store app
        foreach (var tile in Tiles)
        {
          if (tile != null)
            tile.DungeonNodeIndex = NodeIndex;
        }
      }

      public Tile GetNeighborTile(Tile tile, TileNeighborhood neighborhood)
      {
        var pt = GetNeighborPoint(tile, neighborhood);
        return GetTile(pt);
      }

      public Point GetNeighborPoint(Tile tile, TileNeighborhood neighborhood)
      {
        Point pt = tile.Point;
        switch (neighborhood)
        {
          case TileNeighborhood.North:
            pt.Y -= 1;
            break;
          case TileNeighborhood.South:
            pt.Y += 1;
            break;
          case TileNeighborhood.East:
            pt.X += 1;
            break;
          case TileNeighborhood.West:
            pt.X -= 1;
            break;
          default:
            break;
        }

        return pt;
      }

      public Point? GetFirstEmptyPoint()
      {
        var tile = GetEmptyTiles().FirstOrDefault();
        return tile?.Point;
      }

      public virtual List<Tile> GetEmptyTiles(GenerationConstraints constraints = null, bool canBeNextToDoors = true)
      {
        var emptyTiles = new List<Tile>();
        DoGridAction((int col, int row) =>
        {
          if (IsTileEmpty(tiles[row, col])// != null && tiles[row, col].IsEmpty  //null can be outside the walls
          )
          {
            var pt = new Point(col, row);

            if (constraints == null || (constraints.IsInside(pt)))
              emptyTiles.Add(tiles[row, col]);
          }
        });
        //if (constraints != null && constraints.Tiles != null)
        //{
        //  emptyTiles = emptyTiles.Where(i => constraints.Tiles.Contains(i)).ToList();
        //}
        if (!canBeNextToDoors)
        {
          emptyTiles = emptyTiles.Where(i => !GetNeighborTiles(i).Any(j => j is Dungeons.Tiles.Door)).ToList();
        }
        return emptyTiles;
      }

      public Tile GetRandomEmptyTile(GenerationConstraints constraints = null, bool canBeNextToDoors = true, int? nodeIndex = null)
      {
        var emptyTiles = GetEmptyTiles(constraints, canBeNextToDoors);
        if (nodeIndex != null)
          emptyTiles = emptyTiles.Where(i => i.dungeonNodeIndex == nodeIndex.Value).ToList();

        if (emptyTiles.Any())
        {
          var emptyTileIndex = random.Next(emptyTiles.Count);
          var res = emptyTiles[emptyTileIndex];
          Container.GetInstance<ILogger>().LogInfo("GetRandomEmptyTile: "+res);
          return res;
        }

        return null;
      }

      public virtual Tile GetTile(Point point)
      {
        if (point.X < 0 || point.Y < 0)
          return null;
        if (point.X >= Width || point.Y >= Height)
          return null;
        return tiles[point.Y, point.X];
      }

      public virtual bool SetTile(Tile tile, Point point,
        bool resetOldTile = true, bool revealReseted = true, bool autoSetTileDungeonIndex = true)
      {
        if (point.X < 0 || point.Y < 0)
        {
          Container.GetInstance<ILogger>().LogError("SetTile failed for: " + point, true);
          return false;
        }
        if (AppendMazeStartPoint != null)
        {
          point.X -= AppendMazeStartPoint.Value.X;
          point.Y -= AppendMazeStartPoint.Value.Y;
        }
        if (point.X >= Width || point.Y >= Height)
        {
          Container.GetInstance<ILogger>().LogError("SetTile failed, point.X >= Width || point.Y >= Height for: " + point, true);
          return false;
        }

        if (tiles[point.Y, point.X] == tile && tile.Point == point)
          return true;

        if (tiles[point.Y, point.X] != null)
        {
          //caused ChildIsland to be revealed at once.
          //var prev = tiles[point.Y, point.X];
          //if (tile != null && tile.DungeonNodeIndex < 0)
          //  tile.DungeonNodeIndex = prev.DungeonNodeIndex;
        }

        tiles[point.Y, point.X] = tile;

        if (tile != null)
        {
          if (tile.DungeonNodeIndex > DungeonNode.ChildIslandNodeIndex && autoSetTileDungeonIndex)//do not touch islands
            SetDungeonNodeIndex(tile);
          if (resetOldTile)
          {
            //reset old tile
            if (tile.IsAtValidPoint && (tile.Point != point) && Width > tile.Point.X && Height > tile.Point.Y)
            {
              var emp = GenerateEmptyTile();
              if (emp != null)
                emp.DungeonNodeIndex = tile.DungeonNodeIndex;//preserve;
              SetTile(emp, tile.Point);
              if (emp != null)
              {
                if (revealReseted)
                  emp.Revealed = true;//if hero goes out of the tile it must be revealed
                if (OnTileRevealed != null)
                  OnTileRevealed(this, new GenericEventArgs<Tile>(emp));
              }
            }
          }

          tile.Point = point;
          return true;
          //if (OnTileRevealed != null)
          //  OnTileRevealed(this, new GenericEventArgs<Tile>(tile));
        }

        return false;
      }

      protected virtual void SetDungeonNodeIndex(Tile tile)
      {
        tile.DungeonNodeIndex = this.NodeIndex;
      }

      Point GetInteriorStartingPoint(int minSizeReduce = 6, DungeonNode child = null)
      {
        return interiorGenerator.GetInteriorStartingPoint(minSizeReduce, child);
      }

      public virtual Tile CreateDoor(Tile original)
      {
        if (generationInfo.ChildIsland)
        {
          Debug.Assert(generationInfo.EntrancesCount > 0);
        }
        else
          Debug.Assert(generationInfo.EntrancesCount == 0);
        var door = CreateDoorInstance();
        bool doorSet = SetTile(door, original.Point);
        Debug.Assert(doorSet);
        door.DungeonNodeIndex = original.DungeonNodeIndex;
        Doors.Add(door);
        return door;
      }

      protected virtual Door CreateDoorInstance()
      {
        return new Door();
      }

      public DungeonNode CreateChildIslandInstance(int w, int h, GenerationInfo gi, DungeonNode parent)
      {
        var dungeon = Container.GetInstance<DungeonNode>();
        var childGenInfo = gi.Clone() as GenerationInfo;
        childGenInfo.ChildIslandAllowed = false;//sub islands are not supported
        dungeon.Create(w, h, childGenInfo, ChildIslandNodeIndex, parent: this);
        return dungeon;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="childMaze"></param>
      /// <param name="destStartPoint"></param>
      /// <param name="childMazeMaxSize"></param>
      /// <param name="childIsland"></param>
      /// <param name="entranceSideToSkip"></param>
      public virtual void AppendMaze(DungeonNode childMaze, Point? destStartPoint = null, Point? childMazeMaxSize = null,
        bool childIsland = false, EntranceSide? entranceSideToSkip = null, DungeonNode prevNode = null)
      {
        childMaze.AppendedSide = entranceSideToSkip;
        Parts.Add(childMaze);

        var start = destStartPoint ?? GetInteriorStartingPoint(4, childMaze);
        if (start.X < 0 || start.Y < 0)
        {
          throw new Exception("AppendMaze start.X < 0 || start.Y < 0");
        }

        if (childMazeMaxSize == null)
          childMazeMaxSize = new Point(childMaze.Width, childMaze.Height);

        childMaze.AppendMazeStartPoint = start;
        if (childIsland)
        {
          childMaze.NodeIndex = NextChildIslandId--;
          childMaze.SetTilesNodeIndex();
        }
        for (int row = 0; row < childMazeMaxSize.Value.Y; row++)
        {
          for (int col = 0; col < childMazeMaxSize.Value.X; col++)
          {
            var tileInChildMaze = childMaze.GetTile(new Point(col, row));
            if (tileInChildMaze == null)
              continue;
            if (entranceSideToSkip != null)
            {
              var childMazeWall = tileInChildMaze as Wall;
              if (childMaze.Sides[entranceSideToSkip.Value].Contains(childMazeWall))
              {
                var indexOfWall = childMaze.Sides[entranceSideToSkip.Value].IndexOf(childMazeWall);
                if (prevNode == null ||
                  (entranceSideToSkip == EntranceSide.Left && indexOfWall < prevNode.Height - 1) ||
                  (entranceSideToSkip == EntranceSide.Top && indexOfWall < prevNode.Width)
                  )
                  continue;
              }
            }
            SetCorner(childMazeMaxSize, row, col, tileInChildMaze);
            int destCol = col + start.X;
            int destRow = row + start.Y;
            tileInChildMaze.Point = new Point(destCol, destRow);

            if (childIsland)
              tileInChildMaze.DungeonNodeIndex = childMaze.NodeIndex;

            this.SetTile(tileInChildMaze, new Point(destCol, destRow), autoSetTileDungeonIndex: false);
          }
        }
      }


      private static void SetCorner(Point? maxSize, int row, int col, Tile tile)
      {
        if ((col == 1 && row == 1)
            || (col == maxSize.Value.X - 2 && (row == 1 || row == maxSize.Value.Y - 2))
            || (col == 1 && row == maxSize.Value.Y - 2)
        )
        {
          if (col == 1 && row == 1)
            tile.corner = TileCorner.SouthWest;
          else if (col == 1 && row == maxSize.Value.Y - 2)
            tile.corner = TileCorner.NorthWest;
          else if (col == maxSize.Value.X - 2 && row == maxSize.Value.Y - 2)
            tile.corner = TileCorner.NorthEast;
          else
            tile.corner = TileCorner.SouthEast;
        }
      }

      public Tuple<int, int> GetMaxXY()
      {
        int maxX = 0;
        int maxY = 0;
        for (int row = 0; row < Height; row++)
        {
          for (int col = 0; col < Width; col++)
          {
            var tile = tiles[row, col];
            if (tile != null && tile.Point.X > maxX)
              maxX = col;
            if (tile != null && tile.Point.Y > maxY)
              maxY = row;
          }
        }
        return new Tuple<int, int>(maxX, maxY);
      }

      protected void GenerateOuterWalls()
      {
        interiorGenerator.GenerateOuterWalls();
      }

      private void PlaceEmptyTiles()
      {
        DoGridAction((int col, int row) =>
        {
          if (tiles[row, col] == null)
            tiles[row, col] = GenerateEmptyTile(new Point(col, row));
        });
      }

      public bool HasTile(Tile tile)
      {
        bool has = false;
        DoGridFunc((int col, int row) =>
        {
          if (tiles[row, col] == tile)
          {
            has = true;
            return true;//break loop
        }
          return false;
        });

        return has;
      }

      public void DoGridFunc(Func<int, int, bool> func)
      {
        bool cancel = false;
        for (int row = 0; row < Height; row++)
        {
          if (cancel)
            break;
          for (int col = 0; col < Width; col++)
          {
            cancel = func(col, row);
            if (cancel)
              break;
          }
        }
      }
      public void DoGridAction(Action<int, int> ac)
      {
        for (int row = 0; row < Height; row++)
        {
          for (int col = 0; col < Width; col++)
          {
            ac(col, row);
          }
        }
      }

      internal Tile GenerateEntrance(List<Wall> points)
      {
        return interiorGenerator.GenerateEntrance(points);
      }

      public T Generate<T>() where T : class, new()
      {
        var instance = new T();
        return instance;
      }

      public T GenerateAtPosition<T>(Point pt) where T : class, new()
      {
        var instance = Generate<T>();
        SetTile(instance as Tile, pt);
        return instance;
      }

      public bool SetEmptyTile(Point pt)
      {
        return SetTile(GenerateEmptyTile(pt), pt);
      }

      public Tile GenerateEmptyTile()
      {
        return GenerateEmptyTile(GenerationConstraints.InvalidPoint);
      }

      public virtual Tile GenerateEmptyTile(Point pt)
      {
        var tile = new Tile(pt, Constants.SymbolBackground);
        tile.DungeonNodeIndex = NodeIndex;
        return tile;
      }

      public List<Door> Doors
      {
        get
        {
          return doors;
        }
      }

      public EntranceSide? AppendedSide { get; private set; }
      public bool Revealed { get => revealed; set => revealed = value; }
      public bool IsChildIsland
      {
        get { return NodeIndex <= ChildIslandNodeIndex; }
      }

      /// <summary>
      /// Delete unreachable doors 
      /// </summary>
      internal void DeleteWrongDoors()
      {
        List<Tile> toDel = new List<Tile>();
        foreach (Tile tile in Tiles)
        {
          if (tile is Door)
          {
            var neibs = GetNeighborTiles(tile);
            if (neibs.Where(i => i is Wall).Count() >= 3 ||
               neibs.Where(i => i == null).Any())
              toDel.Add(tile);
          }
        }
        if (toDel.Any())
        {
          for (int i = 0; i < toDel.Count; i++)
          {
            var wall = CreateWall();
            if (this.SetTile(wall, toDel[i].Point))
            {
              wall.dungeonNodeIndex = toDel[i].DungeonNodeIndex;
              wall.Revealed = toDel[i].Revealed;
            }
          }
        }
      }

      public virtual Wall CreateWall()
      {
        return new Wall();
      }

      public event EventHandler<GenericEventArgs<NodeRevealedParam>> OnRevealed;

      public virtual void Reveal(bool reveal, bool force = false)
      {
        if (reveal && Revealed && !force)//Optimize
          return;

        var revDesc = "reveal: " + reveal + ", for: " + NodeIndex;
        Debug.WriteLine(revDesc + ", start");
        IList<Tile> revealedTiles = new List<Tile>();

        DoGridAction((int col, int row) =>
        {
          if (tiles[row, col] != null)
          {
            var revealTile = reveal;
            if (revealTile)
              revealTile = ShallReveal(row, col);

            tiles[row, col].Revealed = revealTile;
            if (revealTile)
            {
              revealedTiles.Add(tiles[row, col]);
            //Debug.WriteLine("reveal " + tiles[row, col]);
            if (tiles[row, col].DungeonNodeIndex < 0)
              {
              //Debug.WriteLine("reveal < 0" + tiles[row, col]);
            }
            }
          }
        });
        Revealed = reveal;
        if (Revealed && OnRevealed != null)
        {
          //var ev = new GenericEventArgs<IList<Tile>>(revealedTiles);
          OnRevealed(this, new GenericEventArgs<NodeRevealedParam>(new NodeRevealedParam() { NodeIndex = NodeIndex, Tiles = revealedTiles }));
        }

        Debug.WriteLine(revDesc + ", end");
      }

      protected virtual bool ShallReveal(int row, int col)
      {
        return true;
      }

      public Point GetEmptyNeighborhoodPoint(Tile target, TileNeighborhood? prefferedSide = null)
      {
        var set = new List<TileNeighborhood>();
        if (prefferedSide != null)
        {
          set.Add(prefferedSide.Value);
        }
        allNeighborhoods.Shuffle();
        set.AddRange(allNeighborhoods.Where(i => !set.Contains(i)));
        return GetEmptyNeighborhoodPoint(target, set);
      }

      public virtual bool IsTileEmpty(Tile tile)
      {
        return tile != null && tile.IsEmpty;
      }

      protected virtual Point GetEmptyNeighborhoodPoint(Tile target, List<TileNeighborhood> sides)
      {
        Point pt = GenerationConstraints.InvalidPoint;
        foreach (var side in sides)
        {
          Tile tile = GetNeighborTile(target, side);
          if (IsTileEmpty(tile))
          {
            pt = tile.Point;
            break;
          }
        }

        return pt;
      }

      public bool IsPointInBoundaries(Point pt)
      {
        return pt.X >= 0 && pt.Y >= 0 && pt.X < this.Width && pt.Y < this.Height;
      }

      public Tile GetClosestEmpty(Tile baseTile, bool sameNodeId = false)
      {
        var emptyTiles = GetEmptyTiles();
        if (sameNodeId)
          emptyTiles = emptyTiles.Where(i => i.DungeonNodeIndex == baseTile.DungeonNodeIndex).ToList();
        return GetClosestEmpty(baseTile, emptyTiles);
      }

      public Tile GetClosestEmpty(Tile baseTile, List<Tile> emptyTiles)
      {
        return emptyTiles.Where(i => i.DistanceFrom(baseTile) == emptyTiles.Min(j => j.DistanceFrom(baseTile))).FirstOrDefault();
      }

      public List<T> GetTiles<T>() where T : class
      {
        var res = new List<T>();
        foreach (var tile in Tiles)
        {
          if (tile is T)
          {
            res.Add(tile as T);
          }
        }

        return res;
      }

      public List<Tile> GetTiles()
      {
        return GetTiles<Tile>();
      }
    }
  }
}
