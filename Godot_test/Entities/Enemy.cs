using Godot;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Reflection.Metadata.Ecma335;

namespace GodotGame
{
	namespace Entities 
	{
		public partial class Enemy : Godot.Sprite2D
		{
			Roguelike.Tiles.LivingEntities.Enemy enemyTile;
			public Roguelike.Tiles.LivingEntities.Enemy EnemyTile { get => enemyTile; set => enemyTile = value; }
		}
	}
}
