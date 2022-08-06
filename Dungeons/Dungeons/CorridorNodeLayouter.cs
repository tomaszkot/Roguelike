﻿using Dungeons.Core;
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
    //int nodesPadding = 0;
    //bool generateLayoutDoors = true;
    //EntranceSide? forcedNextSide = null;//EntranceSide.Bottom;
    //EntranceSide? forcedEntranceSideToSkip = null;// EntranceSide.Right;
    LayouterOptions options;
    Container container;
    GenerationInfo info;
    DungeonNode lastNonSecretNode;
    List<DungeonNode> mazeNodes;
    Dictionary<DungeonNode, AppendNodeInfo> appendNodeInfos = new Dictionary<DungeonNode, AppendNodeInfo>();
    
    public CorridorNodeLayouter(Container container, GenerationInfo info = null)
    {
      this.container = container;
      this.info = info;
    }

    public DungeonLevel DoLayout(List<DungeonNode> nodes, LayouterOptions opt = null)
    {
      if (nodes.Any(i => !i.Created))
        return container.GetInstance<DungeonLevel>();
      options = opt;
      if (options == null)
        options = new LayouterOptions();
      //totals sizes
      var tw = nodes.Sum(i => i.Width);
      tw += 15; //SK
      var th = nodes.Sum(i => i.Height);
      th += 20;

      var gi = new GenerationInfo();
      gi.GenerateOuterWalls = false;
      gi.GenerateRandomInterior = false;
      gi.GenerateEmptyTiles = false;
      var localLevel = container.GetInstance<DungeonNode>();
      localLevel.Container = this.container;
      //Activator.CreateInstance(typeof(T), new object[] { container }) as T;
      localLevel.Create(tw+1, th+1 /*+ nodesPadding * nodes.Count*/, gi, -1, null, false);

      var maxLoc = localLevel.GetMaxXY();
      if (nodes.Count > 1)
      {
        if (nodes[1].GetTiles().First().DungeonNodeIndex != nodes[1].NodeIndex)
          container.GetInstance<ILogger>().LogError("nodes[1].GetTiles().First().DungeonNodeIndex != nodes[1].NodeIndex");
      }

      LayoutNodes(localLevel, nodes);

      if (localLevel.Tiles[0, 0] != null && localLevel.Tiles[0, 0].DungeonNodeIndex == DungeonNode.DefaultNodeIndex)
      {
        container.GetInstance<ILogger>().LogError("localLevel.Tiles[0, 0].DungeonNodeIndex == " + DungeonNode.DefaultNodeIndex);
      }

      var max = localLevel.GetMaxXY();

      var level = container.GetInstance<DungeonLevel>();
      var width = max.Item1 + 1;
      var height = max.Item2 + 1;
      level.Create(width, height);

      //var doors = localLevel.GetTiles<IDoor>();
      //var secret = doors.Any(i => i.Secret);
      //if(!info.PreventSecretRoomGeneration)
      //  Debug.Assert(secret);
      level.AppendMaze(localLevel, new Point(0, 0), new Point(width, height));
      level.DeleteWrongDoors();
      //if (secret)
      //{
      //  var secretAppended = level.GetTiles<IDoor>().Any(i => i.Secret);
      //  Debug.Assert(secretAppended);
      //}

      level.SecretRoomIndex = -1;
      var sn = nodes.Where(i => i.Secret).FirstOrDefault();
      if (sn != null)
        level.SecretRoomIndex = sn.NodeIndex;
      return level;
    }

    protected virtual void LayoutNodes(DungeonNode level, List<DungeonNode> mazeNodes)
    {
      this.mazeNodes = mazeNodes;
      var d = new DungeonGenerator(container);
      var infoC = new GenerationInfo();
      infoC.NumberOfRooms = 1;
      var numberOfNodes = mazeNodes.Count;

      for (int i=0; i<numberOfNodes; i++)
      {
        mazeNodes[i].Placement = (RoomPlacement)(i);
        mazeNodes[i].GenerateDoors((RoomPlacement)(i));
      }

      var conn = new DungeonNodeConnector(d); //generating normal rooms
      
      var nodesCopy = mazeNodes.ToList();
      var corrNodeIndexStart = 10;
      for (int nodeIndex = 0; nodeIndex < nodesCopy.Count - 1; nodeIndex++)
      {
        var nextNodeIndex = nodeIndex + 1;
        var corridor = conn.ConnectNodes(nodesCopy, nodesCopy[nodeIndex], nodesCopy[nextNodeIndex]);
        corridor.NodeIndex = corrNodeIndexStart++;
        corridor.SetTilesNodeIndex();
        mazeNodes.Add(corridor);
        if (!nodesCopy[nodeIndex].Appened)
          level.AppendMaze(nodesCopy[nodeIndex], conn.Node1Position);

        if (!nodesCopy[nextNodeIndex].Appened)
          level.AppendMaze(nodesCopy[nextNodeIndex], conn.Node2Position);

        corridor.GenerateDoors(conn.placement);
        level.AppendMaze(corridor, conn.CorridorPosition);
      }

      if (numberOfNodes >= 4)
      {
        var corridor = conn.ConnectNodes(nodesCopy, nodesCopy[3], nodesCopy[0]);
        corridor.GenerateDoors(conn.placement);
        level.AppendMaze(corridor, conn.CorridorPosition);
        mazeNodes.Add(corridor);
      }

      if (numberOfNodes > 4)
      {
        for (int i = 5; i < 9; i++) //generatig central room
        {
          var corridor = conn.ConnectNodes(mazeNodes, mazeNodes[i], mazeNodes[4]);
          corridor.NodeIndex = corrNodeIndexStart++;
          corridor.SetTilesNodeIndex();
          mazeNodes.Add(corridor);
          mazeNodes[mazeNodes.Count - 1].GenerateDoors(mazeNodes[mazeNodes.Count - 1].Placement);
          level.AppendMaze(mazeNodes[mazeNodes.Count - 1], conn.CorridorPosition);
        }
      }
      mazeNodes.ForEach(p => p.Reveal(options.RevealAllNodes, true));
    }

    

    private EntranceSide CalcSide(EntranceSide current, EntranceSide next, float chanceForLevelTurn)//, int currentNodeIndex)
    {
      if (info != null && info.ForcedNextRoomSide != null)
        return info.ForcedNextRoomSide.Value;

      EntranceSide side = GetRandSide();
      if (current == next)
      {
        if (RandHelper.GetRandomDouble() >= chanceForLevelTurn)
          side = side == EntranceSide.Bottom ? EntranceSide.Right : EntranceSide.Bottom;
      }

      return side;
    }

    private static EntranceSide GetRandSide()
    {
      return RandHelper.GetRandomDouble() >= .5f ? EntranceSide.Bottom : EntranceSide.Right;
    }
  }

}

