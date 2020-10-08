using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Attributes;
using Roguelike.Managers;
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

    protected virtual void OnInit()
    {
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
    }

    public List<Enemy> GetLimitedEnemies()
    {
      //return game.GameManager.CurrentNode.GetTiles<Enemy>().Take(numEnemies).ToList();
      return game.GameManager.EnemiesManager.GetEnemies().Take(numEnemies).ToList();
    }

    int numEnemies = 0;
    public virtual RoguelikeGame CreateGame(bool autoLoadLevel = true, int numEnemies = 10, int numberOfRooms = 5)
    {
      game = new RoguelikeGame(Container);
      helper = new BaseHelper(this, game);
      if (autoLoadLevel)
      {
        var info = new GenerationInfo();
        info.MinNodeSize = new System.Drawing.Size(30, 30);
        info.MaxNodeSize = info.MinNodeSize;
        this.numEnemies = numEnemies;
        float numEn = ((float)numEnemies) / numberOfRooms;
        info.ForcedNumberOfEnemiesInRoom = (int)(numEn  + 0.5);
        if (info.ForcedNumberOfEnemiesInRoom == 0)
        {
          info.ForcedNumberOfEnemiesInRoom = numEnemies % numberOfRooms;
          numEnemies = numEnemies * numberOfRooms;
        }
        info.NumberOfRooms = numberOfRooms;
        var level = game.GenerateLevel(0, info);
        Assert.GreaterOrEqual(game.GameManager.EnemiesManager.Enemies.Count, numEnemies);//some are auto generated
        Assert.Less(game.GameManager.EnemiesManager.Enemies.Count, numEnemies*4);
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
      game.GameManager.Logger.LogInfo("make allies move");
      game.MakeGameTick();//make allies move
      Assert.AreEqual(game.GameManager.Context.TurnOwner, Roguelike.TurnOwner.Enemies);
      var pend = game.GameManager.Context.PendingTurnOwnerApply;
      var to = game.GameManager.Context.TurnOwner;
      var tac = game.GameManager.Context.TurnActionsCount;
      var ni = game.GameManager.EnemiesManager.GetEnemies().Where(e => e.State != EntityState.Idle).ToList();
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
  }
}
