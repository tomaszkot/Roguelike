using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Abilities;
using Roguelike.Abstract.Tiles;
using Roguelike.Attributes;
using Roguelike.LootContainers;
using Roguelike.Tiles.Abstract;
using SimpleInjector;
using System;
using System.Drawing;
using System.Dynamic;

namespace Roguelike.Tiles.LivingEntities
{
  public class TileProfessionNameParts
  {
    public const string NPC = "NPC_";
    public const string Merchant = "merchant_";
    public const string Mecenary = "paladin_";
    public const string TeutonicKnight = "teutonic_knight_";
    public const string Smith = "smith_";
    public const string Woodcutter = "woodcutter_";
    public const string WarriorJurands = "warrior_Jurands";
    public const string WarriorLeszys = "warrior_Leszys";
    public const string Warrior = "warrior_";
    

    public const string Ally = "ally_";//ally is hound or a skeleton
    public const string Knight = "knight_";

    public static string GetTileTagPart(EntityProffesionKind kind)
    {
      string pref = "";
      string rest = "";
      switch (kind)
      {
        case EntityProffesionKind.Unset:
          break;
        case EntityProffesionKind.King:
          break;
        case EntityProffesionKind.Prince:
          break;
        case EntityProffesionKind.Knight:
          break;
        case EntityProffesionKind.Priest:
          break;
        case EntityProffesionKind.Mercenary:
          pref = NPC;
          rest = Mecenary;
          break;
        case EntityProffesionKind.Merchant:
          break;
        case EntityProffesionKind.Peasant:
          break;
        case EntityProffesionKind.Bandit:
          break;
        case EntityProffesionKind.Adventurer:
          break;
        case EntityProffesionKind.Slave:
          break;
        case EntityProffesionKind.TeutonicKnight:
          break;
        case EntityProffesionKind.Smith:
          break;
        case EntityProffesionKind.Woodcutter:
          break;
        case EntityProffesionKind.Carpenter:
          break;
        case EntityProffesionKind.Warrior:
          break;
        default:
          break;
      }

      return pref + rest;
    }
  }
  public class Merchant : NPC, IAlly, IMerchant
  {
    public const int HoundPrice = 100;
    public bool AllowBuyHound { get; set; } = false;
    public Merchant(Container cont) : base(cont)
    {
      Proffesion = EntityProffesionKind.Merchant;

      Gold = 100000;
      inventory = new Inventory(cont);
      inventory.Capacity = 80;
      RelationToHero.Kind = RelationToHeroKind.Neutral;

      Immortal = true;//Mainly for Sanderus

#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }

    public override void SetNameFromTag1()
    {
      var name = GetNameFromTag1();
      name.Replace(TileProfessionNameParts.Merchant, "");
      name = name.Trim();
      Name = name;
    }

    Inventory inventory = null;
    [JsonIgnore]
    public override Inventory Inventory
    {
      get => inventory;
      set
      {
        Inventory = value;
        Inventory.PriceFactor = 4;
        Inventory.InvBasketKind = InvBasketKind.Merchant;
      }
    }

    internal void OnContextSwitched(Container container)
    {
      Inventory.Container = container;
      Inventory.Owner = this;
    }

    LootAbility lootAb;
    internal LootAbility GetLootAbility()
    {
      if (lootAb == null)
        lootAb = new LootAbility(true);

      return lootAb;
    }

  }
}
