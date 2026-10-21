using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ConditionalConfigSync;
using System.Collections.Generic;
using System.Linq;

namespace ExtraSlotsCustomSlots
{
    [BepInDependency("shudnal.ExtraSlots", "1.2.1")]
    [BepInDependency(AdventureBackpacksSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(BackpacksSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(BowsBeforeHoesSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(JewelcraftingNeckSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(MagicPluginEarringSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CircletExtendedSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(HipLanternSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(JudesEquipmentBackpackSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(EpicLootCompatibility.EpicLootGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(RustyBagsSlot.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(VikingsSummoner.pluginID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("_shudnal.ConditionalConfigSync", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    public class ExtraSlotsCustomSlots : BaseUnityPlugin
    {
        public const string pluginID = "shudnal.ExtraSlotsCustomSlots";
        public const string pluginName = "Extra Slots Custom Slots";
        public const string pluginVersion = "1.0.23";

        internal readonly Harmony harmony = new Harmony(pluginID);

        internal static readonly ConfigSync configSync = new ConfigSync(pluginID) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion, ModRequired = false };

        internal static ExtraSlotsCustomSlots instance;

        public static ConfigEntry<bool> configLocked;
        public static ConfigEntry<bool> loggingEnabled;
        public static ConfigEntry<string> slotsOrder;

        private static bool updatingSlots;
        private static bool slotUpdatePending;

        public static ConfigEntry<bool> adventureBackpackSlotEnabled;
        public static ConfigEntry<string> adventureBackpackSlotName;
        public static ConfigEntry<string> adventureBackpackSlotGlobalKey;
        public static ConfigEntry<string> adventureBackpackSlotItemDiscovered;
        public static ConfigEntry<bool> adventureBackpackItemIsVisible;

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
        public static ConfigEntry<bool> judesEquipmentBackpackItemIsVisible;

        public static ConfigEntry<bool> rustyBagsSlotEnabled;
        public static ConfigEntry<string> rustyBagsSlotName;
        public static ConfigEntry<string> rustyBagsSlotGlobalKey;
        public static ConfigEntry<string> rustyBagsSlotItemDiscovered;
        public static ConfigEntry<bool> rustyBagsSlotCombineWithQuiver;

        public static ConfigEntry<bool> rustyBagsQuiverSlotEnabled;
        public static ConfigEntry<string> rustyBagsQuiverSlotName;
        public static ConfigEntry<string> rustyBagsQuiverSlotGlobalKey;
        public static ConfigEntry<string> rustyBagsQuiverSlotItemDiscovered;

        public static ConfigEntry<bool> vikingsSummonerSlotEnabled;
        public static ConfigEntry<string> vikingsSummonerSlotName;
        public static ConfigEntry<string> vikingsSummonerSlotGlobalKey;
        public static ConfigEntry<string> vikingsSummonerSlotItemDiscovered;


        private void Awake()
        {
            instance = this;

            ConfigInit();
            _ = configSync.AddLockingConfigEntry(configLocked);

            EpicLootCompatibility.Prepare();
            UpdateSlots();

            harmony.PatchAll();
            EpicLootCompatibility.Initialize();
        }

        private void OnDestroy()
        {
            EpicLootCompatibility.Shutdown();
            Config.Save();
            instance = null;
            harmony?.UnpatchSelf();
        }

        public void ConfigInit()
        {
            configLocked = serverConfig("General", "Lock Configuration", defaultValue: true, "Configuration is locked and can be changed by server admins only.");
            loggingEnabled = config("General", "Logging enabled", defaultValue: false, "Enable logging. [Client controlled by default]", synchronizedSetting: false);
            slotsOrder = config("General", "Slots order", defaultValue: CustomSlot.VanillaOrder,
                new ConfigDescription("Comma-separated slot ID order of custom slots", null, new CustomConfigs.ConfigurationManagerAttributes { CustomDrawer = CustomConfigs.DrawOrderedFixedStrings(",") }));

            slotsOrder.SettingChanged += (s, e) => UpdateSlots();

            adventureBackpackSlotEnabled = config("Mod - Adventure Backpacks", "Enabled", true, "Enable adventure backpack slot. Restart the game after change to avoid potential issues.");
            adventureBackpackSlotName = config("Mod - Adventure Backpacks", "Name", "Backpack", "Slot name. Use ExtraSlots translation files to add localized string.");
            adventureBackpackSlotGlobalKey = config("Mod - Adventure Backpacks", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            adventureBackpackSlotItemDiscovered = config("Mod - Adventure Backpacks", "Items discovered", "$vapok_mod_item_backpack_meadows,$vapok_mod_item_backpack_blackforest,$vapok_mod_item_backpack_swamp,$vapok_mod_item_backpack_mountains,$vapok_mod_item_backpack_plains,$vapok_mod_item_backpack_mistlands,$vapok_mod_item_rugged_backpack,$vapok_mod_item_arctic_backpack", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");
            adventureBackpackItemIsVisible = config("Mod - Adventure Backpacks", "Item is visible", true, "Make the equipped backpack visible on the player model.");

            adventureBackpackSlotEnabled.SettingChanged += (s, e) => { AdventureBackpacksCustomSlot.AdventureBackpackItem.PatchBackpackItemOnConfigChange(); UpdateSlots(); };
            adventureBackpackItemIsVisible.SettingChanged += (s, e) => Player.m_localPlayer?.SetupEquipment();

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
            circletExtendedSlotName = config("Mod - CircletExtended", "Name", "Circlet", "Slot name. Use ExtraSlots translation files to add localized string.");
            circletExtendedSlotGlobalKey = config("Mod - CircletExtended", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            circletExtendedSlotItemDiscovered = config("Mod - CircletExtended", "Items discovered", "$item_helmet_dverger", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            circletExtendedSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            hipLanternSlotEnabled = config("Mod - HipLantern", "Enabled", true, "Enable hip lantern slot");
            hipLanternSlotName = config("Mod - HipLantern", "Name", "$hiplantern_slot", "Slot name. Use ExtraSlots translation files to add localized string.");
            hipLanternSlotGlobalKey = config("Mod - HipLantern", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            hipLanternSlotItemDiscovered = config("Mod - HipLantern", "Items discovered", "$item_hiplantern", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            hipLanternSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            jewelcraftingNeckSlotEnabled = config("Mod - Jewelcrafting - Neck", "Enabled", true, "Enable neck slot");
            jewelcraftingNeckSlotName = config("Mod - Jewelcrafting - Neck", "Name", "Neck", "Slot name. Use ExtraSlots translation files to add localized string.");
            jewelcraftingNeckSlotGlobalKey = config("Mod - Jewelcrafting - Neck", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            jewelcraftingNeckSlotItemDiscovered = config("Mod - Jewelcrafting - Neck", "Items discovered", "$jc_necklace_red,$jc_necklace_green,$jc_necklace_blue,$jc_necklace_yellow,$jc_necklace_purple,$jc_necklace_orange,$jc_necklace_dvergrnecklace,$jc_necklace_eitrnecklace,$jc_necklace_fireresistnecklace,$jc_necklace_frostresistnecklace,$jc_necklace_poisonresistnecklace,", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            jewelcraftingNeckSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            jewelcraftingRingSlotEnabled = config("Mod - Jewelcrafting - Ring", "Enabled", true, "Enable Ring slot");
            jewelcraftingRingSlotName = config("Mod - Jewelcrafting - Ring", "Name", "Finger", "Slot name. Use ExtraSlots translation files to add localized string.");
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
            judesEquipmentBackpackSlotName = config("Mod - Judes Equipment", "Name", "Backpack", "Slot name. Use ExtraSlots translation files to add localized string.");
            judesEquipmentBackpackSlotGlobalKey = config("Mod - Judes Equipment", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            judesEquipmentBackpackSlotItemDiscovered = config("Mod - Judes Equipment", "Items discovered", "$BackpackSimple,$BackpackHeavy", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");
            judesEquipmentBackpackItemIsVisible = config("Mod - Judes Equipment", "Item is visible", true, "Make the equipped backpack visible on the player model.");

            judesEquipmentBackpackSlotEnabled.SettingChanged += (s, e) => { JudesEquipmentBackpacksCustomSlot.JudesEquipmentBackpackItem.PatchBackpackItemOnConfigChange(); UpdateSlots(); };
            judesEquipmentBackpackItemIsVisible.SettingChanged += (s, e) => Player.m_localPlayer?.SetupEquipment();

            rustyBagsSlotEnabled = config("Mod - Rusty Bags", "Enabled", true, "Enable Rusty Bags backpack slot.");
            rustyBagsSlotName = config("Mod - Rusty Bags", "Name", "Bag", "Slot name. Use ExtraSlots translation files to add localized string.");
            rustyBagsSlotGlobalKey = config("Mod - Rusty Bags", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            rustyBagsSlotItemDiscovered = config("Mod - Rusty Bags", "Items discovered", "LeatherBag_RS,BarrelBag_RS,MinerBag_RS,UnbjornBag_RS,DvergerBag_RS", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");
            rustyBagsSlotCombineWithQuiver = config("Mod - Rusty Bags", "Allow quivers", false, "By default quivers will not go into this slot. Enable to allow quivers. Do not forget to add quivers from quiver slot item list.");

            rustyBagsSlotEnabled.SettingChanged += (s, e) => UpdateSlots();
            rustyBagsSlotCombineWithQuiver.SettingChanged += (s, e) => UpdateSlots();

            rustyBagsQuiverSlotEnabled = config("Mod - Rusty Bags - Quiver", "Enabled", true, "Enable Rusty Bags backpack slot.");
            rustyBagsQuiverSlotName = config("Mod - Rusty Bags - Quiver", "Name", "Quiver", "Slot name. Use ExtraSlots translation files to add localized string.");
            rustyBagsQuiverSlotGlobalKey = config("Mod - Rusty Bags - Quiver", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            rustyBagsQuiverSlotItemDiscovered = config("Mod - Rusty Bags - Quiver", "Items discovered", "Quiver_RS,MountainQuiver_RS,CrossbowQuiver_RS", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            rustyBagsQuiverSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            vikingsSummonerSlotEnabled = config("Mod - Vikings Summoner", "Enabled", true, "Enable Vikings Summoner grimoire slot.");
            vikingsSummonerSlotName = config("Mod - Vikings Summoner", "Name", "Grimoire", "Slot name. Use ExtraSlots translation files to add localized string.");
            vikingsSummonerSlotGlobalKey = config("Mod - Vikings Summoner", "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            vikingsSummonerSlotItemDiscovered = config("Mod - Vikings Summoner", "Items discovered", "RD_grimoire_01,RD_grimoire_02,RD_grimoire_03", "Comma-separated list of items. Slot will be active only if any item is discovered or list is not set.");

            vikingsSummonerSlotEnabled.SettingChanged += (s, e) => UpdateSlots();

            for (int i = 0; i < UserDefinedSlot.maxAmount; i++)
                new UserDefinedSlot(i);
        }

        public static void UpdateSlots()
        {
            if (instance == null || slotsOrder == null || UserDefinedSlot.userDefinedSlots.Any(slot => slot == null))
                return;

            if (updatingSlots)
            {
                slotUpdatePending = true;
                return;
            }

            updatingSlots = true;
            try
            {
                do
                {
                    slotUpdatePending = false;
                    UpdateSlotRegistrations();
                }
                while (slotUpdatePending);
            }
            finally
            {
                updatingSlots = false;
            }
        }

        private static void UpdateSlotRegistrations()
        {
            List<CustomSlot> previousDefinitions = CustomSlot.slots.ToList();
            List<CustomSlot> previous = previousDefinitions.Where(slot => slot.initialized).GroupBy(slot => slot.slotID).Select(group => group.First()).ToList();
            CustomSlot.slots.Clear();
            try
            {
                // Ignore repeated IDs and append omitted defaults without constructing duplicates.
                slotsOrder.Value.Split(',').Concat(CustomSlot.GetVanillaOrder()).Select(id => id.Trim())
                    .Where(id => !id.IsNullOrWhiteSpace()).Distinct().Do(InitSlot);
            }
            catch
            {
                CustomSlot.slots.Clear();
                CustomSlot.slots.AddRange(previousDefinitions);
                throw;
            }

            Dictionary<string, CustomSlot> previousByID = previous.ToDictionary(slot => slot.slotID);
            for (int i = 0; i < CustomSlot.slots.Count; i++)
            {
                CustomSlot definition = CustomSlot.slots[i];
                if (definition.initialized && previousByID.TryGetValue(definition.slotID, out CustomSlot retained))
                {
                    retained.UpdateDefinition(definition);
                    CustomSlot.slots[i] = retained;
                }
            }

            List<CustomSlot> desired = CustomSlot.slots.Where(slot => slot.initialized).ToList();
            int commonPrefix = 0;
            while (commonPrefix < previous.Count && commonPrefix < desired.Count && previous[commonPrefix].slotID == desired[commonPrefix].slotID &&
                ExtraSlots.API.FindSlot(CustomSlot.GetSlotID(desired[commonPrefix].slotID)) != null)
                commonPrefix++;

            // Retain unchanged registrations and their residents. For the changed suffix, let
            // ExtraSlots own relocation and deferred recovery; never stage items in an inventory.
            for (int i = previous.Count - 1; i >= commonPrefix; i--)
                if (previous[i].RemoveSlot())
                    LogInfo($"Slot {previous[i]} was removed");

            CustomSlot precedingSlot = commonPrefix > 0 ? desired[commonPrefix - 1] : null;
            for (int i = commonPrefix; i < desired.Count; i++)
                if (TryAddSlot(desired[i], precedingSlot))
                    precedingSlot = desired[i];

            UserDefinedSlot.RefreshSlots();
            EpicLootCompatibility.InvalidatePlayerEffectCache(Player.m_localPlayer);
        }

        public static void TryAddSlot(CustomSlot slot) => TryAddSlot(slot, null);

        private static bool TryAddSlot(CustomSlot slot, CustomSlot precedingSlot)
        {
            if (!slot.initialized)
                return false;

            // Replace only the legacy unprefixed registration, not an existing ESCS slot.
            if (ExtraSlots.API.RemoveSlot(slot.slotID))
                LogInfo($"Legacy slot {slot} was removed");

            if (slot.AddSlotAfter(precedingSlot))
            {
                LogInfo($"Slot {slot} was added");
                return true;
            }

            LogWarning($"Error while trying to add new slot {slot}. Check the available custom-slot capacity in ExtraSlots.");
            return false;
        }

        public static void InitSlot(string slotID)
        {
            switch (slotID)
            {
                case BackpacksSlot.ID when backpacksSlotEnabled.Value:
                    new BackpacksSlot();
                    return;
                case AdventureBackpacksSlot.ID when adventureBackpackSlotEnabled.Value:
                    new AdventureBackpacksSlot();
                    return;
                case JudesEquipmentBackpackSlot.ID when judesEquipmentBackpackSlotEnabled.Value:
                    new JudesEquipmentBackpackSlot();
                    return;
                case RustyBagsSlot.ID when rustyBagsSlotEnabled.Value:
                    new RustyBagsSlot();
                    return;
                case RustyBagsQuiverSlot.ID when rustyBagsQuiverSlotEnabled.Value:
                    new RustyBagsQuiverSlot();
                    return;
                case MagicPluginTomeSlot.ID when magicPluginTomeSlotEnabled.Value:
                    new MagicPluginTomeSlot();
                    return;
                case MagicPluginEarringSlot.ID when magicPluginEarringSlotEnabled.Value:
                    new MagicPluginEarringSlot();
                    return;
                case JewelcraftingNeckSlot.ID when jewelcraftingNeckSlotEnabled.Value:
                    new JewelcraftingNeckSlot();
                    return;
                case JewelcraftingRingSlot.ID when jewelcraftingRingSlotEnabled.Value:
                    new JewelcraftingRingSlot();
                    return;
                case BowsBeforeHoesSlot.ID when bbhQuiverSlotEnabled.Value:
                    new BowsBeforeHoesSlot();
                    return;
                case CircletExtendedSlot.ID when circletExtendedSlotEnabled.Value:
                    new CircletExtendedSlot();
                    return;
                case HipLanternSlot.ID when hipLanternSlotEnabled.Value:
                    new HipLanternSlot();
                    return;
                case VikingsSummoner.ID when vikingsSummonerSlotEnabled.Value:
                    new VikingsSummoner();
                    return;
                default: break;
            }

            if (UserDefinedSlot.IsUserDefinedSlot(slotID))
                UserDefinedSlot.UpdateSlot(slotID);
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

#pragma warning disable IDE1006 // Naming Styles
        internal ConfigEntry<T> config<T>(string group, string name, T defaultValue, ConfigDescription description, bool synchronizedSetting = true)
        {
            return configSync.AddConfigEntry(
                Config,
                group,
                name,
                defaultValue,
                description,
                syncMode: ConfigSyncMode.Conditional,
                serverControlledByDefault: synchronizedSetting).SourceConfig;
        }

        internal ConfigEntry<T> serverConfig<T>(string group, string name, T defaultValue, ConfigDescription description)
        {
            return configSync.AddConfigEntry(
                Config,
                group,
                name,
                defaultValue,
                description,
                syncMode: ConfigSyncMode.AlwaysServerControlled,
                serverControlledByDefault: true).SourceConfig;
        }

        internal ConfigEntry<T> config<T>(string group, string name, T defaultValue, string description, bool synchronizedSetting = true) =>
            config(group, name, defaultValue, new ConfigDescription(description), synchronizedSetting);

        internal ConfigEntry<T> serverConfig<T>(string group, string name, T defaultValue, string description) =>
            serverConfig(group, name, defaultValue, new ConfigDescription(description));
#pragma warning restore IDE1006 // Naming Styles
    }
}
