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
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Tiles.Interactive;
using System.Drawing;
using Dungeons.TileContainers;

public partial class Game : Node2D
{
  GameManager gm;
  IGame game;
	DungeonNode dungeon;
  GodotGame.Entities.Hero hero;
  List<GodotGame.Entities.Enemy> enemyList = new();
  Dictionary<Enemy, GodotGame.Entities.Enemy> entityDict => new();
	TileMap tileMap;
	string pathToEnemies = "res://Sprites/LivingEntities/Enemies/";

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
	tileMap = (WorldTileMap)GetNode("TileMap");
  }

	public void GenerateDungeon()
	{
	dungeon = game.GenerateDungeon();
		CallDeferred("GenerateBackgroundTiles");
	}

	private void GenerateBackgroundTiles()
	{
	var tiles = GameManager.CurrentNode.Tiles;
		foreach (var tile in tiles) 
		{
			if (tile is not null && tile.IsEmpty)
			{
				AddTile(tile);
			}
		}
  }


  private void Context_ContextSwitched(object sender, ContextSwitch e)
	{
		if (e.Kind == GameContextSwitchKind.NewGame)
		{
			GameManager.Hero.Name = "Godot Hero";
			var tiles = GameManager.CurrentNode.Tiles;

			foreach (var tile in tiles)
			{
				if (tile is Roguelike.Tiles.Interactive.InteractiveTile) 
				{
					AddTile(tile);
				}
				if (tile is Wall || tile is Door)
				{
					AddTile(tile);
				}
				else if (tile is Hero heroTile)
				{
				 AddTile(tile);
				  AddChildFromScene(tile, "res://Entities/Hero.tscn");
					hero.HeroTile = heroTile;
				}
				else if (tile is Enemy)
				{
					AddChildFromScene(tile, "res://Entities/Enemy.tscn");
					var enemies = GameManager.EnemiesManager.GetEnemies();
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
	if (scenePath == "res://Entities/Enemy.tscn")
	{
	  var en = tile as Enemy;
	  var godotEn = instance.GetChild<GodotGame.Entities.Enemy>(0);
		godotEn.EnemyTile = en;
		godotEn.maxHp = en.Stats.Health;

	  //entityDict[en] = instance.GetChild<GodotGame.Entities.Enemy>(0);
	  //entityDict.Add(en, );
	  var enemies = entityDict.Count;

	  enemyList.Add(godotEn);
			if (ResourceLoader.Exists(pathToEnemies + tile.tag1 + ".png"))
			{
				spr.Texture = LoadTexture(pathToEnemies + tile.tag1);
			}
			else
				spr.Texture = LoadTexture(pathToEnemies + "Bat");
	}

	return spr;
  }

	private Texture2D LoadTexture(string path)
	{
		return ResourceLoader.Load(path + ".png") as Texture2D;
  }

	private void AddTile(Tile tile)
	{
		if (tile is Wall)
		{
			tileMap.SetCell(0, new Vector2I(tile.point.X, tile.point.Y), 0, new Vector2I(0,0));
		}
		if (tile is Door door)
		{
		  tileMap.SetCell(0, new Vector2I(tile.point.X, tile.point.Y), 1, new Vector2I(0, 0));
			if (door.Opened)
			{
				tileMap.EraseCell(0, new Vector2I(tile.point.X, tile.point.Y));
				tileMap.SetCell(1, new Vector2I(tile.point.X, tile.point.Y), 1, new Vector2I(0, 0));
			}
		}
		if (tile.IsEmpty || tile is Roguelike.Tiles.Interactive.InteractiveTile || tile is Hero heroTile)
		{
		  tileMap.SetCell(2, new Vector2I(tile.point.X, tile.point.Y), 2, new Vector2I(0, 0));
		}
	}

  private void Hero_Moved(object sender, Vector2 e)
  {
		GameManager.HandleHeroShift((int)e.X, (int)e.Y);
  }

  private static void SetPositionFromTile(Tile tile, Godot.Sprite2D spr, bool ObjectMoved = false)
  {
		if (!ObjectMoved)
			spr.GlobalPosition = new Vector2(tile.point.X * WorldTileMap.TileSize, tile.point.Y * WorldTileMap.TileSize);
		else
		{
			var tween = spr.CreateTween();
			tween.TweenProperty(spr, "global_position", new Vector2(tile.point.X * WorldTileMap.TileSize, tile.point.Y * WorldTileMap.TileSize), 0.2);
		}
  }

	private static Vector2 GetGamePosition(Point position)
	{
		var gamePosition = new Vector2(position.X * WorldTileMap.TileSize, position.Y * WorldTileMap.TileSize);
		return gamePosition;
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
				SetPositionFromTile(en, enGodot, true);
			}
	  }
	  if (lea.Kind == LivingEntityActionKind.Died && lea.InvolvedEntity is Enemy enemy)
	  {
			var enGodot = enemyList.SingleOrDefault(i => i.EnemyTile == enemy);
			enGodot.GetParent().QueueFree();
		}
		if (lea.Kind == LivingEntityActionKind.GainedDamage)
		{
			if (lea.InvolvedEntity is Enemy en)
			{
			  var enGodot = enemyList.SingleOrDefault(i => i.EnemyTile == en);
				enGodot.updateHpBar((float)lea.InvolvedValue);
			}
			if (lea.InvolvedEntity is Hero)
			{
					hero.ShowDamageLabel((float)lea.InvolvedValue);
			}
		}
		if (lea.Kind == LivingEntityActionKind.Missed)
		{
			if (lea.InvolvedEntity is Enemy en) 
			{
					hero.ShowDamageLabel((float)lea.InvolvedValue, "Evaded");
			}
			if (lea.InvolvedEntity is Hero)
			{
				var targetTile = dungeon.GetTile(lea.targetEntityPosition);
		var enGodot = enemyList.SingleOrDefault(i => i.EnemyTile == targetTile);
		enGodot.updateHpBar((float)lea.InvolvedValue,"Evaded");
		}
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
