using Dungeons.Core;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Abilities;
using Roguelike.Abstract.Multimedia;
using Roguelike.Attributes;
using Roguelike.Managers.Policies;
using Roguelike.Events;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Multimedia;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Core.Utils;
using RoguelikeUnitTests.Helpers;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Dungeons.Tiles.Abstract;
using System.Globalization;
using Roguelike.Abstract;
using System.Xml.Linq;

namespace RoguelikeUnitTests
{

  public interface ITestBase
  {
    void GotoNextHeroTurn();
  }

  [TestFixture]
  public class TestBase : ITestBase
  {
    protected int createTestEnvCounter = 0;
    protected RoguelikeGame game;

    public RoguelikeGame Game { get => game; protected set => game = value; }
    public Container Container { get; set; }
    public BaseHelper Helper { get => helper; set => helper = value; }
    public GameManager GameManager { get => game.GameManager; }

    protected BaseHelper helper;

    [SetUp]
    public void Init()
    {
      CultureInfo.CurrentCulture = new CultureInfo("en-GB");//en-GB
      OnInit();

      Log("--");
      Log("Test SetUp: " + NUnit.Framework.TestContext.CurrentContext.Test.Name);
      Log("--");
      var gi = new GenerationInfo();
      Assert.Greater(gi.NumberOfRooms, 1);
      Assert.Greater(gi.ForcedNumberOfEnemiesInRoom, 2);

      
    }

    [TearDown]
    public void Cleanup()
    {
      Log("--");
      Log("Test Cleanup: " + TestContext.CurrentContext.Test.Name);
      Log("--");
      Log("-");
    }

    protected void IncreaseSpell(Roguelike.Tiles.LivingEntities.Hero hero, SpellKind sk)
    {
      hero.LevelUpPoints = 2;
      hero.IncreaseStatByLevelUpPoint(EntityStatKind.Magic);
      hero.IncreaseStatByLevelUpPoint(EntityStatKind.Magic);
      Assert.True(hero.IncreaseSpell(sk));
    }

    protected void Log(string v)
    {
      Container.GetInstance<ILogger>().LogInfo(v);
      //Debug.WriteLine(v);
    }

    protected Ally AddAlly(Hero hero, bool skeleton, bool second = false)
    {
      var am = GameManager.AlliesManager;
      //Assert.AreEqual(am.AllEntities.Count, second ? 1 : 0);

      if (skeleton)
      {
        var scroll = new Scroll(SpellKind.Skeleton);
        hero.Inventory.Add(scroll);
        Assert.NotNull(GameManager.SpellManager.ApplySpell(hero, scroll));
      }
      else
      {
        var hound = Container.GetInstance<Roguelike.Tiles.LivingEntities.TrainedHound>();
        GameManager.AddAlly(hound);
      }

      //Assert.AreEqual(am.AllEntities.Count, second ? 2 : 1);
      var ally = am.AllAllies.Last() as Ally;
      Assert.AreEqual(ally.Name, skeleton ? "Skeleton" : "Hound");
      Assert.Less(ally.DistanceFrom(hero), 5);
      return ally as Ally;
    }

    protected Enemy SpawnEnemy(char symbol = 'b')
    {
      Enemy en = null;
      if (game != null)
        en = game.GameManager.CurrentNode.SpawnEnemy(1);
      en = Container.GetInstance<Enemy>();
      en.Symbol = symbol;
      return en;
    }

    protected void HeroUseWeaponElementalProjectile( IDestroyable victim)
    {
      var wpn = game.GameManager.LootGenerator.GetLootByAsset("staff") as Weapon;
      SetHeroEquipment(wpn);
      var weapon = game.Hero.GetActiveWeapon();
      Assert.AreEqual(weapon.SpellSource.Kind, SpellKind.FireBall);
      Assert.AreEqual(game.GameManager.SpellManager.ApplyAttackPolicy(game.Hero, victim, weapon.SpellSource), ApplyAttackPolicyResult.OK);
    }

    protected virtual void OnInit()
    {
      Tile.IncludeDebugDetailsInToString = true;
      Container = new Roguelike.ContainerConfigurator().Container;
      Container.Register<ISoundPlayer, BasicSoundPlayer>();
      Container.GetInstance<ILogger>().LogLevel = LogLevel.Info;
    }

