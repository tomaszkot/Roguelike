using Dungeons.Tiles;
using OuaDII.TileContainers;
using OuaDII.Tiles.Interactive;
using Roguelike;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Managers
{
  class InputManager : Roguelike.Managers.InputManager
  {
    OuaDII.Managers.GameManager GameManager
    {
      get { return gm as OuaDII.Managers.GameManager; }
    }

    public InputManager(GameManager gm) : base(gm)
    {

    }

    public override InteractionResult InteractHeroWith(Tile tile)
    {
      var result = InteractionResult.None;
      if (tile is Dungeons.Tiles.IObstacle)
      {
        if (tile is Stairs)
        {
          var stairs = tile as Stairs;
          if (stairs.StairsKind == StairsKind.PitDown || stairs.StairsKind == StairsKind.PitUp)
          {
            if (stairs.Closed)
            {
              GameManager.EventsManager.AppendAction(new InteractiveTileAction()
              {
                InteractiveKind = InteractiveActionKind.HitClosedStairs,
                Info = "Entry is closed",
                InvolvedTile = stairs
              });
              return InteractionResult.Blocked;
            }
            var world = GameManager.World;//TODO World migh not be loaded!// GetCurrentNode<World>();
            var pit = world.GetPit(stairs.PitName);
            if (stairs.PitName == GameManager.GameOnePitDown)
            {
              GameManager.SoundManager.PlayBeepSound();

              GameManager.EventsManager.AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.HitGameOneEntry, Info = "Dungeon from part one of the game - buried with stones" });
              return InteractionResult.Blocked;
            }

            if (stairs.StairsKind == StairsKind.PitDown)
            {
              GameLevel level = null;
              if (!pit.Levels.Any())
              {
                level = GameManager.AddNewLevel(pit);
              }
              else
                level = pit.Levels.First();
              GameManager.SetContext(level, Hero, GameContextSwitchKind.DungeonSwitched, () => { }, stairs);
            }
            else
            {
              var st = world.GetTiles<Stairs>();
              GameManager.SetContext(world, Hero, GameContextSwitchKind.DungeonSwitched, () => { }, stairs);
            }
            result = InteractionResult.ContextSwitched;
          }
        }
        else if (tile is GodGatheringSlot)
        {

        }
      }
      if (result == InteractionResult.None)
        result = base.InteractHeroWith(tile);
      return result;
    }

    protected override InteractionResult HandleInteractionWithInteractive(Roguelike.Tiles.Interactive.InteractiveTile tile)
    {
      if (tile is HeroChest hc)
      {
        gm.AppendAction<HeroAction>((HeroAction ac) =>
        {
          ac.Kind = HeroActionKind.HitPrivateChest;
          ac.Info = "Hero interacted with a private chest"; ac.InvolvedTile = tile;
        });

        return InteractionResult.Blocked;
      }
      return base.HandleInteractionWithInteractive(tile);
    }
  }
}
