using Dungeons.Core;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System;

namespace OuaDII.Tiles.Interactive
{
  public enum GroundPortalKind { Unset, 
    Camp, Miller, //these two are not used now
    Leszy,
    BatPit, RatPit, SpiderPit, SkeletonPit, SnakePit, WormPit
  };

  public class GroundPortal : Roguelike.Tiles.Interactive.InteractiveTile, IApproachableByHero
  {
    public event EventHandler Activated;
    public GroundPortalKind GroundPortalKind { get; set; }

    public GroundPortal(Container cont) : base(cont, '>')
    {
#if ASCII_BUILD
      color = ConsoleColor.Red;
#endif
      tag1 = "portal";

      Revealed = true;
    }

    public bool ParseGroundPortalKind(string kindString)
    {
      GroundPortalKind kind = GroundPortalKind.Unset;
      
      var portalName = kindString;
      if(kindString != "Camp" && kindString != "Leszy")
        portalName += "Pit";
      if (Enum.TryParse(portalName, true, out kind))
      {
        GroundPortalKind = kind;
        return true;
      }
      return false;
    }

    public bool Activate()
    {
      if (!ApproachedByHero)
      {
        ApproachedByHero = true;
        if (Activated != null)
          Activated(this, EventArgs.Empty);
        return true;
      }
      return false;
    }

    public string GetPlaceName()
    {
      return tag1.Replace("ground_portal_", "").ToUpperFirstLetter() + "'s Portal"; ;
    }

    public string ActivationSound { get; set; } = "fire_burst";
  }
}
