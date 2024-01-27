namespace Roguelike.Help
{
  public enum HintKind
  {
    Unset, LootCollectShortcut, BulkLootCollectShortcut, ShowCraftingPanel, HeroLevelTooLow, CanNotPutOnUnidentified,
    LootHightlightShortcut, UseProjectile, UseElementalWeaponProjectile, SwapActiveWeapon, SwapActiveHotBarByUI, PreviewSwapActiveHotBar,
    FoundPlace, SecretLevel, EnchantEquipment, WatchoutEnemyLevel, TalkToNPCs, AbilitiesPassive, AbilitiesActive, AbilitiesSpells,
    EquipAlly, QuicklySell
  }

  /// <summary>
  /// s
  /// </summary>
  public class HintItem
  {
    public string Info { get; set; }
    public string Asset { get; set; }
    public bool Shown { get; set; }
    public HintKind Kind { get; set; }
    public int KeyCode { get; set; }
  }
}
