using Dungeons.Tiles;
using Roguelike;
using Roguelike.Managers;

namespace RoguelikeUnitTests.Helpers
{
  public class BaseHelper
  {
    protected RoguelikeGame game;
    protected TestBase test;

    public GameManager GameManager { get { return game.GameManager; } }

    //public RoguelikeGame BaseHelper(bool autoLoadLevel = true)
    //{
    //  var game = new RoguelikeGame(Container);
    //  if (autoLoadLevel)
    //    Game.GenerateLevel(0);
    //  return Game;
    //}
    public BaseHelper(TestBase test)
    {
      this.test = test;
    }

    public BaseHelper(TestBase test, RoguelikeGame game)
    {
      this.game = game;
      this.test = test;
    }

    public RoguelikeGame Game { get => game; set => game = value; }

    public T AddTile<T>(RoguelikeGame game) where T : Tile, new()
    {
      var tile = new T();
      if (game.Level.SetTile(tile, game.Level.GetFirstEmptyPoint().Value))
        return tile;
      return null;
    }
  }
}
