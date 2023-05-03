using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public class Cannon : Loot
  {
    public Cannon()
    {
      Price = 100;
      Symbol = '&';
      LootKind = LootKind.Other;
      tag1 = "cannon";
      PrimaryStatDescription = "A small cannon making huge damage";
    }
    
  }
}
