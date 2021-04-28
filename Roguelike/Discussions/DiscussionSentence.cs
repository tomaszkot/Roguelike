namespace Roguelike.Discussions
{
  public class DiscussionSentence
  {
    public string Id { get; set; } = "";
    public string Body { get; set; } = "";

    public DiscussionSentence() { }
    public DiscussionSentence(string body)
    {
      Body = body;
      Id = Body;
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
