using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelike.Tiles;

namespace Roguelike.Abstract
{
  public class IMovingDamager
  {
    LivingEntity Caller { get; set; }
  }
}
