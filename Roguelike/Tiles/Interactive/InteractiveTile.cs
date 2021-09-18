using Newtonsoft.Json;
using Roguelike.Abstract.Spells;
using Roguelike.Tiles.Abstract;
using System;

namespace Roguelike.Tiles.Interactive
{
  public enum InteractiveTileKind
  {
    Unset, Stairs, Doors, Barrel, TreasureChest,
    Trap, Lever, DeadBody
  }

  public interface IApproachableByHero
  {
    bool ApproachedByHero { get; set; }
    double DistanceFrom(Dungeons.Tiles.Tile other);
    bool Activate();
    string GetPlaceName();
    string ActivationSound { get; set; }

    event EventHandler Activated;
  }

  public class InteractiveTile : Dungeons.Tiles.InteractiveTile, IObstacle
  {
    private InteractiveTileKind _kind = InteractiveTileKind.Unset;
    public bool OutOfOrder { get; set; }
    public int Level
    {
      get;
      set;
    } = -1;//should match level of dungeon or a level of world part
    [JsonIgnore]
    public virtual string InteractSound { get; set; }
   // Difficulty difficulty { get; }

    public InteractiveTile(char symbol) : base(symbol)
    {

    }

    public virtual void ResetToDefaults()
    {
    }

    public InteractiveTileKind Kind
    {
      get => _kind;
      set
      {
        _kind = value;
        if (_kind == InteractiveTileKind.TreasureChest)
        {

        }
      }
    }

    public bool CanBeHitBySpell()
    {
      return true;
    }

    public virtual bool OnHitBy(ISpell damager)
    {
      return false;
    }

    public override string ToString()
    {
      var res = base.ToString();
      res += ", " + Kind + " Lvl:" + Level;
      return res;
    }

    /// <summary>
    /// Was hero ever close to the tile?
    /// </summary>
    public bool ApproachedByHero { get; set; }
  }
}
