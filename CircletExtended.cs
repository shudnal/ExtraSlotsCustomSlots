using BepInEx.Bootstrap;
using HarmonyLib;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class CircletExtendedSlot : CustomSlot
    {
        public const string ID = "CircletExtended";
        public const string pluginID = "shudnal.CircletExtended";

        public CircletExtendedSlot()
        {
            slots.Add(this);

            GUID = pluginID;
            slotID = ID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            MethodInfo isValid = AccessTools.Method(assembly.GetType("CircletExtended.CircletItem"), "IsCircletItem", new System.Type[] { typeof(ItemDrop.ItemData) });
            if (isValid == null)
            {
                LogWarning("CircletExtended mod is loaded but CircletExtended.CircletItem:IsCircletItem is not found");
                return;
            }

            MethodInfo isCircletSlotKnown = AccessTools.Method(assembly.GetType("CircletExtended.CircletItem"), "IsCircletSlotKnown");

            itemIsValid = item => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => circletExtendedSlotName.Value;

            isActive = () => IsSlotActive(circletExtendedSlotGlobalKey.Value, circletExtendedSlotItemDiscovered.Value) && (isCircletSlotKnown == null || (bool)isCircletSlotKnown.Invoke(null, null));

            initialized = true;
        }
    }
}