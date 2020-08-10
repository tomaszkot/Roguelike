using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public enum TinyTrophyKind { Unset, Fang, Tusk, Claw }//Fang-Tooth
  public enum TinyTrophySize { Small, Medium, Big }

  public class TinyTrophy : StackedLoot
  {
    public TinyTrophyKind TinyTrophyKind { get; set; }
    public TinyTrophySize TinyTrophySize { get; set; } = TinyTrophySize.Small;
    public string primaryStatDescription;

    public override string PrimaryStatDescription => primaryStatDescription;

    public TinyTrophy(TinyTrophyKind kind)
    {
      Price = 5;
      Symbol = '&';
      LootKind = LootKind.TinyTrophy;
      SetKind(kind);
    }

    private void SetKind(TinyTrophyKind kind)
    {
      TinyTrophyKind = kind;
      
      switch (kind)
      {
        case TinyTrophyKind.Unset:
          break;
        case TinyTrophyKind.Fang:
          Name = "Fang";
          primaryStatDescription = "Sharp, hard, ready to bite. " + Strings.PartOfCraftingRecipe;
          break;
        case TinyTrophyKind.Tusk:
          Name = "Tusk";
          primaryStatDescription = "Big, sharp, ready to tear somebody apart. " + Strings.PartOfCraftingRecipe;
          break;
        case TinyTrophyKind.Claw:
          Name = "Claw";
          primaryStatDescription = "Sharp, hard, ready to claw. " + Strings.PartOfCraftingRecipe;
          break;
        default:
          break;
      }
    }
  }
}
