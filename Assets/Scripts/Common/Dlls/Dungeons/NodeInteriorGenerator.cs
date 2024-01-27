using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
#pragma warning disable 8603
#pragma warning disable 8602
namespace Dungeons
{
  public class NodeInteriorGenerator
  {
    DungeonNode dungeonNode;
    GenerationInfo generationInfo;

    public int Width { get { return dungeonNode.Width; } }
    public int Height { get { return dungeonNode.Height; } }
    public event EventHandler<ChildIslandCreationInfo> ChildIslandCreated;

    public NodeInteriorGenerator(DungeonNode dn, GenerationInfo gi)
    {
      this.dungeonNode = dn;
      this.generationInfo = gi;
    }

    public void GenerateRandomInterior(EventHandler<DungeonNode> CustomInteriorDecorator)
    {
      if (!Inited())
        return;

      if (!generationInfo.GenerateRandomInterior && !generationInfo.ForceChildIslandInterior)
        return;

      Interior? interior = null;
      var rand = RandHelper.GetRandomDouble();
      if (!dungeonNode.Corridor && !dungeonNode.Secret && generationInfo.ChildIslandAllowed && (generationInfo.ForceChildIslandInterior || rand < .33))
      {
        var island = GenerateChildIslands();
        if (island == null)
        {
          interior = GenerateRandomSimpleInterior(true);
        }
        else if (CustomInteriorDecorator != null)
          CustomInteriorDecorator(this, island.FirstOrDefault());//currently only one is send 
      }
      else if (generationInfo.GenerateRandomInterior)
        interior = GenerateRandomSimpleInterior();

      if (generationInfo.GenerateRandomInterior)
        GenerateRandomStonesBlocks();
    }

    bool Inited()
    {
      return generationInfo != null && dungeonNode != null;
    }

    Interior? GenerateRandomSimpleInterior(bool addFinishingDecorations = false)
    {
      Interior? interior = null;
      if (Width - generationInfo.MinSimpleInteriorSize > 4
        && Height - generationInfo.MinSimpleInteriorSize > 4)
      {
        interior = RandHelper.GetRandomEnumValue<Interior>();
        GenerateInterior(interior.Value);
        //GenerateInterior(Interior.L);
      }
      else
        addFinishingDecorations = true;
      if (addFinishingDecorations)
      {
        AddDecorations();
      }

      return interior;
    }

    void AddSplitWall(bool vertically, int entrancesCount = 1)
    {
      List<Point> points = new List<Point>();
      bool shadowed = false;
      if (vertically)
      {
        int x = this.Width / 2;
        points = GenerateWallPoints(x, x + 1, 1, Height, 1);
      }
      else
        shadowed = true;
      var tiles = AddWalls(points, shadowed);
      GenerateEntrance(tiles);
    }

    internal Tile GenerateEntrance(List<Wall> points)
    {
      if (!Inited())
        return null;
      int index = RandHelper.Random.Next(points.Count - 2);
      if (index == 0)
        index++;//avoid corner
      var pt = points[index].point;
      var entry = new Tile();
      dungeonNode.SetTile(entry, pt);
      return entry;
    }

    public void GenerateOuterWalls()
    {
      if (!Inited())
        return;

      var topPoints = GenerateWallPoints(0, Width, 0, 1, 0);
      var bottomPoints = GenerateWallPoints(0, Width, Height - 1, Height, 0);
      var leftPoints = GenerateWallPoints(0, 1, 0, Height, 0);
      var rightPoints = GenerateWallPoints(Width - 1, Width, 0, Height, 0);

      var tops = AddWalls(topPoints, true);
      if (tops.Any())
      {
        tops.First().Shadowed = false;
        tops.Last().Shadowed = false;
      }
      var bottoms = AddWalls(bottomPoints, true);
      if (bottoms.Any())
      {
        bottoms.First().Shadowed = false;
        bottoms.Last().Shadowed = false;
      }
      AddWalls(leftPoints, false);
      AddWalls(rightPoints, false);

      SetSideWalls();

      if (this.generationInfo.GenerateDoors)
      {
        var generated = new List<EntranceSide>() {  EntranceSide.Unset};
        for (int i = 0; i < generationInfo.EntrancesCount; i++)
        {
          var entr = GenerateEntranceAtRandomSide(generated.ToArray());
          if (entr.Item2 != null)
          {
            generated.Add(entr.Item1);
            dungeonNode.CreateDoor(entr.Item2, entr.Item1);
          }
        }
      }
      if (Wall.Use25DImages)
      {
        var corners = new[] { new Point(0, 0) , new Point(0, dungeonNode.Height - 1) ,
        new Point(dungeonNode.Width - 1, dungeonNode.Height - 1), 
        new Point(dungeonNode.Width - 1, 0)};
 
        foreach (var co in corners)
        {
          var wall = dungeonNode.GetTile(co) as Wall;
          if (wall != null)
          {
            wall.GenerateUpperChild = true;
            if (co.Y == 0)
              MakeShadowed(wall, false);
          }
        }
      }
    }

