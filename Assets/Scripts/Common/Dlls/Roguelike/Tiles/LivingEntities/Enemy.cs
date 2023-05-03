#define ASCII_BUILD  
using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.TileContainers;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
//using UnityEditor.Experimental.GraphView;
//using static UnityEngine.EventSystems.EventTrigger;

namespace Roguelike.Tiles.LivingEntities
{
  public enum RoomKind { Unset, PuzzleRoom, Island }
  public enum EnemyPowerKind { Unset, Plain, Champion, Boss };
  public enum PrefferedFightStyle { Physical, Magic, Distance }
  public enum IncreaseStatsKind { Level, PowerKind, Name, Difficulty, Ability }

  public class Enemy : LivingEntity
  {
    [JsonIgnore]
    public int RessurectOrdersCounter { get; set; }

    [JsonIgnore]
    public int RessurectOrderCooldown { get; set; }

    
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
    [JsonIgnore]
    public ILootSource LootSource { get; set; }
    
    public Enemy(Container cont) : this(new Point().Invalid(), 'e', cont)
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
      DestroySound = "death";
#if ASCII_BUILD
      color = ConsoleColor.Red;
#endif
      Alive = true;

      if (string.IsNullOrEmpty(Name) && symbol != EnemySymbols.CommonEnemySymbol)
        Name = NameFromSymbol(symbol);

      //if (symbol != EnemySymbols.CommonEnemySymbol)
      //  DeathEffectFromSymbol(symbol);

      AddFightItem(FightItemKind.Stone);
      var knife = AddFightItem(FightItemKind.ThrowingKnife);
      AddFightItem(FightItemKind.ExplosiveCocktail);
      AddFightItem(FightItemKind.PoisonCocktail);
      AddFightItem(FightItemKind.ThrowingTorch);
      //var net = AddFightItem(FightItemKind.WeightedNet);
      //fightItems[FightItemKind.HunterTrap] = new ProjectileFightItem(FightItemKind.HunterTrap, this) { Count = RandHelper.GetRandomInt(3) + 1 };

      SetActiveFightItem(RandHelper.GetRandomElem<FightItem>(this.fightItems.Values.ToList()).FightItemKind);
      //SetActiveFightItem(FightItemKind.ThrowingTorch);

      SetResist(EntityStatKind.ResistCold, 15);
      SetResist(EntityStatKind.ResistFire, 15);
      SetResist(EntityStatKind.ResistPoison, 15);
      SetResist(EntityStatKind.ResistLighting, 15);
    }

    public override void Consume(IConsumable consumable)
    {
      base.Consume(consumable);
      AlignMeleeAttack(); 
    }

    public override bool RemoveFightItem(FightItem fi)
    {
      if (HasFightItem(fi.FightItemKind))
      {
        fightItems[fi.FightItemKind].Count--;
        return true;
      }
      return false;
    }

    public void SendCommand(EntityCommandKind raiseMyFriends)
    {
      var ea = new EnemyAction();
      ea.Enemy = this;
      ea.CommandKind = EntityCommandKind.RaiseMyFriends;
      ea.Kind = EnemyActionKind.SendComand;
      ea.Info = "Raise my friends!";
      Container.GetInstance<EventsManager>().AppendAction(ea);
      RessurectOrderCooldown = 10;
      RessurectOrdersCounter++;
      var sm = Container.GetInstance<GameManager>().SoundManager;
      if (sm != null)
        sm.PlayVoice("raise_my_friends");
    }

    public override bool HasFightItem(FightItemKind fik)
    {
      return fightItems.ContainsKey(fik) && fightItems[fik].Count > 0;
    }

    public override FightItem GetFightItem(FightItemKind kind)
    {
      if (HasFightItem(kind))
        return fightItems[kind];

      return null;
    }

    public override float GetStartStat(EntityStatKind esk)
    {
      var startStat = base.GetStartStat(esk);
      if (esk == EntityStatKind.Strength)
        startStat += StrengthInc;

      return startStat;
    }

