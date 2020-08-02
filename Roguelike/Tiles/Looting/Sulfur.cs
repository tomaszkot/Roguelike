using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles.Looting
{
  public class Sulfur : Loot
  {
    public const string Guid = "2fe17985-47d3-2b35-bddf-99a4af2b1aaa";

    public Sulfur()
    {
      Symbol = '&';
#if ASCII_BUILD
      color = GoldColor;
#endif
      tag1 = "sulfur";
      Name = "Sulfur Nugget";
      Price = 5;
      //StackedInventoryId = new Guid(Guid);
    }

    public string GetPrimaryStatDescription()
    {
      return "Part of crafting recipe";
    }
  }
}
