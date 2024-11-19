using HarmonyLib;
using System.Linq;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class AdventureBackpacksSlot : CustomSlot
    {
        public const string ID = "AdventureBackpacks";
        public const string pluginID = "vapok.mods.adventurebackpacks";

        public AdventureBackpacksSlot()
        {
            slots.Add(this);

            GUID = pluginID;

            if (!PluginInstalled)
                return;

            Assembly adventureBackpacks = AccessTools.AllAssemblies().FirstOrDefault(a => a.FullName.StartsWith("AdventureBackpacks,"));
            if (adventureBackpacks == null)
            {
                LogWarning("Adventure Backpacks mod is loaded but not found with type name AdventureBackpacks");
                return;
            }

            MethodInfo isValid = AccessTools.Method(adventureBackpacks.GetType("AdventureBackpacks.API.ABAPI"), "IsBackpack");
            if (isValid == null)
            {
                LogWarning("AdventureBackpacks mod is loaded but AdventureBackpacks.API.ABAPI:IsBackpack is not found");
                return;
            }

            slotID = ID;

            itemIsValid = (ItemDrop.ItemData item) => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => adventureBackpackSlotName.Value;

            isActive = () => ExtraSlots.API.IsAnyGlobalKeyActive(adventureBackpackSlotGlobalKey.Value) || ExtraSlots.API.IsAnyMaterialDiscovered(adventureBackpackSlotItemDiscovered.Value);

            initialized = true;
        }
    }
}