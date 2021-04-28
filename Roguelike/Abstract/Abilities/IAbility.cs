namespace Roguelike.Abstract.Abilities
{
  public enum AbilityKind { Unset, Passive, Active }

  public class IAbility
  {
    public AbilityKind AbilityKind { get; set; }
  }
}
