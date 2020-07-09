using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelike.Tiles;

namespace Roguelike.Abstract
{
  public interface ISpell
  {
    LivingEntity Caller { get; set; }
    int CoolingDown { get; set; }
  }
}
