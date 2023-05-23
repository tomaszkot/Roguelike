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
			public double maxHp;

			public void updateHpBar(float damageValue, string text = "")
			{
				if (enemyTile.Stats.Health > 0)
				{
				 var hpBar = (Node2D)GetNode("HpBar/Hp");
				 var percentOfHealth = enemyTile.Stats.Health / maxHp;
				 hpBar.Scale = new Vector2((float)hpBar.Scale.X * (float)percentOfHealth, hpBar.Scale.Y);
				 var label = (DamageLabel)ResourceLoader.Load<PackedScene>("res://ClientScripts/damage_label.tscn").Instantiate();
				 label.Text = (Math.Round(damageValue, 2)).ToString();
				 if (text != "")
					label.Text = text;
				 AddChild(label);
				}
				
			}
		}
	}
}
