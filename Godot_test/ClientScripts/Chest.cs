using Godot;
using System;

public partial class Chest : Node
{
	string plainChestTexture = "uid://ddvwf8g3qv5wy";
	string goldenChestClosedTexture = "uid://bqcw1ralpmvp8";
	string goldenChestOpenedTexture = "uid://bbpucbqsiiu45";

	public void updateChestTexture(Roguelike.Tiles.Interactive.Chest chest)
	{
		var spr = (Sprite2D)GetChild(0);
		if (chest.ChestKind == Roguelike.Tiles.Interactive.ChestKind.Plain)	
		{
			if (chest.Closed)
				spr.Texture = ResourceLoader.Load(plainChestTexture) as Texture2D;
			else
				spr.SelfModulate = new Color(0.5f, 0.5f, 0.5f); //Nie mam grafiki otwartej drewnianej skrzyni, wiec po otwarciu sprawiam by była zaciemniona
		}
		else if (chest.ChestKind == Roguelike.Tiles.Interactive.ChestKind.Gold) 
		{
	  if (chest.Closed)
		spr.Texture = ResourceLoader.Load(goldenChestClosedTexture) as Texture2D;
	  else
		spr.Texture = ResourceLoader.Load(goldenChestOpenedTexture) as Texture2D;
	}
	}
}
