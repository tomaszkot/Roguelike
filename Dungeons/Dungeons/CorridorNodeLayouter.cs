using Dungeons.Core;
using Dungeons.TileContainers;
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
    int nodesPadding = 0;
    bool generateLayoutDoors = true;

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
      localLevel.Create(tw, th /*+ nodesPadding * nodes.Count*/, gi, -1, null, false);

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


      mazeNodes[0].Reveal(true);
      mazeNodes[1].Reveal(true);

      var random = new Random(); 

      var distanceBetween = random.Next(12,20);
      var direction = random.Next(0,2);
      
      level.AppendMaze
        (
          mazeNodes[0],
          new Point(0, 0)
        );

      if (direction == 0){
        level.AppendMaze
        (
          mazeNodes[1],
          new Point(distanceBetween, 0)
        );
      }
      else {
        level.AppendMaze
          (
            mazeNodes[1],
            new Point(0, distanceBetween)
          );
      }

      var heightOfPassage = 0;
      var widthOfPassage = 0;

      if (direction == 0){
        heightOfPassage = (mazeNodes[0].Height / 2) - 1;
        widthOfPassage = mazeNodes[0].Width - 1;
      }
      if (direction == 1){
        heightOfPassage = mazeNodes[0].Height - 1;
        widthOfPassage = mazeNodes[0].Width / 2;
      }

      var d = new DungeonGenerator(container);
      var infoC = new GenerationInfo();
      infoC.NumberOfRooms = 1;

      if (direction == 0)
      {
        infoC.MinNodeSize = new Size(distanceBetween - mazeNodes[0].Width + 3, 4);
        infoC.MaxNodeSize = new Size(distanceBetween - mazeNodes[0].Width + 3, 4);
      }
      else{
        infoC.MinNodeSize = new Size(4, distanceBetween - mazeNodes[0].Height + 3);
        infoC.MaxNodeSize = new Size(4, distanceBetween - mazeNodes[0].Height + 3);
      }
      var corrindorNodes = d.CreateDungeonNodes(infoC);
      mazeNodes.Add(corrindorNodes[0]);

      mazeNodes[2].Reveal(true);

      level.AppendMaze
     (
       mazeNodes[2],
       new Point(widthOfPassage, heightOfPassage)
     );

      //info.MinNodeSize = new Size(2, 2);
      //info.MaxNodeSize = new Size(2, 2);
      //var info = new AppendNodeInfo(mazeNodes[0]);
      //info.side = EntranceSide.Right;

      //float chanceForLevelTurn = 0.5f;
      //EntranceSide? prevEntranceSide = null;
      //var secretRoomIndex = mazeNodes.FindIndex(i => i.Secret);

      //for (int currentNodeIndex = 0; currentNodeIndex < mazeNodes.Count; currentNodeIndex++)
      //{
      //  appendNodeInfos.Add(mazeNodes[currentNodeIndex], info);

      //  var currentNode = mazeNodes[currentNodeIndex];
      //  currentNode.Reveal(options.RevealAllNodes, true);
      //  if (!currentNode.Secret)
      //    lastNonSecretNode = currentNode;


      //  bool shallBreak = currentNodeIndex == mazeNodes.Count - 1;
      //  AppendNodeInfo infoNext = new AppendNodeInfo();
      //  //shallBreak = true; // SK delete
      //  if (!shallBreak)
      //  {
      //    infoNext = CalcNextValues(info, chanceForLevelTurn, currentNodeIndex);

      //    if (currentNodeIndex < mazeNodes.Count - 1 && generateLayoutDoors)
      //    {
      //      var nextMaze = mazeNodes[currentNodeIndex + 1];
      //      var secretRoom = currentNodeIndex == 0 ? currentNode.Secret : nextMaze.Secret;
      //      if (!secretRoom)
      //        secretRoom = nextMaze.Secret;

      //      //this call must be done before AppendMaze because AppendMaze changes tiles x,y
      //      List<Tiles.IDoor> doors = null;
      //      if (!currentNode.Secret || currentNode.NodeIndex == 0)
      //        doors = currentNode.GenerateLayoutDoors(infoNext.side, nextMaze.NodeIndex, secretRoom);
      //      if (currentNode.Secret)
      //      {
      //        if (currentNode.NodeIndex == 0)
      //          (doors[0]).CustomDungeonNodeIndex = 1;//to make them revealed ?
      //      }

      //      if (nextMaze.Secret && mazeNodes.Count > currentNodeIndex + 2)
      //      {
      //        doors = currentNode.GenerateLayoutDoors(infoNext.side == EntranceSide.Bottom ? EntranceSide.Right : EntranceSide.Bottom, nextMaze.NodeIndex, false, true);
      //      }
      //    }
      //  }

      //  EntranceSide? entranceSideToSkip = null;
      //  if (secretRoomIndex == 0 && currentNodeIndex == 1)
      //    entranceSideToSkip = null;
      //  else
      //  {
      //    entranceSideToSkip = null;
      //    if (currentNodeIndex > 0)
      //      entranceSideToSkip = info.side == EntranceSide.Bottom ? EntranceSide.Top : EntranceSide.Left;
      //  }

      //  level.AppendMaze
      //  (
      //    currentNode,
      //    info.Position,
      //    null,
      //    false,
      //    entranceSideToSkip,
      //    currentNodeIndex > 0 ? mazeNodes[currentNodeIndex - 1] : null
      //  );
      //  if (shallBreak)
      //    break;
      //  entranceSideToSkip = null;
      //  prevEntranceSide = infoNext.side;
      //  info = infoNext;
      //}
    }

    private AppendNodeInfo CalcNextValues(AppendNodeInfo currentAppendInfo, float chanceForLevelTurn, int currentNodeIndex)
    {
      var currentNode = mazeNodes[currentNodeIndex];
      //copy current append info (struct)
      AppendNodeInfo nextAppendInfo = currentAppendInfo;

      nextAppendInfo.DungeonNode = mazeNodes[currentNodeIndex + 1];
      DungeonNode sizeProvider = currentAppendInfo.DungeonNode;


      if (currentNode.Secret)
      {
        if (currentNode.NodeIndex == 0)
          nextAppendInfo.side = GetRandSide();
        else
        {
          if (info != null && info.ForcedNextRoomSide != null)
            nextAppendInfo.side = info.ForcedNextRoomSide.Value;
          else
          {
            if (currentAppendInfo.side == EntranceSide.Bottom)
              nextAppendInfo.side = EntranceSide.Right;
            else if (currentAppendInfo.side == EntranceSide.Right)
              nextAppendInfo.side = EntranceSide.Bottom;
          }
        }
        if (lastNonSecretNode != null)
        {
          sizeProvider = lastNonSecretNode;
          nextAppendInfo.Position = appendNodeInfos[lastNonSecretNode].Position;
        }
      }
      else
      {
        //if (currentNodeIndex == 0)
        //  nextAppendInfo.side = EntranceSide.Bottom;
        //else
        nextAppendInfo.side = CalcSide(currentAppendInfo.side, nextAppendInfo.side, chanceForLevelTurn);
      }

      var pt = nextAppendInfo.Position;
      if (nextAppendInfo.side == EntranceSide.Bottom)
      {
        pt.Y += sizeProvider.Height - 1 + nodesPadding; //SK -1 => +2
      }
      else if (nextAppendInfo.side == EntranceSide.Right)
      {
        pt.X += sizeProvider.Width - 1;//SK - 1 => +2
      }
      nextAppendInfo.Position = pt;

      return nextAppendInfo;
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

