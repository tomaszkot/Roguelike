﻿using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Tiles;
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
  public abstract class GameNode : Dungeons.DungeonLevel
  {
    public static Tile EmptyTile = new Tile(symbol: Constants.SymbolBackground);
    public Dictionary<Point, Loot> Loot { get; set; } = new Dictionary<Point, Tiles.Loot>();
    [JsonIgnore]
    public ILogger Logger { get; set; }
    public virtual string Name { get; set; } = "";

   
    public GameNode(Container container)
   : base(container)
    {
      Logger = Container.GetInstance<ILogger>();
    }

    public override void AppendMaze(Dungeons.DungeonNode childMaze, Point? destStartPoint = null, Point? childMazeMaxSize = null,
      bool childIsland = false, EntranceSide? entranceSideToSkip = null, Dungeons.DungeonNode prevNode = null)
    {
      base.AppendMaze(childMaze, destStartPoint, childMazeMaxSize, childIsland, entranceSideToSkip, prevNode);
    }

    public List<Tile> GetTiles(bool includeLoot) 
    {
      var res = base.GetTiles();
      if (includeLoot)
      {
        res.AddRange(Loot.Values);
      }
      return res;
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

    public Tile GetRandomEmptyTile(GenerationConstraints constraints = null, bool canBeNextToDoors = true)
    {
      List<Tile> emptyTiles = GetEmptyTiles(constraints, canBeNextToDoors);

      if (emptyTiles.Any())
      {
        var emptyTileIndex = random.Next(emptyTiles.Count);
        return emptyTiles[emptyTileIndex];
      }

      return null;
    }

    public override List<Tile> GetEmptyTiles(GenerationConstraints constraints = null, bool canBeNextToDoors = true)
    {
      List<Tile> emptyTiles = base.GetEmptyTiles(constraints);
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

    public Generators.TileContainers.DungeonNode GetNodeFromTile(Tile tile)
    {
      var parts = Parts;
      var node = Nodes.Where(i => i.NodeIndex == tile.DungeonNodeIndex).SingleOrDefault();
      return node as Generators.TileContainers.DungeonNode;
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
        var node = GetChildIslandFromTile(door);
        var parts = Parts;
        node.Reveal(true);
      }
      else
      {
        var neib = GetNeighborTiles(door).Where(i => 
        i.DungeonNodeIndex != door.DungeonNodeIndex && 
        i != hero && 
        i.DungeonNodeIndex != DungeonNode.DefaultNodeIndex).FirstOrDefault();

        if (neib != null)
        {
          var parts = Parts;
          GetNodeFromTile(neib).Reveal(true);
        }
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

    internal Tile ReplaceTile(Loot loot, Point point)
    {
      var prev = GetTile(point);
      if (SetTile(loot, point))
      {
        Tiles[point.Y, point.X] = new Tile(point);//reset old one
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
  }
}
