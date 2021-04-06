#define ASCII_BUILD  
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;

namespace Roguelike.Tiles.LivingEntities
{
  public enum RoomKind { Unset, PuzzleRoom, Island }
  public enum EnemyPowerKind { Unset, Plain, Champion, Boss };
  public enum PrefferedFightStyle { Physical, Magic, Distance }
  public enum IncreaseStatsKind { Level, PowerKind, Name }

  public class Enemy : LivingEntity
  {
    public const string ChempTagSuffix = "_ch";
    public PrefferedFightStyle PrefferedFightStyle { get; set; }//= PrefferedFightStyle.Magic;

    public static readonly EntityStat BaseAttack = new EntityStat(EntityStatKind.Attack, 10);
    public static readonly EntityStat BaseHealth = new EntityStat(EntityStatKind.Health, 12);
    public static readonly EntityStat BaseDefence = new EntityStat(EntityStatKind.Defense, 5);
    public static readonly EntityStat BaseMana = new EntityStat(EntityStatKind.Mana, 10);
    public static readonly EntityStat BaseMagic = new EntityStat(EntityStatKind.Magic, 10);

    public static readonly EntityStats BaseStats;
    bool levelSet = false;
    public static char[] AllSymbols;

    public int NumberOfCastedEffectsForAllies = 0;
    public int NumberOfEmergencyTeleports = 0;
    public EnemyPowerKind PowerKind { get; set; } = EnemyPowerKind.Plain;
    public bool LevelSet { get => levelSet; set => levelSet = value; }
    public Dictionary<IncreaseStatsKind, bool> StatsIncreased { get; set; } = new Dictionary<IncreaseStatsKind, bool>();
    public Loot DeathLoot { get; set; }

    public bool ShoutedAtHero { get; set; }

    static Enemy()
    {
      BaseStats = new EntityStats();

      BaseStats.SetStat(EntityStatKind.Attack, BaseAttack);
      BaseStats.SetStat(EntityStatKind.Defense, BaseDefence);
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
      //IsWounded = true;
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

      if (string.IsNullOrEmpty(Name) && symbol != EnemySymbols.CommonEnemySymbol)
        Name = NameFromSymbol(symbol);
      //Stats.Experience = 1;
      //kind = PowerKind.Plain;
      //Name = "Enemy";
    }

    protected bool WereStatsIncreased(IncreaseStatsKind kind)
    { 
      if(StatsIncreased.ContainsKey(kind))
        return StatsIncreased[kind];

      return false;
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
        Symbol = (char)((int)Symbol - 32);
      }
      float inc = boss ? 2.5f : 1.8f;
      IncreaseStats(inc, IncreaseStatsKind.PowerKind);

      InitEffectsToUse(boss);
      InitActiveScroll();

      if(!boss)
        this.tag1 += ChempTagSuffix;
    }

