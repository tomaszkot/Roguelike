﻿using Dungeons.Core;
using Roguelike.Abstract;
using Roguelike.Abstract.Tiles;
using Roguelike.Attributes;
using Roguelike.Managers;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.LivingEntities
{
  public class TrainedHound : LivingEntities.AdvancedLivingEntity, IAlly
  {
    public AllyKind Kind { get; set; }
    public bool Active { get ; set ; }

    public TrainedHound() : base(new Point().Invalid(), '!')
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

      CreateInventory(null);

      Dirty = true;//TODO

      tag1 = "hound";

      Kind = AllyKind.Hound;
#if ASCII_BUILD
        color = ConsoleColor.Yellow;
#endif
    }

    public void bark(bool strong)
    {
      PlaySound("ANIMAL_Dog_Bark_02_Mono");
    }

    public override void PlayAllySpawnedSound() 
    {
      bark(false);
    }
  }
}