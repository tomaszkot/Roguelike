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
using God4_1.ClientScripts;

public partial class Game : Node2D
{
  public static GameManager gm;
  public static IGame game;
  public static DungeonNode dungeon;
  public static GodotGame.Entities.Hero hero;
  public static TileMap tileMap;
  public static bool isGameStarted = false;
  public GameEventHandler eventHandler = new GameEventHandler();
  public GameLevel gameLevel = new GameLevel();

  public static GameManager GameManager
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
	GameManager.EventsManager.EventAppended += eventHandler.ActionsManager_ActionAppended;
	GameManager.Context.ContextSwitched += Context_ContextSwitched;
	tileMap = (WorldTileMap)GetNode("TileMap");
	gameLevel = (GameLevel)GetNode("Objects");
  }

  public void GenerateDungeon()
  {
	dungeon = game.GenerateDungeon();
	isGameStarted = true;

	CallDeferred("GenerateBackgroundTiles");
  }

  private void GenerateBackgroundTiles()
  {
	var tiles = GameManager.CurrentNode.Tiles;
	foreach (var tile in tiles)
	{
	  if (tile is not null && tile.IsEmpty)
	  {
			GameLevel.AddTile(tile);
	  }
	}
  }


  private void Context_ContextSwitched(object sender, ContextSwitch e)
  {
	if (e.Kind == GameContextSwitchKind.NewGame)
	{
	  GameManager.Hero.Name = "Godot Hero";
	  var tiles = GameManager.CurrentNode.Tiles;
	  gameLevel.generateMapTiles(tiles);
	  gameLevel.CreateEntities(tiles);
	}
	else if (e.Kind == GameContextSwitchKind.GameLoaded)
	{
	  isGameStarted = true;
	  var tiles = GameManager.CurrentNode.Tiles;
	  gameLevel.generateMapTiles(tiles);
	  gameLevel.CreateEntities(tiles);
	  CallDeferred("GenerateBackgroundTiles");
	}
  }

  public static void SetPositionFromTile(Tile tile, Godot.Sprite2D spr, bool ObjectMoved = false)
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

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
	game.MakeGameTick();
  }

  public void SaveGame()
  {
	GameManager.Save(false);
  }
}
