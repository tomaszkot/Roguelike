using Dungeons.Core;
using Dungeons.TileContainers;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Linq;
using System.Xml.Serialization;

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
    public Point Point;

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
    bool revealed = true;

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
      this.Point = point;
      this.Symbol = symbol;
    }

    public void SetNameFromTag1()
    {
      Name = tag1.Replace("_", " ").ToUpperFirstLetter();
    }

    public int dungeonNodeIndex = DungeonNode.DefaultNodeIndex;
    public int DungeonNodeIndex
    {
      get { return dungeonNodeIndex; }
      set {
        if (value == 100 && Symbol == '@')
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
        if (value != revealed)
        {
          if (DungeonNodeIndex < 0)
          {
            //int k = 0;
          }
          revealed = value;
        }
      }
    }

    public bool IsAtValidPoint
    {
      get { return Point != GenerationConstraints.InvalidPoint; }
    }

    public bool IsEmpty { get { return Symbol == Constants.SymbolBackground; } }

    public string Name
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

    public float RevealPercent { get; set; }

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
      return Point.Equals(other.Point);
    }

    public override string ToString()
    {
      return GetType() + " " + Symbol + " " + DungeonNodeIndex + " " + Point + " " +  tag1 + " " + GetHashCode();
    }

    public double DistanceFrom(Tile other)
    {
      return DistanceFrom(other.Point);
    }

    public double DistanceFrom(Point other)
    {
      var dPowered = (Math.Pow(Point.X - other.X, 2) + Math.Pow(Point.Y - other.Y, 2));
      return Math.Sqrt(dPowered);
    }
  }
}
