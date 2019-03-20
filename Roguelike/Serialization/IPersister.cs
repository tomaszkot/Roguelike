
using System.Collections.Generic;
using Roguelike;
using Roguelike.TileContainers;
using Roguelike.Tiles;

namespace Serialization
{
  public interface IPersister
  {
    void SaveWorld(World world);
    World LoadWorld();
    void SaveHero(Hero hero);
    Hero LoadHero();
    void SavePits(List<DungeonPit> pits);
    List<DungeonPit> LoadPits();
    void SaveGameState(GameState gameState);
    GameState LoadGameState();
  }
}