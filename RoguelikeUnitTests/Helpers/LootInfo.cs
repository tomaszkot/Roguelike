using Roguelike;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests.Helpers
{
  public class LootInfo
  {
    private List<Loot> prev;
    public List<Loot> newLoot;
    //public List<Loot> 
    RoguelikeGame game;

    public List<Loot> Prev { get => prev; set => prev = value; }

    public LootInfo(RoguelikeGame game, InteractiveTile interactWith)
    {
      prev = game.Level.GetTiles<Loot>();
      this.game = game;
      if (interactWith != null)
      {
        game.GameManager.InteractHeroWith(interactWith);
        newLoot = GetDiff();
      }
    }

    public List<Loot> GetDiff()
    {
      var lootAfter = game.Level.GetTiles<Loot>();
      newLoot = lootAfter.Except(prev).ToList();
      return newLoot;
    }

    internal List<T> Get<T>()
    {
      return newLoot.Where(i => i is T).Cast<T>().ToList();
    }
  };
}
