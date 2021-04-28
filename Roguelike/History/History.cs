namespace Roguelike
{
  namespace History
  {
    public class HistoryContent
    {
      public Hints.HintHistory Hints { get; set; } = new Hints.HintHistory();
      public LootHistory Looting { get; set; } = new LootHistory();

    }
  }
}
