using Dungeons.Core;

namespace Roguelike.Tiles.LivingEntities
{
  public class Animal : LivingEntity
  {
    public int RandMoveCoolDown = 1;
    public bool PreventMove { get; set; } = false;

    internal override bool CanMakeRandomMove()
    {
      if (PreventMove)
        return false;
      if(RandMoveCoolDown > 0)
        RandMoveCoolDown--;
      var res = RandMoveCoolDown == 0;
      if (res)
        RandMoveCoolDown = RandHelper.GetRandomInt(4);
      return res;
    }
  }
}
