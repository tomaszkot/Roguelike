using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles
{
  public enum PlantKind
  {
    Unset,
    Thistle,//oset
    Sorrel  //szczaw
  }

  public class Plant : Consumable
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
        EnhancedStat = EntityStatKind.Unset;
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

    public override float GetStatIncrease(LivingEntity caller)
    {
      return 10;// ConsumableHelper.GetStatIncrease(caller, this, 10);
    }

    public override string PrimaryStatDescription => primaryStatDesc;

    //public EntityStatKind StatKind
    //{
    //  get
    //  {
    //    switch (Kind)
    //    {
    //      case PlantKind.Unset:
    //        break;
    //      case PlantKind.Thistle:
    //        break;
    //      case PlantKind.Sorrel:
    //        return EntityStatKind.Health;
    //      default:
    //        break;
    //    }

    //    return EntityStatKind.Unset;
    //  }
    //}
  }
}
