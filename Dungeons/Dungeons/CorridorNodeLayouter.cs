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
      var random = new Random();

      var currentWidth = 0;
      var currentHeight = 0;
      List<int> distances = new List<int>();
      mazeNodes[0].GenerateLayoutDoorsCorrindor(EntranceSide.Bottom);
      mazeNodes[0].GenerateLayoutDoorsCorrindor(EntranceSide.Right);
      mazeNodes[1].GenerateLayoutDoorsCorrindor(EntranceSide.Top);
      mazeNodes[1].GenerateLayoutDoorsCorrindor(EntranceSide.Right);
      mazeNodes[2].GenerateLayoutDoorsCorrindor(EntranceSide.Left);
      mazeNodes[2].GenerateLayoutDoorsCorrindor(EntranceSide.Bottom);
      mazeNodes[3].GenerateLayoutDoorsCorrindor(EntranceSide.Top);
      mazeNodes[3].GenerateLayoutDoorsCorrindor(EntranceSide.Left);
      mazeNodes[4].GenerateLayoutDoorsCorrindor(EntranceSide.Bottom);
      mazeNodes[4].GenerateLayoutDoorsCorrindor(EntranceSide.Top);
      mazeNodes[4].GenerateLayoutDoorsCorrindor(EntranceSide.Left);
      mazeNodes[4].GenerateLayoutDoorsCorrindor(EntranceSide.Right);
      for (int i = 1; i < mazeNodes.Count; i++) //creating rooms
      {
        if (i % 2 == 1)
        {
          level.AppendMaze
          (
           mazeNodes[i - 1],
           new Point(currentWidth, currentHeight)
          );
          currentHeight += random.Next(24, 28);
          distances.Add(currentHeight);
        }
        if (i % 2 == 0)
        {
          if (i == 4)
            currentWidth = random.Next(24, 28);
          level.AppendMaze
          (
           mazeNodes[i - 1],
           new Point(currentWidth, currentHeight)
          );
          currentHeight = 0;
          if (currentWidth == 0)
            currentWidth += random.Next(24, 28);
          distances.Add(currentWidth);
        }
      }
      level.AppendMaze
      (
       mazeNodes[4],
       new Point(12, 12)
      );


      var d = new DungeonGenerator(container);
      var infoC = new GenerationInfo();
      infoC.NumberOfRooms = 1;
      currentWidth = 0;
      currentHeight = 0;
      var h = 0;
      var h1 = 0;
      for (int i = 0; i < 4; i++) //creating corrindors
      {
        if (i % 2 == 0)
        {
          infoC.MinNodeSize = new Size(distances[i+1] - mazeNodes[h].Width + 3, 4);
          infoC.MaxNodeSize = new Size(distances[i+1] - mazeNodes[h].Width + 3, 4);
          h += 1;
        }
        else {
          infoC.MinNodeSize = new Size(4, distances[h1] - mazeNodes[h1].Height + 3);
          infoC.MaxNodeSize = new Size(4, distances[h1] - mazeNodes[h1].Height + 3);
          h1 += 2;
        }
        var corrindorNodes = d.CreateDungeonNodes(infoC);
        mazeNodes.Add(corrindorNodes[0]);
      }


      infoC.MinNodeSize = new Size(4, 12 - (mazeNodes[0].Height / 2) + 1); //creating corrindors to island
      infoC.MaxNodeSize = new Size(4, 12 - (mazeNodes[0].Height / 2) + 1);
      var corrindorIslandNotes = d.CreateDungeonNodes(infoC);
      mazeNodes.Add(corrindorIslandNotes[0]);

      infoC.MinNodeSize = new Size(4, (mazeNodes[1].Height/2) + distances[0] - 10 - mazeNodes[4].Height);
      infoC.MaxNodeSize = new Size(4, (mazeNodes[1].Height / 2) + distances[0] - 10 - mazeNodes[4].Height);
      corrindorIslandNotes = d.CreateDungeonNodes(infoC);
      mazeNodes.Add(corrindorIslandNotes[0]);

      infoC.MinNodeSize = new Size(12 - (mazeNodes[0].Width / 2) + 2, 4);
      infoC.MaxNodeSize = new Size(12 - (mazeNodes[0].Width / 2) + 2, 4);
      corrindorIslandNotes = d.CreateDungeonNodes(infoC);
      mazeNodes.Add(corrindorIslandNotes[0]);

      infoC.MinNodeSize = new Size(distances[1] - 11 - mazeNodes[4].Width + (mazeNodes[2].Width/2),4);
      infoC.MaxNodeSize = new Size(distances[1] - 11 - mazeNodes[4].Width + (mazeNodes[2].Width / 2), 4);
      corrindorIslandNotes = d.CreateDungeonNodes(infoC);
      mazeNodes.Add(corrindorIslandNotes[0]);

      var heightOfPassage = 0;
      var widthOfPassage = 0;
      currentHeight = 0;
      currentWidth = 0;
      h = 0;
      h1 = 0;

      mazeNodes[5].GenerateLayoutDoorsCorrindor(EntranceSide.Left);
      mazeNodes[5].GenerateLayoutDoorsCorrindor(EntranceSide.Right);
      mazeNodes[6].GenerateLayoutDoorsCorrindor(EntranceSide.Bottom);
      mazeNodes[6].GenerateLayoutDoorsCorrindor(EntranceSide.Top);
      mazeNodes[7].GenerateLayoutDoorsCorrindor(EntranceSide.Left);
      mazeNodes[7].GenerateLayoutDoorsCorrindor(EntranceSide.Right);
      mazeNodes[8].GenerateLayoutDoorsCorrindor(EntranceSide.Bottom);
      mazeNodes[8].GenerateLayoutDoorsCorrindor(EntranceSide.Top);

      mazeNodes[9].GenerateLayoutDoorsCorrindor(EntranceSide.Top);
      mazeNodes[9].GenerateLayoutDoorsCorrindor(EntranceSide.Bottom);
      mazeNodes[10].GenerateLayoutDoorsCorrindor(EntranceSide.Top);
      mazeNodes[10].GenerateLayoutDoorsCorrindor(EntranceSide.Bottom);
      mazeNodes[11].GenerateLayoutDoorsCorrindor(EntranceSide.Left);
      mazeNodes[11].GenerateLayoutDoorsCorrindor(EntranceSide.Right);
      mazeNodes[12].GenerateLayoutDoorsCorrindor(EntranceSide.Right);
      mazeNodes[12].GenerateLayoutDoorsCorrindor(EntranceSide.Left);

      for (int i = 0; i<4;i++) //placing corrindors
      {
        if ((i % 2) == 0){
          heightOfPassage = ((mazeNodes[h].Height / 2) - 1) + currentHeight;
          widthOfPassage = mazeNodes[h].Width - 1;
          h++;
          currentHeight = distances[0];
        }
        if ((i % 2) == 1){ //h
          heightOfPassage = mazeNodes[h1].Height - 1;
          widthOfPassage = mazeNodes[h1].Width / 2 + currentWidth - 2;
          currentWidth = distances[1];
          h1 += 2;
        }
          level.AppendMaze
        (
          mazeNodes[i + 5],
          new Point(widthOfPassage, heightOfPassage)
        );
      }

      heightOfPassage = (mazeNodes[0].Height / 2) + 1;
      widthOfPassage = 12 + (mazeNodes[4].Width / 2) - 1;
      level.AppendMaze
       (
         mazeNodes[9],
         new Point(widthOfPassage, heightOfPassage)
       );

      heightOfPassage = 12 + mazeNodes[4].Height - 1;
      level.AppendMaze
       (
         mazeNodes[10],
         new Point(widthOfPassage, heightOfPassage)
       );

      heightOfPassage = ((distances[0] - mazeNodes[0].Height)/2 + mazeNodes[0].Height) - 1;
      widthOfPassage = (mazeNodes[0].Width / 2);
      level.AppendMaze
       (
         mazeNodes[11],
         new Point(widthOfPassage, heightOfPassage)
       );

      widthOfPassage = 12 + mazeNodes[4].Width - 1;
      level.AppendMaze
       (
         mazeNodes[12],
         new Point(widthOfPassage, heightOfPassage)
       );

      mazeNodes.ForEach(p => p.Reveal(options.RevealAllNodes,true));
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

