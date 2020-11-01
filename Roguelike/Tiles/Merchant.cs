using Dungeons.Core;
using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.LootContainers;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public class Merchant : AdvancedLivingEntity, IAlly
  {
    public Merchant(Container cont) : base(new Point().Invalid(), '!')
    {
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
      CreateInventory(cont);
      Inventory.Capacity = 24;//TODO

      Dirty = true;//TODO
#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }

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

    public bool Active { get ; set ; }
    public AllyKind Kind { get => AllyKind.Merchant;  }

    internal void OnContextSwitched(Container container)
    {
      Inventory.Container = container;
      Inventory.Owner = "Merchant";
    }
  }
}
