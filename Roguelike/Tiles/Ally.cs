﻿using Dungeons.Core;
using Roguelike.Attributes;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public enum AllyKind { Unset, Dog, Skeleton }

  public class Ally : AdvancedLivingEntity
  {
    public Ally(Container cont) : base(new Point().Invalid(), '!')
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

      CreateInventory(cont);

      Dirty = true;//TODO
#if ASCII_BUILD
      color = ConsoleColor.Yellow;
#endif
    }

    public AllyKind Kind { get; set; }
  }
}
