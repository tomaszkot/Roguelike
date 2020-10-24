using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Effects
{
  class EffectTypeToStatKind
  {
    public static EntityStatKind Convert(EffectType et)
    {
      EntityStatKind esk = EntityStatKind.Unset;
      if (et == EffectType.Bleeding)
        esk = EntityStatKind.Health;

      else if (et == EffectType.Inaccuracy)
        esk = EntityStatKind.ChanceToHit;

      return esk;
    }
  }
}
