using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Dungeons
{
  class CorridorNodeLayouter : INodeLayouter
  {
    LayouterOptions options;
    Container container;
    GenerationInfo info;
    IDungeonGenerator dg;
    ILogger logger;

    public CorridorNodeLayouter(Container container, IDungeonGenerator dg, GenerationInfo info = null)
    {
      this.container = container;
      this.info = info;
      this.dg = dg;
      logger = container.GetInstance<ILogger>();
    }

    public DungeonLevel DoLayout(List<DungeonNode> nodes, LayouterOptions opt = null)
    {
      var localLevel = container.GetInstance<DungeonLevel>();
      if (nodes.Any(i => !i.Created))
        return localLevel;
      options = opt;
      if (options == null)
        options = new LayouterOptions();
      //totals sizes
      var tw = nodes.Sum(i => i.Width);
      tw += 15; //SK
      var th = nodes.Sum(i => i.Height);
      th += 20;

      var gi = container.GetInstance<GenerationInfo>();
      gi.GenerateOuterWalls = false;
      gi.GenerateRandomInterior = false;
      gi.GenerateEmptyTiles = false;
      
      localLevel.Container = this.container;
      localLevel.Create(tw + 1, th + 1 /*+ nodesPadding * nodes.Count*/, gi, -1, null, false);

      var maxLoc = localLevel.GetMaxXY();
      if (nodes.Count > 1)
      {
        if (nodes[1].GetTiles().First().DungeonNodeIndex != nodes[1].NodeIndex)
          logger.LogError("nodes[1].GetTiles().First().DungeonNodeIndex != nodes[1].NodeIndex");
      }

      LayoutNodes(localLevel, nodes);

      if (localLevel.Tiles[0, 0] != null && localLevel.Tiles[0, 0].DungeonNodeIndex == DungeonNode.DefaultNodeIndex)
      {
        logger.LogError("localLevel.Tiles[0, 0].DungeonNodeIndex == " + DungeonNode.DefaultNodeIndex);
      }
           
      return localLevel;
    }


    List<Point> roomPositions = new List<Point>();
    Dictionary<RoomPlacement, DungeonNode> rooms = new Dictionary<RoomPlacement, DungeonNode>();
    protected virtual void LayoutNodes(DungeonNode level, List<DungeonNode> mazeNodes)
    {
      //this.mazeNodes = mazeNodes;
      var dungeonGenerator = dg;
      var numberOfNodes = mazeNodes.Count;
      

      for (int i = 0; i < numberOfNodes; i++)
      {
        var rp = (RoomPlacement)(i);
        mazeNodes[i].Placement = rp;
        rooms[rp] = mazeNodes[i];
      }

      var gi = info.Clone() as GenerationInfo;
      gi.MinimalContent = true;
      var conn = new DungeonNodeConnector(container, dungeonGenerator, gi); //generating normal rooms

      var nodesCopy = mazeNodes.ToList();

      for (int nodeIndex = 0; nodeIndex < nodesCopy.Count - 1; nodeIndex++)
      {
        var nextNodeIndex = nodeIndex + 1;
        var corridor = conn.ConnectNodes(nodesCopy, nodesCopy[nodeIndex], nodesCopy[nextNodeIndex]);

        if (!nodesCopy[nodeIndex].Appened)
          level.AppendMaze(nodesCopy[nodeIndex], conn.Node1Position);

        if (!nodesCopy[nextNodeIndex].Appened)
          level.AppendMaze(nodesCopy[nextNodeIndex], conn.Node2Position);

        AppendCorridor(level, mazeNodes, conn, corridor);
      }
      
      if (numberOfNodes >= 4)
      {
        var corridor = conn.ConnectNodes(nodesCopy, nodesCopy[3], nodesCopy[0]);
        AppendCorridor(level, mazeNodes, conn, corridor);
      }

      if (numberOfNodes > 4)
      {
        var centerRoom = rooms[RoomPlacement.Center];

        var outerCorrs = new[] {
        RoomPlacement.CorrindorLeftUpperToRightUpper,
        RoomPlacement.CorrindorRightUpperToRightLower,
        RoomPlacement.CorrindorRightLowerToLeftLower,
        RoomPlacement.CorrindorLeftLowerToLeftUpper,
        };
        foreach (var corr in outerCorrs)
        {
          var corridor = conn.ConnectNodes(mazeNodes, rooms[corr], centerRoom);
          AppendCorridor(level, mazeNodes, conn, corridor);
        }
                
        CreateSecretRoom(level, mazeNodes, dungeonGenerator);
      }


      mazeNodes.ForEach(p => p.Reveal(options.RevealAllNodes, true));
    }

    private void CreateSecretRoom(DungeonNode level, List<DungeonNode> mazeNodes, IDungeonGenerator dungeonGenerator)
    {
      logger.LogInfo("CreateSecretRoom start");
      var centerRoom = rooms[RoomPlacement.Center];
      info.SecretRoomIndex = mazeNodes.Count+1;
      var node = dungeonGenerator.CreateDungeonNodeInstance();
      node.Secret = true;
      
      var he = 8;
      var wi = 8;

      var entr = RandHelper.GetRandomDouble() > 0.5f ? EntranceSide.Bottom : EntranceSide.Top;

      if (entr == EntranceSide.Bottom || entr == EntranceSide.Top)
        wi = centerRoom.Width;

      if (entr == EntranceSide.Bottom)
        he = rooms[RoomPlacement.CorrindorLeftUpperToRightUpper].AppendMazeStartPoint.Value.Y - rooms[RoomPlacement.LeftUpper].AppendMazeStartPoint.Value.Y;
      else if (entr == EntranceSide.Top)
      {
        var firstRoom = rooms[RoomPlacement.LeftLower];
        var secRoom = rooms[RoomPlacement.CorrindorRightLowerToLeftLower];
        //he = firstRoom.AppendMazeStartPoint.Value.Y + firstRoom.Height -
        //  secRoom.AppendMazeStartPoint.Value.Y - secRoom.Height;
        he = 6;
      }

      var pt = new Point();
      if (entr == EntranceSide.Bottom)
        pt = new Point(centerRoom.AppendMazeStartPoint.Value.X, 0);
      else if (entr == EntranceSide.Top)
      {
        var secRoom = rooms[RoomPlacement.CorrindorRightLowerToLeftLower];
        pt = new Point(centerRoom.AppendMazeStartPoint.Value.X, secRoom.AppendMazeStartPoint.Value.Y + secRoom.Height -1);
      }

      node.Create(wi, he + 1, info, info.SecretRoomIndex);
      CreateSecretDoor(node, entr);
      dungeonGenerator.GenerateRoomContent(node.NodeIndex, info, node);

      mazeNodes.Add(node);
      level.AppendMaze(node, pt);
      logger.LogInfo("CreateSecretRoom end");
    }

    private void CreateSecretDoor(DungeonNode dn, EntranceSide entranceSide)
    {
      if (!info.GenerateOuterWalls)
        return;
      var walls = dn.Sides[entranceSide];
      if (entranceSide == EntranceSide.Top)
      {
        walls.ForEach(i => dn.InteriorShadowed(i.point));

      }
      var index = Enumerable.Range(1, walls.Count-3).ToList().GetRandomElem();
      var door = dn.CreateDoor(walls[index], entranceSide) as IDoor;
      door.Secret = true;

      if (entranceSide == EntranceSide.Bottom || entranceSide == EntranceSide.Top)
      {
        walls.First().Shadowed = true;
        walls.Last().Shadowed = true;
        if (entranceSide == EntranceSide.Top)
        {
          dn.InteriorShadowed(walls.First().point);
          dn.InteriorShadowed(walls.Last().point);
        }
      }
      
    }

    private void AppendCorridor(DungeonNode level, List<DungeonNode> mazeNodes, DungeonNodeConnector conn, DungeonNode corridor)
    {
      var info = conn.GetRoomPlacementInfo(corridor.Placement);
      var same = roomPositions.Any(i => i == info.Position);
      if (same)
        return;

      rooms[corridor.Placement] = corridor;
      roomPositions.Add(info.Position);
      level.AppendMaze(corridor, info.Position);
      mazeNodes.Add(corridor);
    }

    private static EntranceSide GetRandSide()
    {
      return RandHelper.GetRandomDouble() >= .5f ? EntranceSide.Bottom : EntranceSide.Right;
    }
  }

}

