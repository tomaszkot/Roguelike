using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Managers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.TileContainers
{
  //a single room - might be:
  //1. a DungeonLevel - result of composition  of many DungeonNodes and have size like 100x100
  //2. a World - big one dungeon like 500x500 tiles
  public abstract class AbstractGameLevel : Dungeons.TileContainers.DungeonLevel
  {
    int index;//index of the level or world. DungeonPit can have levels with indexes of 0-[n]
    public int Index
    {
      get { return index; }
      set
      {
        index = value;
        NodeIndex = value;//this field shall not be used in AbstractGameLevel or derived ones, but setting it to index shall be a safe move.
      }
    }


    public static Tile EmptyTile = new Tile(symbol: Constants.SymbolBackground);
    public Dictionary<Point, Loot> Loot { get; set; } = new Dictionary<Point, Tiles.Loot>();
    [JsonIgnore]
    public ILogger Logger { get; set; }
    public virtual string Name { get; set; } = "";


    public AbstractGameLevel(Container container)
   : base(container)
    {
      Logger = Container.GetInstance<ILogger>();
    }

    public override void AppendMaze(Dungeons.TileContainers.DungeonNode childMaze, Point? destStartPoint = null, Point? childMazeMaxSize = null,
      bool childIsland = false, EntranceSide? entranceSideToSkip = null, Dungeons.TileContainers.DungeonNode prevNode = null)
    {
      base.AppendMaze(childMaze, destStartPoint, childMazeMaxSize, childIsland, entranceSideToSkip, prevNode);
    }

    public List<Tile> GetTiles(bool includeLoot) 
    {
      var res = GetTiles<Tile>();
      if (includeLoot)
      {
        res.AddRange(Loot.Values);
      }
      return res;
    }

    public override List<T> GetTiles<T>() 
    {
      var res = base.GetTiles<T>();
      if (typeof(T) == typeof(Loot) || typeof(T).IsSubclassOf(typeof(Loot)))
      {
        res.AddRange(Loot.Values.Where(i => i is T).Cast<T>());
      }
            
      return res;
    }

    //hmm, maybe that method shall be marked as trowing NotSupportedException? (Loot is not considered here)
    public override Tile GetClosestEmpty(Tile baseTile, bool sameNodeId = false, List<Tile> skip = null)
    {
      return base.GetClosestEmpty(baseTile, sameNodeId, skip);
    }

    public Tile GetClosestEmpty(Tile baseTile, bool excludeLootPositions)
    {
      var emptyTiles = GetEmptyTiles();
      if(excludeLootPositions)
        emptyTiles.RemoveAll(i => Loot.Any(j => j.Key == i.Point));
      return GetClosestEmpty(baseTile, emptyTiles);
    }

    public override bool SetTile(Tile tile, Point point, bool resetOldTile = true, bool revealReseted = true,
      bool autoSetTileDungeonIndex = true)
    {
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
        if (Loot.ContainsKey(point))
        {
          if (Logger != null)
            Logger.LogError("loot already at point: " + Loot[point] + ", trying to add: " + tile);
          Debug.Assert(false);
          return false;
        }
        tile.Point = point;
        Loot[point] = tile as Roguelike.Tiles.Loot;

        return true;
      }
      
      Point? prevPos = tile?.Point;
      var res =  base.SetTile(tile, point, resetOldTile, revealReseted, autoSetTileDungeonIndex);
      if (res && tile is LivingEntity && prevPos!=null)
      {
        (tile as LivingEntity).PrevPoint = prevPos.Value;
      }
      return res;
    }

    public virtual void OnGenerationDone()
    {
      
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

    List<Dungeons.TileContainers.DungeonNode> emptyNodes = new List<Dungeons.TileContainers.DungeonNode>();
    [JsonIgnore]
    public virtual List<Dungeons.TileContainers.DungeonNode> GeneratorNodes
    {
      get
      {
        if (!Parts.Any())
          return emptyNodes;
        return Parts[0].Parts.Cast<Dungeons.TileContainers.DungeonNode>().ToList();
        //return Parts[0].Parts.Cast<Dungeons.TileContainers.DungeonNode>().ToList();
      }
    }

    public Dungeons.TileContainers.DungeonNode GetNodeFromTile(Tile tile)
    {
      var node = GeneratorNodes.Where(i => i.NodeIndex == tile.DungeonNodeIndex).SingleOrDefault();
      return node as Dungeons.TileContainers.DungeonNode;
    }

    Dungeons.TileContainers.DungeonNode GetChildIslandFromTile(Tile tile)
    {
      foreach (var node in GeneratorNodes)
      {
        var isl = node.ChildIslands.FirstOrDefault(i => i.NodeIndex == tile.DungeonNodeIndex);
        if (isl != null)
          return isl as Dungeons.TileContainers.DungeonNode;
      }

      return null;
    }

    public virtual bool RevealRoom(Tiles.Door door, Roguelike.Tiles.Hero hero)
    {
      if (door.IsFromChildIsland)
      {
        var node = GetChildIslandFromTile(door);
        //var parts = Parts;
        node.Reveal(true);
      }
      else
      {
        var neib = GetNeighborTiles(door).Where(i => 
        i.DungeonNodeIndex != door.DungeonNodeIndex && 
        i != hero && 
        i.DungeonNodeIndex != Dungeons.TileContainers.DungeonNode.DefaultNodeIndex).FirstOrDefault();

        if (neib != null)
        {
          //var parts = Parts;
          GetNodeFromTile(neib).Reveal(true);
        }
        else
          Container.GetInstance<ILogger>().LogError("neib == null "+ door);
      }
      door.Opened = true;
      return true;
    }

    public override Tile GetTile(Point point)
    {
      var tile = base.GetTile(point);
      if (tile == null || tile.IsEmpty)
      {
        var lootTile = GetLootTile(point);
        return lootTile != null ? lootTile : tile;
      }

      return tile;
    }

    public List<Tiles.Loot> GetLootTilesNearby(Tile tile)
    {
      List<Tiles.Loot> res = Loot.Values
        .Where(i=> i.Revealed && i.DistanceFrom(tile.Point) < 8//5
        //&& i.DungeonNodeIndex == tile.DungeonNodeIndex TODO
        )
        .ToList();//TODO 
      return res;
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

          if (tile is Dungeons.Tiles.IObstacle)
          {
            if (forHeroAlly && tile is LivingEntity)
            {
              int k = 0;
              k++;
            }
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
              //|| tile is Loot
              || (tile is Roguelike.Tiles.Door  /*&& !EnemyCanPassDoors*/ && !(tile as Roguelike.Tiles.Door).Opened)

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

    public Tile ReplaceTile(Tile replacer, Point point)
    {
      Tile toUse = replacer;
      if (replacer == null)
        toUse = new Tile();
      var prev = GetTile(point);
      if (SetTile(toUse, point))
      {
        if(replacer is Loot)
          Tiles[point.Y, point.X] = new Tile(point);//reset old one, as loot is not hold in Tiles table
        return prev;
      }
      return null;
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

    public Stairs GetStairs(StairsKind kind)
    {
      return GetAllStairs(kind).FirstOrDefault();
    }

    public IEnumerable<Stairs> GetAllStairs(StairsKind kind)
    {
      return GetTiles<Stairs>().Where(s => s.StairsKind == kind);
    }

    public Stairs GetPitStairs(string pitName)
    {
      return GetAllStairs(StairsKind.PitDown).Where(i=> i.PitName == pitName).SingleOrDefault();
    }

    internal int GetSpawnEnemyLevel()
    {
      return Index;
    }

    internal Enemy SpawnEnemy(Tile position, EventsManager em)
    {
      var enemy = new Enemy(EnemySymbols.SkeletonSymbol);
      enemy.SetLevel(GetSpawnEnemyLevel());
      enemy.EventsManager = em;
      enemy.tag1 = EnemySymbols.EnemiesToSymbols.Where(i => i.Value == EnemySymbols.SkeletonSymbol).Single().Key;
      return enemy;
    }
  }
}
