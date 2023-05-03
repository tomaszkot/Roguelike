using Dungeons;
using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using Roguelike.Abstract.Tiles;
using Roguelike.Events;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Drawing;
using System.Linq;

namespace Roguelike.Managers
{
  public class InputManager
  {
    protected GameManager gm;

    public InputManager(GameManager gm)
    {
      this.gm = gm;
    }

    public Hero Hero { get => gm.Hero; }

    public ILogger Logger
    {
      get => gm.Logger;
    }

    public GameContext Context
    {
      get => gm.Context;
    }

    public bool CanHeroDoAction(bool moving)
    {
      if (!gm.HeroTurn)
      {
        //gm.EventsManager.AppendAction(new GameAction() { Level = ActionLevel.Normal, Info = "!Hero turn" });
        return false;
      }
      if (!gm.Hero.Alive)
      {
        if (!Context.HeroDeadReported)
        {
          Context.ReportHeroDeath();
        }
        //AppendAction(new HeroAction() { Level = ActionLevel.Critical, KindValue = HeroAction.Kind.Died, Info = Hero.Name + " is dead!" });
        return false;
      }

      var ac = Context.TurnActionsCount[TurnOwner.Hero];
      if (ac == 1)
        return false;

      if (gm.Hero.State != EntityState.Idle)
      {
        if(gm.Hero.State != EntityState.Moving)
          return false;
        if (!moving)
          return false;
      }

      return true;
    }

    public InteractionResult HandleHeroShift(TileNeighborhood neib)
    {
      int horizontal, vertical;
      GetMoveData(neib, out horizontal, out vertical);

      return HandleHeroShift(horizontal, vertical);
    }

    public static void GetMoveData(TileNeighborhood neib, out int horizontal, out int vertical)
    {
      horizontal = 0;
      vertical = 0;
      var res = DungeonNode.GetNeighborPoint(new Tile() { point = new Point(0, 0) }, neib);
      if (res.X != 0)
        horizontal = res.X;
      else
        vertical = res.Y;
    }

    public static MoveResult GetNewPositionFromMove(Point pos, int horizontal, int vertical)
    {
      if (horizontal != 0 || vertical != 0)
      {
        if (horizontal != 0)
        {
          pos.X += horizontal > 0 ? 1 : -1;
        }
        else if (vertical != 0)
        {
          pos.Y += vertical > 0 ? 1 : -1;
        }
        return new MoveResult(true, pos);
      }

      return new MoveResult(false, pos);
    }

    public virtual InteractionResult InteractHeroWith(Tile tile)
    {
      if (gm.Interact != null)
      {
        var res = gm.Interact(tile);
        if (res != InteractionResult.None)
          return res;
      }
      if (tile == null)
      {
        Logger.LogError("tile == null!!!");
        return InteractionResult.None;
      }

      bool tileIsDoor = tile is Tiles.Interactive.Door;
      bool tileIsDoorBySymbol = tile.Symbol == Constants.SymbolDoor;

      if (tile is Enemy || tile is Dungeons.Tiles.Wall || tile is Animal)
      {
        if (tile is Dungeons.Tiles.Wall wall && wall != null && wall.Child != null)
        {
          return HandleInteractionWithInteractive(wall.Child as Tiles.Interactive.InteractiveTile);
        }
        //Logger.LogInfo("Hero attacks " + tile);
        // var en = tile as Enemy;
        //if(!en.Alive)
        //  Logger.LogError("Hero attacks dead!" );
        //else
        //  Logger.LogInfo("Hero attacks en health = "+en.Stats.Health);
        gm.EnemiesManager.RemoveDead();

        gm.ApplyHeroPhysicalAttackPolicy(tile, true);

        return InteractionResult.Attacked;
      }
      else if (tile is INPC npc)
      {
        gm.AppendAction<NPCAction>((NPCAction ac) => { ac.NPCActionKind = NPCActionKind.Engaged; ac.InvolvedTile = npc; });
        return InteractionResult.Blocked;
      }
      else if (tile is IAlly)
      {
        var ally = tile as IAlly;
        gm.AppendAction<AllyAction>((AllyAction ac) => { ac.AllyActionKind = AllyActionKind.Engaged; ac.InvolvedTile = ally; });
        if (ally is TrainedHound th)
        {
          th.bark(false);
          //gm.SoundManager.PlaySound("ANIMAL_Dog_Bark_02_Mono");
        }
        return InteractionResult.Blocked;
      }
      
      else if (tile is INPC)
      {
        return InteractionResult.Blocked;
      }
      else if (tile is Lever lever)
      {
        gm.AppendAction<InteractiveTileAction>((InteractiveTileAction ac) =>
        {
          ac.InteractiveKind = InteractiveActionKind.LeverSwitched;
          ac.InvolvedTile = lever;
          ac.Info = "Hero used a lever";
        });
        return InteractionResult.Blocked;
      }
      else if (tileIsDoor || tileIsDoorBySymbol)
      {
        var door = tile as Tiles.Interactive.Door;
        if (door.Opened)
          return InteractionResult.None;

        if (door.KeyName.Any())
        {
          if (door.KeyPuzzle == Tiles.Looting.KeyPuzzle.LeverSet)
          {
            if (!gm.CurrentNode.BossLeversSolved())
            {
              gm.AppendAction((InteractiveTileAction ac) =>
              {
                ac.InteractiveKind = InteractiveActionKind.DoorLocked;
                ac.InvolvedTile = door;
                ac.Info = "Proper levers sequence not set";
              });
              gm.SoundManager.PlayBeepSound();
              return InteractionResult.Blocked;
            }
          }
          else
          {
            var key = Hero.GetKey(door.KeyName);
            if (key == null)
            {
              gm.AppendAction((InteractiveTileAction ac) =>
              {
                ac.InteractiveKind = InteractiveActionKind.DoorLocked;
                ac.InvolvedTile = door;
                ac.Info = "Proper key not available";
              });
              gm.SoundManager.PlayBeepSound();
              return InteractionResult.Blocked;
            }
            Hero.RemoveLoot(key);
          }
                    
          door.AllInSet.ForEach(i =>
          {
            i.MakeOpen();
          });
        }
        door.Opened = true;

        var opened = gm.CurrentNode.RevealRoom(door, Hero);
        if (opened)
        {
          gm.AppendAction((InteractiveTileAction ac) => { ac.InteractiveKind = InteractiveActionKind.DoorOpened; ac.InvolvedTile = door; });
        }
        return opened ? InteractionResult.Handled : InteractionResult.None;
      }

      else if (tile is Tiles.Interactive.InteractiveTile it)
      {
        return HandleInteractionWithInteractive(it);
      }
      return InteractionResult.None;
    }

