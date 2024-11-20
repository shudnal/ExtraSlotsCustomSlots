using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class MagicPluginTomeSlot : CustomSlot
    {
        public const string ID = "MagicPluginTome";
        public const string pluginID = "blacks7ar.MagicPlugin";

        public MagicPluginTomeSlot()
        {
            slots.Add(this);

            GUID = pluginID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());
            
            MethodInfo isValid = AccessTools.Method(assembly.GetType("MagicPlugin.Functions.MagicSlot"), "IsTomeItem");
            if (isValid == null)
            {
                LogWarning("MagicPlugin mod is loaded but MagicPlugin.Functions.MagicSlot:IsTomeItem is not found");
                return;
            }

            slotID = ID;

            itemIsValid = (ItemDrop.ItemData item) => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => magicPluginTomeSlotName.Value;

            isActive = () => ExtraSlots.API.IsAnyGlobalKeyActive(magicPluginTomeSlotGlobalKey.Value) || ExtraSlots.API.IsAnyMaterialDiscovered(magicPluginTomeSlotItemDiscovered.Value);

            initialized = true;
        }
    }

    public class MagicPluginEarringSlot : CustomSlot
    {
        public const string ID = "MagicPluginEarring";
        public const string pluginID = "blacks7ar.MagicPlugin";

        public MagicPluginEarringSlot()
        {
            slots.Add(this);

            GUID = pluginID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            MethodInfo isValid = AccessTools.Method(assembly.GetType("MagicPlugin.Functions.MagicSlot"), "IsEarringItem");
            if (isValid == null)
            {
                LogWarning("MagicPlugin mod is loaded but MagicPlugin.Functions.MagicSlot:IsEarringItem is not found");
                return;
            }

            slotID = ID;

            itemIsValid = (ItemDrop.ItemData item) => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => magicPluginEarringSlotName.Value;

            isActive = () => ExtraSlots.API.IsAnyGlobalKeyActive(magicPluginEarringSlotGlobalKey.Value) || ExtraSlots.API.IsAnyMaterialDiscovered(magicPluginEarringSlotItemDiscovered.Value);

            initialized = true;
        }
    }

    [HarmonyPatch]
    public static class MagicPlugin_MagicSlot_AddCustomSlot_PreventCustomSlotAddition
    {
        public static MethodBase target;

        public static bool Prepare(MethodBase original)
        {
            if (!Chainloader.PluginInfos.TryGetValue(MagicPluginTomeSlot.pluginID, out PluginInfo plugin))
                return false;

            target ??= AccessTools.Method(Assembly.GetAssembly(plugin.Instance.GetType()).GetType("MagicPlugin.Functions.MagicSlot"), "AddCustomSlot");
            if (target == null)
                return false;

            if (original == null)
                LogInfo("MagicPlugin.Functions.MagicSlot:AddCustomSlot method is patched to prevent adding custom slot call");

            return true;
        }

        public static MethodBase TargetMethod() => target;

        public static bool Prefix() => false;
    }

    [HarmonyPatch]
    public static class MagicPlugin_AzuEPI_IsLoaded_CustomSlotHandle
    {
        public static MethodBase target;

        public static bool Prepare(MethodBase original)
        {
            if (!Chainloader.PluginInfos.TryGetValue(MagicPluginTomeSlot.pluginID, out PluginInfo plugin))
                return false;

            target ??= AccessTools.Method(Assembly.GetAssembly(plugin.Instance.GetType()).GetType("AzuExtendedPlayerInventory.API"), "IsLoaded");
            if (target == null)
                return false;

            if (original == null)
                LogInfo("MagicPlugin.AzuExtendedPlayerInventory.API:IsLoaded method is patched to enable custom slot handling");

            return true;
        }

        public static MethodBase TargetMethod() => target;

        public static void Postfix(ref bool __result) => __result = true;
    }
}