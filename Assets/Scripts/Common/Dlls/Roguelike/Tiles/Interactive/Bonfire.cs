using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roguelike.Tiles.Interactive
{
  public class Bonfire : InteractiveTile
  {
    public Bonfire(Container cont) : base(cont, ',')
    {
#if ASCII_BUILD
      color = ConsoleColor.Blue;
#endif
      DestPointActivityKind = DestPointActivityKind.Grill;
      Kind = InteractiveTileKind.Bonfire;
      tag1 = "bonfire";
      InteractSound = "punch";
    }

    internal override bool InteractWith(LivingEntity le)
    {
      SetBusy(le);
      return true;
    }

  }
}
