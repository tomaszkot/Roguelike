using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roguelike.Tiles.Interactive
{
  public class Privy : InteractiveTile
  {
    public Privy(Container cont) : base(cont, ',')
    {
#if ASCII_BUILD
      color = ConsoleColor.Blue;
#endif
      tag1 = "privy";
      Kind = InteractiveTileKind.Privy;
      HidesInteractedEntity = true;
      DestPointActivityKind = DestPointActivityKind.Privy;
    }

    internal override bool InteractWith(LivingEntity le)//typically npc
    {
      if (Busy)
        return false;
      SetBusy(le);
      return true;
    }

  }
}
