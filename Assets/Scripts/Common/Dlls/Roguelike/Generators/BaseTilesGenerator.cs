using Dungeons.Core;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Roguelike.Generators
{
  public abstract class BaseTilesGenerator
  {
    public Difficulty Difficulty { get; private set; }
    SimpleInjector.Container container;

    public BaseTilesGenerator(SimpleInjector.Container container)
    {
      this.Difficulty = GenerationInfo.Difficulty;
      this.container = container;
    }
    abstract protected void SetILootSourceLevel(ILootSource src, Hero hero);

    abstract protected List<string> Filter(List<string> enemyNames);

    protected virtual List<string> GetEnemyNames(Dungeons.TileContainers.DungeonNode node)
    {
      List<string> enemyNames = EnemySymbols.EnemiesToSymbols.Keys.ToList();
      return Filter(enemyNames);
    }

    public virtual Enemy CreateEnemyInstance(SimpleInjector.Container container, string enemyName, bool reveal, bool setLevel)
    {
      var enemy = container.GetInstance<Enemy>();

      enemy.Container = container;
      enemy.tag1 = enemyName;
      enemy.Name = enemyName;
      if(setLevel)
        SetILootSourceLevel(enemy, null);
      if (EnemySymbols.EnemiesToSymbols.ContainsKey(enemy.Name))
        enemy.Symbol = EnemySymbols.EnemiesToSymbols[enemy.Name];

      if (enemy.tag1.Contains("bandit"))
      {
        enemy.tag1 = "bandit" + (RandHelper.GetRandomInt(6) + 1);
      }
      
      return enemy;
    }
    public void SetILootSourceLevel(Dungeons.TileContainers.DungeonNode node, Hero hero)
    {
      var lootSources = new List<ILootSource>();

      //these call are fast, 2 ms with grid 200x200
      lootSources.AddRange(node.GetTiles<Barrel>());
      lootSources.AddRange(node.GetTiles<Chest>());
      lootSources.AddRange(node.GetTiles<Enemy>());
      SetILootSourceLevel(lootSources, hero);
    }

    public virtual void SetILootSourceLevel(List<ILootSource> lss, Hero hero)
    {
      lss.ForEach(i => SetILootSourceLevel(i, hero));
    }

    protected virtual List<Enemy> CreateEnemiesPack(GenerationInfo gi, string enemyName)
    {
      List<Enemy> enemiesPack = new List<Enemy>();

      var packSize = gi.ForcedNumberOfEnemiesInRoom;
      if (packSize == -1)
        packSize = RandHelper.GetRandomElem<int>(new List<int>() { 3, 4, GenerationInfo.MaxEnemyPackCount });

      if (gi.MinimalContent)
        packSize /= 2;

      for (int enIndex = 0; enIndex < packSize; enIndex++)
      {
        var enemy = CreateEnemyInstance(this.container, enemyName, false, false);
        enemiesPack.Add(enemy);
      }

      return enemiesPack;
    }
  }
}
