using Dungeons.Core;
using Dungeons.TileContainers;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Linq;

namespace Dungeons.Tiles
{
  public class Constants
  {
    public const int MinNormalNodeIndex = 0;

    public const char SymbolBackground = '.';
    public const char SymbolDoor = '+';
    public const char SymbolWall = '#';
  }

  //[XmlInclude(typeof(Wall))]
  [Serializable]
  public class Tile
  {
    //members public for speed purposes
    public Point point;

    private char symbol = Constants.SymbolBackground;
    public string name;
    private string displayedName;
    public string tag1 = "";//custom purpose field
    public string tag2 = "";//custom purpose field
    public ConsoleColor color = ConsoleColor.White;

    /// <summary>
    /// The index of the node (room) the tile belongs to
    /// </summary>
    //public int DungeonNodeIndex = Constants.MinNormalNodeIndex - 1;

    /// <summary>
    /// If the tile is at node's corner this member says which corner it is.
    /// </summary>
    public TileCorner? corner;

    /// <summary>
    /// If false the tile is not visible. The revealed flag shall be typically set to true when a door leading to room are opened.
    /// </summary>
    bool revealed = GenerationInfo.DefaultRevealedValue;

    [JsonIgnore]
    public string DestroySound { get; set; }


    public Tile() : this(Constants.SymbolBackground)
    {
    }

    public Tile(char symbol = Constants.SymbolBackground) : this(GenerationConstraints.InvalidPoint, symbol)
    {

    }

    public Tile(Point point, char symbol = Constants.SymbolBackground)
    {
      this.Name = GetType().Name;
      this.point = point;
      this.Symbol = symbol;
    }

    public string GetNameFromTag1()
    {
      return tag1.Replace("_", " ").ToUpperFirstLetter();
    }

    public virtual void SetNameFromTag1()
    {
      Name = GetNameFromTag1();
    }

    public int dungeonNodeIndex = DungeonNode.DefaultNodeIndex;

    [JsonIgnore]
    public int DungeonNodeIndex
    {
      get { return dungeonNodeIndex; }
      set
      {
        if (value == 100 && Symbol == '@')
        {
          int k = 0;
          k++;
        }
        if (value == DungeonNode.ChildIslandNodeIndex)
        {
          int k = 0;
          k++;
        }
        if (dungeonNodeIndex == DungeonNode.ChildIslandNodeIndex && value != DungeonNode.ChildIslandNodeIndex)
        {
          int k = 0;
          k++;
        }

        dungeonNodeIndex = value;
      }
    }

    public bool Revealed
    {
      get { return revealed; }
      set
      {
        if (revealed != value)
        {
          if (value)
          {
            if (DungeonNodeIndex > 0 && DungeonNodeIndex < 999)
            {
              int k = 0;
              k++;
            }
            if (DungeonNodeIndex == 0)
            {
              int k = 0;
              k++;
            }
          }
          revealed = value;
        }
      }
    }

    [JsonIgnore]
    public bool IsAtValidPoint
    {
      get { return point != GenerationConstraints.InvalidPoint; }
    }

    [JsonIgnore]
    public bool IsEmpty { get { return Symbol == Constants.SymbolBackground; } }

    [JsonIgnore]
    public virtual string Name
    {
      get
      {
        return name;
      }
      set
      {
        name = value.Trim();//call GetCapitalized(value) ?;
      }
    }

    private static string GetCapitalized(string val)
    {
      var result = "";
      if (val.Any())
      {
        var parts = val.Split(' ');
        foreach (var part in parts.Where(i => i.Any()))
        {
          var resPart = part.First().ToString().ToUpper() + part.Substring(1);
          result += resPart + " ";
        }
      }

      return result;
    }

    public bool IsFromChildIsland
    {
      get { return DungeonNodeIndex < Constants.MinNormalNodeIndex; }
    }

    //public float RevealPercent { get; set; }

    public virtual char Symbol
    {
      get
      {
        return symbol;
      }

      set
      {
        symbol = value;
      }
    }

    [JsonIgnore]
    public ConsoleColor Color { get => color; set => color = value; }

    public string DisplayedName
    {
      get
      {
        if (displayedName == null)
          return Name;

        return displayedName;
      }
      set => displayedName = value;
    }

    public bool IsAtSamePosition(Tile other)
    {
      return point.Equals(other.point);
    }

    public static bool IncludeDebugDetailsInToString = true;

    public override string ToString()
    {
      string res = GetType().ToString();
      if (IncludeDebugDetailsInToString)
        res += " " + Symbol + " " + DungeonNodeIndex + " " + point + " " + tag1 + " " + GetHashCode();
      return res;
    }

    public double DistanceFrom(Tile other)
    {
      return DistanceFrom(other.point);
    }

    public double DistanceFrom(Point other)
    {
      var dPowered = (Math.Pow(point.X - other.X, 2) + Math.Pow(point.Y - other.Y, 2));
      return Math.Sqrt(dPowered);
    }
  }
}
