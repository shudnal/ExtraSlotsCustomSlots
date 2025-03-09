using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using System.Collections.Generic;
using System.Linq;

namespace ExtraSlotsCustomSlots
{
    [BepInDependency("shudnal.ExtraSlots", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(AdventureBackpacksSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(BackpacksSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(BowsBeforeHoesSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(JewelcraftingNeckSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MagicPluginEarringSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CircletExtendedSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(HipLanternSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(JudesEquipmentBackpackSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    public class ExtraSlotsCustomSlots : BaseUnityPlugin
    {
        public const string pluginID = "shudnal.ExtraSlotsCustomSlots";
        public const string pluginName = "Extra Slots Custom Slots";
        public const string pluginVersion = "1.0.9";

        internal readonly Harmony harmony = new Harmony(pluginID);

        internal static readonly ConfigSync configSync = new ConfigSync(pluginID) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion };

        internal static ExtraSlotsCustomSlots instance;

        public static ConfigEntry<bool> configLocked;
        public static ConfigEntry<bool> loggingEnabled;
        public static ConfigEntry<string> slotsOrder;

        public static ConfigEntry<bool> adventureBackpackSlotEnabled;
        public static ConfigEntry<string> adventureBackpackSlotName;
        public static ConfigEntry<string> adventureBackpackSlotGlobalKey;
        public static ConfigEntry<string> adventureBackpackSlotItemDiscovered;

        public static ConfigEntry<bool> backpacksSlotEnabled;
        public static ConfigEntry<string> backpacksSlotName;
        public static ConfigEntry<string> backpacksSlotGlobalKey;
        public static ConfigEntry<string> backpacksSlotItemDiscovered;

        public static ConfigEntry<bool> bbhQuiverSlotEnabled;
        public static ConfigEntry<string> bbhQuiverSlotName;
        public static ConfigEntry<string> bbhQuiverSlotGlobalKey;
        public static ConfigEntry<string> bbhQuiverSlotItemDiscovered;

        public static ConfigEntry<bool> circletExtendedSlotEnabled;
        public static ConfigEntry<string> circletExtendedSlotName;
        public static ConfigEntry<string> circletExtendedSlotGlobalKey;
        public static ConfigEntry<string> circletExtendedSlotItemDiscovered;

        public static ConfigEntry<bool> hipLanternSlotEnabled;
        public static ConfigEntry<string> hipLanternSlotName;
        public static ConfigEntry<string> hipLanternSlotGlobalKey;
        public static ConfigEntry<string> hipLanternSlotItemDiscovered;

        public static ConfigEntry<bool> jewelcraftingNeckSlotEnabled;
        public static ConfigEntry<string> jewelcraftingNeckSlotName;
        public static ConfigEntry<string> jewelcraftingNeckSlotGlobalKey;
        public static ConfigEntry<string> jewelcraftingNeckSlotItemDiscovered;

        public static ConfigEntry<bool> jewelcraftingRingSlotEnabled;
        public static ConfigEntry<string> jewelcraftingRingSlotName;
        public static ConfigEntry<string> jewelcraftingRingSlotGlobalKey;
        public static ConfigEntry<string> jewelcraftingRingSlotItemDiscovered;

        public static ConfigEntry<bool> magicPluginTomeSlotEnabled;
        public static ConfigEntry<string> magicPluginTomeSlotName;
        public static ConfigEntry<string> magicPluginTomeSlotGlobalKey;
        public static ConfigEntry<string> magicPluginTomeSlotItemDiscovered;

        public static ConfigEntry<bool> magicPluginEarringSlotEnabled;
        public static ConfigEntry<string> magicPluginEarringSlotName;
        public static ConfigEntry<string> magicPluginEarringSlotGlobalKey;
        public static ConfigEntry<string> magicPluginEarringSlotItemDiscovered;

        public static ConfigEntry<bool> judesEquipmentBackpackSlotEnabled;
        public static ConfigEntry<string> judesEquipmentBackpackSlotName;
        public static ConfigEntry<string> judesEquipmentBackpackSlotGlobalKey;
        public static ConfigEntry<string> judesEquipmentBackpackSlotItemDiscovered;

        private void Awake()
        {
            instance = this;

            ConfigInit();
            _ = configSync.AddLockingConfigEntry(configLocked);

            UpdateSlots();

            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            Config.Save();
            instance = null;
            harmony?.UnpatchSelf();
        }

        public void ConfigInit()
        {
            configLocked = config("General", "Lock Configuration", defaultValue: true, "Configuration is locked and can be changed by server admins only. ");
            loggingEnabled = config("General", "Logging enabled", defaultValue: false, "Enable logging. [Not synced with Server]", synchronizedSetting: false);
            slotsOrder = config("General", "Slots order", defaultValue: CustomSlot.VanillaOrder, 
                new ConfigDescription("Comma-separated slot ID order of custom slots", null, new CustomConfigs.ConfigurationManagerAttributes { CustomDrawer = CustomConfigs.DrawOrderedFixedStrings(",") }));

            slotsOrder.SettingChanged += (s, e) => UpdateSlots();

            adventureBackpackSlotEnabled = config("Mod - Adventure Backpacks", "Enabled", true, "Enable adventure backpack slot. Restart the game after change to avoid potential issues.");
            adventureBackpackSlotName = config("Mod - Adventure Backpacks", "Name", "Backpack", "Slot name");
            adventureBackpackSlotGlobalKey = config("Mod - Adventure Backpacks", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            adventureBackpackSlotItemDiscovered = config("Mod - Adventure Backpacks", "Items discovered", "$vapok_mod_item_backpack_meadows,$vapok_mod_item_backpack_blackforest,$vapok_mod_item_backpack_swamp,$vapok_mod_item_backpack_mountains,$vapok_mod_item_backpack_plains,$vapok_mod_item_backpack_mistlands,$vapok_mod_item_rugged_backpack,$vapok_mod_item_arctic_backpack", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            adventureBackpackSlotEnabled.SettingChanged += (s, e) => { AdventureBackpacksCustomSlot.AdventureBackpackItem.PatchBackpackItemOnConfigChange(); UpdateSlots(); };

            backpacksSlotEnabled = config("Mod - Backpacks", "Enabled", true, "Enable backpack slot");
            backpacksSlotName = config("Mod - Backpacks", "Name", "$bp_backpack_slot_name", "Slot name");
            backpacksSlotGlobalKey = config("Mod - Backpacks", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            backpacksSlotItemDiscovered = config("Mod - Backpacks", "Items discovered", "$item_explorer", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            backpacksSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            bbhQuiverSlotEnabled = config("Mod - BowsBeforeHoes", "Enabled", true, "Enable quiver slot");
            bbhQuiverSlotName = config("Mod - BowsBeforeHoes", "Name", "$bbh_slot_quiver", "Slot name");
            bbhQuiverSlotGlobalKey = config("Mod - BowsBeforeHoes", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            bbhQuiverSlotItemDiscovered = config("Mod - BowsBeforeHoes", "Items discovered", "$item_quiver_blackforest,$item_quiver_seeker,$item_quiver_leather,$item_quiver_odinplus,$item_quiver_plainslox", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            bbhQuiverSlotEnabled.SettingChanged += (s, e) => { UpdateSlots(); BowsBeforeHoesCompat.PatchBackpackItemOnConfigChange(); };

            circletExtendedSlotEnabled = config("Mod - CircletExtended", "Enabled", true, "Enable circlet slot");
            circletExtendedSlotName = config("Mod - CircletExtended", "Name", "Circlet", "Slot name");
            circletExtendedSlotGlobalKey = config("Mod - CircletExtended", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            circletExtendedSlotItemDiscovered = config("Mod - CircletExtended", "Items discovered", "$item_helmet_dverger", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            circletExtendedSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            hipLanternSlotEnabled = config("Mod - HipLantern", "Enabled", true, "Enable hip lantern slot");
            hipLanternSlotName = config("Mod - HipLantern", "Name", "Lantern", "Slot name");
            hipLanternSlotGlobalKey = config("Mod - HipLantern", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            hipLanternSlotItemDiscovered = config("Mod - HipLantern", "Items discovered", "$item_hiplantern", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            hipLanternSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            jewelcraftingNeckSlotEnabled = config("Mod - Jewelcrafting - Neck", "Enabled", true, "Enable neck slot");
            jewelcraftingNeckSlotName = config("Mod - Jewelcrafting - Neck", "Name", "Neck", "Slot name");
            jewelcraftingNeckSlotGlobalKey = config("Mod - Jewelcrafting - Neck", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            jewelcraftingNeckSlotItemDiscovered = config("Mod - Jewelcrafting - Neck", "Items discovered", "$jc_necklace_red,$jc_necklace_green,$jc_necklace_blue,$jc_necklace_yellow,$jc_necklace_purple,$jc_necklace_orange,$jc_necklace_dvergrnecklace,$jc_necklace_eitrnecklace,$jc_necklace_fireresistnecklace,$jc_necklace_frostresistnecklace,$jc_necklace_poisonresistnecklace,", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            jewelcraftingNeckSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            jewelcraftingRingSlotEnabled = config("Mod - Jewelcrafting - Ring", "Enabled", true, "Enable Ring slot");
            jewelcraftingRingSlotName = config("Mod - Jewelcrafting - Ring", "Name", "Finger", "Slot name");
            jewelcraftingRingSlotGlobalKey = config("Mod - Jewelcrafting - Ring", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            jewelcraftingRingSlotItemDiscovered = config("Mod - Jewelcrafting - Ring", "Items discovered", "$jc_ring_purple,$jc_ring_green,$jc_ring_red,$jc_ring_blue,$jc_ring_black,$jc_ring_dvergrring,$jc_ring_eitrring,$jc_ring_fireresistring,$jc_ring_frostresistring,$jc_ring_poisonresistring", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            jewelcraftingRingSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            magicPluginTomeSlotEnabled = config("Mod - Magic Plugin - Tome", "Enabled", true, "Enable tome slot");
            magicPluginTomeSlotName = config("Mod - Magic Plugin - Tome", "Name", "$bmp_tomeslot", "Slot name");
            magicPluginTomeSlotGlobalKey = config("Mod - Magic Plugin - Tome", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            magicPluginTomeSlotItemDiscovered = config("Mod - Magic Plugin - Tome", "Items discovered", "$bmp_advance_magicbook,$bmp_beginners_magicbook", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            magicPluginTomeSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            magicPluginEarringSlotEnabled = config("Mod - Magic Plugin - Earring", "Enabled", true, "Enable earring slot");
            magicPluginEarringSlotName = config("Mod - Magic Plugin - Earring", "Name", "$bmp_earringslot", "Slot name");
            magicPluginEarringSlotGlobalKey = config("Mod - Magic Plugin - Earring", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            magicPluginEarringSlotItemDiscovered = config("Mod - Magic Plugin - Earring", "Items discovered", "$bmp_dvergr_earring,$bmp_fireresist_earring,$bmp_frostresist_earring,$bmp_poisonresist_earring,$bmp_eitr_earring", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            magicPluginEarringSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            judesEquipmentBackpackSlotEnabled = config("Mod - Judes Equipment", "Enabled", true, "Enable Judes Equipment backpack slot. Restart the game after change to avoid potential issues.");
            judesEquipmentBackpackSlotName = config("Mod - Judes Equipment", "Name", "Backpack", "Slot name");
            judesEquipmentBackpackSlotGlobalKey = config("Mod - Judes Equipment", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            judesEquipmentBackpackSlotItemDiscovered = config("Mod - Judes Equipment", "Items discovered", "$BackpackSimple,$BackpackHeavy", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            judesEquipmentBackpackSlotEnabled.SettingChanged += (s, e) => { JudesEquipmentBackpacksCustomSlot.JudesEquipmentBackpackItem.PatchBackpackItemOnConfigChange(); UpdateSlots(); };
        }

        public static void UpdateSlots()
        {
            CustomSlot.slots.Do(slot => slot.RemoveSlot());
            CustomSlot.slots.Clear();

            // In case slots order config value was changed outside of configuration manager 
            // Iterate slots order from config value then add what's left from vanilla slots order

            slotsOrder.Value.Split(',').Select(s => s.Trim()).Where(s => !s.IsNullOrWhiteSpace()).Do(InitSlot);

            List<string> vanillaSlots = CustomSlot.VanillaOrder.Split(',').ToList();
            
            CustomSlot.slots.Do(slot => vanillaSlots.Remove(slot.slotID));

            vanillaSlots.Do(InitSlot);

            CustomSlot.slots.Do(TryAddSlot);
        }

        public static void TryAddSlot(CustomSlot slot)
        {
            if (slot.RemoveSlot())
                LogInfo($"Slot {slot} was removed");

            if (slot.AddSlot())
                LogInfo($"Slot {slot} was added");
            else if (slot.initialized)
                LogWarning($"Error while trying to add new slot {slot}.");
        }

        public static void InitSlot(string slot)
        {
            switch (slot)
            {
                case BackpacksSlot.ID when backpacksSlotEnabled.Value:
                    new BackpacksSlot();
                    break;
                case AdventureBackpacksSlot.ID when adventureBackpackSlotEnabled.Value:
                    new AdventureBackpacksSlot();
                    break;
                case JudesEquipmentBackpackSlot.ID when judesEquipmentBackpackSlotEnabled.Value:
                    new JudesEquipmentBackpackSlot();
                    break;
                case MagicPluginTomeSlot.ID when magicPluginTomeSlotEnabled.Value:
                    new MagicPluginTomeSlot();
                    break;
                case MagicPluginEarringSlot.ID when magicPluginEarringSlotEnabled.Value:
                    new MagicPluginEarringSlot();
                    break;
                case JewelcraftingNeckSlot.ID when jewelcraftingNeckSlotEnabled.Value:
                    new JewelcraftingNeckSlot();
                    break;
                case JewelcraftingRingSlot.ID when jewelcraftingRingSlotEnabled.Value:
                    new JewelcraftingRingSlot();
                    break;
                case BowsBeforeHoesSlot.ID when bbhQuiverSlotEnabled.Value:
                    new BowsBeforeHoesSlot();
                    break;
                case CircletExtendedSlot.ID when circletExtendedSlotEnabled.Value:
                    new CircletExtendedSlot();
                    break;
                case HipLanternSlot.ID when hipLanternSlotEnabled.Value:
                    new HipLanternSlot();
                    break;
                default: break;
            }
        }

        public static void LogInfo(object data)
        {
            if (loggingEnabled.Value)
                instance.Logger.LogInfo(data);
        }

        public static void LogMessage(object data)
        {
            instance.Logger.LogMessage(data);
        }

        public static void LogWarning(object data)
        {
            instance.Logger.LogWarning(data);
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, defaultValue, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true) => config(group, name, defaultValue, new ConfigDescription(description), synchronizedSetting);
    }
}
