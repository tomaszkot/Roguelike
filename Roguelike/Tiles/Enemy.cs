#define ASCII_BUILD  
using System.Collections.Generic;
using System.Linq;
using System;
using System.Drawing;
using Dungeons.Core;

using Newtonsoft.Json;
using Roguelike.Attributes;

namespace Roguelike.Tiles
{
  public enum RoomKind { Unset, PuzzleRoom, Island }
  public enum EnemyPowerKind { Unset, Plain, Champion, Boss };

  public class Enemy : LivingEntity
  {
    public int Level { get; set; } = 1;

    public static readonly EntityStat BaseAttack = new EntityStat(EntityStatKind.Attack, 13f);
    public static readonly EntityStat BaseHealth = new EntityStat(EntityStatKind.Health, 13);
    public static readonly EntityStat BaseDefence = new EntityStat(EntityStatKind.Defence, 5);
    public static readonly EntityStat BaseMana = new EntityStat(EntityStatKind.Mana, 4);
    public static readonly EntityStat BaseMagic = new EntityStat(EntityStatKind.Magic, 10);

    public static readonly EntityStats BaseStats;
    bool levelSet = false;
    public static char[] AllSymbols;

    // public int NumberOfCastedEffectsForAllies = 0;
    public int NumberOfEmergencyTeleports = 0;
    public EnemyPowerKind PowerKind { get; set; } = EnemyPowerKind.Plain;
    public bool LevelSet { get => levelSet; set => levelSet = value; }
    public bool StatsIncreasedByPowerKind { get => statsIncreased; set => statsIncreased = value; }
    public bool StatsIncreasedByLevel { get; set; }

    static Enemy()
    {
      BaseStats = new EntityStats();

      BaseStats.Stats[EntityStatKind.Attack] = BaseAttack;
      BaseStats.Stats[EntityStatKind.Defence] = BaseDefence;
      BaseStats.Stats[EntityStatKind.Health] = BaseHealth;
      BaseStats.Stats[EntityStatKind.Mana] = BaseMana;
      var mag = new EntityStat(EntityStatKind.Magic, BaseMagic.Value.Nominal + 2);
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
        var nv = BaseStats.Stats[basicStats].Value.Nominal;
        Stats.SetNominal(basicStats, nv);
      }

      //Stats.Experience = 1;
      //kind = PowerKind.Plain;
      Name = "Enemy";
    }

    public void SetChampion()
    {
      SetNonPlain(false);
    }

    public void SetBoss()
    {
      SetNonPlain(true);
    }

    public virtual void SetNonPlain(bool boss)
    {
      if (PowerKind != EnemyPowerKind.Plain)
      {
        AssertFalse("SetNonPlain Kind != PowerKind.Plain " + this);
        return;
      }
      PowerKind = boss ? EnemyPowerKind.Boss : EnemyPowerKind.Champion;
      if (PowerKind == EnemyPowerKind.Boss)
        Color = ConsoleColor.Yellow;
      else
        Color = ConsoleColor.DarkRed;

      if (Symbol >= 'a' && Symbol <= 'z')
      {
        Symbol = (char)((int)Symbol-32);
      }
      float inc = boss ? 1.4f : 1.2f;
      IncreaseStats(inc, true);
    }

    protected void Assert(bool cond)
    {
      if (!cond)
      {
        AssertFalse("assert failed!");
      }
    }

    protected void AssertFalse(string info)
    {
      AppendAction(new Roguelike.Events.GameStateAction()
      {
        Type = Roguelike.Events.GameStateAction.ActionType.Assert,
        Info = info
      });
    }

    public void SetLevel(int level)
    {
      Assert(level >= 1);
      this.Level = level;

      var hard = false;// GameManager.Instance.GameSettings.DifficultyLevel == Commons.GameSettings.Difficulty.Hard;
      var inc = GetIncrease(hard ? level + 1 : level);
      IncreaseStats(inc, false);
      //SetResistance();
      LevelSet = true;


    }

    public float EnemyStatsIncreasePerLevel = .31f;
    private bool statsIncreased;

    private float GetIncrease(int level, float factor = 1)
    {
      return 1 + (level * EnemyStatsIncreasePerLevel * factor);
    }

    private void IncreaseStats(float inc, bool fromPowerKind)
    {
      if ((fromPowerKind && StatsIncreasedByPowerKind)
          ||
          (!fromPowerKind && StatsIncreasedByLevel))
      {
        AssertFalse("inc == " + inc + " PowerKind =" + PowerKind + " StatsIncreasedByPowerKind=" + StatsIncreasedByPowerKind);
        return;
      }
      if (inc == 0)
      {
        AssertFalse("inc == 0 PowerKind =" + PowerKind + " fromPowerKind="+ fromPowerKind);
        inc = 1.2f;
      }

      foreach (var key in Stats.Stats.Keys)
      {
        var incToUse = inc;
        var val = Stats.Stats[key].Value.TotalValue * incToUse;
        Stats.SetNominal(key, val);
      }

      if (fromPowerKind)
        StatsIncreasedByPowerKind = true;
      else
        StatsIncreasedByLevel = true;

    }


  }
}