    public FightItemKind ActiveFightItemKind { get; set; }

    public ProjectileFightItem SetActiveFightItem(FightItemKind kind)
    {
      ActiveFightItemKind = kind;
      if (!fightItems.ContainsKey(kind))
        SetFightItem(new ProjectileFightItem(kind));
      return fightItems[kind] as ProjectileFightItem;
    }

    public override FightItem ActiveFightItem
    {
      get
      {
        if (ActiveFightItemKind == FightItemKind.Unset)
          return null;
        if(!fightItems.ContainsKey(ActiveFightItemKind))
          return null;
        return fightItems[ActiveFightItemKind];
      }
      set
      {
        var pfi = value as ProjectileFightItem;
        if (value == null)
          ActiveFightItemKind = FightItemKind.Unset;
        else
          ActiveFightItemKind = pfi.FightItemKind;
        
        SetFightItem(pfi);
      }
    }

    public ProjectileFightItem AddFightItem(FightItemKind kind)
    {
      SetFightItem(new ProjectileFightItem(kind, this) { Count = RandHelper.GetRandomInt(3) + 1 });
      return fightItems[kind] as ProjectileFightItem;
    }

    public void SetFightItem(ProjectileFightItem fi)
    {
      if (fi == null)
        return;
      fightItems[fi.FightItemKind] = fi;
    }

    protected bool WereStatsIncreased(IncreaseStatsKind kind)
    {
      if (StatsIncreased.ContainsKey(kind))
        return StatsIncreased[kind];

      return false;
    }

    bool specialNonplainCase;
        
    public virtual void SetNonPlain(bool boss, bool specialCase = false)
    {
      if (PowerKind != EnemyPowerKind.Plain)
      {
        //if(!specialNonplainCase && tag1 != "tree_monster_ch" && !Herd.Any())//TODO
          //AssertFalse("SetNonPlain Kind != PowerKind.Plain " + this);
        return;
      }
      specialNonplainCase = specialCase;
      NumberOfEmergencyTeleports = 2;
      PowerKind = boss ? EnemyPowerKind.Boss : EnemyPowerKind.Champion;
      if (PowerKind == EnemyPowerKind.Boss)
        Color = ConsoleColor.Magenta;
      else
        Color = ConsoleColor.Red;

      if (boss)
        DestroySound = "boss_death";
      else
        DestroySound = "chemp_death";

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
        //AssertFalse("inc == " + inc + " " + kind + ", increasing for second time? " + this);//TODO
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
      else if (Name.ToLower() == "lava elemental" ||
               Name.ToLower() == "stone golem")
      {
        ActiveManaPoweredSpellSource = new Scroll(SpellKind.FireStone);
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
              tag1.ToLower().Contains("golem") ||
              tag1.ToLower().Contains("tree_monster");
            
      }
    }

