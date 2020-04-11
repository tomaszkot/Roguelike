using Dungeons.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public class Gold : Loot
  {
    public int Amount = 5;
    public Gold()
    {
      Symbol = GoldSymbol;
#if ASCII_BUILD
      color = GoldColor;
#endif
      tag1 = "coin";
      Amount = RandHelper.Random.Next(4, 8);
      LootKind = LootKind.Gold;
    }

    //public override string GetPrimaryStatDescription()
    //{
    //  return Amount + " gold coins";
    //}
  }
}
