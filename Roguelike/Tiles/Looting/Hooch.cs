using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles.Looting
{
  public class Hooch : Loot
  {
    public const string HoochGuid = "4fe17985-47d3-2b35-bddf-99a4af2b1dfc";
    static Dictionary<EntityStatKind, double> effects = new Dictionary<EntityStatKind, double>();
    public static float Strength = 50;
    public static float ChanceToHit = 15;
    public static int TourLasting = 7;

    public Hooch()
    {
//      Symbol = '&';
//#if ASCII_BUILD
//      color = GoldColor;
//#endif
//      tag1 = "hooch";
//      Name = "Hooch";
//      Price = 5;
//      StackedInventoryId = new Guid(HoochGuid);
    }

    public string GetPrimaryStatDescription()
    {
      return "Powerful liquid, can be drunk or used as part of a recipe.";
      
    }

    string[] extDesc;
    public override string[] GetExtraStatDescription()
    {
      if (extDesc == null)
      {
        extDesc = new string[4];
        //extDesc[0] = "Press O to drink it.";
        extDesc[0] = "Drink Effect:";
        extDesc[1] = "Strength +" + Strength + "%";
        extDesc[2] = "ChanceToChit -"+ChanceToHit+"%";
        extDesc[3] = "Tour Lasting: "+ TourLasting;
      }
      return extDesc;
    }
  }
}
