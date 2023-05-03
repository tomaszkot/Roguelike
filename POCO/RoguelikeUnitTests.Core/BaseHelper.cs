using Dungeons.Tiles;
using NUnit.Framework;
using Roguelike;
using Roguelike.Generators;
using Roguelike.Managers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;

namespace RoguelikeUnitTests.Helpers
{
  public class BaseHelper
  {
    protected RoguelikeGame game;
    protected TestBase test;

    public GameManager GameManager { get { return game.GameManager; } }
    public LootGenerator LootGenerator { get { return GameManager.LootGenerator; } }

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
    public TestBase Test { get => test; set => test = value; }

    public T AddTile<T>(T tile) where T : Tile
    {
      if (game.Level.SetTile(tile, game.Level.GetFirstEmptyPoint().Value))
        return tile;
      return null;
    }

    public T AddTile<T>() where T : Tile, new()
    {
      var tile = new T();
      if (game.Level.SetTile(tile, game.Level.GetFirstEmptyPoint().Value))
        return tile;
      return null;
    }

    static void PlaceCloseToHero(GameManager GameManager, Hero hero, Enemy enemy)
    {
      var empty = GameManager.CurrentNode.GetClosestEmpty(hero);
      GameManager.CurrentNode.SetTile(enemy, empty.point);
    }

    public static void AlignOneAfterAnotherNextToHero(GameManager GameManager, Enemy en1, Enemy en2, Hero hero)
    {
      PlaceCloseToHero(GameManager, hero, en1);
      PlaceCloseToHero(GameManager, hero, en2);
      var pt = en1.point;
      if (en1.point.X == hero.point.X)
      {
        pt.Y = en1.point.Y + 1;
        if (pt == hero.point)
          pt.Y = en1.point.Y - 1;
      }
      else
      {
        pt.X = en1.point.X + 1;
        if (pt == hero.point)
          pt.X = en1.point.X - 1;
      }
      var tileAt = GameManager.CurrentNode.GetTile(pt);
      if (!tileAt.IsEmpty)
      {
        Assert.True(GameManager.CurrentNode.SetEmptyTile(pt));
        tileAt = GameManager.CurrentNode.GetTile(pt);
        //Assert.True(false);
      }
      Assert.True(tileAt.IsEmpty || tileAt is Loot);
      GameManager.CurrentNode.SetTile(en2, pt);
    }
  }
}
