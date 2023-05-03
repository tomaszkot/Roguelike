using OuaDII.Managers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Tiles.Interactive
{
  //interface IPortalDestination
  //{
  //  Point Position { get; set; }
  //}

  public class Portal : Roguelike.Tiles.Interactive.Portal
  {
    GroundPortalKind knownPortal;

    public GroundPortalKind KnownPortal { get => knownPortal; set => knownPortal = value; }

    public Portal(Container cont, LivingEntity caller) : base(cont, caller)
    {
      Caller = caller;
    }

    public Portal(Container cont) : base(cont)
    {
    }
  }
}
