using Dungeons.Core;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Abstract.Multimedia;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Multimedia;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using RoguelikeUnitTests.Helpers;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RoguelikeUnitTests
{

  [TestFixture]
  public class TestBase
  {
    protected int createTestEnvCounter = 0;
    protected RoguelikeGame game;

    public RoguelikeGame Game { get => game; protected set => game = value; }
    public Container Container { get; set; }
    public BaseHelper Helper { get => helper; set => helper = value; }
    GameManager GameManager { get => game.GameManager; }

    protected BaseHelper helper;

    [SetUp]
    public void Init()
    {
      var gi = new GenerationInfo();
      Assert.Greater(gi.NumberOfRooms, 1);
      Assert.Greater(gi.ForcedNumberOfEnemiesInRoom, 2);

      OnInit();
    }

    protected Enemy SpawnEnemy()
    {
      if (game != null)
        return game.GameManager.CurrentNode.SpawnEnemy(1);
      return Container.GetInstance<Enemy>();
    }

    protected virtual void OnInit()
    {
      Tile.IncludeDebugDetailsInToString = true;
      Container = new Roguelike.ContainerConfigurator().Container;
      Container.Register<ISoundPlayer, BasicSoundPlayer>();

    }

    protected T GenerateRandomEqOnLevelAndCollectIt<T>() where T : Equipment, new()
    {
      var eq = GenerateRandomEqOnLevel<T>();
      CollectLoot(eq);
      return eq;
    }

    private void CollectLoot(Loot loot)
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
        var tile = Game.GameManager.CurrentNode.SetTileAtRandomPosition(loot);
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
      get { return game.GameManager.EnemiesManager.GetActiveEnemies().Where(i=>i.PowerKind == EnemyPowerKind.Plain).ToList(); }
    }


    List<Roguelike.Tiles.LivingEntities.Enemy> ActiveEnemies
    {
      get { return game.GameManager.EnemiesManager.GetActiveEnemies().ToList(); }
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
      get { return PlainEnemies.Where(i=>!i.IsStrongerThanAve).ToList(); }
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
      //return Enemies.Take(numEnemies).ToList();
    }

    int numEnemies = 0;
    public virtual RoguelikeGame CreateGame(bool autoLoadLevel = true, int numEnemies = 20, int numberOfRooms = 5, GenerationInfo gi = null)
    {
      if (createTestEnvCounter > 0)
      {
        OnInit();
      }
      game = new RoguelikeGame(Container);

      game.GameManager.EventsManager.ActionAppended += (object sender, Roguelike.Events.GameEvent e) =>
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
          this.numEnemies = numEnemies;
          if (numberOfRooms == 1)
            info.PreventSecretRoomGeneration = true;

          if (numEnemies > 1)
          {
            float numEn = ((float)numEnemies) / numberOfRooms;
            info.ForcedNumberOfEnemiesInRoom = (int)(numEn + 0.5);

            if (info.ForcedNumberOfEnemiesInRoom == 0)
            {
              info.ForcedNumberOfEnemiesInRoom = numEnemies % numberOfRooms;
              numEnemies = numEnemies * numberOfRooms;
            }
          }
          info.NumberOfRooms = numberOfRooms;
          gi = info;
        }

        var level = game.GenerateLevel(0, gi);
                
        var enemies = level.GetTiles<Enemy>();
        if (enemies.Count > numEnemies)
        {
          var aboveThreshold = enemies.Skip(numEnemies).ToList();
          foreach (var en in aboveThreshold)
          {
            level.SetEmptyTile(en.point);
            GameManager.EnemiesManager.AllEntities.Remove(en);
          }
        }
        enemies = level.GetTiles<Enemy>();
        Assert.LessOrEqual(enemies.Count, numEnemies);

        Assert.GreaterOrEqual(AllEnemies.Count, numEnemies);//some are auto generated
        Assert.Less(ActiveEnemies.Count, numEnemies * 4);

        if (AllEnemies.Any() && !AllEnemies.Where(i => i.Revealed).Any())
        {
          AllEnemies[0].Revealed = true;
        }
      }
      createTestEnvCounter++;
      if (game.Hero != null)
        game.Hero.Name = "Hero";
      return game;
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

    [TearDown]
    public void Cleanup()
    {
    }

    public void InteractHeroWith(InteractiveTile tile)
    {
      Game.GameManager.InteractHeroWith(tile);
      GotoNextHeroTurn(game);
    }

    public void InteractHeroWith(Enemy tile)
    {
      Game.GameManager.InteractHeroWith(tile);
      GotoNextHeroTurn(game);
    }

    protected void SkipTurns(int number)
    {
      for(int i=0;i<number;i++)
        GotoNextHeroTurn();
    }

    protected void GotoNextHeroTurn(Roguelike.RoguelikeGame game = null)
    {
      if (game == null)
        game = this.game;
      if (game.GameManager.Context.TurnOwner == TurnOwner.Hero)
        game.GameManager.SkipHeroTurn();
      Assert.AreEqual(game.GameManager.Context.TurnOwner, Roguelike.TurnOwner.Allies);
      //game.GameManager.Logger.LogInfo("make allies move");
      game.MakeGameTick();//make allies move
      Assert.AreEqual(game.GameManager.Context.TurnOwner, Roguelike.TurnOwner.Enemies);
      var pend = game.GameManager.Context.PendingTurnOwnerApply;
      var to = game.GameManager.Context.TurnOwner;
      var tac = game.GameManager.Context.TurnActionsCount;
      var ni = ActiveEnemies.Where(e => e.State != EntityState.Idle).ToList();
      //game.GameManager.Logger.LogInfo("make enemies move " + game.GameManager.Context.PendingTurnOwnerApply);
      game.MakeGameTick();//make enemies move
      game.MakeGameTick();//make animals move
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

    protected void PlaceCloseToHero(IDestroyable enemy)
    {
      var hero = game.Hero;
      var empty = GameManager.CurrentNode.GetClosestEmpty(hero);
      GameManager.CurrentNode.SetTile(enemy as Tile, empty.point);
    }
    protected Tuple<Point, Dungeons.TileNeighborhood> SetCloseToHero(Dungeons.Tiles.Tile tile)
    {
      var level = game.Level;
      var emptyHeroNeib = level.GetEmptyNeighborhoodPoint(game.Hero, Dungeons.TileContainers.DungeonNode.EmptyNeighborhoodCallContext.Move);
      Assert.AreNotEqual(GenerationConstraints.InvalidPoint, emptyHeroNeib);
      level.Logger.LogInfo("emptyHeroNeib = " + emptyHeroNeib);
      var set = level.SetTile(tile, emptyHeroNeib.Item1);
      Assert.True(set);
      return emptyHeroNeib;
    }

    protected bool UseScroll(Hero caster, SpellKind sk)
    {
      var scroll = new Scroll(sk);
      return UseScroll(caster, scroll);
    }

    protected bool UseScroll(Hero caster, SpellSource spellSource)
    {
      caster.Inventory.Add(spellSource);
      return game.GameManager.SpellManager.ApplyPassiveSpell(caster, spellSource) != null;
    }

    protected bool UseSpellSource(Hero caster, IDestroyable victim, SpellSource spellSource)
    {
      if (victim is LivingEntity le)
        le.Stats[EntityStatKind.ChanceToEvadeElementalProjectileAttack].Nominal = 0;

      if (spellSource is Scroll || spellSource is Book)
        caster.Inventory.Add(spellSource);
      if (caster is Hero)
      {
        game.Hero.ActiveManaPoweredSpellSource = spellSource;
        return game.GameManager.SpellManager.ApplyAttackPolicy(caster, victim, spellSource) == Roguelike.Managers.ApplyAttackPolicyResult.OK;
      }
      return false;
    }

    protected bool UseFireBallSpellSource(Hero caster, IDestroyable victim, bool useScroll)
    {
      return UseSpellSource(caster, victim, useScroll, Roguelike.Spells.SpellKind.FireBall);
    }

    protected bool UseSpellSource(Hero caster, IDestroyable victim, bool useScroll, Roguelike.Spells.SpellKind sk)
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

    protected ProjectileFightItem ActivateFightItem(FightItemKind fik, Hero hero)
    {
      var fi = new ProjectileFightItem(fik, hero);
      fi.Count = 3;
      fi.AlwaysHit = true;
      hero.Inventory.Add(fi);
      hero.ActiveFightItem = fi;
      return fi;
    }

    protected bool SetHeroEquipment(Equipment eq, CurrentEquipmentKind cek = CurrentEquipmentKind.Unset)
    {
      if (!game.Hero.Inventory.Contains(eq))
        game.Hero.Inventory.Add(eq);
      return game.Hero.MoveEquipmentInv2Current(eq, cek);
    }

    protected bool UseFightItem(Hero hero, LivingEntity enemy, ProjectileFightItem fi)
    {
      hero.Inventory.Add(fi);
      if (fi.FightItemKind == FightItemKind.Stone)
        return game.GameManager.ApplyAttackPolicy(hero, enemy, fi);

      return game.GameManager.TryApplyAttackPolicy(fi, enemy);
    }

    protected Enemy CreateEnemy()
    {
      return new Enemy(this.Container);
    }
  }
}
