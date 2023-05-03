using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Interactive
{
  public class TorchSlot : InteractiveTile
  {
    public TorchSlot(Container cont) : base(cont, '~')
    {
      tag1 = "torch_slot";
      Symbol = '~';
      Name = "Torch slot";

      Kind = InteractiveTileKind.TorchSlot;
    }
  }
}
