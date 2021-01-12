using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles.Looting
{
  public class Sulfur : StackedLoot
  {
    public Sulfur()
    {
      Symbol = '&';
#if ASCII_BUILD
      color = GoldColor;
#endif
      tag1 = "sulfur";
      Name = "Sulfur Nugget";
      Price = 5;
      PrimaryStatDescription = "Part of crafting recipe";
      //StackedInventoryId = new Guid(Guid);
    }
  }
}
