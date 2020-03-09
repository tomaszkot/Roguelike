using System;
using System.Drawing;

namespace Dungeons
{
  /// <summary>
  /// Info describing creation of the dungeon node. Members public to enhance speed.
  /// </summary>
  public class GenerationInfo : ICloneable
  {
    //Number of rooms inside a level, not counting ChildIslands (smallers rooms inside a room)
    public int NumberOfRooms = 2;

    /// <summary>
    /// Normally true, can be set to false for issue testing purposes
    /// </summary>
    public bool CreateDoors = true;

    public int EntrancesCount = 0;
    public bool ChildIsland;
    public bool GenerateOuterWalls = true;
    public bool GenerateRandomInterior = true;
    public bool ForceChildIslandInterior = false;
    public bool FirstNodeSmaller = false;
    public bool GenerateRandomStonesBlocks = true;

    public Size MinNodeSize = new Size(9,9);
    public Size MaxNodeSize = new Size(16, 16);

    public readonly int MinSubMazeNodeSize = 5;
    public readonly int MinSimpleInteriorSize = 3;
    public int MinRoomLeft = 6;
    public int MaxNumberOfChildIslands = 1;
    public bool ChildIslandAllowed = true;
    internal bool GenerateEmptyTiles = true;
    public bool GenerateDoors = true;
    public bool RevealTiles { get; set; } = true;
    public bool RevealAllNodes { get; set; } = false;

    public GenerationInfo()
    {
    }

    public virtual object Clone()
    {
      return this.MemberwiseClone() as GenerationInfo;
    }
  }
}
