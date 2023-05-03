using Dungeons.Core;
using Dungeons.Tiles;
using OuaDII.TileContainers;
using Roguelike;
using Roguelike.Abstract;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII
{
  public class GameContext : Roguelike.GameContext
  {
    World world;
    public World World { get => world; set => world = value; }

    public GameContext(Container container) : base(container)
    {
    }

    public override AbstractGameLevel CurrentNode
    {
      get { return base.CurrentNode; }
      set
      {
        if (value is World)
          World = value as World;
        base.CurrentNode = value;
      }
    }

    public override HeroPlacementResult PlaceLoadedHero(AbstractGameLevel node, Roguelike.State.GameState gs)
    {
      //return base.PlaceLoadedHero(node, gs);
      var res = new HeroPlacementResult();
      var world = node as World;
      if (world != null)
      {
        var nodeSet = world.PlaceLoadedHero(Hero, gs);
        res.Node = nodeSet;
        res.Point = Hero.point;
        return res;
      }

      return null;
    }

    public override HeroPlacementResult PlaceHeroAtDungeon(AbstractGameLevel node, Roguelike.State.GameState gs, GameContextSwitchKind context, Stairs stairs)
    {
      var res = new HeroPlacementResult();
      if (context == GameContextSwitchKind.NewGame)
      {
        res.Node = node;
        var herPoint = Hero.point;
        if (!herPoint.IsValid() || gs.CoreInfo.Mode == Roguelike.Settings.GameMode.Roguelike)
          herPoint = node.GetFirstEmptyPoint().Value;//in UI set by a unity tile
        res.Point = herPoint;
        node.PlaceHeroAtTile(context, Hero, node.GetTile(herPoint));
        return res;
      }
      Tile heroStartTile = null;
      if (stairs != null)
      {
        if (stairs.StairsKind == StairsKind.PitUp)
        {
          try
          {
            heroStartTile = node.GetPitStairs(stairs.PitName);
          }
          catch (Exception ex)
          {
            Logger.LogError(ex.Message);
            throw;
          }
        }
      }

      if (heroStartTile == null)
        res = base.PlaceHeroAtDungeon(node, gs, context, stairs);
      else
        res = node.PlaceHeroNextToTile(context, Hero, heroStartTile);

      return res;
    }

    protected override void PlaceHeroByPortal(AbstractGameLevel destLevel, GameContextSwitchKind gameContext, Portal portal)
    {
      if (portal.PortalKind == PortalDirection.Src)
      {
        if (!(CurrentNode is World))
        {
          var emp = destLevel.GetCamp();
          destLevel.PlaceHeroNextToTile(gameContext, Hero, emp);
        }
        else
        {
          destLevel.PlaceHeroNextToTile(gameContext, Hero, portal);
        }
      }
      else
      {
        Logger.LogError("PlaceHeroByPortal not supported!");
      }
    }
  }
}
