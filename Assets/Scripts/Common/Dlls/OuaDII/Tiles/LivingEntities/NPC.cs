using Dungeons.Core;
using OuaDII.Discussions;
using OuaDII.Managers;
using OuaDII.Quests;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.Tiles.LivingEntities
{
  public class NPC : Roguelike.Tiles.LivingEntities.NPC
  {
    public NPC(Container cont) : base(cont)
    {
      this.Discussion = cont.GetInstance<Roguelike.Discussions.Discussion>();
    }

    public Discussion OuaDDiscussion
    {
      get { return base.Discussion as Discussion; }
    }

    public bool AcceptQuest(QuestManager mgr, Hero hero, QuestKind questKind)
    {
      var acc = OuaDDiscussion.AcceptQuest(questKind);
      if (acc)
      {
        return mgr.EnsureQuestAssigned(questKind);
      }

      return false;
    }
  }
}
