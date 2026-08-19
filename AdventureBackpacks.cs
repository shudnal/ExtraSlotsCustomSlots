using BepInEx.Bootstrap;
using ExtraSlotsCustomSlots.AdventureBackpacksCustomSlot;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class AdventureBackpacksSlot : CustomSlot
    {
        public const string ID = "AdventureBackpacks";
        public const string pluginID = "vapok.mods.adventurebackpacks";
        public static Assembly assembly;

        public static bool IsLoaded => Chainloader.PluginInfos.ContainsKey(pluginID);
        public static bool IsActive => IsLoaded && adventureBackpackSlotEnabled.Value;

        public AdventureBackpacksSlot()
        {
            slots.Add(this);

            GUID = pluginID;
            slotID = ID;

            if (!PluginInstalled)
                return;

            assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            MethodInfo isValid = AccessTools.Method(assembly.GetType("AdventureBackpacks.API.ABAPI"), "IsBackpack");
            if (isValid == null)
            {
                LogWarning("AdventureBackpacks mod is loaded but AdventureBackpacks.API.ABAPI:IsBackpack is not found");
                return;
            }

            itemIsValid = item => item != null && (bool)isValid.Invoke(null, new[] { item });

            getName = () => adventureBackpackSlotName.Value;

            isActive = () => IsSlotActive(adventureBackpackSlotGlobalKey.Value, adventureBackpackSlotItemDiscovered.Value);

            initialized = true;

            CustomItemType.InitBackpackFunc(itemIsValid);

            AdventureBackpacksPatches.UnpatchUnequip();
        }
    }

    public static class AdventureBackpacksPatches
    {
        public static MethodInfo prefixHumanoidUnequip;

        public static void UnpatchUnequip()
        {
            // Unpatch redundant method that unequip backpack as shoulder item
            // It will be called directly with shoulder item swap
            MethodInfo method = AccessTools.Method(typeof(Humanoid), nameof(Humanoid.UnequipItem));
            prefixHumanoidUnequip = AccessTools.Method(AdventureBackpacksSlot.assembly.GetType("AdventureBackpacks.Patches.HumanoidPatches+HumanoidUnequipItemPatch"), "Prefix");
            if (method != null && prefixHumanoidUnequip != null)
            {
                instance.harmony.Unpatch(method, prefixHumanoidUnequip);
                LogInfo("AdventureBackpacks.Patches.HumanoidPatches+HumanoidUnequipItemPatch:Prefix was unpatched and will be called directly.");
            }
            else
            {
                if (method == null)
                    LogWarning("Humanoid:UnequipItem was not found.");
                if (prefixHumanoidUnequip == null)
                    LogWarning("AdventureBackpacks.Patches.HumanoidPatches+HumanoidUnequipItemPatch:Prefix was not found.");
            }
        }

        [HarmonyPatch]
        public static class AdventureBackpacks_PlayerExtensions_CustomSlotItem
        {
            public static List<MethodBase> targets;

            public static List<MethodBase> GetTargets()
            {
                List<MethodBase> list = new List<MethodBase>();

                if (AdventureBackpacksSlot.assembly == null)
                    return list;

                if (AccessTools.Method(AdventureBackpacksSlot.assembly.GetType("AdventureBackpacks.Extensions.PlayerExtensions"), "IsBackpackEquipped") is MethodInfo method0)
                {
                    LogInfo("AdventureBackpacks.Extensions.PlayerExtensions:IsBackpackEquipped method is patched to make it work with custom slot");
                    list.Add(method0);
                }
                else
                    LogWarning("AdventureBackpacks.Extensions.PlayerExtensions:IsBackpackEquipped method was not found");

                if (AccessTools.Method(AdventureBackpacksSlot.assembly.GetType("AdventureBackpacks.Extensions.PlayerExtensions"), "IsThisBackpackEquipped") is MethodInfo method1)
                {
                    LogInfo("AdventureBackpacks.Extensions.PlayerExtensions:IsThisBackpackEquipped method is patched to make it work with custom slot");
                    list.Add(method1);
                }
                else
                    LogWarning("AdventureBackpacks.Extensions.PlayerExtensions:IsThisBackpackEquipped method was not found");

                if (AccessTools.Method(AdventureBackpacksSlot.assembly.GetType("AdventureBackpacks.Extensions.PlayerExtensions"), "GetEquippedBackpack") is MethodInfo method2)
                {
                    LogInfo("AdventureBackpacks.Extensions.PlayerExtensions:GetEquippedBackpack method is patched to make it work with custom slot");
                    list.Add(method2);
                }
                else
                    LogWarning("AdventureBackpacks.Extensions.PlayerExtensions:GetEquippedBackpack method was not found");

                return list;
            }

            public static bool Prepare() => AdventureBackpacksSlot.IsLoaded && (targets ??= GetTargets()).Count > 0;

            private static IEnumerable<MethodBase> TargetMethods() => targets;

            public static void Prefix(Player player, ref ItemDrop.ItemData __state)
            {
                if (!AdventureBackpacksSlot.IsActive)
                    return;

                __state = player.m_shoulderItem;
                player.m_shoulderItem = player.GetAdventureBackpack();
            }

            public static void Postfix(Player player, ItemDrop.ItemData __state)
            {
                if (!AdventureBackpacksSlot.IsActive)
                    return;

                player.m_shoulderItem = __state;
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
        public static class Humanoid_UnequipItem_CustomItemType_FirstPrefix
        {
            public static bool Prepare() => AdventureBackpacksSlot.IsLoaded;

            private static void Prefix(Humanoid __instance, ItemDrop.ItemData item)
            {
                if (!AdventureBackpacksSlot.IsActive || prefixHumanoidUnequip == null)
                    return;

                if (item == null || Player.m_localPlayer != __instance)
                    return;

                if (SceneManager.GetActiveScene().name.Equals("start"))
                    return;

                if (CustomItemType.IsBackpack(item) && item == __instance.GetAdventureBackpack() && item != __instance.m_shoulderItem)
                {
                    ItemDrop.ItemData tempItem = __instance.m_shoulderItem;
                    __instance.m_shoulderItem = item;
                    prefixHumanoidUnequip.Invoke(null, new object[] { item });
                    __instance.m_shoulderItem = tempItem;

                    LogInfo($"Shoulder item {tempItem}");
                    LogInfo($"swapped with");
                    LogInfo($"{item}");
                }
            }
        }

    }
}
