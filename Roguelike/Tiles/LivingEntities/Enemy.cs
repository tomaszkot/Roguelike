#define ASCII_BUILD  
using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Calculated;
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
  public enum IncreaseStatsKind { Level, PowerKind, Name, Difficulty, Ability }

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
    public const float StrengthInc = 2;//no matter what difficulty this is start value
    //FightItemKind fightItemKind = FightItemKind.Stone;

    public Enemy(Container cont) : this(new Point().Invalid(), 'e', null)
    {
    }

    public Enemy(char symbol, Container cont) : this(new Point().Invalid(), symbol, cont)
    {
    }

    public Enemy(Point point, char symbol, Container cont) : base(point, symbol, cont)
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

      AddFightItem(FightItemKind.Stone);
      var knife = AddFightItem(FightItemKind.ThrowingKnife);
      AddFightItem(FightItemKind.ExplosiveCocktail);
      AddFightItem(FightItemKind.PoisonCocktail);
      //var net = AddFightItem(FightItemKind.WeightedNet);
      //fightItems[FightItemKind.HunterTrap] = new ProjectileFightItem(FightItemKind.HunterTrap, this) { Count = RandHelper.GetRandomInt(3) + 1 };

      SetActiveFightItem(RandHelper.GetRandomElem<FightItem>(this.fightItems.Values.ToList()).FightItemKind);
      //SetActiveFightItem(knife.FightItemKind);

      SetResist(EntityStatKind.ResistCold, 15);
      SetResist(EntityStatKind.ResistFire, 15);
      SetResist(EntityStatKind.ResistPoison, 15);
      SetResist(EntityStatKind.ResistLighting, 15);
    }

    public override float GetStartStat(EntityStatKind esk)
    {
      var startStat = base.GetStartStat(esk);
      if (esk == EntityStatKind.Strength)
        startStat += StrengthInc;

      return startStat;
    }

    public ProjectileFightItem SetActiveFightItem(FightItemKind kind)
    {
      this.ActiveFightItem = fightItems[kind];
      return this.ActiveFightItem as ProjectileFightItem;
    }

    public ProjectileFightItem AddFightItem(FightItemKind kind)
    {
      fightItems[kind] = new ProjectileFightItem(kind, this) { Count = RandHelper.GetRandomInt(2) + 1 };
      return fightItems[kind] as ProjectileFightItem;
    }
        
    public override void RemoveFightItem(FightItem fi)
    {
      fightItems[fi.FightItemKind].Count--;
      EverUsedFightItem = true;
    }
        
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
      if (set)
      {
        foreach (var fi in this.fightItems)
        {
          if (fi.Value is ProjectileFightItem pfi)
          {
            int inc = 80;
            if (pfi.FightItemKind == FightItemKind.ExplosiveCocktail ||
                pfi.FightItemKind == FightItemKind.PoisonCocktail)
              inc = 60;
            pfi.baseDamage = Roguelike.Calculated.FactorCalculator.AddFactor(pfi.baseDamage, inc);
          }
        }
      }
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
        Stats.SetNominal(EntityStatKind.Mana, 10000);//TODO
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
        SetResist(esk);
      }
    }

    private void SetResist(EntityStatKind esk, int inc = 30)
    {
      var val = this.Stats.GetNominal(esk);
      val += 30;
      if (val > 75)
        val = 75;
      this.Stats.SetNominal(esk, val);
    }

    public bool IsStrongerThanAve
    {
      get 
      {
        return tag1.ToLower().Contains("bear") ||
              tag1.ToLower().Contains("demon") ||
              tag1.ToLower().Contains("tree_monster");
            
      }
    }

    protected override void InitStatsFromName()
    {
      if (!WereStatsIncreased(IncreaseStatsKind.Name))
      {
        if (IsStrongerThanAve)
        {
          IncreaseStats(1.5f, IncreaseStatsKind.Name);
        }
      }
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

        if(EntityKind == EntityKind.Unset)
          EntityKind = Enemy.EntityKindFromSymbol(value);
        SetSpecialAttackStat();
      }
    }

    static readonly string[] AnimalNames = new[] { "bear", "boar", "lynx", "worm", "wolverine" };

    private static EntityKind SpeciesKindFromName(string name)
    {
      if (AnimalNames.Contains(name))
        return EntityKind.Animal;

      return EntityKind.Unset;
    }

    private static EntityKind EntityKindFromSymbol(char value)
    {
      if (value == EnemySymbols.VampireSymbol ||
          value == EnemySymbols.MerchantBroSymbol ||
          value == EnemySymbols.WizardSymbol ||
          value == EnemySymbols.CommonEnemySymbol || 
          value == EnemySymbols.MorphSymbol ||
          value == EnemySymbols.TreantSymbol ||
          value == EnemySymbols.OgreSymbol ||
          value == EnemySymbols.ManEaterSymbol || 
          value == EnemySymbols.FallenOneSymbolPhantom ||
          value == EnemySymbols.QuestBoss
          )
      {
        return EntityKind.Unset;
      }
      else if(value == EnemySymbols.SkeletonSymbol || value == EnemySymbols.ZombieSymbol)
        return EntityKind.Undead;

      else if (value == EnemySymbols.FallenOneSymbol || value == EnemySymbols.DaemonSymbol)
        return EntityKind.Daemon;

      return EntityKind.Animal;
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
      var enemy = new Enemy(symbol, cont);
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
          this.Stats.SetNominal(EntityStatKind.Mana, LivingEntity.StartStatValues[EntityStatKind.Mana] * 100);
          this.color = ConsoleColor.Magenta;
        }
        if (name.Contains("hornet"))
          SetStat(EntityStatKind.PoisonAttack);

        if (EntityKind == EntityKind.Unset)
          EntityKind = SpeciesKindFromName(name);
      }
    }

    //public FightItemKind FightItemKind 
    //{ 
    //  get => fightItemKind; 
    //  set => fightItemKind = value; 
    //}

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
