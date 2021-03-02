using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Quests
{
  public enum QuestStatus { Unset, Proposed, Accepted, Rejected, FailedToDo, AwaitingReward, Done }

  public class Quest
  {
    public QuestStatus Status { get; set; }
    public string SenderName { get; set; }
    public string Description { get; set; }
  }
}