    protected virtual InteractionResult HandleInteractionWithInteractive(Tiles.Interactive.InteractiveTile tile)
    {
      if (tile is Stairs)
      {
        var stairs = tile as Stairs;
        var destLevelIndex = -1;
        if (stairs.StairsKind == StairsKind.LevelDown ||
        stairs.StairsKind == StairsKind.LevelUp)
        {
          var level = gm.GetCurrentDungeonLevel();
          if (stairs.StairsKind == StairsKind.LevelDown)
          {
            destLevelIndex = level.Index + 1;
          }
          else if (stairs.StairsKind == StairsKind.LevelUp)
          {
            destLevelIndex = level.Index - 1;
          }
          if (gm.DungeonLevelStairsHandler != null)
            return gm.DungeonLevelStairsHandler(destLevelIndex, stairs);
        }
      }
      else if (tile is Portal)
      {
        return gm.HandlePortalCollision(tile as Portal);
      }
      else
      {
        gm.ApplyHeroPhysicalAttackPolicy(tile, true);
        return InteractionResult.Attacked;
      }
      return InteractionResult.Blocked;//blok hero by default
    }

    public InteractionResult HandleHeroShift(int horizontal, int vertical)
    {
      InteractionResult res = InteractionResult.None;

      try
      {
        if (!CanHeroDoAction(false))
          return res;

        if (gm.HeroMoveAllowed != null && !gm.HeroMoveAllowed())
          return res;
        var newPos = GetNewPositionFromMove(Hero.point, horizontal, vertical);
        if (!newPos.Possible)
        {
          return res;
        }
        var hc = gm.CurrentNode.GetHashCode();
        var tile = gm.CurrentNode.GetTile(newPos.Point);
        //logger.LogInfo(" tile at " + newPos.Point + " = "+ tile);
        if (tile == null)
        {
          gm.Logger.LogInfo(" tile null at " + newPos.Point);
          gm.CurrentNode.SetEmptyTile(newPos.Point);
          //res = InteractionResult.Blocked;
        }
        else if (!tile.IsEmpty)
          res = InteractHeroWith(tile);

        if (res == InteractionResult.ContextSwitched || res == InteractionResult.Blocked)
          return res;

        if (res == InteractionResult.Handled || res == InteractionResult.Attacked)
        {
          //ASCII printer needs that event
          //logger.LogInfo(" InteractionResult " + res + ", ac="  + ac);
          var lea = new LivingEntityAction(LivingEntityActionKind.Interacted) { InvolvedEntity = Hero };
          lea.InteractionResult = res;
          gm.EventsManager.AppendAction(lea);
        }
        else
        {
          //logger.LogInfo(" Hero ac ="+ ac);
          gm.ApplyMovePolicy(gm.Hero, newPos.Point, null, (e) =>
          {
            gm.OnHeroPolicyApplied(e);
          });
        }
      }
      catch (Exception ex)
      {
        Logger.LogError(ex);
      }

      return res;
    }
  }
}
