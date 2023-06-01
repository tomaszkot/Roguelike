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
  private string defaultWeponTexture = "res://Sprites/Loot/Equipment/Weapons/Swords/rusty_sword.png";
  private string defaultArmorTexture = "res://Sprites/Loot/Equipment/Armor/Armors/tattered_shirt.png";
  private string defaultScroolTexture = "res://Sprites/Loot/Scrools/fire_scroll.png";
  private string defaultConsumableTexture = "res://Sprites/Loot/Consumable/health_potion.png";
  private string coinTexture = "res://Sprites/Loot/Consumable/coin.png";
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

	if (loot is Weapon w)
	{
	  var texture = pathToEquipment + "Weapons/";
	  texture += weaponKindPaths[w.kind];
	  texture += w.tag1 + ".png";

	  if (ResourceLoader.Exists(texture))
		LoadTexture(texture);
	  else
		LoadTexture(defaultWeponTexture);
	}
	else if (loot is Equipment a && loot is not Weapon)
	{
	  var texture = pathToEquipment + "Armor/";
	  texture += armorsKindPaths[a.EquipmentKind];
	  texture += a.tag1 + ".png";

	  if (ResourceLoader.Exists(texture))
		LoadTexture(texture);
	  else
		LoadTexture(defaultArmorTexture);
	}
	else if (loot is Roguelike.Tiles.Looting.MagicDust || loot is Roguelike.Tiles.Looting.Gold)
	{
	  LoadTexture(coinTexture);
	}
	else if (loot is Scroll)
	{
	  string texture = pathToScrools + loot.tag1 + ".png";

	  if (ResourceLoader.Exists(texture))
		LoadTexture(texture);
	  else
		LoadTexture(defaultScroolTexture);
	}
	else if (loot is Consumable)
	{
	  string texture = pathToConsumables + loot.tag1 + ".png";
	  if (ResourceLoader.Exists(texture))
		LoadTexture(texture);
	  else
		LoadTexture(defaultConsumableTexture);
	}
  }
}
