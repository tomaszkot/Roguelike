﻿using Dungeons;
using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using Roguelike.Abstract;
using Roguelike.Events;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using System;
using System.Drawing;

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

    public bool CanHeroDoAction()
    {
      if (!gm.HeroTurn)
        return false;
      if (!gm.Hero.Alive)
      {
        if (!Context.HeroDeadReported)
        {
          Context.EventsManager.AppendAction(Context.Hero.GetDeadAction());
          Context.HeroDeadReported = true;
        }
        //AppendAction(new HeroAction() { Level = ActionLevel.Critical, KindValue = HeroAction.Kind.Died, Info = Hero.Name + " is dead!" });
        return false;
      }

      if (gm.Hero.State != EntityState.Idle)
        return false;

      var ac = Context.TurnActionsCount[TurnOwner.Hero];
      if (ac == 1)
        return false;

      return true;
    }

    public InteractionResult HandleHeroShift(TileNeighborhood neib)
    {
      int horizontal = 0;
      int vertical = 0;
      var res = DungeonNode.GetNeighborPoint(new Tile() { Point = new Point(0, 0) }, neib);
      if (res.X != 0)
        horizontal = res.X;
      else
        vertical = res.Y;

      return HandleHeroShift(horizontal, vertical);
    }

    public MoveResult GetNewPositionFromMove(Point pos, int horizontal, int vertical)
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
      bool tileIsDoor = tile is Tiles.Door;
      bool tileIsDoorBySumbol = tile.Symbol == Constants.SymbolDoor;

      if (tile is Enemy || tile is Dungeons.Tiles.Wall)
      {
        //Logger.LogInfo("Hero attacks " + tile);
        // var en = tile as Enemy;
        //if(!en.Alive)
        //  Logger.LogError("Hero attacks dead!" );
        //else
        //  Logger.LogInfo("Hero attacks en health = "+en.Stats.Health);
        Context.ApplyPhysicalAttackPolicy(Hero, tile, (p) => gm.OnHeroPolicyApplied(this, p));

        return InteractionResult.Attacked;
      }
      //else if (tile is Merchant)
      //{
      //  AppendAction<MerchantAction>((MerchantAction ac) => { ac.MerchantActionKind = MerchantActionKind.Engaged; ac.InvolvedTile = tile as Merchant; });
      //  return InteractionResult.Blocked;
      //}
      else if (tile is IAlly)
      {
        var ally = tile as IAlly;
        gm.AppendAction<AllyAction>((AllyAction ac) => { ac.AllyActionKind = AllyActionKind.Engaged; ac.InvolvedTile = ally; });
        if (ally is TrainedHound)
        {
          gm.SoundManager.PlaySound("ANIMAL_Dog_Bark_02_Mono");
        }
        return InteractionResult.Blocked;
      }
      else if (tileIsDoor || tileIsDoorBySumbol)
      {
        var door = tile as Tiles.Door;
        if (door.Opened)
          return InteractionResult.None;

        var opened = gm.CurrentNode.RevealRoom(door, Hero);
        if (opened)
        {
          gm.AppendAction<InteractiveTileAction>((InteractiveTileAction ac) => { ac.InteractiveKind = InteractiveActionKind.DoorOpened; ac.InvolvedTile = door; });
        }
        return opened ? InteractionResult.Handled : InteractionResult.None;
      }

      else if (tile is Roguelike.Tiles.InteractiveTile)
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
          Context.ApplyPhysicalAttackPolicy(Hero, tile, (policy) => gm.OnHeroPolicyApplied(this, policy));
          return InteractionResult.Attacked;
        }
        return InteractionResult.Blocked;//blok hero by default
      }
      return InteractionResult.None;
    }

    public InteractionResult HandleHeroShift(int horizontal, int vertical)
    {
      InteractionResult res = InteractionResult.None;

      try
      {
        if (!CanHeroDoAction())
          return res;

        if (gm.HeroMoveAllowed != null && !gm.HeroMoveAllowed())
          return res;
        var newPos = GetNewPositionFromMove(Hero.Point, horizontal, vertical);
        if (!newPos.Possible)
        {
          return res;
        }
        var hc = gm.CurrentNode.GetHashCode();
        var tile = gm.CurrentNode.GetTile(newPos.Point);
        //logger.LogInfo(" tile at " + newPos.Point + " = "+ tile);
        if (!tile.IsEmpty)
          res = InteractHeroWith(tile);

        if (res == InteractionResult.ContextSwitched || res == InteractionResult.Blocked)
          return res;

        if (res == InteractionResult.Handled || res == InteractionResult.Attacked)
        {
          //ASCII printer needs that event
          //logger.LogInfo(" InteractionResult " + res + ", ac="  + ac);
          gm.EventsManager.AppendAction(new LivingEntityAction(LivingEntityActionKind.Interacted) { InvolvedEntity = Hero });
        }
        else
        {
          //logger.LogInfo(" Hero ac ="+ ac);
          Context.ApplyMovePolicy(gm.Hero, newPos.Point, (e) =>
          {
            gm.OnHeroPolicyApplied(this, e);
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