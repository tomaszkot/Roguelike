using Roguelike.Abstract;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Tiles.Interactive
{
  public enum PortalDirection { Unset, Src, Dest }

  public class Portal : InteractiveTile, ISpell
  {
    public PortalDirection PortalKind { get; set; }

    public Portal(LivingEntity caller) : this()
    {
      Caller = caller;
    }

    public Portal() : base('>')
    {
#if ASCII_BUILD
      color = ConsoleColor.Red;
#endif
      tag1 = "portal";
    }

    public LivingEntity Caller { get; set ; }
    public int CoolingDown { get; set; } = 0;
    public bool Used { get; set; }
    public EntityStatKind StatKind { get; set; }
    public float StatKindFactor { get; set; }
    public int TourLasting { get; set; }

    public int ManaCost => 5;

    public bool Utylized { get ; set ; }
  }
}
