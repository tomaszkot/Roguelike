using Dungeons;
using Dungeons.Core;
using Roguelike.Abstract;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Generators
{
  public class LevelGenerator : Dungeons.Generator
  {
    string pitName;
    ILogger logger;
    //Roguelike.Tiles.Hero hero;
    
    public GameNode Dungeon { get; set; }

    public LevelGenerator(ILogger logger): this("pit1", logger)
    {
      
    }

    public LevelGenerator(string pitName, ILogger logger)
    {
      this.pitName = pitName;
      this.logger = logger;
    }

    protected override DungeonNode CreateNode(int w, int h, GenerationInfo gi, int index)
    {
      var node = new GameNode(w, h, gi, index);
      if (index == 0)
      {
        Stairs stairs = new Stairs() { Kind = StairsKind.PitUp, Symbol = '<' };
        stairs.PitName = pitName;
        node.SetTile(stairs, new System.Drawing.Point(3, 1));
        //node.SetTile(stairs, node.GetFirstEmptyPoint().Value);

        var enemy = new Enemy();
        node.SetTile(enemy, new System.Drawing.Point(3,2));// node.GetRandomEmptyTile().Point);
        logger.LogInfo("added enemy at :"+ enemy.Point);
        //node.SetTile(enemy, node.GetEmptyTiles().Last().Point);
      }

      return node;
    }

    protected override GenerationInfo CreateLevelGenerationInfo()
    {
      var gi = new GenerationInfo();
      gi.RevealTiles = false;

      return gi;
    }

    public override Dungeons.DungeonNode Generate()
    {
      LayouterOptions opt = new LayouterOptions();
      opt.RevealAllNodes = false;
      var level = base.Generate<DungeonLevel>(0, opt);
      level.OnGenerationDone();
      this.Dungeon = level;

      var sts = level.GetTiles<Stairs>();
      //var level = new DungeonLevel();// new List<TileContainers.DungeonNode> { node });
      // level.AppendMaze(node);
      
      return level;
    }

    
  }
}
