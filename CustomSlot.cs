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

        public string afterSlot;

        public void AddSlot() => ExtraSlots.API.AddSlotAfter(GetSlotID(slotID), getName, itemIsValid, isActive, afterSlot);

        public void RemoveSlot() => ExtraSlots.API.RemoveSlot(GetSlotID(slotID));


        public static string GetSlotID(string slotID) => slotPrefix + slotID.ToString();

        public override string ToString() => slotID + (initialized ? "" : " (inactive)");

        public static readonly List<CustomSlot> slots = new List<CustomSlot>();

        public static readonly string VanillaOrder = $"{BackpacksSlot.ID},{AdventureBackpacksSlot.ID}";
    }
}