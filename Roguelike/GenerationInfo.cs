using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  public class GenerationInfo : Dungeons.GenerationInfo
  {
    public const int LevelUpPoints = 5;
    public const float NextExperienceIncrease = 1.002f;

    public bool GenerateEnemies { get; set; } = true && !ForceEmpty;

    public bool GenerateLoot { get; set; } = true && !ForceEmpty;

    public bool GenerateInteractiveTiles { get; set; } = true && !ForceEmpty;

    public override void MakeEmpty()
    {
      base.MakeEmpty();
      GenerateEnemies = false;
      GenerateInteractiveTiles = false;
      GenerateLoot = false;
    }
  }
}
