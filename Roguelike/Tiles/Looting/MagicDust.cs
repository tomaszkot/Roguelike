using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Tiles.Looting
{
  public class MagicDust : Loot
  {
    public const string MagicDustGuid = "4fe06985-47d3-2b24-bddf-99a4af2b1dfc";

    public MagicDust()
    {
      Symbol = '&';
#if ASCII_BUILD
      color = GoldColor;
#endif
      tag1 = "magic_dust";
      Name = "Magic Dust";
      Price = 5;
      //StackedInventoryId = new Guid(MagicDustGuid);
      //Revealed = true;
    }

    public override string PrimaryStatDescription
    {
      get { return "Part of a crafting recipe"; }
    }
  }
}
