////#define ASCII_BUILD  

using Roguelike.Tiles;
using Roguelike.Tiles.Looting;

namespace Roguelike.Tiles.Looting
{
  public enum KeyKind { Unset, Room, BossRoom, Chest }
  public enum KeyPuzzle { Unset, SecretRoom, Barrel, Chest,  Half, DeadBody, Grave, Enemy,
    Mold,   //forma z zelaza
    LeverSet
  }

  public interface IKey
  {
    //int MatchingLevel { get; set; }
    string KeyName { get; set; }
    KeyKind Kind { get; set; }
  }

  public class Key : Loot, IKey
  {
    public Key()
    {
      //AutoOpenDoors = true;
      //MatchingLevel = -1;
      Symbol = ';';
#if ASCII_BUILD
      color = GoldColor;
#endif
      //AssetName = "key";
      Name = "Key";
      tag1 = "key";
      Revealed = true;
    }

    public string KeyName { get; set; }

    public KeyKind kind;
    public KeyKind Kind
    {
      get { return kind; }
      set
      {
        kind = value;
        if (kind == KeyKind.Chest)
          tag1 = "gold_chest_key";

        SetDescFromKind();
      }
    }

    private void SetDescFromKind()
    {
      var desc = "";
      if (kind == KeyKind.Room)
        desc = "Opens a door";
      else if (kind == KeyKind.BossRoom)
        desc = "Opens a special door";
      else if (kind == KeyKind.Chest)
        desc = "Opens a chest";

      PrimaryStatDescription = desc;
    }

    public bool Half { get; set; }
  }
}

public class KeyHalf : Loot, IKey
{
  public string PitName { get; set; }
 
  public string KeyName { get; set; }
  public KeyKind Kind { get ; set; }

  public KeyHalf()
  {
    Symbol = ';';
#if ASCII_BUILD
            color = GoldColor;
#endif
    Name = "Key Part";
    PrimaryStatDescription = "Part of a key - useless without the second one";
    tag1 = "key_part_lower";
  }
  public void SetHandlePart()
  {
    tag1 = "key_part_upper";
  }//handle part or lower one ?

  public bool Matches(KeyHalf other)
  {
    return tag1 != other.tag1;
  }
}

public class KeyMold : Loot, IKey
{
  public string PitName { get; set; }
  public bool HandlePart { get; set; }//handle part or lower one ?
  public string KeyName { get; set; }
  public KeyKind Kind { get; set; }

  public KeyMold()
  {
    Symbol = ';';
#if ASCII_BUILD
            color = GoldColor;
#endif
    Name = "Key Mold";
    PrimaryStatDescription = "Mold - can be used to create a key";
    tag1 = "key_mold";
  }
}



