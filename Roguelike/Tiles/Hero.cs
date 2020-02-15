#define ASCII_BUILD  
using Dungeons.Core;
using System.Drawing;
using System;

namespace Roguelike.Tiles
{
  public class Hero : AdvancedLivingEntity
  {
    public Hero(): base(new Point().Invalid(), '@')
    {
      Stats.SetNominal(EntityStatKind.Health, 15);//level up +2
                                                                                     // Character.Mana = 40;
      Stats.SetNominal(EntityStatKind.Strength, 15);//15
      Stats.SetNominal(EntityStatKind.Magic, 10);
      Stats.SetNominal(EntityStatKind.Mana, 40);
      Stats.SetNominal(EntityStatKind.Defence, 10);

      CreateInventory();

      Dirty = true;//TODO
#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }

    public override string ToString()
    {
      return base.ToString();// + Data.AssetName;
    }
  }
}
