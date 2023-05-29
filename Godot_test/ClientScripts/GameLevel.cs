using Godot;
using Roguelike.Abstract;
using Roguelike;
using Roguelike.Managers;
using Roguelike.Events;
using System.IO;
using Dungeons.Tiles;
using Roguelike.Tiles.LivingEntities;
using static System.Net.WebRequestMethods;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Tiles.Interactive;
using Dungeons.TileContainers;
using God4_1.ClientScripts;
using GodotGame.Entities;

namespace God4_1.ClientScripts
{
  public partial class GameLevel : Node
  {
	string pathToEnemies = "res://Sprites/LivingEntities/Enemies/";
	public Dictionary<Roguelike.Tiles.LivingEntities.Enemy, GodotGame.Entities.Enemy> enemyList = new();
	private Dictionary<Roguelike.Tiles.Interactive.InteractiveTile, Node> interactiveList = new();
	private Dictionary<Roguelike.Tiles.Looting.Equipment, Node> lootingList = new();
	const int defaultLayer = 0;
	const int transparentLayer = 1;
	const int backgroundLayer = 2;
	const int StairsUpTileIndex = 4;
	const int StairsDownTileIndex = 3;

	public void generateMapTiles(Dungeons.Tiles.Tile[,] tiles)
	{
	  foreach (var tile in tiles)
	  {
		if (tile is Roguelike.Tiles.Interactive.InteractiveTile)
		{
		  AddTile(tile);
		}
		else if (tile is Wall)
		{
		  AddTile(tile);
		}
	  }
	}

	public void CreateEntities(Dungeons.Tiles.Tile[,] tiles)
	{
	  foreach (var tile in tiles)
	  {
		if (tile is Roguelike.Tiles.Interactive.Barrel)
		{
		  AddChildFromScene(tile, "res://Entities/Barrel.tscn");
		}
		else if (tile is Roguelike.Tiles.Interactive.Chest)
		{
		  AddChildFromScene(tile, "res://Entities/Chest.tscn");
		}
		else if (tile is Roguelike.Tiles.LivingEntities.Hero heroTile)
		{
		  AddTile(tile);
		  AddChildFromScene(tile, "res://Entities/Hero.tscn");
		  Game.hero.HeroTile = heroTile;
		}
		else if (tile is Roguelike.Tiles.LivingEntities.Enemy)
		{
		  AddChildFromScene(tile, "res://Entities/Enemy.tscn");
		  var enemies = Game.GameManager.EnemiesManager.GetEnemies();
		}
	  }
	}

	public Godot.Sprite2D AddChildFromScene(Tile tile, string scenePath)
	{
	  var scene = GD.Load<PackedScene>(scenePath);
	  var instance = scene.Instantiate();
	  var spr = instance.GetChild<Godot.Sprite2D>(0);
	  Game.SetPositionFromTile(tile, spr);

	  if (scenePath == "res://Entities/Hero.tscn")
	  {
		Game.hero = instance.GetChild<GodotGame.Entities.Hero>(0);
		Game.hero.Moved += Hero_Moved;
	  }
	  else if (scenePath == "res://Entities/Enemy.tscn")
	  {
		var en = tile as Roguelike.Tiles.LivingEntities.Enemy;
		var godotEn = instance.GetChild<GodotGame.Entities.Enemy>(0);
		godotEn.EnemyTile = en;

		enemyList.Add(en, godotEn);
		if (ResourceLoader.Exists(pathToEnemies + tile.tag1 + ".png"))
		{
		  spr.Texture = LoadTexture(tile.tag1, tile);
		}
		else
		  spr.Texture = LoadTexture("bat", tile);
	  }
	  else if (scenePath == "res://Entities/equipment_item.tscn")
	  {
		var t = tile as Roguelike.Tiles.Looting.Equipment;
		lootingList.Add(t, instance);
		var e = (EquipmentItem)instance;
		e.equipment = t;
	  }
	  if (tile is Roguelike.Tiles.Interactive.InteractiveTile)
	  {
		var t = tile as Roguelike.Tiles.Interactive.InteractiveTile;
		interactiveList.Add(t, instance);
	  }
	  if (tile is Roguelike.Tiles.Interactive.Chest c)
	  {
		var chest = (Chest)instance;
		chest.updateChestTexture(c);
	  }

	  AddChild(instance);
	  return spr;
	}

	public static void AddTile(Tile tile)
	{
	  if (tile is Wall)
	  {
		Game.tileMap.SetTileCell(tile, defaultLayer, 0);
	  }
	  else if (tile is Door door)
	  {
		Game.tileMap.SetTileCell(tile, defaultLayer, 1);
		if (door.Opened)
		{
		  Game.tileMap.EraseCell(0, new Vector2I(tile.point.X, tile.point.Y));
		  Game.tileMap.SetTileCell(tile, transparentLayer, 1);
		}
	  }
	  else if (tile is Stairs s)
	  {
		if (s.StairsKind == StairsKind.LevelDown)
		  Game.tileMap.SetTileCell(tile, defaultLayer, StairsDownTileIndex);
		else if (s.StairsKind == StairsKind.LevelUp)
		  Game.tileMap.SetTileCell(tile, defaultLayer, StairsUpTileIndex);
	  }
	  else if (tile.IsEmpty || tile is Roguelike.Tiles.Interactive.InteractiveTile || tile is Roguelike.Tiles.LivingEntities.Hero heroTile || tile is Roguelike.Tiles.Loot)
	  {
		Game.tileMap.SetTileCell(tile, backgroundLayer, 2);
	  }
	}

	private Texture2D LoadTexture(string path, Tile tile)
	{
	  var texture = new Texture2D();
	  if (tile is Roguelike.Tiles.LivingEntities.Enemy)
		texture = ResourceLoader.Load(pathToEnemies + path + ".png") as Texture2D;
	  return texture;
	}

	private void Hero_Moved(object sender, Vector2 e)
	{
	  Game.GameManager.HandleHeroShift((int)e.X, (int)e.Y);
	}

	public Node GetNode(Dungeons.Tiles.Tile tile)
	{
	  var node = new Node();
	  if (tile is Roguelike.Tiles.Interactive.InteractiveTile)
		node = interactiveList[(Roguelike.Tiles.Interactive.InteractiveTile)tile];
	  else if (tile is Roguelike.Tiles.Looting.Equipment)
		node = lootingList[(Roguelike.Tiles.Looting.Equipment)tile];
	  return node;
	}
  }
}
