using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract.Abilities
{
  public enum AbilityKind {Unset, Passive, Active }

  public class IAbility
  {
    public AbilityKind AbilityKind { get; set; }
  }
}
