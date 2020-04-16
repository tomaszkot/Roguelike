using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Policies
{
  public enum PolicyKind { Move, Attack, SpellCast }

  public class Policy
  {
    public PolicyKind Kind { get; set; }
    public event EventHandler<Policy> OnApplied;

    protected virtual void ReportApplied(LivingEntity entity)
    {
      entity.State = EntityState.Idle;
      if (OnApplied != null)
        OnApplied(this,this);
    }
  }
}
