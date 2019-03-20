
using System.Collections.Generic;
using Roguelike;
using Roguelike.TileContainers;
using Roguelike.Tiles;

namespace Roguelike.Serialization
{
  public interface IPersister
  {
    
    void SaveHero(Hero hero);
    Hero LoadHero();
    
    void SaveGameState(GameState gameState);
    GameState LoadGameState();
  }
}