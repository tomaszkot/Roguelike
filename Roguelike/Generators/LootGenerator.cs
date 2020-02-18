using Roguelike.Attributes;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Generators
{
  public class LootGenerator
  {
    List<Scroll> scrolls = new List<Scroll>();

    void AddScrolls()
    {
      var loot = new Scroll();
      loot.tag = "fire_ball_scroll";
      loot.Kind = Spells.SpellKind.FireBall;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "NESW_fire_scroll";
      loot.Kind = Spells.SpellKind.NESWFireBall;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "cracked_stone_scroll";
      loot.Kind = Spells.SpellKind.CrackedStone;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "trap_stone_scroll";
      loot.Kind = Spells.SpellKind.Trap;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "skeleton_stone_scroll";
      loot.Kind = Spells.SpellKind.Skeleton;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "transform_scroll";
      loot.Kind = Spells.SpellKind.Transform;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "poison_ball_scroll";
      loot.Kind = Spells.SpellKind.PoisonBall;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "ice_ball_scroll";
      loot.Kind = Spells.SpellKind.IceBall;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "frighten_scroll";
      loot.Kind = Spells.SpellKind.Frighten;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "healing_scroll";
      loot.Kind = Spells.SpellKind.Healing;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "mana_shield_scroll";
      loot.Kind = Spells.SpellKind.ManaShield;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "telekinesis_scroll";
      loot.Kind = Spells.SpellKind.Telekinesis;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "mana_scroll";
      loot.Kind = Spells.SpellKind.Mana;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "rage_scroll";
      loot.Kind = Spells.SpellKind.Rage;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "weaken_scroll";
      loot.Kind = Spells.SpellKind.Weaken;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "iron_skin_scroll";
      loot.Kind = Spells.SpellKind.IronSkin;
      scrolls.Add(loot);
      //loot = new Scroll();
      //loot.tag = "mind_control_scroll";
      //loot.Kind = Spells.SpellKind.MindControl;
      //scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "teleport_scroll";
      loot.Kind = Spells.SpellKind.Teleport;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "call_merchant_scroll";
      loot.Kind = Spells.SpellKind.CallMerchant;
      scrolls.Add(loot);

      loot = new Scroll();
      loot.tag = "call_god_scroll";
      loot.Kind = Spells.SpellKind.CallGod;
      scrolls.Add(loot);

      //
      loot = new Scroll();
      loot.tag = "lighting_scroll";
      loot.Kind = Spells.SpellKind.LightingBall;
      scrolls.Add(loot);
    }

    public virtual Weapon GetRandomWeapon()
    {
      var item = new Weapon();
      item.Name = "Sword";
      item.Kind = Weapon.WeaponKind.Sword;
      item.EquipmentKind = EquipmentKind.Weapon;
      item.PrimaryStatKind = EntityStatKind.Attack;
      item.PrimaryStatValue = 5;
      return item;
    }

    public virtual Equipment GetRandomHelmet()
    {
      var item = new Equipment(EquipmentKind.Helmet);
      item.Name = "Helmet";
      //item.Kind = Weapon.WeaponKind.Sword;
      item.PrimaryStatKind = EntityStatKind.Defence;
      item.PrimaryStatValue = 2;
      return item;
    }

    public virtual Equipment GetRandomShield()
    {
      var item = new Equipment(EquipmentKind.Shield);
      item.Name = "Buckler";
      item.PrimaryStatKind = EntityStatKind.Defence;
      item.PrimaryStatValue = 1;
      return item;
    }

    public virtual Equipment GetRandomGloves()
    {
      var item = new Equipment(EquipmentKind.Gloves);
      item.Name = "Gloves";
      item.PrimaryStatKind = EntityStatKind.Defence;
      item.PrimaryStatValue = 1;
      return item;
    }

    public virtual Jewellery GetRandomJewellery(EntityStatKind sk, EquipmentKind eq = EquipmentKind.Unset)
    {
      if (eq == EquipmentKind.Amulet)
        return createAmulet(sk, 1, 3);
      return AddRing("", sk, 1, 3);
    }

    Jewellery createJewellery(EquipmentKind kind, int minDropDungeonLevel)
    {
      var juwell = new Jewellery();
      juwell.EquipmentKind = kind;
      juwell.MinDropDungeonLevel = minDropDungeonLevel;
      juwell.Price = 10;
      return juwell;
    }

    private Jewellery createAmulet(EntityStatKind sk, int minDungeonLevel, int statValue)
    {
      var jew = createJewellery(EquipmentKind.Amulet, minDungeonLevel);
      jew.tag = sk.ToString() + "_amulet";
      int AmuletStatAddition = 2;
      jew.SetPrimaryStat(sk, statValue + AmuletStatAddition);

      var name = "amulet of ";// "amulet of ";
      jew.Name = name + sk.ToString();
      if (sk == EntityStatKind.ResistCold || sk == EntityStatKind.ResistFire || sk == EntityStatKind.ResistPoison)
      {
        jew.Name += " resistance";
      }
      
      return jew;
    }

    private Jewellery AddRing(string asset, EntityStatKind sk, int minDropDungeonLevel,
      int statValue)
    {
      var jew = createJewellery(EquipmentKind.RingLeft, minDropDungeonLevel);
      jew.tag = asset;
      //juw.ExtendedInfo.Stats.SetFactor(EntityStatKind.ResistCold, 10);
      jew.SetPrimaryStat(sk, statValue);
      var name = "ring of ";
      jew.Name = name + sk;

      if (sk == EntityStatKind.ResistCold || sk == EntityStatKind.ResistFire || sk == EntityStatKind.ResistPoison)
      {
        jew.Name += " resistance";
      }
      jew.MinDropDungeonLevel = minDropDungeonLevel;
      
      return jew;
    }



    public virtual Loot GetRandomLoot()
    {
      var loot = new Mushroom();
      loot.SetKind(MushroomKind.Boletus);
      loot.tag = "mash3";
      return loot;
    }

    public virtual Loot GetRandomStackedLoot()
    {
      var loot = new Mushroom();
      loot.SetKind(MushroomKind.Boletus);
      loot.tag = "mash3";
      return loot;
    }
  }
}
