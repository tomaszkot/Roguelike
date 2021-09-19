using Roguelike.Tiles;

namespace Roguelike.Quests
{
  public enum QuestStatus
  {
    Unset, Proposed, Accepted, Rejected, FailedToDo,
    AwaitingReward,
    Done
  }

  public class QuestRequirement
  {

  }

  public class Quest
  {
    public QuestStatus Status { get; set; }
    public string Tag { get; set; }
    public string Name { get; set; }
    public string QuestPrincipalName { get; set; }//typically merchant's name
    public QuestRequirement QuestRequirement { get; set; } = new QuestRequirement();
    public LootKind RewardLootKind { get; set; }
    public string RewardLootName { get; set; }
    public bool SkipReward { get; set; }
  }


}
