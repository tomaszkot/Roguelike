using System.Collections.Generic;

namespace Roguelike.Tiles.Looting
{
  public enum EnchantSrc { Unset, Ruby, Emerald, Diamond, Amber, Fang, Tusk, Claw }
  public enum EnchanterSize { Small, Medium, Big }

  public abstract class Enchanter : StackedLoot
  {    
    public EnchanterSize EnchanterSize { get; set; } = EnchanterSize.Small;
    public EnchantSrc EnchantSrc { get; set; }
    //public EnchanterKind EnchanterKind { get; set; }

    protected string primaryStatDescription;
    protected static Dictionary<EnchanterSize, int> wpnAndArmorValues = new Dictionary<EnchanterSize, int>();
    protected static Dictionary<EnchanterSize, int> otherValues = new Dictionary<EnchanterSize, int>();

    static Enchanter()
    {
      wpnAndArmorValues[EnchanterSize.Big] = 6;
      wpnAndArmorValues[EnchanterSize.Medium] = 4;
      wpnAndArmorValues[EnchanterSize.Small] = 2;

      otherValues[EnchanterSize.Big] = 15;
      otherValues[EnchanterSize.Medium] = 10;
      otherValues[EnchanterSize.Small] = 5;
    }
  }
}
