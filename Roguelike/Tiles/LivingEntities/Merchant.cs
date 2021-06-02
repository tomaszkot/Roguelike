using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Abstract.Tiles;
using Roguelike.Attributes;
using Roguelike.LootContainers;
using Roguelike.Tiles.Abstract;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public class Merchant : NPC, IAlly, IMerchant
  {
    public const int HoundPrice = 100;
    public bool AllowBuyHound { get; set; } = false;

    public Merchant(Container cont) : base(cont)
    {
      Proffesion = EntityProffesionKind.Merchant;
      Stats.SetNominal(EntityStatKind.Health, 15);
      // Character.Mana = 40;
      var str = 15;
      Stats.SetNominal(EntityStatKind.Strength, str);//15
      Stats.SetNominal(EntityStatKind.Attack, str);
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defense, 10);
      Stats.SetNominal(EntityStatKind.Dexterity, 10);

      Gold = 100000;
      Inventory.Capacity = 64;
#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }

    public override void SetNameFromTag1()
    {
      var name = GetNameFromTag1();
      name.Replace("ally_Merchant", "");
      name = name.Trim();
      Name = name;
    }

    [JsonIgnore]
    public override Inventory Inventory
    {
      get => base.Inventory;
      set
      {
        base.Inventory = value;
        Inventory.PriceFactor = 4;
        Inventory.InvBasketKind = InvBasketKind.Merchant;
      }
    }

    public bool Active { get; set; }
    public AllyKind Kind { get => AllyKind.Merchant; }
    public Point Point { get => point; set => point = value; }

    public bool TakeLevelFromCaster { get; }

    internal void OnContextSwitched(Container container)
    {
      Inventory.Container = container;
      Inventory.Owner = this;
    }
  }
}
