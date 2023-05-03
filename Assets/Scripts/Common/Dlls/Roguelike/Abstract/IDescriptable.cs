namespace Roguelike.Abstract
{
  public interface IDescriptable
  {
    string GetPrimaryStatDescription();
    string[] GetExtraStatDescription(bool currentLevel);

    bool Revealed { get; set; }
    string Name { get; set; }
  }
}
