using Dungeons.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public enum PlantKind { Unset, Thistle }

  public class Plant : Loot
  {
    public PlantKind Kind { get; set; }

    public Plant() : this(RandHelper.GetRandomEnumValue<PlantKind>())
    {
    }

    public Plant(PlantKind kind)
    {
      StackedInInventory = true;
      Symbol = '-';
      LootKind = LootKind.Plant;
      SetKind(kind);
      Price = 15;
    }

    public void SetKind(PlantKind kind)
    {
      Kind = kind;
      Name = kind.ToString();
      //this.AssetName = "red_toadstool";
      Name += " of " + Kind;

      DisplayedName = Name;
      SetPrimaryStatDesc();
     
    }

    void SetPrimaryStatDesc()
    {
      string desc = "";
      if (Kind == PlantKind.Thistle)
      {
        desc = "Unpleasant to touch, part of recipe";
      }
      
      primaryStatDesc = desc;
    }
  }
}
