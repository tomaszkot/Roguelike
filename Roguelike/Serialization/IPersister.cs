using Roguelike.Abstract.Tiles;
using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;

namespace Roguelike.Serialization
{
  public interface IPersister
  {
    void SaveHero(Hero hero);
    Hero LoadHero(string heroName);

    AlliesStore LoadAllies();
    void SaveAllies(AlliesStore allies);

    void SaveGameState(string heroName, GameState gameState);
    GameState LoadGameState(string heroName);

    void SaveLevel(string heroName, GameLevel level);
    GameLevel LoadLevel(string heroName, int index);
  }
}