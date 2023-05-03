using Dungeons.Tiles;
using OuaDII.TileContainers;
using OuaDII.Tiles.Interactive;
using Roguelike.Abstract.Tiles;
using Roguelike.TileContainers;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Managers
{
  public class PortalManager
  {
    GameManager gm;
    OuaDII.Tiles.Interactive.Portal srcPortal;
    OuaDII.Tiles.Interactive.Portal destPortal;

    public PortalManager(GameManager gm)
    {
      this.gm = gm;
    }

    public Tiles.Interactive.Portal SrcPortal { get => srcPortal; set => srcPortal = value; }
    public Tiles.Interactive.Portal DestPortal { get => destPortal; set => destPortal = value; }
    AbstractGameLevel srcPortalLevel = null;
    AbstractGameLevel destPortalLevel = null;

    public Tiles.Interactive.Portal AppendPortal(PortalDirection kind, Point pt, GroundPortalKind knownPortal)
    {
      if (kind == PortalDirection.Src)
      {
        if (srcPortal != null)
        {
          RemoveSrcPortal(srcPortalLevel);
        }

        Func<Roguelike.Abstract.Spells.ISpell> spellFac = () => 
        {
          return new Tiles.Interactive.Portal(gm.Container, gm.Hero); 
        };
        var spell = gm.SpellManager.ApplyPassiveSpell(gm.Hero, gm.Hero.ActiveSpellSource, pt, spellFac);
        srcPortal = spell as Tiles.Interactive.Portal;
        if (srcPortal != null)
        {
          srcPortal.KnownPortal = knownPortal;
          srcPortal.PortalKind = PortalDirection.Src;
          SrcPortal = srcPortal;
          srcPortalLevel = gm.CurrentNode;
          return srcPortal;
        }
      }
      else if (kind == PortalDirection.Dest)
      {
        gm.Assert(srcPortal != null, "srcPortal!=null");
        gm.Assert(knownPortal != GroundPortalKind.Unset, "knownPortal != KnownPortal.Unset");
        destPortal = new Tiles.Interactive.Portal(gm.Container, gm.Hero);

        destPortal.PortalKind = PortalDirection.Dest;
        if (gm.AppendTile(destPortal, pt))
        {
          DestPortal = destPortal;
          destPortalLevel = gm.World;
          return destPortal;
        }
      }

      return null;
    }

    /// <summary>
    /// Teleports hero to the world ground portal (used on the World Map)
    /// </summary>
    /// <param name="dest"></param>
    public void GotoGroundPortal(GroundPortal dest)
    {
      TransferEntities(dest, gm.World, gm.World);
    }

    public void UsePortal(Roguelike.Tiles.Interactive.Portal portal, GroundPortalKind knownPortalDestination)
    {
      //bool needToSwitchContext = false;
      var srcPortalLevelIsWorld = srcPortalLevel is World;
      var srcLevel = gm.CurrentNode;

      if (portal.PortalKind == PortalDirection.Src)
      {
        if (portal.Used)
        {
          gm.SoundManager.PlayBeepSound();
          return;
        }

        //dest tile for Src Portal is always World
        Tile destTile = gm.World.GetEmptyNextToCamp();
        var point = destTile.point;
        if (point != Point.Empty)
        {
          if (srcPortalLevelIsWorld)
          {
            UserSrcPortal(portal, knownPortalDestination, srcLevel, destTile, true);
          }
          else
          {
            gm.SetContext(gm.World, gm.Hero, Roguelike.GameContextSwitchKind.Teleported, () =>
            {
              if (this.destPortal != null)
              {
                //remove old one, so that there is always one
                gm.ReplaceTile(new Tile(), this.destPortal.point, false, this.destPortal, destPortalLevel);
              }

              UserSrcPortal(portal, knownPortalDestination, srcLevel, destTile, false);

            }, null, portal);
          }
        }
      }
      else //if (portal.PortalKind == PortalKind.Src)
      {
        if (srcPortalLevelIsWorld)
        {
          UseDestPortal(srcLevel, true);
        }
        else
        {
          //go back to the dungeon
          gm.SetContext(srcPortalLevel, gm.Hero, Roguelike.GameContextSwitchKind.Teleported, () =>
          {
            UseDestPortal(srcLevel, false);
          }, null, SrcPortal);
        }
      }
    }

    private Roguelike.Tiles.Interactive.InteractiveTile GetCamp()
    {
      return gm.World.GetCamp();
    }

    private void UseDestPortal(AbstractGameLevel srcLevel, bool transferEntities)
    {
      if (srcPortal != null)
      {
        if (transferEntities)
        {
          var emptyForHero = srcPortalLevel.GetClosestEmpty(srcPortal);
          if (emptyForHero != null)
          {
            var set = srcPortalLevel.SetTile(gm.Hero, emptyForHero.point);
            gm.Assert(set, "srcPortalLevel.SetTile(gm.Hero, empty.Item1)");
            if (set)
              TransferEntities(srcPortal, srcLevel, srcPortalLevel);
          }
        }
        //remove src portal
        RemoveSrcPortal(srcPortalLevel);
        //remove dest portal
        RemoveDestPortal();
      }
      if (!transferEntities)
      {
        AppendTeleportedEvent();
      }
    }

    private void UserSrcPortal(Roguelike.Tiles.Interactive.Portal srcPortal, GroundPortalKind knownPortalDestination, AbstractGameLevel srcLevel, Tile destTile,
      bool transferEntities)
    {
      var destPortal = AppendPortal(PortalDirection.Dest, destTile.point, knownPortalDestination);
      if (transferEntities)
      {
        TransferEntities(destTile, srcLevel, gm.World);
      }
      else
      {
        AppendTeleportedEvent();
      }
      srcPortal.Used = true;
    }

    void AppendTeleportedEvent()
    {
      var les = new List<LivingEntity>();
      les.Add(gm.Hero);
      les.AddRange(gm.AlliesManager.AllEntities);
      foreach (var le in les)
      {
        AppendTeleportedEvent(le);
      }

    }


    void TransferEntities(Tile startTile, AbstractGameLevel srcLevel, AbstractGameLevel destLevel)
    {
      var nextTile = startTile;

      destLevel.PlaceHeroNextToTile(Roguelike.GameContextSwitchKind.Teleported, gm.Hero, startTile);
      AppendTeleportedEvent(gm.Hero);
      gm.Context.PlaceAllies(gm.AlliesManager, srcLevel, destLevel, Roguelike.GameContextSwitchKind.Teleported, gm.GameState, null,
        (LivingEntity ally) =>{
          AppendTeleportedEvent(ally);
        });


    }

    private void RemoveDestPortal()
    {
      gm.ReplaceTile(new Tile(), destPortal.point, false, destPortal, destPortalLevel);
      destPortal = null;
      destPortalLevel = null;
    }

    private void RemoveSrcPortal(AbstractGameLevel srcPortalLevel)
    {
      gm.ReplaceTile(new Tile(), srcPortal);
      srcPortalLevel.SetEmptyTile(srcPortal.point);
      srcPortal = null;
      srcPortalLevel = null;
    }

    private void AppendTeleportedEvent(Roguelike.Tiles.LivingEntities.LivingEntity le)
    {
      gm.EventsManager.AppendAction(new Roguelike.Events.LivingEntityAction(Roguelike.Events.LivingEntityActionKind.UsedPortal) { InvolvedEntity = le });
      gm.SoundManager.PlaySound("teleport");
    }
  }
}