    private void InitEffectsToUse(bool boss)
    {
      int effectsToUseCount = boss ? 2 : 1;
      var keys = effectsToUse.Keys.ToList();
      foreach (var ef in keys)
      {
        effectsToUse[ef] = effectsToUseCount;
      }
      //too many effects, let's delete random one
      if (RandHelper.Random.NextDouble() > .5)
        effectsToUse.Remove(EffectType.Weaken);
      else
        effectsToUse.Remove(EffectType.IronSkin);
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

    public override void SetLevel(int level)
    {
      Assert(level >= 1);
      if (level > 6)
      {
        int k = 0;
        k++;
      }
      base.SetLevel(level);
      
      if (!WereStatsIncreased(IncreaseStatsKind.Name))
        UpdateStatsFromName();

      var hard = false;// GameManager.Instance.GameSettings.DifficultyLevel == Commons.GameSettings.Difficulty.Hard;
      var inc = GetIncrease(hard ? level + 1 : level);
      IncreaseStats(inc, IncreaseStatsKind.Level);
      SetResistance();
      InitActiveScroll();
      LevelSet = true;
    }

    static List<SpellKind> attackSpells = new List<SpellKind>()
    {
      SpellKind.FireBall, SpellKind.IceBall, SpellKind.PoisonBall
    };

    private void InitActiveScroll()
    {
      //ActiveScroll = new Scroll(SpellKind.IceBall);
      if (Name.ToLower() == "druid" || PowerKind != EnemyPowerKind.Plain)
      {
        ActiveScroll = new Scroll(attackSpells.GetRandomElem());
        SetResistanceFromScroll(ActiveScroll);
        Stats.SetNominal(EntityStatKind.Mana, 1000);//TODO
      }
    }

    void SetResistance()
    {
      float resistBasePercentage = 5 * GetIncrease(this.Level, 3f);
      var incPerc = GetResistanceLevelFactor(this.Level);
      resistBasePercentage += resistBasePercentage * incPerc / 100;

      //if (PlainSymbol == GolemSymbol || PlainSymbol == VampireSymbol || PlainSymbol == WizardSymbol
      //  || kind != PowerKind.Plain)
      //{
      //  resistBasePercentage += 14;
      //}
      this.Stats.SetNominal(EntityStatKind.ResistFire, resistBasePercentage);
      this.Stats.SetNominal(EntityStatKind.ResistPoison, resistBasePercentage);
      this.Stats.SetNominal(EntityStatKind.ResistCold, resistBasePercentage);
      var rli = resistBasePercentage * 2.5f / 3f;
      this.Stats.SetNominal(EntityStatKind.ResistLighting, rli);
    }

        
    private void SetResistanceFromScroll(Scroll activeScroll)
    {
      if (activeScroll == null)
        return;
      var esk = EntityStatKind.Unset;
      if (activeScroll.Kind == SpellKind.FireBall)
        esk = EntityStatKind.ResistFire;
      else if (activeScroll.Kind == SpellKind.PoisonBall)
        esk = EntityStatKind.ResistPoison;
      else if (activeScroll.Kind == SpellKind.IceBall)
        esk = EntityStatKind.ResistCold;

      if (esk != EntityStatKind.Unset)
      {
        var val = this.Stats.GetNominal(esk);
        val += 30;
        if (val > 75)
          val = 75;
        this.Stats.SetNominal(esk, val);
      }
    }

    public static float GetResistanceLevelFactor(int level)
    {
      //TODO
      return (level +1)* 10;
      //if (!ResistanceFactors.Any())
      //{
      //  for (int i = 0; i <= GameManager.MaxLevelIndex; i++)
      //  {
      //    double inp = ((double)i) / GameManager.MaxLevelIndex;
      //    float incPerc = (float)Sigmoid(inp);
      //    ////Debug.WriteLine(i.ToString() + ") ResistanceLevelFactor = " + fac);
      //    ResistanceFactors.Add(incPerc);
      //  }
      //}
      //if (level >= ResistanceFactors.Count)
      //  return 0;

      //return ResistanceFactors[GameManager.MaxLevelIndex - level] * 20;
    }

    public float EnemyStatsIncreasePerLevel = .31f;
    
    private float GetIncrease(int level, float factor = 1)
    {
      return 1 + (level * EnemyStatsIncreasePerLevel * factor);
    }

    protected void UpdateStatsFromName()
    {
      if (tag1.ToLower().Contains("bear") ||
          tag1.ToLower().Contains("demon"))
      {
        IncreaseStats(1.5f, IncreaseStatsKind.Name);
      }
      //else if (tag1.ToLower().Contains("bear"))
      //{
      //  IncreaseStats(1.5f, IncreaseStatsKind.Name);
      //}
    }

    protected void IncreaseStats(float inc, IncreaseStatsKind kind)
    {
      var wereInc = WereStatsIncreased(kind);
      if (wereInc)
      {
        AssertFalse("inc == " + inc + " " + kind + ", increasing for second time?");
        return;
      }
      if (inc == 0)
      {
        AssertFalse("inc == 0 PowerKind =" + PowerKind + " fromPowerKind="+ kind);
        inc = 1.2f;
      }

      foreach (var kv in Stats.GetStats())
      {
        var incToUse = inc;
        var val = kv.Value.Value.TotalValue * incToUse;//TODO TotalValue ? -> SetNominal ?
        Stats.SetNominal(kv.Key, val);
      }

      StatsIncreased[kind] = true;
    }

    public override string ToString()
    {
      return base.ToString() + " " + PowerKind + "";
    }

    public override char Symbol 
    { 
      get => base.Symbol;
      set
      {
        base.Symbol = value;
        if(!Name.Any() && value != EnemySymbols.Unset)
          Name = Enemy.NameFromSymbol(value);
        SetSpecialAttackStat();
      }
    }

    private void SetSpecialAttackStat()
    {
      if (Symbol == EnemySymbols.SnakeSymbol ||
                 Symbol == EnemySymbols.SpiderSymbol)
      {
        var poisonAttack = Stats.GetStat(EntityStatKind.PoisonAttack);
        poisonAttack.Value.Nominal = 2;
      }
    }

    public static string NameFromSymbol(char symbol)
    {
      var namePair = EnemySymbols.EnemiesToSymbols.Where(i => i.Value == symbol);
      if (namePair.Any())
      {
        return namePair.First().Key;
      }
      return "";
    }

    public static Enemy Spawn(char symbol, int level)
    {
      var enemy = new Enemy(symbol);
      enemy.SetLevel(level);
      enemy.tag1 = EnemySymbols.EnemiesToSymbols.Where(i => i.Value == symbol).Single().Key;
      enemy.Revealed = true;
      
      return enemy;
    }

    public override string Name 
    { 
      get => base.Name;
      set
      {
        base.Name = value;
        if (Name.ToLower() == "drowned man")
        {
          SetSurfaceSkillLevel(SurfaceKind.ShallowWater, 1);
          SetSurfaceSkillLevel(SurfaceKind.DeepWater, 1);
        }
        if (Name.ToLower().Contains("druid"))
        {
          PrefferedFightStyle = PrefferedFightStyle.Magic;
          this.Stats.SetNominal(EntityStatKind.Mana, BaseMana.Value.TotalValue * 100);
          this.color = ConsoleColor.Magenta;
        }
      }
    }

    public override Scroll GetAttackingScroll()
    {
      if (Name.ToLower().Contains("druid"))
      {
        return ActiveScroll;
      }
      return base.GetAttackingScroll();
    }
  }
}
