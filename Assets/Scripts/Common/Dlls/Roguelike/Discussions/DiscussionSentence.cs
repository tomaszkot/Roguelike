using Roguelike.Extensions;

namespace Roguelike.Discussions
{
  public class DiscussionSentence
  {
    public string Id { get; set; } = "";
    public string Body { get; set; } = "";

    public float VoiceClipTimeFrom { get; set; }

    public float VoiceClipTimeTo { get; set; }
    public string VoiceClipFileName { get; set; }


    public KnownSentenceKind KnownSentenceKind { get; set; }

    public DiscussionSentence() { }

    public DiscussionSentence(KnownSentenceKind knownSentenceKind) : this(knownSentenceKind.ToDescription(), knownSentenceKind)
    {
    }

    public DiscussionSentence(string body, KnownSentenceKind knownSentenceKind = KnownSentenceKind.Unset)
    {
      KnownSentenceKind = knownSentenceKind;
      Body = body;
      if(knownSentenceKind == KnownSentenceKind.Unset)
        Id = "";
      else
        Id = knownSentenceKind.ToString();
    }

    public DiscussionSentence(string body, string id)
    {
      Body = body;
      Id = id;
    }

    public override string ToString()
    {
      return Body;
    }
    //public DiscussionSentence Next { get;set;}
  }
}
