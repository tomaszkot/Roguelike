using Dungeons.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons.TileContainers
{
  public class DungeonNodeConnector
  {
    public Point Node1Position;
    public Point Node2Position;
    public Point CorridorPosition;
    public RoomPlacement placement = new RoomPlacement();
    public const int centralRoomPosition = 12;
    int[] wallLength = { 0, 1, 2, 3 };
    public int upperLenght = RandHelper.Random.Next(24, 27);
    public int bottomLenght = RandHelper.Random.Next(24, 27);
    public int rightLenght = RandHelper.Random.Next(24, 27);
    public int leftLenght = RandHelper.Random.Next(24, 27);

    public DungeonNode ConnectNodes(List<DungeonNode> nodes, DungeonNode node1, DungeonNode node2, DungeonGenerator dungeon)
    {
      Node1Position = PlaceNodes(node1);
      Node2Position = PlaceNodes(node2);

      var infoC = new GenerationInfo();
      infoC.NumberOfRooms = 1;
      if (node1.Placement == RoomPlacement.LeftUpper)
      {
        infoC.MinNodeSize = new Size(Node2Position.X - node1.Width + wallLength[3], 4);
        infoC.MaxNodeSize = new Size(Node2Position.X - node1.Width + wallLength[3], 4);
        CorridorPosition = new Point(node1.Width - wallLength[1], Node1Position.Y + (node1.Height / 2) - wallLength[1]);
        placement = RoomPlacement.CorrindorHorizontalTop;
      }
      if (node1.Placement == RoomPlacement.RightUpper)
      {
        infoC.MinNodeSize = new Size(4, Node2Position.Y - node1.Height + wallLength[3]);
        infoC.MaxNodeSize = new Size(4, Node2Position.Y - node1.Height + wallLength[3]);
        CorridorPosition = new Point(Node1Position.X + (node1.Width / 2) - wallLength[1], node1.Height - wallLength[1]);
        placement = RoomPlacement.CorrindorVerticalRight;
      }
      if (node1.Placement == RoomPlacement.RightLower)
      {
        infoC.MinNodeSize = new Size(bottomLenght - node2.Width + wallLength[3], 4);
        infoC.MaxNodeSize = new Size(bottomLenght - node2.Width + wallLength[3], 4);
        CorridorPosition = new Point(node2.Width - wallLength[1], Node2Position.Y + (node2.Height / 2) - wallLength[1]);
        placement = RoomPlacement.CorrindorHorizontalBottom;
      }
      if (node1.Placement == RoomPlacement.LeftLower)
      {
        infoC.MinNodeSize = new Size(4, Node1Position.Y - node2.Height + wallLength[3]);
        infoC.MaxNodeSize = new Size(4, Node1Position.Y - node2.Height + wallLength[3]);
        CorridorPosition = new Point((node2.Width / 2) - wallLength[1], node2.Height - wallLength[1]);
        placement = RoomPlacement.CorrindorVerticalLeft;
      }
      if (node1.Placement == RoomPlacement.CorrindorHorizontalTop)
      {
        infoC.MinNodeSize = new Size(4, centralRoomPosition - (nodes[(int)RoomPlacement.LeftUpper].Height / 2) + wallLength[1]);
        infoC.MaxNodeSize = new Size(4, centralRoomPosition - (nodes[(int)RoomPlacement.LeftUpper].Height / 2) + wallLength[1]);
        CorridorPosition = new Point(centralRoomPosition + (node2.Width / 2) - wallLength[1], (nodes[(int)RoomPlacement.LeftUpper].Height / 2) + wallLength[1]);
        placement = RoomPlacement.CorrindorVerticalRight;
      }
      if (node1.Placement == RoomPlacement.CorrindorVerticalRight)
      {
        infoC.MinNodeSize = new Size(upperLenght - centralRoomPosition - node2.Width + (nodes[(int)RoomPlacement.RightUpper].Width / 2) + wallLength[2], 4);
        infoC.MaxNodeSize = new Size(upperLenght - centralRoomPosition - node2.Width + (nodes[(int)RoomPlacement.RightUpper].Width / 2) + wallLength[2], 4);
        CorridorPosition = new Point(centralRoomPosition + node2.Width - wallLength[1], centralRoomPosition + (node2.Height / 2) - wallLength[1]);
        placement = RoomPlacement.CorrindorHorizontalTop;
      }
      if (node1.Placement == RoomPlacement.CorrindorHorizontalBottom)
      {
        infoC.MinNodeSize = new Size(4, leftLenght - centralRoomPosition + (nodes[(int)RoomPlacement.LeftLower].Height / 2) - node2.Height + wallLength[2]);
        infoC.MaxNodeSize = new Size(4, leftLenght - centralRoomPosition + (nodes[(int)RoomPlacement.LeftLower].Height / 2) - node2.Height + wallLength[2]);
        CorridorPosition = new Point(centralRoomPosition + (node2.Width / 2) - wallLength[1], centralRoomPosition + node2.Height - wallLength[1]);
        placement = RoomPlacement.CorrindorVerticalLeft;
      }
      if (node1.Placement == RoomPlacement.CorrindorVerticalLeft)
      {
        infoC.MinNodeSize = new Size(12 - (nodes[(int)RoomPlacement.LeftUpper].Width / 2) + wallLength[1], 4);
        infoC.MaxNodeSize = new Size(12 - (nodes[(int)RoomPlacement.LeftUpper].Width / 2) + wallLength[1], 4);
        CorridorPosition = new Point((nodes[(int)RoomPlacement.LeftUpper].Width / 2) + wallLength[1], centralRoomPosition + (node2.Height / 2) - wallLength[1]);
        placement = RoomPlacement.CorrindorHorizontalBottom;
      }

      var corridor = dungeon.CreateDungeonNodes(infoC);
      corridor[0].Placement = placement;
      return corridor[0];
    }


    public Point PlaceNodes(DungeonNode node)
    {
      var position = new Point();
      if (node.Placement == RoomPlacement.LeftUpper)
        position = new Point(0, 0);

      if (node.Placement == RoomPlacement.RightUpper)
        position = new Point(upperLenght, 0);

      if (node.Placement == RoomPlacement.LeftLower)
        position = new Point(0, leftLenght);

      if (node.Placement == RoomPlacement.RightLower)
        position = new Point(bottomLenght, rightLenght);

      if (node.Placement == RoomPlacement.Center)
        position = new Point(centralRoomPosition, centralRoomPosition);
      return position;
    }
  }
}