    public void SetSideWalls()
    {
      for (int row = 0; row < Height; row++)
      {
        for (int col = 0; col < Width; col++)
        {
          if (row == 0)
          {
            var wall = this.dungeonNode.Tiles[row, col] as Wall;
            dungeonNode.Sides[EntranceSide.Top].Add(wall);
            MakeShadowed(wall, dungeonNode.IsChildIsland, true);
          }

          if (row == Height - 1)
          {
            var wall = this.dungeonNode.Tiles[row, col] as Wall;
            dungeonNode.Sides[EntranceSide.Bottom].Add(this.dungeonNode.Tiles[row, col] as Wall);
            if(col !=0 && col!= Width-1)
              MakeShadowed(wall, fromSetSide: true);
          }

          if (col == 0)
            dungeonNode.Sides[EntranceSide.Left].Add(this.dungeonNode.Tiles[row, col] as Wall);

          if (col == Width - 1)
            dungeonNode.Sides[EntranceSide.Right].Add(this.dungeonNode.Tiles[row, col] as Wall);
        }
      }

      
      foreach (var side in dungeonNode.Sides)
      {
        int sideCounter = 0;
        foreach (var tile in side.Value)
        {
          if (tile == null)
            continue;
          var wall = tile as Wall;
          if (wall != null)
          {
            wall.EntranceSide = side.Key;
            sideCounter++;
          }
          else
            dungeonNode.Container.GetInstance<ILogger>().LogError("if (wall != null) , si=" + tile);
        }

        if (sideCounter < 3)
        {
          //dungeonNode.Container.GetInstance<ILogger>().LogError("sideCounter < 3 = "+ sideCounter + " side: " + side.Key + " NodeInd: "+ dungeonNode);
        }
      }
    }

    Tuple<EntranceSide, Tile> GenerateEntranceAtRandomSide(EntranceSide[] skip)
    {
      EntranceSide side = RandHelper.GetRandomEnumValue<EntranceSide>(skip);
      return GenerateEntranceAtSide(side);
    }

    Tuple<EntranceSide, Tile> GenerateEntranceAtSide(EntranceSide side)
    {
      var tile = GenerateEntrance(dungeonNode.Sides[side]);
      Tuple<EntranceSide, Tile> res = new Tuple<EntranceSide, Tile>(side, tile);
      return res;
    }

    void Split(bool vertically)
    {
      AddSplitWall(vertically);
    }

    List<Point> GenerateWallPoints(int startX, int endX, int startY, int endY, int entrancesCount = 0)
    {
      var wall = new List<Point>();
      for (int row = startY; row < endY; row++)
      {
        for (int col = startX; col < endX; col++)
        {
          wall.Add(new Point(col, row));
        }
      }
      return wall;
    }
#nullable enable
    public Point GetInteriorStartingPoint(int minSizeReduce = 6, DungeonNode? child = null)
    {
      if (!Inited())
        return GenerationConstraints.InvalidPoint;
      int islandWidth = child != null ? child.Width : (this.Width - minSizeReduce);

      int islandHeight = child != null ? child.Height : (this.Height - minSizeReduce);

      int xMiddle = Width / 2;
      int yMiddle = Height / 2;

      int xIsland = xMiddle - islandWidth / 2;
      int yIsland = yMiddle - islandHeight / 2;

      var sp = new Point(xIsland, yIsland);
      return sp;
    }

