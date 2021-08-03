using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons.Tiles.Abstract
{
  public interface IProjectile
  {
    Dungeons.Tiles.IObstacle Target { get; set; }
  }
}
