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

        public static readonly string VanillaOrder = $"{BackpacksSlot.ID},{AdventureBackpacksSlot.ID},{CircletExtendedSlot.ID},{JewelcraftingNeckSlot.ID},{MagicPluginEarringSlot.ID},{JewelcraftingRingSlot.ID},{MagicPluginTomeSlot.ID},{BowsBeforeHoesSlot.ID},{HipLanternSlot.ID}";
    }
}