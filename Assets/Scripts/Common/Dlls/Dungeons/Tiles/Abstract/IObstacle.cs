using Dungeons.Core.Policy;
using Dungeons.Fight;
using Dungeons.Tiles.Abstract;
using System;
using System.Drawing;

namespace Dungeons.Fight
{
  public enum HitResult { Unset, Hit, Evaded }
}

namespace Dungeons.Tiles.Abstract
{
 

  public interface IObstacle : IHitable
  {
    
  }
}
