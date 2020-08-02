using Dungeons.Core;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public enum PlantKind
  {
    Unset,
    Thistle,//oset
    Sorrel  //szczaw
  }

  public class Plant : StackedLoot
  {
    public PlantKind Kind { get; set; }

    public Plant() : this(RandHelper.GetRandomEnumValue<PlantKind>(true))
    {
    }

    public Plant(PlantKind kind)
    {
      Symbol = '-';
      LootKind = LootKind.Plant;
      SetKind(kind);
      Price = 15;
    }

    public void SetKind(PlantKind kind)
    {
      Kind = kind;
      Name = kind.ToString();
      //Name += " of " + Kind;

      DisplayedName = Name;
      SetPrimaryStatDesc();
     
    }

    void SetPrimaryStatDesc()
    {
      string desc = "";
      if (Kind == PlantKind.Thistle)
      {
        desc = "Unpleasant to touch, part of recipe";
        tag1 = "Thistle1";
      }
      else if (Kind == PlantKind.Sorrel)
      {
        desc = "Eatable, sour plant";
        tag1 = "szczaw";
      }
      primaryStatDesc = desc;
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind;
    }
  }
}
