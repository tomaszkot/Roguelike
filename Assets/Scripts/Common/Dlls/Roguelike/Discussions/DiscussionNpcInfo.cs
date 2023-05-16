using Roguelike.Discussions;
using System.Collections;
using System.Collections.Generic;

namespace Roguelike.Discussions
{
  public abstract class DiscussionNpcInfo
  {
    string npc;
    protected SimpleInjector.Container container;

    public DiscussionNpcInfo(SimpleInjector.Container container, string npc)
    {
      this.npc = npc;
      this.container = container;
    }

    public DiscussionTopic GetTopicByLeftRight(string rightId, string leftId)
    {
      string left, right;
      GetRightLeft(rightId, out left, out right);
      var topic = container.GetInstance<DiscussionTopic> ();
      topic.Init(right, left);
      //var topic = new DiscussionTopic(container, right, left);
      topic.Left.Id = leftId;
      topic.Right.Id = rightId;
      if (topic.Right.Id == KnownSentenceKind.WhatsUp.ToString())
        topic.Right.KnownSentenceKind = KnownSentenceKind.WhatsUp;
      SetClipTimes(rightId, topic);
      return topic;
    }

    protected void SetClipTimes(string rightId, DiscussionTopic topic)
    {
      SetHeroClipTimes(rightId, topic);
      SetNpcClipTimes(rightId, topic);
    }

    private void SetHeroClipTimes(string rightId, DiscussionTopic topic)
    {
      float start;
      float end;
      string fileName;
      GetHeroClipTimes(rightId, out fileName, out start, out end);
      topic.Right.VoiceClipFileName = fileName;
      topic.Right.VoiceClipTimeFrom = start;
      topic.Right.VoiceClipTimeTo = end;
    }

    private void SetNpcClipTimes(string rightId, DiscussionTopic topic)
    {
      float start;
      float end;
      string fileName;
      GetNpcClipTimes(rightId, out fileName, out start, out end);
      topic.Left.VoiceClipFileName = fileName;
      topic.Left.VoiceClipTimeFrom = start;
      topic.Left.VoiceClipTimeTo = end;
    }

    //public DiscussionTopic GetTopic(string rightId, KnownSentenceKind rightKnown, QuestKind questKind, bool skipReward = false)
    //{
    //  string left, right;
    //  GetRightLeft(rightId, out left, out right);
    //  var topic = new DiscussionTopic(container, right, rightKnown, questKind, skipReward: skipReward);
    //  topic.Right.Id = rightId;
    //  SetClipTimes(rightId, topic);
    //  return topic;
    //}

    protected abstract void GetRightLeft(string rightId, out string left, out string right);
    public abstract void GetHeroClipTimes(string clipId, out string fileName, out float start, out float end);
    public virtual void GetNpcClipTimes(string clipId, out string fileName, out float start, out float end)
    {
      fileName = "";
      start = 0;
      end = 0;
    }
  }
}