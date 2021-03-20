using Roguelike.Discussions;
using Roguelike.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract.Discussions
{
  public interface IDiscussPanel
  {
    GenericListModel<DiscussionTopic> BindTopics(Roguelike.Discussions.DiscussionTopic discItem, Roguelike.Tiles.LivingEntities.AdvancedLivingEntity npc);
    void Hide();

    void Bind
    (
      Roguelike.Tiles.LivingEntities.AdvancedLivingEntity leftEntity,
      Roguelike.Tiles.LivingEntities.AdvancedLivingEntity rightEntity,
      GenericListModel<Roguelike.Discussions.DiscussionTopic> topics = null
    );
  }
}
