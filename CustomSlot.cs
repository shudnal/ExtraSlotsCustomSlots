using BepInEx;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;

namespace ExtraSlotsCustomSlots
{
    public class CustomSlot
    {
        public const string slotPrefix = "ESCS";

        public string GUID;

        public bool PluginInstalled => Chainloader.PluginInfos.ContainsKey(GUID);

        public bool initialized;

        public string slotID;

        public Func<ItemDrop.ItemData, bool> itemIsValid;

        public Func<string> getName;

        public Func<bool> isActive;

        public bool AddSlot() => initialized && ExtraSlots.API.AddSlot(GetSlotID(slotID), getName, itemIsValid, isActive);

        public bool RemoveSlot() => initialized && (ExtraSlots.API.RemoveSlot(GetSlotID(slotID)) || ExtraSlots.API.RemoveSlot(slotID));

        public static string GetSlotID(string slotID) => slotPrefix + slotID.ToString();

        public override string ToString() => initialized ? slotID.ToString() : $"{GUID} (inactive)";

        public static readonly List<CustomSlot> slots = new List<CustomSlot>();

        public static readonly string VanillaOrder = string.Join(",", GetVanillaOrder());

        public static List<string> GetVanillaOrder()
        {
            List<string> slotOrder = new List<string>
            {
                BackpacksSlot.ID,
                AdventureBackpacksSlot.ID,
                JudesEquipmentBackpackSlot.ID,
                RustyBagsSlot.ID,
                CircletExtendedSlot.ID,
                JewelcraftingNeckSlot.ID,
                MagicPluginEarringSlot.ID,
                JewelcraftingRingSlot.ID,
                MagicPluginTomeSlot.ID,
                BowsBeforeHoesSlot.ID,
                HipLanternSlot.ID
            };

            for (int i = 0; i < UserDefinedSlot.maxAmount; i++)
                slotOrder.Add(UserDefinedSlot.GetSlotID(i));

            return slotOrder;
        }

        public static bool IsSlotActive(string globalKey, string itemDiscovered)
        {
            bool globalKeyIsSet = !globalKey.IsNullOrWhiteSpace();
            bool itemIsSet = !itemDiscovered.IsNullOrWhiteSpace();

            // If nothing is set - slot is always active
            if (!globalKeyIsSet && !itemIsSet)
                return true;

            // If both are set - one of both should work
            if (globalKeyIsSet && itemIsSet)
                return ExtraSlots.API.IsAnyGlobalKeyActive(globalKey) || ExtraSlots.API.IsAnyMaterialDiscovered(itemDiscovered);

            // If global is set, item is not - check only global, otherwise check item
            return globalKeyIsSet ? ExtraSlots.API.IsAnyGlobalKeyActive(globalKey) : ExtraSlots.API.IsAnyMaterialDiscovered(itemDiscovered);
        }
    }
}