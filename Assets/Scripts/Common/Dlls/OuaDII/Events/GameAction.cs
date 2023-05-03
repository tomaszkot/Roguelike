using Roguelike.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Events
{
  public enum SpecificHeroActionKind { HitGatheringGodSlot, OpenedGatheringEntry , HitGatheringGodSlotNoStatueAvailable };

  public class HeroAction : Roguelike.Events.HeroAction
  {
    public SpecificHeroActionKind SpecificKind { get; set; }
  }
}