    protected override void InitStatsFromName()
    {
      if (tag1.Any() && !WereStatsIncreased(IncreaseStatsKind.Name))
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
        if ((!Name.Any() || Name == "Enemy") && value != EnemySymbols.Unset)
          Name = Enemy.NameFromSymbol(value);

        if(EntityKind == EntityKind.Unset)
          EntityKind = Enemy.EntityKindFromSymbol(Char.ToLower(value));
        SetSpecialAttackStatFromSymbol();
      }
    }

    void AddImmunityFromName()
    {
      if (name == "Skeleton" || name.Contains("Golem"))
        AddImmunity(EffectType.Bleeding);
      if (name.Contains("Fire") || name.Contains("Fire"))
        AddImmunity(EffectType.Firing);
    }

    static readonly string[] AnimalNames = new[] { "bear", "boar", "lynx", "worm", "wolverine" };
    static readonly string[] HumanNames = new[] { "bandit", "druid" };

    private static EntityKind SpeciesKindFromName(string name)
    {
      if (AnimalNames.Contains(name))
        return EntityKind.Animal;
      if (HumanNames.Contains(name))
        return EntityKind.Human;
      if (name.Contains("skeleton"))
      {
        return EntityKind.Undead;
      }

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

    public void SetSpecialAttackStatFromSymbol()
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
        return namePair.First().Key.ToUpperFirstLetter();
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
        var nam = value;
        if (nam.ToLower().StartsWith("bandit"))
        {
          nam = "Bandit";//remove digit
          DisplayedName = nam;
        }

        base.Name = nam;
        if (nam == "Enemy")
          return;
        var name = Name.ToLower();
        if (
          name.Contains("drowned")||
          name.Contains("otter")
          )
        {
          SetSurfaceSkillLevel(SurfaceKind.ShallowWater, 1);
          SetSurfaceSkillLevel(SurfaceKind.DeepWater, 1);
        }
        if (name.Contains("druid") || name.Contains("lava el"))
        {
          SetPrefferedFightStyle(PrefferedFightStyle.Magic);
        }
        if (name.Contains("hornet"))
          SetStat(EntityStatKind.PoisonAttack);

        if (EntityKind == EntityKind.Unset)
          EntityKind = SpeciesKindFromName(name);

        if (name == "wolf_skeleton")
          DisplayedName = "Wolf's Skeleton";

        if (IsBreakable(name))
        {
          DeathEffect = DeathEffect.BreakApart;
        }

        InitStatsFromName();
        AddImmunityFromName();
      }
    }

    public bool HitRandomTarget { get; internal set; }

    public void SetPrefferedFightStyle(PrefferedFightStyle pfs)
    {
      PrefferedFightStyle = pfs;
      if (pfs == PrefferedFightStyle.Magic)
      {
        this.Stats.SetNominal(EntityStatKind.Mana, LivingEntity.StartStatValues[EntityStatKind.Mana] * 100);
        this.color = ConsoleColor.Magenta;
      }
    }

    public static bool IsBreakable(string name)
    {
      return name == "lava_golem" || name == "skeleton" || name == "stone_golem";
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
      else if (Name.ToLower().Contains("lava el"))
      {
        return ActiveManaPoweredSpellSource;
      }
      return base.GetAttackingScroll();
    }

    public void InitFromTag(string tag, bool loadingGame)
    {
      if (!loadingGame)
      {
        tag1 = tag;

        if (tag.EndsWith("_ch"))
          SetNonPlain(false);
        else if (tag.EndsWith("_king"))
          SetNonPlain(true);

        SetNameFromTag1();
        var symbol = Roguelike.EnemySymbols.GetSymbolFromName(tag);
        if (symbol != Roguelike.EnemySymbols.Unset)
        {
          Symbol = symbol;
        }
      }
    }

    public override bool CalcShallMoveFaster(AbstractGameLevel node)
    {
      var canMoveFaster = false;
      var sks = node.GetSurfaceKindsUnderTile(this);
      if (sks.Any(i=> GetSurfaceSkillLevel(i) > 0))
        canMoveFaster = true;

      return canMoveFaster;
    }
    
    public bool CanFly { get; set; }

    [JsonIgnore]
    public bool EverSummonedChild { get; internal set; }

    public override bool HandleOrder(BattleOrder order, Hero hero, DungeonNode node)
    {
      if (order == BattleOrder)
      {
        if (this.DistanceFrom(hero) < 10 && CanFly)
        {
          Tile toUse = null;
          var empties = node.GetEmptyNeighborhoodTiles(hero, false);
          if (empties.Any())
            toUse = empties.First();
          else
            toUse = node.GetClosestEmpty(hero);
          if (toUse != null)
          {
            node.SetTile(this, toUse.point);
            BattleOrder = BattleOrder.Unset;
            
          //  EventsManager.AppendAction(new LivingEntityAction(kind: LivingEntityActionKind.Moved)
          //  {
          //    Info = Name + " moved",
          //    InvolvedEntity = this,
          //    MovePolicy = Container.GetInstance<MovePolicy>()
          //});
            return true;
          }
        }
      }

      return base.HandleOrder(order, hero, node);
    }
  }
}
