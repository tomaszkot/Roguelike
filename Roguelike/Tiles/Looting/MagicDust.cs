using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles.Looting
{
  public class MagicDust : StackedLoot
  {
    public MagicDust()
    {
      LootKind = LootKind.Other;
      Symbol = '&';
#if ASCII_BUILD
      color = GoldColor;
#endif
      tag1 = "magic_dust";
      Name = "Magic Dust";
      Price = 5;
      PrimaryStatDescription = PartOfCraftingRecipe;
    }
  }
}
