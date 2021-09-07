//#define DEBUG_PROPS
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
    //public Point point
    //{
    //  get { return _point; }
    //  set { _point = value; }
    //}

    private char symbol = Constants.SymbolBackground;
    public string name;
    private string displayedName;

#if DEBUG_PROPS
    public string _tag1 = "";
    public string tag1
    {
      get { return _tag1; }
      set
      {
        _tag1 = value;
        if (_tag1 == "xxx")
        {
        }
      }
    } //custom purpose field
#else
   public string tag1 = "";//custom purpose field
#endif

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

#if DEBUG_PROPS
  bool revealed;
  public bool Revealed 
  { 
    get
    {
        return revealed;
    }
    set
    {
      if (this.DungeonNodeIndex == DungeonNode.ChildIslandNodeIndex)
      {
          if (this is Wall)
          {
            int k = 0;
            k++;  
          }
          if (this is IDoor)
          {
            int k = 0;
            k++;
          }
        }
      if(value == true)
      {
        int k = 0;
        k++;
        if (this is IDoor)
        {
          int kk = 0;
          kk++;
        }
      }
      revealed  = value;
    } 
  }
#else
    /// <summary>
    /// If false the tile is not visible. The revealed flag shall be typically set to true when a door leading to room are opened.
    /// </summary>
    public bool Revealed = GenerationInfo.DefaultRevealedValue;
#endif


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

    //some classes might need a prop
    public int CustomDungeonNodeIndex
    {
      get { return DungeonNodeIndex; }
      set { DungeonNodeIndex = value; }
    }
        
#if DEBUG_PROPS
    int dungeonNodeIndex = DungeonNode.DefaultNodeIndex;
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
#else
    public int DungeonNodeIndex = DungeonNode.DefaultNodeIndex;
#endif

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

    //[JsonIgnore]
    public virtual string DisplayedName
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
      string res = GetType().ToString() + " " + Name;
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
      //var dPowered =   (Math.Pow(point.X - other.X, 2) + Math.Pow(point.Y - other.Y, 2));
      //return Math.Sqrt(dPowered);
      return point.DistanceFrom(other);
    }
  }
}
