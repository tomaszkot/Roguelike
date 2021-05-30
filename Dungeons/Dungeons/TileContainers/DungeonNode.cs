using Dungeons.Core;
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
      public DungeonNode ChildIslandNode { get; set; }
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
      bool contentGenerated = false;
      bool secret = false;

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

      List<IDoor> doors = new List<IDoor>();

      [JsonIgnore]
      public HiddenTiles HiddenTiles { get; set; } = new HiddenTiles();

      
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
      protected static List<TileNeighborhood> AllNeighborhoods = new List<TileNeighborhood> { TileNeighborhood.East, TileNeighborhood.West, TileNeighborhood.North, TileNeighborhood.South };

      //it's assummed, level has there are less rooms that 999. 
      public const int DefaultNodeIndex = 999;

      public const int ChildIslandNodeIndex = -1;
      public static int NextChildIslandId = ChildIslandNodeIndex;
      int nodeIndex = 0;
      DungeonNode parent;

      public event EventHandler<GenericEventArgs<Tile>> OnTileRevealed;
      public event EventHandler<ChildIslandCreationInfo> ChildIslandCreated;
      NodeInteriorGenerator interiorGenerator;
      bool revealed;
      bool created;

      [JsonIgnore]
      public Container Container { get; set; }

      public EventHandler<DungeonNode> CustomInteriorDecorator;
      public EventHandler<DungeonNode> BeforeInteriorGenerated;
      ILogger logger;
      public enum EmptyNeighborhoodCallContext { Unset, Move, LootPlacement };

      //ctors
      static DungeonNode()
      {
        random = new Random();
      }

      public DungeonNode(Container container)
      {
        this.Container = container;

        Sides.Add(EntranceSide.Top, new List<Wall>());
        Sides.Add(EntranceSide.Bottom, new List<Wall>());
        Sides.Add(EntranceSide.Left, new List<Wall>());
        Sides.Add(EntranceSide.Right, new List<Wall>());
      }

      public virtual void Create(int width = 10, int height = 10, GenerationInfo info = null,
                         int nodeIndex = DefaultNodeIndex, DungeonNode parent = null, bool generateContent = true)
      {
        Create(null, info, nodeIndex, parent);
        tiles = new Tile[height, width];

        if (generateContent)
        {
          GenerateContent();
        }
        this.created = true;
      }

      public void Create(Tile[,] tiles, GenerationInfo gi = null, int nodeIndex = DefaultNodeIndex, DungeonNode parent = null)
      {
        this.Parent = parent;
        this.NodeIndex = nodeIndex;

        this.generationInfo = gi;
        this.interiorGenerator = new NodeInteriorGenerator(this, generationInfo);
        interiorGenerator.ChildIslandCreated += (s, e) =>
        {
          ChildIslandCreated?.Invoke(s, e);
        };
        this.tiles = tiles;
        this.created = true;
      }

      Dictionary<string, bool> hiddenTilesAlreadyAdded = new Dictionary<string, bool>();
      public bool GetHiddenTilesAlreadyAdded(string key)
      {
        return hiddenTilesAlreadyAdded[key];
      }

      public void SetAlreadyAdded(string key, bool alreadyAdded)
      {
        hiddenTilesAlreadyAdded[key] = alreadyAdded;
      }

      void EnsureLogger()
      {
        if (this.logger == null && Container != null)
          logger = Container.GetInstance<ILogger>();
      }

      void Log(string info, bool error)
      {
        EnsureLogger();
        if (logger != null)
        {
          if (error)
            logger.LogError(info);
          else
            logger.LogInfo(info);
        }
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

      internal List<IDoor> GenerateLayoutDoors(EntranceSide side, int nextNodeIndex, bool secret, bool overWallls = false)
      {
        var res = new List<IDoor>();
        List<Wall> wall = sides[side];
        if (secret)
        {
          var count = sides[side].Count;
          var diff = GenerationInfo.MaxRoomSideSize - GenerationInfo.MinRoomSideSize;
          var index = Enumerable.Range(1, count - diff).ToList().GetRandomElem();
          var door = CreateDoor(wall[index]) as IDoor;
          door.Secret = true;
          res.Add(door);
          //if (nextNodeIndex > 0)
          // door.DungeonNodeIndex = nextNodeIndex;
          return res;
        }

        for (int i = 0; i < wall.Count; i++)
        {
          if (i > 0 && i % 2 == 0)// && (side != EntranceSide.Bottom || i < nextNode.Width))
          {
            res.Add(CreateDoor(wall[i]) as Door);
          }
        }
        return res;
      }

      protected virtual void GenerateContent()
      {
        if (generationInfo == null)
          return;

        if (generationInfo.GenerateEmptyTiles)
          PlaceEmptyTiles();
        if (generationInfo.GenerateOuterWalls)
          GenerateOuterWalls();

        if (BeforeInteriorGenerated != null)
          BeforeInteriorGenerated(this, this);

        interiorGenerator.GenerateRandomInterior(CustomInteriorDecorator);

        if (CustomInteriorDecorator != null)
          CustomInteriorDecorator(this, this);

        Reveal(generationInfo.RevealTiles);
      }

      public bool IsCornerWall(Wall wall)
      {
        var neibs = GetNeighborTiles(wall).Where(i => i is Wall).ToList();
        if (neibs.Count >= 3)
          return true;
        if (neibs.Count != 2)
          return false;
        if (neibs.Count(i => i.point.X == wall.point.X) == 2 ||
          neibs.Count(i => i.point.Y == wall.point.Y) == 2)
          return false;
        return true;

      }

      public List<T> GetNeighborTiles<T>(Tile tile, bool incDiagonal = false) where T : Tile
      {
        return GetNeighborTiles(tile, incDiagonal).Where(i => i != null && i is T).Cast<T>().ToList();
      }

      public void AddChildIsland(Point? destStartPoint, DungeonNode childIsland)
      {
        AppendMaze(childIsland, destStartPoint, childIsland: true);
        ChildIslands.Add(childIsland);
      }

      public List<Tile> GetNeighborTiles(Tile tile, bool incDiagonal = false)
      {
        var neibs = new List<Tile>();
        foreach (var i in AllNeighborhoods)
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

        return neibs.Distinct().ToList();
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

      public static Point GetNeighborPoint(Tile tile, TileNeighborhood neighborhood)
      {
        Point pt = tile.point;
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
        return tile?.point;
      }

      public virtual List<Tile> GetEmptyTiles
      (
        GenerationConstraints constraints = null,
        bool canBeNextToDoors = true,
        bool nodeIndexMustMatch = false//allows skipping childIsland tiles
      )
      {
        var tt = new TimeTracker();
        var emptyTiles = new List<Tile>();
        if (!created)
          return emptyTiles;
        DoGridAction((int col, int row) =>
        {
          if (IsTileEmpty(tiles[row, col])// != null && tiles[row, col].IsEmpty  //null can be outside the walls
          )
          {
            var tile = tiles[row, col];
            if (!nodeIndexMustMatch || tile.dungeonNodeIndex == NodeIndex)
            {
              var pt = new Point(col, row);
              if (constraints == null || (constraints.IsInside(pt)))
                emptyTiles.Add(tile);
            }
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
        //Log("GetEmptyTiles time: "+tt.TotalSeconds, false);
        return emptyTiles;
      }

      public Tile GetRandomEmptyTile(GenerationConstraints constraints = null, bool canBeNextToDoors = true, int? nodeIndex = null)
      {
        var emptyTiles = GetEmptyTiles(constraints, canBeNextToDoors);

        return GetRandomEmptyTile(emptyTiles, nodeIndex);
      }

      public Tile GetRandomEmptyTile(List<Tile> emptyTiles, int? nodeIndex = null)
      {
        if (nodeIndex != null)
          emptyTiles = emptyTiles.Where(i => i.dungeonNodeIndex == nodeIndex.Value).ToList();

        if (emptyTiles.Any())
        {
          var emptyTileIndex = random.Next(emptyTiles.Count);
          var res = emptyTiles[emptyTileIndex];
          //Log("GetRandomEmptyTile: " + res);
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

      public bool NullTilesAllowed { get; set; }

      public virtual bool SetTile(Tile tile, Point point,
        bool resetOldTile = true, bool revealReseted = true, bool autoSetTileDungeonIndex = true, bool reportError = true)
      {
        if (point.X < 0 || point.Y < 0)
        {
          Log("SetTile failed for: " + point, true);
          return false;
        }

        if (!NullTilesAllowed && tile == null)
        {
          Log("tile == null", false);
        }

        if (AppendMazeStartPoint != null)
        {
          point.X -= AppendMazeStartPoint.Value.X;
          point.Y -= AppendMazeStartPoint.Value.Y;
        }
        if (point.X >= Width || point.Y >= Height)
        {
          Log("SetTile failed, node:" + this + ", point.X >= Width || point.Y >= Height point: " + point + ", tile: " + tile, true);
          return false;
        }

        if (tiles[point.Y, point.X] == tile && tile.point == point)
          return true;

        if (tiles[point.Y, point.X] != null)
        {
          //caused ChildIsland to be revealed at once.
          var prev = tiles[point.Y, point.X];
          if (tile != null && !tile.IsFromChildIsland())
            tile.DungeonNodeIndex = prev.DungeonNodeIndex;
        }

        tiles[point.Y, point.X] = tile;

        if (tile != null)
        {
          if (!tile.IsFromChildIsland() && autoSetTileDungeonIndex)//do not touch islands
            SetDungeonNodeIndex(tile);
          if (resetOldTile)
          {
            //reset old tile
            if (tile.IsAtValidPoint && (tile.point != point) && Width > tile.point.X && Height > tile.point.Y)
            {
              var emp = GenerateEmptyTile();
              if (emp != null)
                emp.DungeonNodeIndex = tile.DungeonNodeIndex;//preserve;
              SetTile(emp, tile.point);
              if (emp != null)
              {
                if (revealReseted)
                  emp.Revealed = true;//if hero goes out of the tile it must be revealed
                if (OnTileRevealed != null)
                  OnTileRevealed(this, new GenericEventArgs<Tile>(emp));
              }
            }
          }

          tile.point = point;
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

      public virtual InteractiveTile CreateDoor(Tile original)
      {
        if (generationInfo != null)
        {
          if (generationInfo.ChildIsland)
          {
            Debug.Assert(generationInfo.EntrancesCount > 0);
          }
          else
            Debug.Assert(generationInfo.EntrancesCount == 0);
        }
        var door = CreateDoorInstance();
        Debug.Assert(door != null);
        bool doorSet = SetTile(door, original.point);
        Debug.Assert(doorSet);
        door.DungeonNodeIndex = original.DungeonNodeIndex;

        var doorInterface = door as IDoor;
        Debug.Assert(doorInterface != null);
        Doors.Add(doorInterface);

        return door;
      }

      protected InteractiveTile CreateDoorInstance()
      {
        return Container.GetInstance<IDoor>() as InteractiveTile;
      }

      public DungeonNode CreateChildIslandInstance(int w, int h, GenerationInfo gi, DungeonNode parent)
      {
        var dungeon = Container.GetInstance<DungeonNode>();
        dungeon.Container = this.Container;
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
      public virtual void AppendMaze
      (
        DungeonNode childMaze,
        Point? destStartPoint = null,
        Point? childMazeMaxSize = null,
        bool childIsland = false,
        EntranceSide? entranceSideToSkip = null,
        DungeonNode prevNode = null
      )
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
                //if (prevNode == null || !prevNode.Secret)
                {
                  var indexOfWall = childMaze.Sides[entranceSideToSkip.Value].IndexOf(childMazeWall);
                  if (prevNode == null ||
                    (entranceSideToSkip == EntranceSide.Left && indexOfWall < prevNode.Height - 1) ||
                    (entranceSideToSkip == EntranceSide.Top && indexOfWall < prevNode.Width)
                    )
                    continue;
                }
              }
            }
            SetCorner(childMazeMaxSize, row, col, tileInChildMaze);
            int destCol = col + start.X;
            int destRow = row + start.Y;
            tileInChildMaze.point = new Point(destCol, destRow);

            if (childIsland)
              tileInChildMaze.DungeonNodeIndex = childMaze.NodeIndex;

            //var at = this.GetTile(new Point(destCol, destRow));
            //if (at.name == "Bat")
            //{

            //}
            var destPt = new Point(destCol, destRow);
            var prevSecret = prevNode != null && prevNode.Secret && prevNode.NodeIndex == 0;
            if (prevSecret && this.GetTile(destPt) is IDoor)
            {
              continue;
            }
            var set = this.SetTile(tileInChildMaze, destPt, autoSetTileDungeonIndex: false, reportError: false);
            if (set)
            {
              if (prevSecret)
                tileInChildMaze.DungeonNodeIndex = childMaze.NodeIndex;
            }
            else
            {
              var emp = this.GetClosestEmpty(tileInChildMaze);
              if (emp != null)
              {
                set = this.SetTile(tileInChildMaze, emp.point, autoSetTileDungeonIndex: false);
              }
              if (!set)
              {
                var err = "SetTile failed " + tileInChildMaze + " emp:" + (emp != null);
                var tileAtPt = this.GetTile(destPt);
                this.Log(err + " tileAtPt: " + tileAtPt, true);
              }
            }
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
            if (tile != null && tile.point.X > maxX)
              maxX = col;
            if (tile != null && tile.point.Y > maxY)
              maxY = row;
          }
        }
        return new Tuple<int, int>(maxX, maxY);
      }

      public void GenerateOuterWalls()
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

      public List<IDoor> Doors
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

      public bool Created { get => created; set => created = value; }
      public bool ContentGenerated { get => contentGenerated; set => contentGenerated = value; }
      public bool Secret { get => secret; set => secret = value; }

      /// <summary>
      /// Delete unreachable doors 
      /// </summary>
      internal void DeleteWrongDoors()
      {
        List<Tile> toDel = new List<Tile>();
        foreach (Tile tile in Tiles)
        {
          if (tile is IDoor)
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
            if (this.SetTile(wall, toDel[i].point))
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

      public event EventHandler<NodeRevealedParam> OnRevealed;

      public virtual void Reveal(bool reveal, bool force = false)
      {
        if (reveal)
        {
          Log("reveal start - TRUE!", false);
        }
        if (reveal && Revealed && !force)//Optimize
          return;

        var revDesc = "reveal: " + reveal + ", for: " + NodeIndex;
        //Log(revDesc + ", start", false);
        IList<Tile> revealedTiles = new List<Tile>();

        DoGridAction((int col, int row) =>
        {
          var tile = GetTile(new Point(col, row));
          if (tile != null)///tiles[row, col] != null)
          {
            var revealTile = reveal;
            if (revealTile)
              revealTile = ShallReveal(row, col);

            //tiles[row, col].Revealed = revealTile;
            tile.Revealed = revealTile;
            if (revealTile)
            {
              revealedTiles.Add(tiles[row, col]);
              //Log("reveal " + tiles[row, col], false);
              if (tile.IsFromChildIsland())
              {
                //Log("reveal < 0" + tiles[row, col], false);
              }
            }
          }
        });
        Revealed = reveal;
        if (Revealed && OnRevealed != null)
        {
          //var ev = new GenericEventArgs<IList<Tile>>(revealedTiles);
          OnRevealed(this, new NodeRevealedParam() { NodeIndex = NodeIndex, Tiles = revealedTiles });
        }

        //Log(revDesc + ", end", false);
      }

      protected virtual bool ShallReveal(int row, int col)
      {
        return true;
      }

      public Tuple<Point, TileNeighborhood> GetEmptyNeighborhoodPoint(Tile target, TileNeighborhood? prefferedSide = null, List<Type> extraTypesConsideredEmpty = null)
      {
        var set = new List<TileNeighborhood>();
        if (prefferedSide != null)
        {
          set.Add(prefferedSide.Value);
        }
        AllNeighborhoods.Shuffle();
        set.AddRange(AllNeighborhoods.Where(i => !set.Contains(i)));
        var res = GetEmptyNeighborhoodPoint(target, set, extraTypesConsideredEmpty);
        return res;
      }

      public List<Tile> GetEmptyNeighborhoodTiles(Tile target, bool incDiagonal = true)
      {
        var neibs = GetNeighborTiles(target, incDiagonal).Where(i => i != null).ToList();
        if (neibs.Any())
        {
          neibs.Shuffle();
          var neibsEmpty = neibs.Where(i => IsTileEmpty(i)).ToList();
          if (neibsEmpty.Any())
            return neibsEmpty;

          return GetEmptyNeighborhoodTiles(neibs.First(), incDiagonal);
        }

        return null;
      }

      public virtual bool IsTileEmpty(Tile tile)
      {
        return tile != null && tile.IsEmpty;
      }

      protected bool IsTypeMatching(Type first, Type other)
      {
        return other == first || other.IsSubclassOf(first);
      }

      protected virtual List<Tuple<Point, TileNeighborhood>> GetEmptyNeighborhoodPoints(Tile target, List<TileNeighborhood> sides, List<Type> extraTypesConsideredEmpty)
      {
        var res = new List<Tuple<Point, TileNeighborhood>>();

        foreach (var side in sides)
        {
          Tile tile = GetNeighborTile(target, side);
          if (tile == null)
            continue;
          if (IsTileEmpty(tile) || (extraTypesConsideredEmpty != null &&
                                    extraTypesConsideredEmpty.Any(i => IsTypeMatching(i, tile.GetType()))))
          {
            res.Add(new Tuple<Point, TileNeighborhood>(tile.point, side));
          }
        }

        return res;
      }

      protected virtual Tuple<Point, TileNeighborhood> GetEmptyNeighborhoodPoint(Tile target, List<TileNeighborhood> sides, List<Type> extraTypesConsideredEmpty)
      {
        var empties = GetEmptyNeighborhoodPoints(target, sides, extraTypesConsideredEmpty);
        if (empties.Any())
          return empties.First();

        return null;
      }

      public bool IsPointInBoundaries(Point pt)
      {
        return pt.X >= 0 && pt.Y >= 0 && pt.X < this.Width && pt.Y < this.Height;
      }

      public virtual Tile GetClosestEmpty(Tile baseTile, bool sameNodeId = false, List<Tile> skip = null, bool incDiagonals = true)
      {
        var fastVersionResult = GetEmptyNeighborhoodPoint(baseTile);
        if (fastVersionResult != null)
        {
          var tile = GetTile(fastVersionResult.Item1);
          if (skip == null || !skip.Contains(tile))
          {
            if (tile != null && (!sameNodeId || tile.DungeonNodeIndex == baseTile.DungeonNodeIndex))
              return tile;
          }
        }

        Log("GetClosestEmpty - failed to find empty fast way!", true);
        var emptyTiles = GetEmptyTiles();
        if (skip != null)
          emptyTiles.RemoveAll(i => skip.Contains(i));
        if (sameNodeId)
          emptyTiles = emptyTiles.Where(i => i.DungeonNodeIndex == baseTile.DungeonNodeIndex).ToList();
        return GetClosestEmpty(baseTile, emptyTiles);
      }

      public Tile GetClosestEmpty(Tile baseTile, List<Tile> emptyTiles)
      {
        return emptyTiles.Where(i => i.DistanceFrom(baseTile) == emptyTiles.Min(j => j.DistanceFrom(baseTile))).FirstOrDefault();
      }

      public virtual List<T> GetTiles<T>() where T : class
      {
        var res = new List<T>();
        if (Tiles == null)
          return res;
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

      public virtual Tile SetTileAtRandomPosition(Tile tile, bool matchNodeIndex = true)
      {
        var node = matchNodeIndex == true ? (int?)NodeIndex : null;
        var empty = this.GetRandomEmptyTile(nodeIndex: node);
        if (empty == null)
          return null;

        var set = SetTile(tile, empty.point);

        return set ? tile : null;
      }

      internal T SetTileAtRandomPosition<T>(bool matchNodeIndex = true) where T : Tile, new()
      {
        var tile = new T();
        return SetTileAtRandomPosition(tile, matchNodeIndex) as T;
      }
    }
  }
}
