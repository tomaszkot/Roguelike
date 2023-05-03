using Dungeons.Core;
using Roguelike;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Generators
{
  public class ChanceAtGameStartStatus
  {
    public bool Done { get; set; }
    public float Chance { get; set; }

    public override string ToString()
    {
      return base.ToString() + "Done = " + Done;
    }
  }

  public class Counts
  {
    public int WorldExtraEnemiesCloseToCampCount { get; set; } = 5;
    public int WorldEnemiesPacksCount { get; set; } = 20;
    public int WorldEnemiesCount { get; set; } = 130;
    public int WorldBarrelsCount { get; set; } = 150;
    public int WorldChestsCount { get; set; } = 70;

    public int DeadBodiesCount { get; set; } = 30;

    public int OilFlowsCount { get; set; } = 10;

    public int MushroomsCount { get; set; } = 120;
    public int MagicDustsCount { get; set; } = 50;

    public int FoodCount { get; set; } = 100;
    
    public int PlantCount { get; set; } = 100;

    public void SetMin()
    {
      WorldExtraEnemiesCloseToCampCount = 0;
      WorldEnemiesPacksCount = 0;
      WorldEnemiesCount = 0;
      WorldBarrelsCount = 1;
      WorldChestsCount = 1;
      DeadBodiesCount = 1;
      MushroomsCount = 1;
      MagicDustsCount = 1;
      FoodCount = 1;
      PlantCount = 1;
    }
  }

  public class GenerationInfo : Roguelike.Generators.GenerationInfo
  {
    public static bool TestLoot = false;
    public static bool GenerateDynamicTiles = true;
    public Counts Counts { get; set; } = new Counts();
    public bool allowSmallWordSize { get; set; } = false;
    public bool allowNulls { get; set; } = false;

    public GenerationInfo()
    {
      //Counts.SetMin();
    }

    public void SetMinWorldSize(int size)
    {
      MinNodeSize = new System.Drawing.Size(size, size);
      MaxNodeSize = MinNodeSize;
    }

  }
}
