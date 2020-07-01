using System.Collections.Generic;
using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Tiles;

namespace RoguelikeUnitTests.Helpers
{
  public class BaseHelper
  {
    protected RoguelikeGame game;
    protected TestBase test;

    public GameManager GameManager { get { return game.GameManager; } }
    public LootGenerator LootGenerator { get { return GameManager.LootGenerator; }  }

    public BaseHelper(TestBase test)
    {
      this.test = test;
    }

    public BaseHelper(TestBase test, RoguelikeGame game)
    {
      this.game = game;
      this.test = test;
      var gi = new GenerationInfo();
      Assert.Greater(gi.NumberOfRooms, 1);
      Assert.Greater(gi.ForcedNumberOfEnemiesInRoom, 2);
    }

    public RoguelikeGame Game { get => game; set => game = value; }
    public List<Enemy> Enemies { get; internal set; }

    public T AddTile<T>() where T : Tile, new()
    {
      var tile = new T();
      if (game.Level.SetTile(tile, game.Level.GetFirstEmptyPoint().Value))
        return tile;
      return null;
    }
  }
}
