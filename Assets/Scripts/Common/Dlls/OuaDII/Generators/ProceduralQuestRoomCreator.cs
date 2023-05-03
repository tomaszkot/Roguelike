using OuaDII.Quests;
using OuaDII.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Generators
{
  /// <summary>
  /// Currently not used by game, just for UT
  /// </summary>
  public class ProceduralQuestRoomCreator : IQuestRoomCreator
  {
    QuestKind questKind;
    public QuestKind QuestKind { get { return questKind; } }
    public void SetPredefiniedDungeons(List<IPredefiniedDungeon> predefiniedDungeon) { }
    public void Init(IQuestRoomCreatorOwner owner, QuestKind questKind, LevelGenerator lg)
    {
      this.questKind = questKind;
      if (questKind == QuestKind.CrazyMiller)
      {
        int roomWidth = 10;
        int roomHeight = 15;

        lg.CustomNodeCreator = (int nodeIndex, Dungeons.GenerationInfo gi) =>
        {
          var dungeon = lg.CreateDungeonNodeInstance();
          gi.GenerateOuterWalls = true;
          if (lg.LevelIndex == 0)
          {
            //moved to LevelGenerator
            //gi.ForcedNextRoomSide = Dungeons.EntranceSide.Right;
            //gi.ChildIslandAllowed = false;
          }
          if (lg.LevelIndex == 1)
          {
            roomWidth *= 2;
            gi.ForceChildIslandInterior = true;
            gi.ForcedChilldIslandSize = new System.Drawing.Size(10, 9);
          }

          dungeon.ChildIslandCreated += (object sender, Dungeons.TileContainers.ChildIslandCreationInfo e) =>
          {
            var islandGi = lg.Container.GetInstance<Roguelike.Generators.GenerationInfo>();
            lg.GenerateRoomContent(e.ChildIslandNode, islandGi, questKind);
            //e.Child.SetTileAtRandomPosition<Enemy>();
          };
          //for(int row=0;<5;i++)
          dungeon.Create(roomWidth, roomHeight, gi, nodeIndex);
          //dungeon.GenerateOuterWalls();
          return dungeon;
        };

        lg.NodeCreated += (object s, Dungeons.TileContainers.DungeonNode node) =>
        {
          //f (node.NodeIndex == 0)
          lg.HandleQuestNodeCreated(questKind, node);

          //lg.GenerateRoomContent(node, gi, questKind);
        };
      }
    }

    //private void Dungeon_ChildIslandCreated(object sender, Dungeons.TileContainers.ChildIslandCreationInfo e)
    //{
    //  var gi = lg.Container.GetInstance<Roguelike.Generators.GenerationInfo>();
    //  lg.GenerateRoomContent(node, gi, questKind);
    //  //e.Child.SetTileAtRandomPosition<Roguelike.Tiles.Barrel>();
    //}
  }
}