    /// <summary>
    /// makes Interior.T or Interior.L
    /// </summary>
    /// <param name="interior"></param>
    void GenerateInterior(Interior interior)
    {
      if (!generationInfo.GenerateInterior)
        return;
      var pointsX = new List<Point>();
      var pointsY = new List<Point>();
      var pointsExtraShadowed = new List<Point>();
      var pointsExtraUpperChild = new List<Point>();
      var startPoint = GetInteriorStartingPoint();
      if (interior == Interior.T)
      {
        int endX = Width - startPoint.X;
        pointsX = GenerateWallPoints(startPoint.X, endX, startPoint.Y, startPoint.Y + 1, 1);

        int legX = (startPoint.X + endX) / 2;
        pointsY.AddRange(GenerateWallPoints(legX, legX + 1, startPoint.Y, Height - startPoint.Y));

        if (pointsY.Count > 6)
        {
          pointsExtraShadowed.Add(pointsY[(pointsY.Count / 2) + 1]);
          pointsY.RemoveAt(pointsY.Count / 2);
        }
      }
      else if (interior == Interior.L)
      {
        int legX = startPoint.X;
        //vertical
        pointsY.AddRange(GenerateWallPoints(legX, legX + 1, startPoint.Y, Height - startPoint.Y));
        if (pointsY.Count > 6)
        {
          pointsExtraShadowed.Add(pointsY[(pointsY.Count / 2) + 1]);
          pointsY.RemoveAt(pointsY.Count / 2);
        }
        pointsExtraShadowed.Add(pointsY.First());

        int endX = Width - startPoint.X;
        int endY = Height - startPoint.Y;
        //horiz
        pointsX.AddRange(GenerateWallPoints(legX, endX, endY - 1, endY));
        pointsExtraUpperChild.Add(pointsX.First());
      }

      var walls = AddWalls(pointsX, false);
      foreach (var wall in walls)
      {
        if (interior != Interior.L || wall != walls.First())
          MakeShadowed(wall);
      }

      walls = AddWalls(pointsY, false);
      //if (interior == Interior.T)
      //walls.Last().Color = ConsoleColor.Red;
      pointsExtraShadowed.ForEach(i => {
        var wall = walls.Where(j => j.point == i).SingleOrDefault();
        MakeShadowed(wall);
      });
      pointsExtraUpperChild.ForEach(i =>
      {
        var wall = walls.Where(j => j.point == i).SingleOrDefault();
        if(wall!=null)
          wall.GenerateUpperChild = true;
      });
      AddDecorations();
    }

    public void AddDecorations()
    {
      if (!Inited())
        return;
      if(!generationInfo.GenerateDecorations)
        return;
      Func<Tile, bool> areNeibsEmpty = (Tile i) => { return dungeonNode.GetNeighborTiles(i, true).All(j => j != null && j.IsEmpty); };

      var empty = dungeonNode.GetEmptyTiles().Where(i => areNeibsEmpty(i)).ToList();
      if (empty.Any())
      {
        var t = empty[RandHelper.Random.Next(empty.Count())];
        var pts = new List<Point>() { t.point };

        var others = dungeonNode.GetNeighborTiles(t).Where(i => areNeibsEmpty(i)).ToList();
        if (others.Any())
        {
          int maxDecLen = 6;
          int max = RandHelper.Random.Next(1, maxDecLen);
          for (int i = 0; i < max && i < others.Count; i++)
            pts.Add(others[i].point);
        }

        var walls = AddWalls(pts, false);
        foreach (var wall in walls)
        {
          //Note API is reversed on Y in Unity - North means South. So do not trust Console Client here! 
          var northIsWall = dungeonNode.GetNeighborTile(wall, TileNeighborhood.South) is Wall;
          if (!northIsWall)
          {
            var southIsWall = dungeonNode.GetNeighborTile(wall, TileNeighborhood.North) is Wall;
            if (!southIsWall)
            {
              MakeShadowed(wall);
            }
            else
              wall.GenerateUpperChild = true;
          }
          else
          {
            var southIsWall = dungeonNode.GetNeighborTile(wall, TileNeighborhood.North) is Wall;
            if (!southIsWall)
            {
              MakeShadowed(wall);
            }
          }
        }
      }
    }

