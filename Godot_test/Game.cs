using Dungeons.ASCIIDisplay;
using Godot;
using Roguelike.Abstract.Multimedia;
using Roguelike.Abstract;
using Roguelike;
using Roguelike.Managers;
using Roguelike.Multimedia;
using System;
using Roguelike.Events;
using System.IO;
using Dungeons.Tiles;
using Roguelike.Tiles.LivingEntities;
using static System.Net.WebRequestMethods;

public partial class Game : Node2D
{
  GameManager gm;
  IGame game;
  GodotGame.Entities.Hero hero;
  

  public GameManager GameManager
  {
    get { return game.GameManager; }
    set { game.GameManager = value; }//TODO remove?
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
	{
    var container = new Roguelike.ContainerConfigurator().Container;

    //container.Register<GameController, GameController>();
    container.Register<IDrawingEngine, ConsoleDrawingEngine>();
    container.Register<IGame, RoguelikeGame>();
    container.Register<ISoundPlayer, BasicSoundPlayer>();

    game = container.GetInstance<IGame>();
    this.GameManager.EventsManager.EventAppended += ActionsManager_ActionAppended;
    this.GameManager.Context.ContextSwitched += Context_ContextSwitched;

    GD.Print("GenerateDungeon...");
    game.GenerateDungeon();

   
  }

  private void Context_ContextSwitched(object sender, ContextSwitch e)
  {
    if (e.Kind == GameContextSwitchKind.NewGame)
    {
      GameManager.Hero.Name = "Godot Hero";
      var tiles = GameManager.CurrentNode.Tiles;

      foreach (var tile in tiles)
      {
        if (tile is Wall)
        {
          AddChildFromScene(tile, "res://Entities/Wall.tscn");
        }
        else if (tile is Hero heroTile)
        {
          AddChildFromScene(tile, "res://Entities/Hero.tscn");
          hero.HeroTile = heroTile;
        }
      }
    }
  }

  private Godot.Sprite2D AddChildFromScene(Tile tile, string scenePath)
  {
    var scene = GD.Load<PackedScene>(scenePath);
    var instance = scene.Instantiate();
    var spr = instance.GetChild<Godot.Sprite2D>(0);
    SetPositionFromTile(tile, spr);
    AddChild(instance);

    if (scenePath == "res://Entities/Hero.tscn")
    {
      this.hero = instance.GetChild<GodotGame.Entities.Hero>(0);
      this.hero.Moved += Hero_Moved;
    }

    return spr;
  }

  private void Hero_Moved(object sender, Vector2 e)
  {
    GameManager.HandleHeroShift((int)e.X,(int) e.Y);
  }

  private static void SetPositionFromTile(Tile tile, Godot.Sprite2D spr)
  {
    spr.GlobalPosition = new Vector2(tile.point.X * GodotGame.Entities.Hero.TileSize, tile.point.Y * GodotGame.Entities.Hero.TileSize);
  }

  private void ActionsManager_ActionAppended(object sender, GameEvent ev)
  {
	if (ev is LivingEntityAction)
	{
	  var lea = ev as LivingEntityAction;
	  if (lea.Kind == LivingEntityActionKind.Moved)
	  {
		if (lea.InvolvedEntity is Hero)
		{
		  SetPositionFromTile(hero.HeroTile, hero, true);
		}
		if (lea.InvolvedEntity is Enemy en)
		{
		  var enGodot = enemyList.SingleOrDefault(i => i.EnemyTile == en);
			GD.Print(en.Stats);
		  SetPositionFromTile(en, enGodot, true);
		}
	  }
	  if (lea.Kind == LivingEntityActionKind.Died && lea.InvolvedEntity is Enemy enemy)
	  {
		var enGodot = enemyList.SingleOrDefault(i => i.EnemyTile == enemy);


		enGodot.GetParent().QueueFree();
	  }
	}
	else if (ev is InteractiveTileAction)
	{
		var ita = ev as InteractiveTileAction;
		if (ita.InteractiveKind == InteractiveActionKind.DoorOpened) 
		{
			AddTile(ita.InvolvedTile);
		}
	}
	else if (ev is GameStateAction)
	{
		
	}
	else if (ev is LootAction)
	{
		
	}
	
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
	{
    game.MakeGameTick();

   
  }
}
