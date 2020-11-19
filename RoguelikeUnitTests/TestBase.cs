using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Spells;
using Roguelike.Tiles;
using RoguelikeUnitTests.Helpers;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;

namespace RoguelikeUnitTests
{

  [TestFixture]
  public class TestBase
  {
    protected RoguelikeGame game;

    public RoguelikeGame Game { get => game; protected set => game = value; }
    public Container Container { get; set; }
    public BaseHelper Helper { get => helper; set => helper = value; }

    protected BaseHelper helper;

    [SetUp]
    public void Init()
    {
      OnInit();
    }

    protected Enemy SpawnEnemy()
    {
      return game.GameManager.CurrentNode.SpawnEnemy(1);
    }

    protected virtual void OnInit()
    {
      Tile.IncludeDebugDetailsInToString = true;
      Container = new Roguelike.ContainerConfigurator().Container;
      Container.Register<ISoundPlayer, BasicSoundPlayer>();
      var gi = new GenerationInfo();
      Assert.Greater(gi.NumberOfRooms, 1);
      Assert.Greater(gi.ForcedNumberOfEnemiesInRoom, 2);
    }

    protected T GenerateRandomEqOnLevelAndCollectIt<T>() where T : Equipment, new()
    {
      var eq = GenerateRandomEqOnLevel<T>();
      CollectLoot(eq);
      return eq;
    }

    private void CollectLoot(Loot loot) 
    {
      var set = Game.GameManager.CurrentNode.SetTile(game.Hero, loot.Point);
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

    protected List<Roguelike.Tiles.Enemy> ActiveEnemies
    {
      get { return game.GameManager.EnemiesManager.GetActiveEnemies().ToList(); }
    }

    protected List<Roguelike.Tiles.Enemy> AllEnemies
    {
      get { return game.GameManager.EnemiesManager.AllEntities.Cast<Enemy>().ToList(); }
    }

    public List<Enemy> GetLimitedEnemies()
    {
      return game.GameManager.CurrentNode.GetTiles<Enemy>().Take(numEnemies).ToList();
      //return Enemies.Take(numEnemies).ToList();
    }

    int numEnemies = 0;
    public virtual RoguelikeGame CreateGame(bool autoLoadLevel = true, int numEnemies = 10, int numberOfRooms = 5, GenerationInfo gi = null)
    {
      game = new RoguelikeGame(Container);

      game.GameManager.EventsManager.ActionAppended += (object sender, Roguelike.Events.GameAction e)=>
      {
        if (e is GameStateAction)
        {
          var gsa = e as GameStateAction;
          if(gsa.Type == GameStateAction.ActionType.Assert)
            throw new System.Exception(gsa.Info);
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
          float numEn = ((float)numEnemies) / numberOfRooms;
          info.ForcedNumberOfEnemiesInRoom = (int)(numEn + 0.5);
          if (info.ForcedNumberOfEnemiesInRoom == 0)
          {
            info.ForcedNumberOfEnemiesInRoom = numEnemies % numberOfRooms;
            numEnemies = numEnemies * numberOfRooms;
          }
          info.NumberOfRooms = numberOfRooms;
          gi = info;
        }

        var level = game.GenerateLevel(0, gi);
        Assert.GreaterOrEqual(game.GameManager.EnemiesManager.AllEntities.Count, numEnemies);//some are auto generated
        Assert.Less(ActiveEnemies.Count, numEnemies*4);
      }
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

    public void InteractHeroWith(Roguelike.Tiles.InteractiveTile tile)
    {
      Game.GameManager.InteractHeroWith(tile);
      GotoNextHeroTurn(game);
    }

    public void InteractHeroWith(Enemy tile)
    {
      Game.GameManager.InteractHeroWith(tile);
      GotoNextHeroTurn(game);
    }

    protected void GotoNextHeroTurn(Roguelike.RoguelikeGame game = null)
    {
      if (game == null)
        game = this.game;
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
      en.StatsIncreasedByLevel = false;
      en.SetLevel(level);
    }

    protected void TryToMoveHero()
    {
      Assert.AreEqual(game.GameManager.Context.TurnOwner, TurnOwner.Hero);
      var emptyHeroNeib = game.Level.GetEmptyNeighborhoodPoint(game.Hero, Dungeons.TileContainers.DungeonNode.EmptyNeighborhoodCallContext.Move);
      game.GameManager.HandleHeroShift(emptyHeroNeib.Item2);
      Assert.False(game.GameManager.HeroTurn);
    }

    protected void GotoSpellEffectEnd(PassiveSpell spell)
    {
      for (int i = 0; i < spell.TourLasting; i++)
      {
        game.GameManager.SkipHeroTurn();
        GotoNextHeroTurn();
      }
    }
  }
}
