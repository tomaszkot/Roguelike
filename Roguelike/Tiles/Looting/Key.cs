////#define ASCII_BUILD  

namespace Roguelike.Tiles.Looting
{
  public enum KeyKind { Unset, Room, BossRoom, Chest }

  public interface IKey
  {
    //int MatchingLevel { get; set; }
    //bool AutoOpenDoors { get; set; }
    string KeyName { get; set; }
    KeyKind Kind { get; set; }
  }

  public class Key : Loot, IKey
  {
    //public bool AutoOpenDoors { get; set; }

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

        var desc = "";
        if (kind == KeyKind.Room)
          desc = "Opens door";
        else if (kind == KeyKind.BossRoom)
          desc = "Opens special door";
        else if (kind == KeyKind.Chest)
          desc = "Opens chest";

        PrimaryStatDescription = desc;
      }
    }

    public bool Half { get; set; }

    //public int MatchingLevel
    //{
    //  get; set;
    //}
  }
}

//	public class KeyHalf : LootBase
//	{
//		public int MatchingLevel { get; set; }

//		public KeyHalf()
//		{
//			Symbol = ';';
//#if ASCII_BUILD
//            color = GoldColor;
//#endif
//			Name = "Key part";
//		}

//		public override string GetPrimaryStatDescription()
//		{
//          string desc = "";
//          var gm = GameManager.Instance;
//          if (gm.GameSettings.AllowInPlaceInventoryCrafting)
//            desc += "Part of a key. Drop it on the other part in the Inventory.";
//          else
//            desc += "Use Custom Recipe and the other part on the Crafting Panel.";
//          return desc;
//		}

//		public override LootBase CreateCrafted(LootBase other)
//    {
//			return new Key() { MatchingLevel = this.MatchingLevel };
//		}
//		public override bool IsCraftable()
//		{
//			return true;
//		}
//  }

//	public class KeyHalf1 : KeyHalf
//	{
//    public KeyHalf1()
//    {
//      Symbol = ';';
//      #if ASCII_BUILD
//            color = GoldColor;
//      #endif
//      AssetName = "key_half1";
//      Name = "Key part";
//    }

//    public override bool IsCraftableWith(LootBase other)
//    {
//      return other is KeyHalf2;
//    }
//  }

//  public class KeyHalf2 : KeyHalf
//	{
//    public KeyHalf2()
//    {
//      Symbol = ';';
//      #if ASCII_BUILD
//            color = GoldColor;
//      #endif
//      AssetName = "key_half2";
//      Name = "Key part";
//    }

//    public override bool IsCraftableWith(LootBase other)
//    {
//      return other is KeyHalf1;
//    }
//  }
//}
