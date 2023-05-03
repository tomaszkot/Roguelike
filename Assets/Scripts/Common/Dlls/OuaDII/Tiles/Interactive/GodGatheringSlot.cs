using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Tiles.Interactive
{
  public class GodGatheringSlot : Roguelike.Tiles.Interactive.InteractiveTile
  {
    public GodKind GodKind { get; set; }
    public bool IsOn { get; set; }

    public GodGatheringSlot(Container cont) : base(cont, '~')
    {
#if ASCII_BUILD
      color = ConsoleColor.Red;
#endif
      Revealed = true;
      //Kind = Roguelike.Tiles.InteractiveTileKind.;
    }

    public override string ToString()
    {
      return base.ToString() + ", on: "+IsOn;
    }
  }
}
