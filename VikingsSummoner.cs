using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class VikingsSummoner : CustomSlot
    {
        public const string ID = "VikingsSummoner";
        public const string pluginID = "radamanto.Vikings_Summoner";

        public VikingsSummoner()
        {
            slots.Add(this);

            GUID = pluginID;
            slotID = ID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            MethodInfo isValid = AccessTools.Method(assembly.GetType("Vikings_Summoner.TomeCustomSlotManager"), "IsTomeItem");
            if (isValid == null)
            {
                LogWarning("Vikings_Summoner mod is loaded but Vikings_Summoner.TomeCustomSlotManager:IsTomeItem is not found");
                return;
            }

            itemIsValid = item => item != null && (bool)isValid.Invoke(null, new object[] { item });

            getName = () => vikingsSummonerSlotName.Value;

            isActive = () => IsSlotActive(vikingsSummonerSlotGlobalKey.Value, vikingsSummonerSlotItemDiscovered.Value);

            initialized = true;
        }

        [HarmonyPatch]
        public static class VikingsSummoner_MagicSlot_AddCustomSlot_PreventCustomSlotAddition
        {
            public static MethodBase target;

            public static bool Prepare(MethodBase original)
            {
                if (!Chainloader.PluginInfos.TryGetValue(pluginID, out PluginInfo plugin))
                    return false;

                target ??= AccessTools.Method(Assembly.GetAssembly(plugin.Instance.GetType()).GetType("Vikings_Summoner.TomeCustomSlotManager"), "IsSlotSystemEnabled");
                if (target == null)
                    return false;

                if (original == null)
                    LogInfo("Vikings_Summoner.TomeCustomSlotManager:IsSlotSystemEnabled method is patched to enable internal custom slot logic");

                return true;
            }

            public static MethodBase TargetMethod() => target;

            public static void Postfix(ref bool __result) => __result = vikingsSummonerSlotEnabled.Value;
        }
    }
}