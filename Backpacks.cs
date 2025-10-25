using BepInEx.Bootstrap;
using HarmonyLib;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class BackpacksSlot : CustomSlot
    {
        public const string ID = "Backpacks";
        public const string pluginID = "org.bepinex.plugins.backpacks";

        public BackpacksSlot()
        {
            slots.Add(this);

            GUID = pluginID;
            slotID = ID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            MethodInfo isValid = AccessTools.Method(assembly.GetType("Backpacks.Backpacks"), "validateBackpack");
            if (isValid == null)
            {
                LogWarning("Backpacks mod is loaded but Backpacks.Backpacks:validateBackpack is not found");
                return;
            }

            itemIsValid = item => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => backpacksSlotName.Value;

            isActive = () => IsSlotActive(backpacksSlotGlobalKey.Value, backpacksSlotItemDiscovered.Value);

            initialized = true;
        }
    }
}