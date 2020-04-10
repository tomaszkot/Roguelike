using NUnit.Framework;
using Roguelike;
using Roguelike.Managers;
using Roguelike.Tiles;
using SimpleInjector;

namespace RoguelikeUnitTests
{
  [TestFixture]
  public class TestBase
  {
    protected RoguelikeGame game;

    public RoguelikeGame Game { get => game; protected set => game = value; }
    public Container Container { get; set; }

    [SetUp]
    public void Init()
    {
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

    public void PutEqOnLevelAndCollectIt(Equipment eq)
    {
      PutLootOnLevel(eq);
      CollectLoot(eq);
    }

    public virtual RoguelikeGame CreateGame(bool autoLoadLevel = true)
    {
      Game = new RoguelikeGame(Container);
      if (autoLoadLevel)
        Game.GenerateLevel(0);
      return Game;
    }

    [TearDown]
    public void Cleanup()
    { 
    }
  }
}
