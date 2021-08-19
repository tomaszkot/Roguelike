using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons.Tiles.Abstract
{
  public interface IProjectile
  {
    //Dungeons.Tiles.IObstacle TargetObstacle { get; }
    Dungeons.Tiles.Tile Target { get; set; }
  }
}
