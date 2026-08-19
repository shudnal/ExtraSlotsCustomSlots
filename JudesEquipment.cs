using BepInEx.Bootstrap;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;
using ExtraSlotsCustomSlots.JudesEquipmentBackpacksCustomSlot;

namespace ExtraSlotsCustomSlots
{
    public class JudesEquipmentBackpackSlot : CustomSlot
    {
        public const string ID = "JudesEquipmentBackpack";
        public const string pluginID = "GoldenJude_JudesEquipment";
        public static Assembly assembly;

        public static bool IsLoaded => Chainloader.PluginInfos.ContainsKey(pluginID);
        public static bool IsActive => IsLoaded && judesEquipmentBackpackSlotEnabled.Value;

        public JudesEquipmentBackpackSlot()
        {
            slots.Add(this);

            GUID = pluginID;
            slotID = ID;

            if (!PluginInstalled)
                return;

            assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            itemIsValid = item => item != null && item.m_dropPrefab != null && (item.m_dropPrefab.name == "BackpackSimple" || item.m_dropPrefab.name == "BackpackHeavy");

            getName = () => judesEquipmentBackpackSlotName.Value;

            isActive = () => IsSlotActive(judesEquipmentBackpackSlotGlobalKey.Value, judesEquipmentBackpackSlotItemDiscovered.Value);

            initialized = true;

            CustomItemType.InitBackpackFunc(itemIsValid);
        }
    }

}
