using Roguelike.Abstract.Projectiles;
using Roguelike.Abstract.Spells;
using Roguelike.Spells;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public class WeaponSpellSource : SpellSource
  {
    public Weapon Weapon { get; set; }

    public int Level
    {
      get
      {
        return this.Weapon.LevelIndex;
      }
    }
    int initChargesCount = 0;
    public int RestoresCount { get; set; }

    public WeaponSpellSource(Weapon weapon, SpellKind kind, int chargesCount = 15) : base(kind)
    {
      this.Weapon = weapon;
      InitChargesCount = chargesCount;
    }

    public void Restore()
    {
      RestoresCount++;
      RestoredChargesCount = initChargesCount - 2 * RestoresCount;
      Count = RestoredChargesCount;
    }

    public int InitChargesCount
    {
      get => initChargesCount;
      set
      {
        initChargesCount = value;
        Count = value;
        RestoredChargesCount = Count;
      }
    }
    public int RestoredChargesCount { get; set; }

    public override string GetExtraStatDescriptionFormatted(LivingEntity caller)
    {
      var statDescCurrent = GetExtraStatDescription(caller, true);
      if (statDescCurrent == null)
        return "";
      //var res = "Level: " + Level + "\r\n";
      var str = string.Join("\r\n", statDescCurrent.GetDescription());
      //res += str;

      return str;
    }

    public override ISpell CreateSpell(LivingEntity caller)
    {
      ISpell spell = null;
      var weapon = Weapon;
      switch (this.Kind)
      {
        case SpellKind.FireBall:
          spell = new FireBallSpell(caller, weapon);
          break;
        case SpellKind.PoisonBall:
          spell = new PoisonBallSpell(caller, weapon);
          break;
        case SpellKind.IceBall:
          spell = new IceBallSpell(caller, weapon);
          break;
      }

      if(spell == null)
        spell = base.CreateSpell(caller);

      if (spell is IProjectile proj)
      {
        var rangeInc = 0;
        if (Level > 1)
        {
          if (Level == 2)
            rangeInc = 1;
          else
            rangeInc = Level/2;
        }
        proj.Range += rangeInc;
      }

      return spell;
    }

  }
}
