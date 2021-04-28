using Roguelike.Discussions;
using Roguelike.UI.Models;

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
