using OuaDII.Quests;
using OuaDII.UI.Models;
using Roguelike.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Extensions
{
  public static class DiscussionExt
  {
    public static Discussions.DiscussionTopic AsOuaDItem(this Roguelike.Discussions.DiscussionTopic baseItem)
    {
      return baseItem as Discussions.DiscussionTopic;
    }

    public static Discussions.Discussion AsOuaDDiscussion(this Roguelike.Discussions.Discussion baseItem)
    {
      return baseItem as Discussions.Discussion;
    }
  }
}
