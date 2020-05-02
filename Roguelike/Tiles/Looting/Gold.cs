using Dungeons.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public class Gold : StackedLoot
  {
    public Gold(int amount)
    {
      Symbol = GoldSymbol;
#if ASCII_BUILD
      color = GoldColor;
#endif
      tag1 = "coin";
      Count = amount;
      LootKind = LootKind.Gold;
    }

    public Gold() : this(RandHelper.Random.Next(4, 8))
    {
      
    }

    //public override string GetPrimaryStatDescription()
    //{
    //  return Amount + " gold coins";
    //}
  }
}