    protected T GenerateRandomEqOnLevelAndCollectIt<T>() where T : Equipment, new()
    {
      var eq = GenerateRandomEqOnLevel<T>();
      CollectLoot(eq);
      return eq;
    }

    protected void CollectLoot(Loot loot)
    {
      var set = Game.GameManager.CurrentNode.SetTile(game.Hero, loot.point);
      game.GameManager.CollectLootOnHeroPosition();
    }

    protected T GenerateRandomEqOnLevel<T>() where T : Equipment, new()
    {
      T eq = null;
      EquipmentKind kind = EquipmentKind.Unset;
      if (typeof(T) == typeof(Weapon))
        kind = EquipmentKind.Weapon;

      if (kind == EquipmentKind.Weapon)
        eq = game.GameManager.GenerateRandomEquipment(EquipmentKind.Weapon) as T;

      PutLootOnLevel(eq);
      return eq;
    }

    public void PutLootOnLevel(Loot loot)
    {
      if (loot != null)
      {
        var tile = Game.GameManager.CurrentNode.SetTileAtRandomPosition(loot, emptyCheckContext: Dungeons.TileContainers.DungeonNode.EmptyCheckContext.DropLoot);
        Assert.AreEqual(loot, tile);
      }
    }

    public void PutEqOnLevelAndCollectIt(Loot eq)
    {
      PutLootOnLevel(eq);
      CollectLoot(eq);
      GotoNextHeroTurn();
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> ActivePlainEnemies
    {
      get { return ActiveEnemies.Where(i => i.PowerKind == EnemyPowerKind.Plain).ToList(); }
    }


    List<Roguelike.Tiles.LivingEntities.Enemy> ActiveEnemies
    {
      get { return game.GameManager.EnemiesManager.GetActiveEnemies().Where(i=> i is Enemy).Cast<Enemy>().ToList(); }
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> AllEnemies
    {
      get { return game.GameManager.EnemiesManager.AllEntities.Cast<Enemy>().ToList(); }
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> PlainEnemies
    {
      get { return game.GameManager.EnemiesManager.AllEntities.Cast<Enemy>().Where(i => i.PowerKind == EnemyPowerKind.Plain).ToList(); }
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> PlainNormalEnemies
    {
      get { return PlainEnemies.Where(i => !i.IsStrongerThanAve).ToList(); }
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> ChampionEnemies
    {
      get { return game.GameManager.EnemiesManager.AllEntities.Cast<Enemy>().Where(i => i.PowerKind == EnemyPowerKind.Champion).ToList(); }
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> ChampionNormalEnemies
    {
      get { return ChampionEnemies.Where(i => !i.IsStrongerThanAve).ToList(); }
    }

    protected List<Roguelike.Tiles.LivingEntities.Enemy> BossEnemies
    {
      get { return game.GameManager.EnemiesManager.AllEntities.Cast<Enemy>().Where(i => i.PowerKind == EnemyPowerKind.Boss).ToList(); }
    }

    public List<Enemy> GetLimitedEnemies()
    {
      return game.GameManager.CurrentNode.GetTiles<Enemy>().Take(numEnemies).ToList();
    }

    protected void EnsureUniqNames(Enemy enFirst, Enemy enSec)
    {
      if (enFirst.Name == enSec.Name)
      {
        var newName = "Skeleton";
        if (enFirst.Name == "Skeleton")
          newName = "Spider";

        enSec.Name = newName;
      }
    }

    int numEnemies = 0;
    public virtual RoguelikeGame CreateGame
    (
      bool autoLoadLevel = true,
      int? genNumOfEnemies = null,
      int numberOfRooms = 5,
      GenerationInfo gi = null,
      LogLevel logLevel = LogLevel.Info
    )
    {
      Container.GetInstance<ILogger>().LogLevel = logLevel;
      var genNumOfEnemiesToUse = genNumOfEnemies ?? 20;
      if (createTestEnvCounter > 0)
      {
        OnInit();
      }
      Roguelike.Generators.GenerationInfo.DefaultMaxLevelIndex = 1;
      game = new RoguelikeGame(Container);

      game.GameManager.EventsManager.EventAppended += (object sender, Roguelike.Events.GameEvent e) =>
      {
        if (e is GameStateAction)
        {
          var gsa = e as GameStateAction;
          if (gsa.Type == GameStateAction.ActionType.Assert)
            game.GameManager.Logger.LogError(new System.Exception(gsa.Info));
        }
      };

      helper = new BaseHelper(this, game);
      if (autoLoadLevel)
      {
        if (gi == null)
        {
          var info = new GenerationInfo();
          info.MinNodeSize = new System.Drawing.Size(30, 30);
          info.MaxNodeSize = info.MinNodeSize;
          this.numEnemies = genNumOfEnemiesToUse;
          if (numberOfRooms == 1)
            info.PreventSecretRoomGeneration = true;

          info.NumberOfRooms = numberOfRooms;
          info.ForcedDungeonLayouterKind = Dungeons.DungeonLayouterKind.Default;
          gi = info;
        }

        if (genNumOfEnemiesToUse > 1)
        {
          float numEn = ((float)genNumOfEnemiesToUse) / numberOfRooms;
          gi.ForcedNumberOfEnemiesInRoom = (int)(numEn + 0.5);

          if (gi.ForcedNumberOfEnemiesInRoom == 0)
          {
            gi.ForcedNumberOfEnemiesInRoom = genNumOfEnemiesToUse % numberOfRooms;
            genNumOfEnemiesToUse = genNumOfEnemiesToUse * numberOfRooms;
          }
        }
        game.GameManager.GameState.CoreInfo.Difficulty = GenerationInfo.Difficulty;
        var level = game.GenerateLevel(0, gi);

        var enemies = level.GetTiles<Enemy>();
        if (genNumOfEnemies.HasValue && enemies.Count > genNumOfEnemiesToUse)
        {
          var aboveThreshold = enemies.Skip(genNumOfEnemiesToUse).ToList();
          foreach (var en in aboveThreshold)
          {
            level.SetEmptyTile(en.point);
            GameManager.EnemiesManager.AllEntities.Remove(en);
          }
        }
        enemies = level.GetTiles<Enemy>();

        if (genNumOfEnemies.HasValue && genNumOfEnemies.Value > 0)
        {
          Assert.LessOrEqual(enemies.Count, genNumOfEnemiesToUse);
          Assert.GreaterOrEqual(AllEnemies.Count, genNumOfEnemiesToUse);//some are auto generated
          Assert.Less(ActiveEnemies.Count, genNumOfEnemiesToUse * 4);
        }

        RevealAllEnemies();
        //if (AllEnemies.Any() && !AllEnemies.Where(i => i.Revealed).Any())
        //{
        //  AllEnemies[0].Revealed = true;
        //}
      }
      createTestEnvCounter++;
      if (game.Hero != null)
        game.Hero.Name = "Hero";
      return game;
    }

    protected void RevealAllEnemies()
    {
      AllEnemies.ForEach(i => i.Revealed = true);
    }

    protected Jewellery AddJewelleryToInv(Roguelike.RoguelikeGame game, EntityStatKind statKind)
    {
      Jewellery juw = GenerateJewellery(game, statKind);

      AddItemToInv(juw);
      return juw;
    }

    public Jewellery GenerateJewellery(RoguelikeGame game, EntityStatKind statKind)
    {
      var juw = game.GameManager.LootGenerator.LootFactory.EquipmentFactory.GetRandomJewellery(statKind);
      Assert.AreEqual(juw.PrimaryStatKind, EntityStatKind.Defense);
      Assert.IsTrue(juw.PrimaryStatValue > 0);
      return juw;
    }

    protected void AddItemToInv(Loot loot)
    {
      game.Hero.Inventory.Add(loot);
      Assert.IsTrue(game.Hero.Inventory.Contains(loot));
    }

    

    public void InteractHeroWith(InteractiveTile tile)
    {
      Game.GameManager.InteractHeroWith(tile);
      GotoNextHeroTurn();
    }

    public void InteractHeroWith(Enemy tile)
    {
      Game.GameManager.InteractHeroWith(tile);
      GotoNextHeroTurn();
    }

    protected void SkipTurns(int number)
    {
      for (int i = 0; i < number; i++)
        GotoNextHeroTurn();
    }

    public void GotoNextHeroTurn()
    {
      GotoNextHeroTurn(game.GameManager);
    }

    public void GotoNextHeroTurn(GameManager gm)
    {
      if (gm == null)
        gm = game.GameManager;

      if (gm.Context.TurnOwner == TurnOwner.Hero)
        gm.SkipHeroTurn();
      Assert.AreEqual(gm.Context.TurnOwner, Roguelike.TurnOwner.Allies);
      //game.GameManager.Logger.LogInfo("make allies move");
      game.MakeGameTick();//make allies move
      Assert.AreEqual(gm.Context.TurnOwner, Roguelike.TurnOwner.Enemies);
      var pend = gm.Context.PendingTurnOwnerApply;
      var to = gm.Context.TurnOwner;
      var tac = gm.Context.TurnActionsCount;
      var ni = ActiveEnemies.Where(e => e.State != EntityState.Idle).ToList();
      //game.GameManager.Logger.LogInfo("make enemies move " + game.GameManager.Context.PendingTurnOwnerApply);
      game.MakeGameTick();//make enemies move
      Assert.AreEqual(gm.Context.TurnOwner, Roguelike.TurnOwner.Animals);
      game.MakeGameTick();//make animals move
      Assert.AreEqual(gm.Context.TurnOwner, Roguelike.TurnOwner.Npcs);
      game.MakeGameTick();
      if (!game.GameManager.HeroTurn)
      {
        var tac1 = game.GameManager.Context.TurnActionsCount;
        int k = 0;
        k++;
      }

      Assert.True(game.GameManager.HeroTurn);
    }

    protected void SetEnemyLevel(Enemy en, int level)
    {
      en.StatsIncreased[IncreaseStatsKind.Level] = false;
      en.SetLevel(level);
      Assert.AreEqual(en.Level, level);
    }

    protected void TryToMoveHero()
    {
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      var emptyHeroNeib = game.Level.GetEmptyNeighborhoodPoint(game.Hero, Dungeons.TileContainers.DungeonNode.EmptyNeighborhoodCallContext.Move);
      var res = game.GameManager.HandleHeroShift(emptyHeroNeib.Item2);
      if (res != InteractionResult.None)
        Assert.False(game.GameManager.HeroTurn);
    }

    protected void GotoSpellEffectEnd(PassiveSpell spell)
    {
      for (int i = 0; i < spell.Duration; i++)
      {
        game.GameManager.SkipHeroTurn();
        GotoNextHeroTurn();
      }
    }

    protected void PlaceCloseToHero(IDestroyable destr, int? minDistance = null)
    {
      PlaceCloseToHero(GameManager, destr, minDistance);
    }

    public void PlaceCloseToHero(GameManager gm, IDestroyable destr, int? minDistance = null)
    {
      var hero = gm.Hero;
      PlaceClose(hero, destr, minDistance);
    }

    public void PlaceClose(LivingEntity le, IDestroyable destr, int? minDistance)
    {
      var gm = GameManager;
      Tile empty;
      if (minDistance == null)
        empty = gm.CurrentNode.GetClosestEmpty(le);
      else
      {
        empty = gm.CurrentNode.GetEmptyTiles()
          .Where(i => i.DistanceFrom(le) >= minDistance.Value)
          .OrderBy(i => i.DistanceFrom(le))
          .FirstOrDefault();
      }
      gm.CurrentNode.SetTile(destr as Tile, empty.point);
      if (destr is Enemy le1 && !gm.EnemiesManager.AllEntities.Contains(le1))
        gm.EnemiesManager.AddEntity(le1);
    }

    protected Tuple<Point, Dungeons.TileNeighborhood> SetCloseToLivingEntity(Dungeons.Tiles.Tile tile, LivingEntity le)
    {
      var level = game.Level;
      var emptyHeroNeib = level.GetEmptyNeighborhoodPoint(le, Dungeons.TileContainers.DungeonNode.EmptyNeighborhoodCallContext.Move);
      Assert.AreNotEqual(GenerationConstraints.InvalidPoint, emptyHeroNeib);
      level.Logger.LogInfo("emptyHeroNeib = " + emptyHeroNeib);
      var set = level.SetTile(tile, emptyHeroNeib.Item1);
      Assert.True(set);
      return emptyHeroNeib;
    }

    protected Tuple<Point, Dungeons.TileNeighborhood> SetCloseToHero(Dungeons.Tiles.Tile tile)
    {
      LivingEntity le = game.Hero;
      return SetCloseToLivingEntity(tile, le);
    }

    protected bool UseScroll(Hero caster, SpellKind sk)
    {
      var scroll = new Scroll(sk);
      return UseScroll(caster, scroll);
    }

    protected bool UseScroll(Hero caster, SpellSource spellSource)
    {
      caster.Inventory.Add(spellSource);
      return game.GameManager.SpellManager.ApplyPassiveSpell<PassiveSpell>(caster, spellSource) != null;
    }

    protected bool UseSpellSource(Hero caster, IHitable victim, SpellSource spellSource)
    {
      //caster.Stats.SetNominal(EntityStatKind.Mana, 100);
      if (victim is LivingEntity le)
        le.Stats[EntityStatKind.ChanceToEvadeElementalProjectileAttack].Nominal = 0;

      if (spellSource is Scroll || spellSource is Book)
        caster.Inventory.Add(spellSource);
      if (caster is Hero)
      {
        game.Hero.SelectedManaPoweredSpellSource = spellSource;
        return game.GameManager.SpellManager.ApplyAttackPolicy(caster, victim, spellSource) == ApplyAttackPolicyResult.OK;
      }
      return false;
    }

    protected bool UseFireBallSpellSource
    (
      Hero caster, IHitable victim, bool useScroll, Roguelike.Spells.SpellKind Scroll = SpellKind.FireBall
    )
    {
      return UseSpellSource(caster, victim, useScroll, Scroll);
    }

    protected bool UseSpellSource(Hero caster, IHitable victim, bool useScroll, Roguelike.Spells.SpellKind sk)
    {
      PlaceCloseToHero(victim as IDestroyable);
      SpellSource src = null;
      if (useScroll)
        src = new Scroll(sk);
      else
        src = new Book(sk);

      return UseSpellSource(caster, victim, src);

    }

    public T GenerateEquipment<T>(string name) where T : Equipment
    {
      return game.GameManager.LootGenerator.GetLootByTileName<T>(name);
    }

    public Dungeons.TileContainers.DungeonLevel CurrentNode
    {
      get { return game.GameManager.CurrentNode; }
    }

    protected ProjectileFightItem ActivateFightItem(FightItemKind fik, Hero hero, int fiCount = 3)
    {
      var fi = new ProjectileFightItem(fik, hero);
      fi.Count = fiCount;
      fi.AlwaysHit = true;
      hero.Inventory.Add(fi);

      //var ak = FightItem.GetAbilityKind(fi);
      //if(ak!= AbilityKind.Unset)
      //  hero.SelectedActiveAbility = hero.GetActiveAbility(ak);
      //else
      hero.SelectedFightItem = fi;
      return fi;
    }

    protected bool SetHeroEquipment(Equipment eq, CurrentEquipmentKind cek = CurrentEquipmentKind.Unset)
    {
      if (!game.Hero.Inventory.Contains(eq))
        game.Hero.Inventory.Add(eq);
      return game.Hero.MoveEquipmentInv2Current(eq, cek);
    }

    protected bool UseFightItem(Hero hero, IHitable enemy, ProjectileFightItem fi)
    {
      if (!hero.Inventory.Contains(fi))
        hero.Inventory.Add(fi);
      if (fi.FightItemKind == FightItemKind.Stone || fi.FightItemKind == FightItemKind.ThrowingTorch)
        return game.GameManager.ApplyAttackPolicy(hero, enemy, fi);

      return game.GameManager.TryApplyAttackPolicy(fi, enemy as Tile);
    }

    protected Enemy CreateEnemy()
    {
      return new Enemy(this.Container);
    }

    protected Scroll PrepareScroll(Hero hero, SpellKind spellKind, Enemy enemyToPlaceNearby = null, int scrollCount = 1)
    {
      var emp = game.GameManager.CurrentNode.GetClosestEmpty(hero);
      if (enemyToPlaceNearby != null)
        Assert.True(game.GameManager.CurrentNode.SetTile(enemyToPlaceNearby, emp.point));
      var scroll = new Scroll(spellKind) { Count = scrollCount };
      if(spellKind == SpellKind.Swiatowit)
        scroll = new SwiatowitScroll() { Count = scrollCount }; ;
      hero.Inventory.Add(scroll);

      return scroll;
    }

    public static float AssertHealthDiffPercentageInRange(DamageComparer dc1, DamageComparer dc2, int percMin, int percMax)
    {
      Assert.Greater(dc2.HealthDifference, dc1.HealthDifference);
      var diff = dc2.CalcHealthDiffPerc(dc1);
      Assert.Greater(diff, percMin);
      Assert.Less(diff, percMax);
      return diff;
    }

    public static float AssertHealthDiffPercentageNotBigger(DamageComparer dc1, DamageComparer dc2, int percMax)
    {
      var diff1 = Math.Abs(dc2.CalcHealthDiffPerc(dc1));
      var diff2 = Math.Abs(dc1.CalcHealthDiffPerc(dc2));
      var diff = dc2.HealthDifference > dc1.HealthDifference ? diff1 : diff2;
      Assert.Less(diff, percMax);
      return diff;
    }

    public static void AssertDurationDiffInRange(DamageComparer dc1, DamageComparer dc2, int min, int max)
    {
      Assert.Greater(dc2.EffectDuration, dc1.EffectDuration);
      var diff = dc2.EffectDuration * 100 / dc1.EffectDuration;
      Assert.Greater(diff, min);
      Assert.Less(diff, max);
    }

    protected bool UseFightItem(Hero hero, Enemy enemy, ProjectileFightItem fi)
    {
      if (!hero.Inventory.Contains(fi))
        hero.Inventory.Add(fi);

      var res = game.GameManager.TryApplyAttackPolicy(fi, enemy);
      if (hero.SelectedActiveAbility != null)
        Assert.Greater(hero.SelectedActiveAbility.CoolDownCounter, 0);
      return res;
    }

    protected void Save()
    {
      game.GameManager.Save(false);
    }

    protected void Load()
    {
      game.GameManager.Load(game.Hero.Name, false);
    }

    //protected void Delete()
    //{ 
    //}

    protected void SaveLoad()
    {
      Save();
      Load();
    }

    protected void PrepareToBeBeaten(LivingEntity le)
    {
      le.Stats.GetStat(EntityStatKind.Health).Value.Nominal = 300;
    }

    public void MakeEnemyThrowProjectileAtHero
    (
      Enemy en, 
      GameManager gm, 
      FightItemKind fightItemKind, 
      bool breakAfterOne,
      Action gotoNextHeroTurn,
      bool addFightItems,
      int pfiCount = 3
    )
    {
      en.SelectedFightItem = en.SetActiveFightItem(fightItemKind);
      en.Name = "bandit555";
      en.d_canMove = false;//make sure will not move, but fight at distance
      en.AlwaysHit[AttackKind.PhysicalProjectile] = true;
      PlaceCloseToHero(gm, en, 2);
      if (addFightItems && en.SelectedFightItem.Count != pfiCount)
        en.SelectedFightItem.Count = pfiCount;

      Assert.AreEqual(gm.Hero.GetFightItemKindHitCounter(fightItemKind), 0);

      var hero = gm.Hero;
      PrepareToBeBeaten(hero);
      var beginHeroHealth = hero.Stats.Health;

      for (int i = 0; i < 20; i++)
      {
        gotoNextHeroTurn();
        if (breakAfterOne && beginHeroHealth > hero.Stats.Health)
          break;
      }
      Assert.Greater(beginHeroHealth, hero.Stats.Health);
      var fic = gm.Hero.GetFightItemKindHitCounter(fightItemKind);
      if (breakAfterOne)
      {
        Assert.AreEqual(fic, 1);
        Assert.Greater(en.SelectedFightItem.Count, 0);
      }
      Assert.Greater(fic, 0);
      Assert.Less(fic, 4);
    }

    public void MaximizeAbility(Ability ab, AdvancedLivingEntity le)
    {
      while (ab.Level < ab.MaxLevel)
      {
        Assert.True(le.IncreaseAbility(ab.Kind));
      }
    }

    protected Enemy AddEnemy(char symbol = 'b')
    {
      var en = SpawnEnemy(symbol);
      en.Revealed = true;
      GameManager.CurrentNode.SetTileAtRandomPosition(en);
      GameManager.EnemiesManager.AddEntity(en);
      return en;
    }
  }
}
