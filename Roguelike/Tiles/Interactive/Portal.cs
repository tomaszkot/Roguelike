using Roguelike.Abstract;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Interactive
{
  public enum PortalDirection { Unset, Src, Dest }

  public class Portal : InteractiveTile, ISpell
  {
    public PortalDirection PortalKind { get; set; }

    public Portal(LivingEntity caller) : this()
    {
      Caller = caller;
    }

    public Portal() : base('>')
    {
#if ASCII_BUILD
      color = ConsoleColor.Red;
#endif
      tag1 = "portal";
    }

    public LivingEntity Caller { get; set ; }
    public int CoolingDown { get; set; } = 0;
    public bool Used { get; set; }
  }
}
