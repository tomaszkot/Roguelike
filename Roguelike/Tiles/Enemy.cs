#define ASCII_BUILD  
using System;
using System.Drawing;
using Dungeons.Core;
using Roguelike.Attributes;

namespace Roguelike.Tiles
{
  public enum RoomKind { Unset, PuzzleRoom, Island }
  public enum EnemyPowerKind { Unset, Plain, Champion, Boss };
  public enum PrefferedFightStyle { Physical, Magic, Distance }

  public class Enemy : LivingEntity
  {
    //public int Level { get; set; } = 1;
    public PrefferedFightStyle PrefferedFightStyle { get; set; }//= PrefferedFightStyle.Magic;

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

      BaseStats.SetStat(EntityStatKind.Attack, BaseAttack);
      BaseStats.SetStat(EntityStatKind.Defence, BaseDefence);
      BaseStats.SetStat(EntityStatKind.Health, BaseHealth);
      BaseStats.SetStat(EntityStatKind.Mana,  BaseMana);
      var mag = new EntityStat(EntityStatKind.Magic, BaseMagic.Value.Nominal + 2);
      BaseStats.SetStat(EntityStatKind.Magic, mag);
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
        var nv = BaseStats[basicStats].Nominal;
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
        Color = ConsoleColor.Magenta;
      else
        Color = ConsoleColor.Red;

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

      foreach (var kv in Stats.GetStats())
      {
        var incToUse = inc;
        var val = kv.Value.Value.TotalValue * incToUse;//TODO TotalValue ? -> SetNominal ?
        Stats.SetNominal(kv.Key, val);
      }

      if (fromPowerKind)
        StatsIncreasedByPowerKind = true;
      else
        StatsIncreasedByLevel = true;

    }

    public override string ToString()
    {
      return base.ToString() + " " + PowerKind;
    }

  }
}
