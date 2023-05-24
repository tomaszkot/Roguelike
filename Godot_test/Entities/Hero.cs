using Godot;
using Roguelike.Tiles.LivingEntities;
using System;

namespace GodotGame
{
	namespace Entities
	{
		public partial class Hero : Godot.Sprite2D
		{
			const int moveStep = WorldTileMap.TileSize;
			Roguelike.Tiles.LivingEntities.Hero heroTile;
			public event EventHandler<Vector2> Moved;

			public Roguelike.Tiles.LivingEntities.Hero HeroTile { get => heroTile; set => heroTile = value; }

	  public override void _Ready()
	  {
			CallDeferred("updateHealthBar");
	  }

	  public override void _UnhandledKeyInput(InputEvent @event)
			{
				base._UnhandledKeyInput(@event);

				int vertical = 0;
				int horizontal = 0;
				if (@event.IsActionPressed("ui_up"))
				{
					vertical = -1;
					//GlobalPosition += new Vector2(0, -moveStep);
				}
				else if (@event.IsActionPressed("ui_down"))
				{
					vertical = 1;
				}
				else if (@event.IsActionPressed("ui_left"))
				{
					horizontal = -1;
				}
				else if (@event.IsActionPressed("ui_right"))
				{
					horizontal = 1;
				}

				if (horizontal != 0 || vertical != 0)
				{
					if (Moved != null)
						Moved(this, new Vector2(horizontal, vertical));
				}
			}

			// Called every frame. 'delta' is the elapsed time since the previous frame.
			public override void _Process(double delta)
			{
				//var up = Input.IsActionPressed("walk_up");
				//if(up)
				//  GlobalPosition += new Vector2(0, 1);
				//else
				//{
				//  GlobalPosition += new Vector2(1, 0);
				//}

			}

			public void ShowDamageLabel(float damageValue, string text = "")
			{
				updateHealthBar();
				var label = (DamageLabel)ResourceLoader.Load<PackedScene>("res://ClientScripts/damage_label.tscn").Instantiate();
				label.Text = (Math.Round(damageValue, 2)).ToString();
				label.SelfModulate = new Color(255, 255, 255);
				if (text != "")
					label.Text = text;
				AddChild(label);
			}

			private void updateHealthBar()
			{
		var hpBar = (Node2D)GetNode("HpBar/Hp");
		var percentOfHealth = heroTile.Stats.Health / heroTile.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.Health);
		hpBar.Scale = new Vector2((float)percentOfHealth, hpBar.Scale.Y);
	  }


	  public override void _Input(InputEvent @event)
			{
				base._Input(@event);
				if (@event is InputEventMouseButton m)
				{
					if (m.IsPressed())
					{
						var camera = (Camera2D)GetNode("Camera2D");
						if (m.ButtonIndex == MouseButton.WheelUp)
						{
							if (camera.Zoom > new Vector2(0.4f, 0.4f))
								camera.Zoom = new Vector2(camera.Zoom.X - 0.05f, camera.Zoom.Y - 0.05f);
						}
						if (m.ButtonIndex == MouseButton.WheelDown)
						{
							if (camera.Zoom < new Vector2(1.2f, 1.2f))
								camera.Zoom = new Vector2(camera.Zoom.X + 0.05f, camera.Zoom.Y + 0.05f);
						}
					}
				}
			}
	}
  }
}
