using God4_1.Entities;
using Godot;
using Godot.Collections;
using Roguelike.Probability;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;

public partial class EquipmentItem : Entity
{
  public Roguelike.Tiles.Loot loot;
  private string pathToEquipment = "res://Sprites/Loot/Equipment/";
  private string pathToScrools = "res://Sprites/Loot/Scrools/";
  private string pathToConsumables = "res://Sprites/Loot/Consumable/";
  private Dictionary<Weapon.WeaponKind, string> weaponKindPaths = new Dictionary<Weapon.WeaponKind, string> { { Weapon.WeaponKind.Sword, "Swords/" },
  { Weapon.WeaponKind.Bashing, "Bashing/" }, { Weapon.WeaponKind.Axe, "Axes/" }, { Weapon.WeaponKind.Dagger, "Daggers/" } };
  private Dictionary<EquipmentKind, string> armorsKindPaths = new Dictionary<EquipmentKind, string> { { EquipmentKind.Armor, "Armors/" },
  { EquipmentKind.Helmet, "Helmets/" }, { EquipmentKind.Shield, "Shields/" } };
  public override void _Ready()
  {
	base._Ready();
	CallDeferred("SetSprite");
  }

  public void SetSprite()
  {
	if (loot is Equipment)
	{
	  if (loot is Weapon w)
	  {
		var texture = pathToEquipment + "Weapons/";
		texture += weaponKindPaths[w.kind];
		texture += w.tag1 + ".png";

		LoadTexture(texture);
	  }
	  else if (loot is Armor a)
	  {
		var texture = pathToEquipment + "Armor/";
		texture += armorsKindPaths[a.EquipmentKind];
		texture += a.tag1 + ".png";

		LoadTexture(texture);
	  }
	}
	else if (loot is Scroll)
	{
	  LoadTexture(pathToScrools + loot.tag1 + ".png");
	}
	else if (loot is Consumable)
	{
	  LoadTexture(pathToConsumables + loot.tag1 + ".png");
	}
  }
}
