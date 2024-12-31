using BepInEx.Bootstrap;
using HarmonyLib;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class JewelcraftingNeckSlot : CustomSlot
    {
        public const string ID = "JewelcraftingNeck";
        public const string pluginID = "org.bepinex.plugins.jewelcrafting";

        public JewelcraftingNeckSlot()
        {
            slots.Add(this);

            GUID = pluginID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            MethodInfo isValid = AccessTools.Method(assembly.GetType("Jewelcrafting.Visual"), "IsNeckItem");
            if (isValid == null)
            {
                LogWarning("Jewelcrafting mod is loaded but Jewelcrafting.Visual:IsNeckItem is not found");
                return;
            }

            slotID = ID;

            itemIsValid = (ItemDrop.ItemData item) => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => jewelcraftingNeckSlotName.Value;

            isActive = () => IsSlotActive(jewelcraftingNeckSlotGlobalKey.Value, jewelcraftingNeckSlotItemDiscovered.Value);

            initialized = true;
        }
    }

    public class JewelcraftingRingSlot : CustomSlot
    {
        public const string ID = "JewelcraftingRing";
        public const string pluginID = "org.bepinex.plugins.jewelcrafting";

        public JewelcraftingRingSlot()
        {
            slots.Add(this);

            GUID = pluginID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            MethodInfo isValid = AccessTools.Method(assembly.GetType("Jewelcrafting.Visual"), "IsFingerItem");
            if (isValid == null)
            {
                LogWarning("Jewelcrafting mod is loaded but Jewelcrafting.Visual:IsFingerItem is not found");
                return;
            }

            slotID = ID;

            itemIsValid = (ItemDrop.ItemData item) => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => jewelcraftingRingSlotName.Value;

            isActive = () => IsSlotActive(jewelcraftingRingSlotGlobalKey.Value, jewelcraftingRingSlotItemDiscovered.Value);

            initialized = true;
        }
    }
}