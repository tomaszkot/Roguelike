using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Spells;
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
        

    string[] extraStatDescription = new string[0];
    public string[] GetExtraStatDescription(bool currentLevel)
    {
      return extraStatDescription;
    }

    public string[] GetFeatures(bool w)
    {
      return extraStatDescription;
    }
    
    public LivingEntity Caller { get; set; }
    public int CoolingDown { get; set; } = 0;
    public bool Used { get; set; }
    public EntityStatKind StatKind { get; set; }
    public float StatKindFactor { get; set; }
    public int TourLasting { get; set; }
    public int CurrentLevel { get { return 1; } }

    public int ManaCost => 5;

    public bool Utylized { get; set; }

    SpellKind ISpell.Kind => SpellKind.Portal;

    public SpellStatsDescription CreateSpellStatsDescription(bool currentLevel) 
    { 
      return new SpellStatsDescription(1, ManaCost, 10, ((ISpell)this).Kind, 0); 
    }

    public EntityStat[] GetEntityStats()
    {
      return new EntityStat[] { new EntityStat() { Kind = EntityStatKind.Mana, Value = new StatValue() { Nominal = ManaCost } } };
    }
  }
}
