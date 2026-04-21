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
        public static Type bag;
        public static Type quiver;

        public RustyBagsSlot()
        {
            slots.Add(this);

            GUID = pluginID;
            slotID = ID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            bag = AccessTools.GetTypesFromAssembly(assembly).FirstOrDefault(type => type.FullName == "RustyBags.Bag");
            if (bag == null)
            {
                LogWarning("RustyBags mod is loaded but RustyBags.Bag type is not found");
                return;
            }

            quiver = AccessTools.GetTypesFromAssembly(assembly).FirstOrDefault(type => type.FullName == "RustyBags.Quiver");
            if (quiver != null && !rustyBagsSlotCombineWithQuiver.Value)
                itemIsValid = item => item != null && bag.IsInstanceOfType(item) && !quiver.IsInstanceOfType(item);
            else
                itemIsValid = item => item != null && bag.IsInstanceOfType(item);

            getName = () => rustyBagsSlotName.Value;

            isActive = () => IsSlotActive(rustyBagsSlotGlobalKey.Value, rustyBagsSlotItemDiscovered.Value);

            initialized = true;
        }
    }

    public class RustyBagsQuiverSlot : CustomSlot
    {
        public const string ID = "RustyBagsQuiver";
        public const string pluginID = "RustyMods.RustyBags";
        public static Type quiver;

        public RustyBagsQuiverSlot()
        {
            slots.Add(this);

            GUID = pluginID;
            slotID = ID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            quiver = AccessTools.GetTypesFromAssembly(assembly).FirstOrDefault(type => type.FullName == "RustyBags.Quiver");
            if (quiver == null)
            {
                LogWarning("RustyBags mod is loaded but RustyBags.Quiver type is not found");
                return;
            }

            itemIsValid = item => item != null && quiver.IsInstanceOfType(item);

            getName = () => rustyBagsQuiverSlotName.Value;

            isActive = () => IsSlotActive(rustyBagsQuiverSlotGlobalKey.Value, rustyBagsQuiverSlotItemDiscovered.Value);

            initialized = true;
        }
    }
}