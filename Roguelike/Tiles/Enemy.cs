#define ASCII_BUILD  
using System.Collections.Generic;
using System.Linq;
using System;
using System.Drawing;
using Dungeons.Core;

using Newtonsoft.Json;

namespace Roguelike.Tiles
{
  public enum RoomKind { None, PuzzleRoom, Island }

  public class Enemy : LivingEntity
  {
    public enum PowerKind { Plain, Champion, Boss };
    //PowerKind kind;

    public static readonly EntityStat BaseAttack = new EntityStat(EntityStatKind.Attack, 13f);
    public static readonly EntityStat BaseHealth = new EntityStat(EntityStatKind.Health, 13);
    public static readonly EntityStat BaseDefence = new EntityStat(EntityStatKind.Defence, 5);
    public static readonly EntityStat BaseMana = new EntityStat(EntityStatKind.Mana, 4);
    public static readonly EntityStat BaseMagic = new EntityStat(EntityStatKind.Magic, 10);

    public static readonly EntityStats BaseStats;
    //bool statsIncreased = false;
    public static char[] AllSymbols;

   // public int NumberOfCastedEffectsForAllies = 0;
    public int NumberOfEmergencyTeleports = 0;
 
    static Enemy()
    {
      BaseStats = new EntityStats();

      BaseStats.Stats[EntityStatKind.Attack] = BaseAttack;
      BaseStats.Stats[EntityStatKind.Defence] = BaseDefence;
      BaseStats.Stats[EntityStatKind.Health] = BaseHealth;
      BaseStats.Stats[EntityStatKind.Mana] = BaseMana;
      var mag = new EntityStat(EntityStatKind.Magic, BaseMagic.NominalValue + 2);
      BaseStats.Stats[EntityStatKind.Magic] = mag;

      
    }

    public Enemy() : this(new Point().Invalid(), 'e')
    {

    }

    public Enemy(char symbol) : this(new Point().Invalid(), symbol)
    {
    }

    public Enemy(Point point, char symbol) : base(point, symbol)
    {
      //MovesCountPerTurn = 2;

      this.Symbol = symbol;
    
#if ASCII_BUILD
      color = ConsoleColor.Red;
#endif
      Alive = true;

     
      foreach (var basicStats in EntityStat.BasicStats)
      {
        var nv = BaseStats.Stats[basicStats].NominalValue;
        Stats.SetNominal(basicStats, nv);
      }

      Stats.Experience = 1;
      //kind = PowerKind.Plain;
      Name = "Enemy";
    }

    

  }
}
