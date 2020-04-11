using Dungeons.Core;
using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public enum FoodKind { Unset, Plum, Herb }

  public class Food : Loot
  {
    public FoodKind Kind;
    EntityStatKind enhancedStat = EntityStatKind.Unset;

    public Food() : this(RandHelper.GetRandomEnumValue<FoodKind>())
    {
    }

    public Food(FoodKind kind)
    {
      StackedInInventory = true;
      Symbol = '-';
      LootKind = LootKind.Food;
      SetKind(kind);
      Price = 15;
    }

    public EntityStatKind EnhancedStat { get => enhancedStat; set => enhancedStat = value; }

    public void SetKind(FoodKind kind)
    {
      Kind = kind;
      Name = kind.ToString();
      //this.AssetName = "red_toadstool";
      Name += " of " + Kind;
     
      DisplayedName = Name;
      SetPrimaryStatDesc();
      enhancedStat = EntityStatKind.Health;
    }

    void SetPrimaryStatDesc()
    {
      string desc = "";
      if (Kind == FoodKind.Herb)
      {
        desc = "Eatable, suprisengly nutritious plant";
      }
      else if (Kind == FoodKind.Plum)
      {
        desc = "Suprisengly nutritious friut";
      }
      primaryStatDesc = desc;
    }
  }
}
