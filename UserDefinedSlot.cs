using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class UserDefinedSlot : CustomSlot
    {
        public ConfigEntry<bool> slotEnabled;
        public ConfigEntry<string> slotName;
        public ConfigEntry<string> slotGlobalKey;
        public ConfigEntry<string> slotItemList;
        public ConfigEntry<bool> itemIsVisible;

        public string groupName;
        public List<string> itemList = new List<string>();

        public const int maxAmount = 8;

        public static UserDefinedSlot[] userDefinedSlots = new UserDefinedSlot[maxAmount];

        public void UpdateItemList()
        {
            itemList.Clear();
            slotItemList.Value.Split(',').Select(s => s.Trim()).Where(s => !s.IsNullOrWhiteSpace()).Do(itemList.Add);
        }

        public UserDefinedSlot(int index)
        {
            userDefinedSlots[index] = this;

            groupName = $"User defined - Custom slot {index + 1}";

            slotEnabled = instance.config(groupName, "Enabled", false, "Enable custom slot.");
            slotName = instance.config(groupName, "Name", "Slot", "Slot name. Use ExtraSlots translation files to add localized string.");
            slotGlobalKey = instance.config(groupName, "Global keys", "", "Comma-separated list of global keys and player unique keys. Slot will be active only if any key is enabled or list is not set.");
            slotItemList = instance.config(groupName, "Item list", "", "Comma-separated list of items. Slot will be active only if any item is discovered.");
            itemIsVisible = instance.config(groupName, "Item is visible", true, "Make item in that slot visible on player model (if supported by an item itself).");

            slotEnabled.SettingChanged += (sender, args) => UpdateSlots();
            slotItemList.SettingChanged += (sender, args) => UpdateItemList();
            itemIsVisible.SettingChanged += (sender, args) => Player.m_localPlayer?.SetupEquipment();

            UpdateItemList();

            GUID = GetSlotID(index);

            slotID = GUID;

            itemIsValid = item => item != null && (item.m_shared != null && itemList.Contains(item.m_shared.m_name) || item.m_dropPrefab != null && itemList.Contains(item.m_dropPrefab.name));

            getName = () => slotName.Value;

            isActive = () => IsSlotActive(slotGlobalKey.Value, slotItemList.Value);

            initialized = true;
        }

        public static void UpdateSlot(string slotID) 
        {
            if (userDefinedSlots.FirstOrDefault(slot => slot.slotID == slotID) is UserDefinedSlot slot && slot.slotEnabled.Value)
                slots.Add(slot);
        }

        public const string UserDefinedSlotID = "CustomSlot";

        public static bool IsUserDefinedSlot(string slotID) => slotID.StartsWith(UserDefinedSlotID);

        public static string GetSlotID(int index) => $"{UserDefinedSlotID}{index + 1}";

        public static bool IsItemInSlotVisible(int index) => userDefinedSlots[index] is UserDefinedSlot slot && slot.slotEnabled.Value && slot.itemIsVisible.Value;
    }
}
