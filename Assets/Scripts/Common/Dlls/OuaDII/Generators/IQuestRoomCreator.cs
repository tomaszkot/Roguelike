using OuaDII.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Generators
{
  public interface IQuestRoomCreator
  {
    void Init(IQuestRoomCreatorOwner owner, QuestKind questKind, LevelGenerator lg);
    void SetPredefiniedDungeons(List<IPredefiniedDungeon> predefiniedDungeon);

    QuestKind QuestKind { get; }
  }

  public interface IQuestRoomCreatorOwner
  {
    void OnCustomNodeCreatorStart(IQuestRoomCreator questRoomCreator);
  }

  public interface IPredefiniedDungeon
  {

  }
}
