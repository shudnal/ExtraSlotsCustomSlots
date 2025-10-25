using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class RustyBagsSlot : CustomSlot
    {
        public const string ID = "RustyBagsBag";
        public const string pluginID = "RustyMods.RustyBags";
        
        public RustyBagsSlot()
        {
            slots.Add(this);

            GUID = pluginID;
            slotID = ID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            Type bag = AccessTools.GetTypesFromAssembly(assembly).FirstOrDefault(type => type.FullName == "RustyBags.Bag");
            if (bag == null)
            {
                LogWarning("RustyBags mod is loaded but RustyBags.Bag type is not found");
                return;
            }

            itemIsValid = item => item != null && bag.IsInstanceOfType(item);

            getName = () => rustyBagsSlotName.Value;

            isActive = () => IsSlotActive(rustyBagsSlotGlobalKey.Value, rustyBagsSlotItemDiscovered.Value);

            initialized = true;
        }
    }
}