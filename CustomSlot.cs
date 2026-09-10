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

        // Keep these callbacks bound to the retained definition, rather than to one revision
        // of its delegates. Configuration refreshes can then preserve registered slot objects.
        private string GetName() => getName?.Invoke() ?? "";
        private bool ItemIsValid(ItemDrop.ItemData item) => InventoryCompatibility.IsRuntimeItem(item) && itemIsValid?.Invoke(item) == true;
        private bool IsActive() => initialized && (isActive?.Invoke() ?? true);

        public bool AddSlot() => initialized && ExtraSlots.API.AddSlot(GetSlotID(slotID), GetName, ItemIsValid, IsActive);

        internal bool AddSlotAfter(CustomSlot precedingSlot) => precedingSlot == null ? AddSlot() :
            initialized && ExtraSlots.API.AddSlotAfter(GetSlotID(slotID), GetName, ItemIsValid, IsActive, GetSlotID(precedingSlot.slotID));

        internal void UpdateDefinition(CustomSlot definition)
        {
            GUID = definition.GUID;
            initialized = definition.initialized;
            itemIsValid = definition.itemIsValid;
            getName = definition.getName;
            isActive = definition.isActive;
        }

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
                RustyBagsQuiverSlot.ID,
                CircletExtendedSlot.ID,
                JewelcraftingNeckSlot.ID,
                MagicPluginEarringSlot.ID,
                JewelcraftingRingSlot.ID,
                MagicPluginTomeSlot.ID,
                VikingsSummoner.ID,
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
