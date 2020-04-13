using Dungeons.Tiles;
using Roguelike;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests.Helpers
{
  public class LootingTestsHelper : BaseHelper
  {
    public LootingTestsHelper():base(null) 
    {
    }

    public LootingTestsHelper(TestBase test, RoguelikeGame game) : base(test, game)
    {
    }

    public void AddThenDestroyInteractive<T>
    (
    int numberOfTilesToTest = 50,
    Action<InteractiveTile> init = null
    ) where T : InteractiveTile, new()
    {
      for (int i = 0; i < numberOfTilesToTest; i++)
      {
        var tile = AddTile<T>(game);
        if (init != null)
          init(tile);
        game.GameManager.InteractHeroWith(tile);
      }
    }
  }
}
