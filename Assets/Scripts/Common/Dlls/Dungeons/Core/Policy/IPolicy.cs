using System;
using System.Collections.Generic;
using System.Text;

namespace Dungeons.Core.Policy
{
  public interface IPolicy
  {
    event EventHandler<Tiles.Abstract.IHitable> TargetHit;

    void ReportHit(Dungeons.Tiles.Abstract.IHitable entity);
  }

  public interface IAttackPolicy : IPolicy
  {
    
  }
}