    void MakeShadowed(Wall wall, bool canHaveDecor = true, bool fromSetSide = false)
    {
      if (wall == null)
        return;
      
      wall.Shadowed = true;
      if (dungeonNode.Secret && fromSetSide)
        return;
      if (canHaveDecor)
      {
        dungeonNode.InteriorShadowed(wall.point);
      }
    }

    List<Wall> AddWalls(List<Point> points, bool shadowed)
    {
      var tiles = new List<Wall>();
      foreach (var pt in points)
      {
        var curr = dungeonNode.GetTile(pt);
        if (curr == null || curr.IsEmpty)
        {
          var wall = dungeonNode.CreateWall();
          dungeonNode.SetTile(wall, pt);
          tiles.Add(wall);
        }
      }

      return tiles;
    }

    internal DungeonNode[] GenerateChildIslands()
    {
      if (!Inited())
        return null;

      List<DungeonNode> nodes = new List<DungeonNode>();
      int islandWidth = generationInfo.ForcedChilldIslandSize.Width;
      int islandHeight = generationInfo.ForcedChilldIslandSize.Height;
      if (generationInfo.ForcedChilldIslandSize.Height == 0)
      {
        var roomLeft = Width - generationInfo.MinSubMazeNodeSize * generationInfo.MaxNumberOfChildIslands;
        if (roomLeft < generationInfo.MinRoomLeft)
        {
          throw new Exception("roomLeft < generationInfo.MinRoomLeft");
        }
        roomLeft = Height - generationInfo.MinSubMazeNodeSize * generationInfo.MaxNumberOfChildIslands;
        if (roomLeft < generationInfo.MinRoomLeft)
        {
          throw new Exception("roomLeft < generationInfo.MinRoomLeft");
        }
        islandWidth = this.Width - generationInfo.MinRoomLeft;// * generationInfo.NumberOfChildIslands;
        if (generationInfo.MaxNumberOfChildIslands > 1)
          islandWidth -= 2;//TODO
        islandHeight = this.Height / generationInfo.MaxNumberOfChildIslands - generationInfo.MinRoomLeft;// * generationInfo.NumberOfChildIslands;

        var xRandRange = islandWidth - generationInfo.MinSubMazeNodeSize;
        if (xRandRange > 0)
          islandWidth -= RandHelper.Random.Next(xRandRange);

        if (islandWidth < generationInfo.MinSubMazeNodeSize)
          islandWidth = generationInfo.MinSubMazeNodeSize;
        if (islandHeight < generationInfo.MinSubMazeNodeSize)
          islandHeight = generationInfo.MinSubMazeNodeSize;
      }

      var generationInfoIsl = generationInfo.Clone() as GenerationInfo;//TODO
      generationInfoIsl.EntrancesCount = 4;
      generationInfoIsl.ChildIsland = true;
      Point? destStartPoint = null;
      if (generationInfo.MaxNumberOfChildIslands > 1)
        destStartPoint = new Point(generationInfo.MinRoomLeft / 2 + 1, generationInfo.MinRoomLeft / 2);
      for (int i = 0; i < generationInfo.MaxNumberOfChildIslands; i++)
      {
        var child = dungeonNode.CreateChildIslandInstance(islandWidth, islandHeight, generationInfoIsl,
          parent: dungeonNode);
        if (ChildIslandCreated != null)
          ChildIslandCreated(this, new ChildIslandCreationInfo() { ChildIslandNode = child, GenerationInfoIsl = generationInfoIsl, ParentDungeonNode = dungeonNode });
        dungeonNode.AddChildIsland(destStartPoint, child);
        nodes.Add(child);

        if (destStartPoint != null)
        {
          var nextPoint = new Point();
          nextPoint.X = destStartPoint.Value.X;
          nextPoint.Y = destStartPoint.Value.Y + islandHeight + 1;
          destStartPoint = nextPoint;
        }
      }

      return nodes.ToArray();
    }

    public void GenerateRandomStonesBlocks()
    {
      if (!Inited())
        return;
      if (generationInfo.GenerateRandomStonesBlocks)
      {
        int maxDec = (Width + Height) / 4;
        int numDec = RandHelper.Random.Next(3, maxDec > 3 ? maxDec : 3);
        for (int i = 0; i < numDec; i++)
          AddDecorations();
      }
    }
  }
}
