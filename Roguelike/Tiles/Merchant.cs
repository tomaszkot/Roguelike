using Dungeons.Core;
using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public class Merchant : AdvancedLivingEntity
  {
    public Merchant() : base(new Point().Invalid(), '!')
    {
      Stats.SetNominal(EntityStatKind.Health, 15);
      // Character.Mana = 40;
      var str = 15;
      Stats.SetNominal(EntityStatKind.Strength, str);//15
      Stats.SetNominal(EntityStatKind.Attack, str);
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defence, 10);
      Stats.SetNominal(EntityStatKind.Dexterity, 10);

      Gold = 100000;
      CreateInventory();
      Inventory.Capacity = 24;//TODO

      Dirty = true;//TODO
#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }
  }
}
