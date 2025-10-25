using BepInEx.Bootstrap;
using HarmonyLib;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class HipLanternSlot : CustomSlot
    {
        public const string ID = "HipLantern";
        public const string pluginID = "shudnal.HipLantern";

        public HipLanternSlot()
        {
            slots.Add(this);

            GUID = pluginID;
            slotID = ID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            MethodInfo isValid = AccessTools.Method(assembly.GetType("HipLantern.LanternItem"), "IsLanternItem", new System.Type[] { typeof(ItemDrop.ItemData) });
            if (isValid == null)
            {
                LogWarning("HipLantern mod is loaded but HipLantern.LanternItem:IsLanternItem is not found");
                return;
            }

            itemIsValid = item => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => hipLanternSlotName.Value;

            isActive = () => IsSlotActive(hipLanternSlotGlobalKey.Value, hipLanternSlotItemDiscovered.Value);

            initialized = true;
        }
    }
}