#define ASCII_BUILD  
using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Spells;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Roguelike.Tiles.LivingEntities
{
  public enum RoomKind { Unset, PuzzleRoom, Island }
  public enum EnemyPowerKind { Unset, Plain, Champion, Boss };
  public enum PrefferedFightStyle { Physical, Magic, Distance }
  public enum IncreaseStatsKind { Level, PowerKind, Name, Difficulty }

  public class Enemy : LivingEntity
  {
    bool LevelSet { get; set; }
    public const string ChempTagSuffix = "_ch";
    public PrefferedFightStyle PrefferedFightStyle { get; set; }//= PrefferedFightStyle.Magic;
    public static char[] AllSymbols;

    public int NumberOfCastedEffectsForAllies = 0;
    public int NumberOfEmergencyTeleports = 0;
    public EnemyPowerKind PowerKind { get; set; } = EnemyPowerKind.Plain;

    public Dictionary<IncreaseStatsKind, bool> StatsIncreased { get; set; } = new Dictionary<IncreaseStatsKind, bool>();
    public Loot DeathLoot { get; set; }
    public bool ShoutedAtHero { get; set; }
    public Dictionary<FightItemKind, FightItem> fightItems = new Dictionary<FightItemKind, FightItem>();
    FightItemKind fightItemKind = FightItemKind.Stone;

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

      if (string.IsNullOrEmpty(Name) && symbol != EnemySymbols.CommonEnemySymbol)
        Name = NameFromSymbol(symbol);

      fightItems[FightItemKind.Stone] = new ProjectileFightItem(FightItemKind.Stone, this) { Count = RandHelper.GetRandomInt(3)+1 };
      fightItems[FightItemKind.ThrowingKnife] = new ProjectileFightItem(FightItemKind.ThrowingKnife, this) { Count = RandHelper.GetRandomInt(3)+1 };
      fightItems[FightItemKind.ExplosiveCocktail] = new ProjectileFightItem(FightItemKind.ExplosiveCocktail, this) { Count = RandHelper.GetRandomInt(3) + 1 };

      fightItemKind = RandHelper.GetRandomEnumValue<FightItemKind>();// (new[] { FightItemKind.Unset, FightItemKind.Trap });
    }

    //internal void RemoveFightItem(FightItemKind kind)
    //{
    //  fightItems[kind].Count--;
    //}

    public override void RemoveFightItem(FightItem fi)
    {
      fightItems[fi.FightItemKind].Count--;
    }

    //public FightItemKind GetAvaiableFightItemKind()
    //{
    //  //fightItems.Where(i=>i.Value.Count > 0).
    //  //RandHelper.GEt
    //}

    public FightItem GetFightItem(FightItemKind kind)
    {
      if (fightItems.ContainsKey(kind) && fightItems[kind].Count > 0)
        return fightItems[kind];

      return null;
    }

    protected bool WereStatsIncreased(IncreaseStatsKind kind)
    {
      if (StatsIncreased.ContainsKey(kind))
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

      if (!boss && !tag1.EndsWith(ChempTagSuffix))
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

    protected override void IncreaseStats(float inc, IncreaseStatsKind kind)
    {
      var wereInc = WereStatsIncreased(kind);
      if (wereInc)
      {
        AssertFalse("inc == " + inc + " " + kind + ", increasing for second time?");
        return;
      }
      if (inc == 0)
      {
        AssertFalse("inc == 0 PowerKind =" + PowerKind + " fromPowerKind=" + kind);
        inc = 1.2f;
      }
      base.IncreaseStats(inc, kind);

      StatsIncreased[kind] = true;
    }

    public override bool SetLevel(int level, Difficulty? diff = null)
    {
      var set = base.SetLevel(level, diff);
      LevelSet = set;
      return set;
    }

    protected override bool CanIncreaseLevel()
    {
      return Level == 1 || !LevelSet;
    }

    static List<SpellKind> attackSpells = new List<SpellKind>()
    {
      SpellKind.FireBall, SpellKind.IceBall, SpellKind.PoisonBall
    };

    protected override void InitActiveScroll()
    {
      //ActiveScroll = new Scroll(SpellKind.IceBall);
      if (Name.ToLower() == "druid" || PowerKind == EnemyPowerKind.Boss)
      {
        ActiveManaPoweredSpellSource = new Scroll(attackSpells.GetRandomElem());
        SetResistanceFromScroll(ActiveManaPoweredSpellSource);
        Stats.SetNominal(EntityStatKind.Mana, 1000);//TODO
      }
    }

    private void SetResistanceFromScroll(SpellSource activeScroll)
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

    protected override void InitStatsFromName()
    {
      if (!WereStatsIncreased(IncreaseStatsKind.Name))
      {
        if (tag1.ToLower().Contains("bear") ||
            tag1.ToLower().Contains("demon") ||
            tag1.ToLower().Contains("tree_monster")
            )
        {
          IncreaseStats(1.5f, IncreaseStatsKind.Name);
        }
      }
      //else if (tag1.ToLower().Contains("bear"))
      //{
      //  IncreaseStats(1.5f, IncreaseStatsKind.Name);
      //}
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
        if (!Name.Any() && value != EnemySymbols.Unset)
          Name = Enemy.NameFromSymbol(value);
        SetSpecialAttackStat();
      }
    }

    public void SetSpecialAttackStat()
    {
      if (Symbol == EnemySymbols.SnakeSymbol ||
         Symbol == EnemySymbols.SpiderSymbol
         //hornet has it done in : public override string Name
         )
      {
        SetStat(EntityStatKind.PoisonAttack);
      }
    }

    private void SetStat(EntityStatKind esk)
    {
      var att = Stats.GetStat(esk);
      att.Value.Nominal = 1.5f;
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

    public static Enemy Spawn(char symbol, int level, Container cont, Difficulty? difficulty = null)
    {
      var enemy = new Enemy(symbol);
      enemy.Container = cont;
      enemy.SetLevel(level, difficulty);
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
        var name = Name.ToLower();
        if (
          name.Contains("drowned")||
          name.Contains("otter")
          )
        {
          SetSurfaceSkillLevel(SurfaceKind.ShallowWater, 1);
          SetSurfaceSkillLevel(SurfaceKind.DeepWater, 1);
        }
        if (name.Contains("druid"))
        {
          PrefferedFightStyle = PrefferedFightStyle.Magic;
          this.Stats.SetNominal(EntityStatKind.Mana, BaseMana.Value.TotalValue * 100);
          this.color = ConsoleColor.Magenta;
        }
        if (name.Contains("hornet"))
          SetStat(EntityStatKind.PoisonAttack);
                
      }
    }

    public FightItemKind FightItemKind { get => fightItemKind; set => fightItemKind = value; }

    public override SpellSource GetAttackingScroll()
    {
      if (Name.ToLower().Contains("druid"))
      {
        return ActiveManaPoweredSpellSource;
      }
      return base.GetAttackingScroll();
    }

  }
}
