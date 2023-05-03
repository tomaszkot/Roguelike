using OuaDII.Managers;
using OuaDII.Quests;
using Roguelike.Extensions;
using Roguelike.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Extensions
{
  public static class QuestExt
  {
    //public static Quests. AsOuadItem(this Roguelike.Discussions.DiscussionItem baseItem)
    //{
    //  return baseItem as Discussions.DiscussionItem;
    //}
    public static string Kind2String(QuestKind kind)
    {
      return kind.ToDescription();
    }

    public static QuestKind GetKind(this Quest quest)
    {
      var types = Enum.GetValues(typeof(QuestKind)).Cast<QuestKind>().ToList();
      var questDesc = types.Where(i => Kind2String(i) == quest.Tag).FirstOrDefault();
      return questDesc;
    }

    public static T GetQuestRequirement<T>(this Quest quest) where T : QuestRequirement
    {
      return quest.QuestRequirement as T;
    }

    public static void SetLootQuestRequirement<T>(this Quest quest, QuestManager qm, int lootQuantity) where T : QuestRequirement
    {
      var ex = quest.GetQuestRequirement<LootQuestRequirement>();
      ex.LootQuantity = lootQuantity;
      quest.Description = qm.GetQuestDesc(quest.GetKind(), ex);
    }
  }
}
