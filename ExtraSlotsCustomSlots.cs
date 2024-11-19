using BepInEx;
using BepInEx.Configuration;
using CheatDeath;
using HarmonyLib;
using ServerSync;
using System.Collections.Generic;
using System.Linq;

namespace ExtraSlotsCustomSlots
{
    [BepInDependency("shudnal.ExtraSlots", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(BackpacksSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(AdventureBackpacksSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    public class ExtraSlotsCustomSlots : BaseUnityPlugin
    {
        public const string pluginID = "shudnal.ExtraSlotsCustomSlots";
        public const string pluginName = "Extra Slots Custom Slots";
        public const string pluginVersion = "1.0.0";

        internal readonly Harmony harmony = new Harmony(pluginID);

        internal static readonly ConfigSync configSync = new ConfigSync(pluginID) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion };

        internal static ExtraSlotsCustomSlots instance;

        public static ConfigEntry<bool> configLocked;
        public static ConfigEntry<bool> loggingEnabled;
        public static ConfigEntry<string> slotsOrder;

        public static ConfigEntry<bool> backpackSlotEnabled;
        public static ConfigEntry<string> backpackSlotName;
        public static ConfigEntry<string> backpackSlotGlobalKey;
        public static ConfigEntry<string> backpackSlotItemDiscovered;

        public static ConfigEntry<bool> adventureBackpackSlotEnabled;
        public static ConfigEntry<string> adventureBackpackSlotName;
        public static ConfigEntry<string> adventureBackpackSlotGlobalKey;
        public static ConfigEntry<string> adventureBackpackSlotItemDiscovered;

        private void Awake()
        {
            instance = this;

            ConfigInit();
            _ = configSync.AddLockingConfigEntry(configLocked);

            harmony.PatchAll();

            UpdateSlots();
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

            backpackSlotEnabled = config("Mod - Backpacks", "Enabled", true, "Enable backpack slot");
            backpackSlotName = config("Mod - Backpacks", "Name", "$bp_backpack_slot_name", "Slot name");
            backpackSlotGlobalKey = config("Mod - Backpacks", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            backpackSlotItemDiscovered = config("Mod - Backpacks", "Items discovered", "$item_explorer", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            backpackSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            adventureBackpackSlotEnabled = config("Mod - Adventure Backpacks", "Enabled", true, "Enable adventure backpack slot");
            adventureBackpackSlotName = config("Mod - Adventure Backpacks", "Name", "AdvPack", "Slot name");
            adventureBackpackSlotGlobalKey = config("Mod - Adventure Backpacks", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            adventureBackpackSlotItemDiscovered = config("Mod - Adventure Backpacks", "Items discovered", "$item_explorer", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            adventureBackpackSlotEnabled.SettingChanged += (s, e) => UpdateSlots();
        }

        public static void UpdateSlots()
        {
            CustomSlot.slots.Do(slot => slot.RemoveSlot());
            CustomSlot.slots.Clear();

            // In case slots order config value was changed outside of configuration manager 
            // Iterate slots order from config value then add what's left from vanilla slots order

            slotsOrder.Value.Split(',').Select(s => s.Trim()).Where(s => !s.IsNullOrWhiteSpace()).Do(TryAddSlot);

            List<string> vanillaSlots = CustomSlot.VanillaOrder.Split(',').Select(slot => CustomSlot.GetSlotID(slot)).ToList();
            CustomSlot.slots.Do(slot => { slot.AddSlot(); vanillaSlots.Remove(slot.slotID); } );

            vanillaSlots.Do(TryAddSlot);
        }

        public static void TryAddSlot(string slot)
        {
            switch (slot)
            {
                case BackpacksSlot.ID when backpackSlotEnabled.Value:
                    new BackpacksSlot();
                    break;
                case AdventureBackpacksSlot.ID when adventureBackpackSlotEnabled.Value:
                    new AdventureBackpacksSlot();
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
