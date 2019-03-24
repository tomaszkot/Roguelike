﻿using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Abstract;
using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Diagnostics;
using System.Linq;

namespace Roguelike
{
  public enum GameContextSwitchKind { DungeonSwitched, NewGame, GameLoaded}

  public class ContextSwitch
  {
    public GameContextSwitchKind Kind { get; set; }
    public GameNode CurrentNode { get;  set; }
    public Hero Hero { get; set; }
  }

  public class GameContext
  {
    Hero hero;
    
    public virtual GameNode CurrentNode { get; protected set; }
    public Hero Hero { get => hero; set => hero = value; }
    public event EventHandler EnemiesTurn;
    public event EventHandler<GenericEventArgs<ContextSwitch>> ContextSwitched;
    public EventsManager EventsManager { get ; set ; }
    ILogger logger;

    public GameContext(ILogger logger)
    {
      this.logger = logger;
    }

    public virtual void SwitchTo(GameNode node, Hero hero, GameContextSwitchKind context, Stairs stairs = null)
    {
      if (node == CurrentNode)
      {
        Debug.Assert(false);
        return;
      }
      
      this.Hero = hero;

      if (context != GameContextSwitchKind.GameLoaded && context != GameContextSwitchKind.NewGame)
      {
        var heros = CurrentNode.GetTiles<Hero>();
        var heroInNode = heros.SingleOrDefault();
        Debug.Assert(heroInNode != null);
        if (heroInNode == null)
          logger.LogError("SwitchTo heros.Count = " + heros.Count);

        if (heroInNode != null)
          CurrentNode.SetEmptyTile(heroInNode.Point);//Hero is going to be placed in the node, remove it from the old one (CurrentNode)

        Tile heroStartTile = PlaceHeroAtDungeon(node, stairs);
        node.SetTile(this.Hero, heroStartTile.Point, false);
      }
      else
      {
        if (!node.SetTile(Hero, Hero.Point))
        {
          logger.LogError("!node.SetTile "+ Hero);
        }
      }
      
      CurrentNode = node;
      //EventsManager.AppendAction(new GameStateAction() { InvolvedNode = node, Type = GameStateAction.ActionType.ContextSwitched });
      EmitContextSwitched(context);
    }

    protected virtual Tile PlaceHeroAtDungeon(GameNode node, Stairs stairs)
    {
      Tile heroStartTile = node.GetEmptyTiles().First();

      return heroStartTile;
    }

    public void EmitContextSwitched(GameContextSwitchKind context)
    {
      if (ContextSwitched != null)
        ContextSwitched(this, new GenericEventArgs<ContextSwitch>(new ContextSwitch() { Kind = context, CurrentNode = this.CurrentNode, Hero = this.hero }));
    }

    bool heroTurn = true;
    public bool HeroTurn
    {
      get { return heroTurn; }
      set
      {
        heroTurn = value;
        if (!heroTurn)
        {
          if (EnemiesTurn != null)
            EnemiesTurn(this, EventArgs.Empty);
          heroTurn = true;
        }

      }
    }
  }
}
