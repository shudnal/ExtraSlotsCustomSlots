using BepInEx.Bootstrap;
using HarmonyLib;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class BowsBeforeHoesSlot : CustomSlot
    {
        public const string ID = "BowsBeforeHoes";
        public const string pluginID = "Azumatt.BowsBeforeHoes";

        public BowsBeforeHoesSlot()
        {
            slots.Add(this);

            GUID = pluginID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            MethodInfo isValid = AccessTools.Method(assembly.GetType("BowsBeforeHoes.Util.Functions"), "IsQuiverSlot");
            if (isValid == null)
            {
                LogWarning("BowsBeforeHoes mod is loaded but BowsBeforeHoes.Util.Functions:IsQuiverSlot is not found");
                return;
            }

            slotID = ID;

            itemIsValid = (ItemDrop.ItemData item) => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => bbhQuiverSlotName.Value;

            isActive = () => ExtraSlots.API.IsAnyGlobalKeyActive(bbhQuiverSlotGlobalKey.Value) || ExtraSlots.API.IsAnyMaterialDiscovered(bbhQuiverSlotItemDiscovered.Value);

            initialized = true; 
        }
    }
}