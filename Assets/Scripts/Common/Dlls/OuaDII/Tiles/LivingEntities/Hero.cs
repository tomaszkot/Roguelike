using Algorithms;
using Newtonsoft.Json;
using OuaDII.Extensions;
using OuaDII.LootContainers;
using OuaDII.Quests;
using OuaDII.Tiles.Looting;
using Roguelike.Abilities;
using Roguelike.Abstract.HotBar;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Managers;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;

namespace OuaDII.Tiles.LivingEntities
{
  public class Hero : Roguelike.Tiles.LivingEntities.Hero
  {
    private Roguelike.LootContainers.InventoryOwner chest = null;
    public LootContainers.Inventory OuadIIInventory { get { return Inventory as LootContainers.Inventory; } }
    ShortcutsBar shortcutsBar;

    public ShortcutsBar ShortcutsBar
    {
      get { return shortcutsBar; }
      set
      {
        shortcutsBar = value;
        shortcutsBar.Owner = this;
        if (OuadIIInventory != null)
          OuadIIInventory.ShortcutsBar = value;

        shortcutsBar.ActiveItemDigitSet += (s,e) => {
          //if (ActiveFightItem != null && ActiveFightItem != RecentlyActivatedFightItem)
          //  RecentlyActivatedFightItem = ActiveFightItem;
          //else 
          //if (SelectedActiveAbility !=null)
          //{
          //  if (SelectedActiveAbility.Kind == AbilityKind.Rage)
          //  {
          //    LastingEffectsSet.AddPercentageLastingEffect(Roguelike.Effects.EffectType.Rage, new AbilityLastingEffectSrc(SelectedActiveAbility), null);
          //  }
          //}
        };
      }
    }


    public override FightItem GetFightItemFromActiveProjectileAbility()
    {
      var ab = GetActivePhysicalProjectileAbility();
      if (ab == null)
        return null;
      var bowLikeKinds = Weapon.AllBowLikeAmmoKinds;
      var ind = -1;
      foreach (var fi in bowLikeKinds)
      {
        ind = shortcutsBar.GetProjectileDigit(fi);
        if (ind >= 0)
          break;
      }
      if (ind < 0)
        ind = shortcutsBar.GetProjectileDigit(FightItemKind.WeightedNet);
      if (ind < 0)
        return null;
      return shortcutsBar.GetAt(ind) as FightItem;
    }
    public override Ability GetActivePhysicalProjectileAbility()
    {
      var ab = SelectedActiveAbility;
      if (ab == null)
        return null;
      if (ab.Kind == AbilityKind.ArrowVolley ||
         ab.Kind == AbilityKind.PiercingArrow ||
        ab.Kind == AbilityKind.PerfectHit ||
        ab.Kind == AbilityKind.WeightedNet ||
        ab.Kind == AbilityKind.Smoke
        )
        return ab;
      return null;
    }
        
    public override FightItem ActiveFightItem 
    {
      get 
      {
        if (shortcutsBar.ActiveItemDigit >= 0)
        {
          return shortcutsBar.GetAt(shortcutsBar.ActiveItemDigit) as FightItem;
        }

        return null;
      }
      set 
      {
        if (value == null)
          return;
        var itemDigit = shortcutsBar.GetProjectileDigit(value.FightItemKind);
        if (itemDigit >= 0)
        {
          shortcutsBar.ActiveItemDigit = itemDigit;
        }
      }
    }

    [JsonIgnore]
    public override Ability SelectedActiveAbility
    {
      get
      {
        if (shortcutsBar.ActiveItemDigit >= 0)
        {
          var itemAt = shortcutsBar.GetAt(shortcutsBar.ActiveItemDigit);
          var ab = itemAt as Ability;
          if (ab == null)
          {
            var pfi = itemAt as ProjectileFightItem;
            if (pfi != null)
            {
              var ak = FightItem.GetAbilityKind(pfi);
              if (ak != AbilityKind.Unset)
                ab = GetActiveAbility(ak);
            }
          }

          if (ab != null)
            return ab as ActiveAbility;
        }

        return null;
      }
      set
      {
        if (value == null)
          return;
        var itemDigit = shortcutsBar.GetItemDigit(value);
        if (itemDigit >= 0)
        {
          shortcutsBar.ActiveItemDigit = itemDigit;
          if (shortcutsBar.ActiveItemDigit == itemDigit && value is ActiveAbility ab)
          {
            //AddLastingEffectFromAbility(ab);
          }
        }
      }
    }

    

    public override SpellSource ActiveManaPoweredSpellSource
    {
      get
      {
        if (shortcutsBar.ActiveItemDigit >= 0)
        {
          return shortcutsBar.GetAt(shortcutsBar.ActiveItemDigit) as SpellSource;
        }
        
        return base.ActiveManaPoweredSpellSource;
      }
    }

   

    public override Roguelike.LootContainers.Inventory Inventory
    {
      get => base.Inventory;

      set
      {
        base.Inventory = value;

        if (ShortcutsBar != null)
          OuadIIInventory.ShortcutsBar = ShortcutsBar;
      }
    }

