using HarmonyLib;
using System.Linq;
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

            if (!PluginInstalled)
                return;

            Assembly backpacks = AccessTools.AllAssemblies().FirstOrDefault(a => a.FullName.StartsWith("Backpacks,"));
            if (backpacks == null)
            {
                LogWarning("Backpacks mod is loaded but not found with type name Backpacks");
                return;
            }

            MethodInfo isValid = AccessTools.Method(backpacks.GetType("Backpacks.Backpacks"), "validateBackpack");
            if (isValid == null)
            {
                LogWarning("Backpacks mod is loaded but Backpacks.Backpacks:validateBackpack is not found");
                return;
            }

            slotID = ID;

            itemIsValid = (ItemDrop.ItemData item) => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => backpackSlotName.Value;

            isActive = () => ExtraSlots.API.IsAnyGlobalKeyActive(backpackSlotGlobalKey.Value) || ExtraSlots.API.IsAnyMaterialDiscovered(backpackSlotItemDiscovered.Value);

            initialized = true;
        }
    }
}