    public Hero(Container container) : base(container)
    {
      this.Container = container;
      ShortcutsBar = new ShortcutsBar(container);
      var chest = new Roguelike.LootContainers.InventoryOwner();

      var chestInv = new Inventory(container);
      chestInv.InvBasketKind = Roguelike.LootContainers.InvBasketKind.HeroChest;
      chest.Inventory = chestInv;
      Chest = chest;
    }

    public GodStatue GetGodStatue(GodKind godKind)
    {
      var statues = Inventory.GetItems<GodStatue>().ToList();
      return statues.SingleOrDefault(i => i.GodKind == godKind);
    }

    public bool HasGodStatue(GodKind godKind)
    {
      return GetGodStatue(godKind) != null;
    }

    public override void OnContextSwitched(Container container)
    {
      base.OnContextSwitched(container);
      var em = container.GetInstance<EventsManager>();

      ShortcutsBar.AutoSelectItem = (IHotbarItem loot) => 
      {
        var wpn = GetActiveWeapon();
        var fi = loot as FightItem;
        if (fi != null && fi.FightItemKind.IsBowLikeAmmunition()  && (wpn == null || !wpn.IsBowLike)) 
          return false;

        return true;
      };
      em.EventAppended += EventsManager_ActionAppended;
    }

    private void EventsManager_ActionAppended(object sender, Roguelike.Events.GameEvent e)
    {
      if (e is ShortcutsBarAction sa)
      {
        if (sa.Kind == ShortcutsBarActionKind.ShortcutsBarChanged)
        {
          //if (ActiveShortcutsBarItemDigit == -1)
          //{
          //  var item = shortcutsBar.GetAt(sa.Digit);
          //  if (item != null)
          //    ActiveShortcutsBarItemDigit = sa.Digit;// that would have to selecet a frame in UI bar!
          //}
        }
      }

    }

    public override Container Container
    {
      get => base.Container;
      set
      {
        base.Container = value;
      }
    }

    public int ActiveShortcutsBarItemDigit
    {
      get { return shortcutsBar.ActiveItemDigit; }
      set
      {
        var itemAt = shortcutsBar.GetAt(value);
        if (itemAt is Ability ab)
        {
          if (value < 0)
          { 
            
          }
          else
            SelectedActiveAbility = ab;
        }
        else
          shortcutsBar.ActiveItemDigit = value;
      }
    }

    public Roguelike.LootContainers.InventoryOwner Chest
    {
      get => chest;
      set
      {
        chest = value;
        chest.Inventory.Owner = this;

      }
    }

    public Roguelike.Quests.Quest GetQuest(string lootName)
    {
      return this.Quests.Where(i => (i.QuestRequirement is LootQuestRequirement lqr) && lqr.LootName == lootName).SingleOrDefault();
    }

    public Roguelike.Quests.Quest GetQuest(QuestKind questKind)
    {
      return Quests.Where(i => i.GetKind() == questKind).SingleOrDefault();
    }

    public LootQuestRequirement GetLootQuestRequirement(QuestKind questKind)
    {
      return GetQuest(questKind).QuestRequirement as LootQuestRequirement;
    }

    public override List<PathFinderNode> PathToTarget 
    {
      get => base.PathToTarget; 
      set => base.PathToTarget = value; 
    }

    public override bool CanUseAbility(AbilityKind kind, AbstractGameLevel node, out string reason, Dungeons.Tiles.IHitable victim = null)
    {
      reason = "";

      var baseCan = base.CanUseAbility(kind, node, out reason, victim);
      if (!baseCan)
        return false;
      if (this.Abilities.IsActive(kind))
      {
        var ab = GetActiveAbility(kind);
        var ready = SelectedActiveAbilityKind == kind && CanUseAbilityDueToCoolDown(ab, ref reason);
        if (ready)
        {
          if (kind == AbilityKind.OpenWound || kind == AbilityKind.ElementalVengeance)
          {
            var wpn = this.GetCurrentEquipment(EquipmentKind.Weapon) as Weapon;
            if (wpn == null)
              return false;
            if (kind == AbilityKind.ElementalVengeance && wpn.IsBowLike)
              return false;
            if (kind == AbilityKind.OpenWound && wpn.Kind != Weapon.WeaponKind.Sword && wpn.Kind != Weapon.WeaponKind.Dagger && wpn.Kind != Weapon.WeaponKind.Axe)
              return false;
          }
          if (kind == AbilityKind.ZealAttack)
          {
            //shall it depend on weapon ?
          }

          return true;
        }
        return false;
      }
      return true;

    }

    public override AbilityKind SelectedActiveAbilityKind
    {
      get { return SelectedActiveAbility != null ? SelectedActiveAbility.Kind : AbilityKind.Unset; }
    }

    public override SpellSource GetIdentificationSpellSource()
    {
      if (ActiveSpellSource != null)
      { 
        if(ActiveSpellSource.Kind == Roguelike.Spells.SpellKind.Identify)
          return ActiveSpellSource;
      }
      
      return base.GetIdentificationSpellSource();
    }
  }
}
