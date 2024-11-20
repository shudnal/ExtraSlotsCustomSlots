using BepInEx;
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
        
        public static bool IsActive => Chainloader.PluginInfos.ContainsKey(pluginID) && adventureBackpackSlotEnabled.Value;

        public AdventureBackpacksSlot()
        {
            slots.Add(this);

            GUID = pluginID;

            if (!PluginInstalled)
                return;

            Assembly assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            MethodInfo isValid = AccessTools.Method(assembly.GetType("AdventureBackpacks.API.ABAPI"), "IsBackpack");
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

            CustomItemType.InitBackpackFunc(itemIsValid, assembly);
        }
    }

    public static class AdventureBackpacksPatches
    {
        [HarmonyPatch]
        public static class AdventureBackpacks_PlayerExtensions_CustomSlotItem
        {
            public static List<MethodBase> targets = new List<MethodBase>();

            public static bool Prepare()
            {
                if (!Chainloader.PluginInfos.TryGetValue(AdventureBackpacksSlot.pluginID, out PluginInfo plugin))
                    return false;

                Assembly assembly = Assembly.GetAssembly(plugin.Instance.GetType());

                if (AccessTools.Method(assembly.GetType("AdventureBackpacks.Extensions.PlayerExtensions"), "IsBackpackEquipped") is MethodInfo method0)
                {
                    LogInfo("AdventureBackpacks.Extensions.PlayerExtensions:IsBackpackEquipped method will be patched to make it work with custom slot");
                    targets.Add(method0);
                }

                if (AccessTools.Method(assembly.GetType("AdventureBackpacks.Extensions.PlayerExtensions"), "IsThisBackpackEquipped") is MethodInfo method1)
                {
                    LogInfo("AdventureBackpacks.Extensions.PlayerExtensions:IsThisBackpackEquipped method will be patched to make it work with custom slot");
                    targets.Add(method1);
                }

                if (AccessTools.Method(assembly.GetType("AdventureBackpacks.Extensions.PlayerExtensions"), "GetEquippedBackpack") is MethodInfo method2)
                {
                    LogInfo("AdventureBackpacks.Extensions.PlayerExtensions:GetEquippedBackpack method will be patched to make it work with custom slot");
                    targets.Add(method2);
                }

                if (targets.Count == 0)
                    return false;

                return true;
            }

            private static IEnumerable<MethodBase> TargetMethods() => targets;

            public static void Prefix(Player player, ref ItemDrop.ItemData __state)
            {
                if (!AdventureBackpacksSlot.IsActive)
                    return;

                if ((__state = player.m_shoulderItem) == null)
                    return;

                player.m_shoulderItem = player.GetAdventureBackpack();
            }

            public static void Postfix(Player player, ItemDrop.ItemData __state)
            {
                if (__state == null)
                    return;

                player.m_shoulderItem = __state;
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
        public static class Humanoid_UnequipItem_CustomItemType_FirstPrefix
        {
            public static ItemDrop.ItemData m_shoulderItem;

            public static bool Prepare() => Chainloader.PluginInfos.ContainsKey(AdventureBackpacksSlot.pluginID);
            
            [HarmonyPriority(Priority.First)]
            [HarmonyBefore(AdventureBackpacksSlot.pluginID)]
            private static void Prefix(Humanoid __instance, ItemDrop.ItemData item)
            {
                if (!AdventureBackpacksSlot.IsActive)
                    return;

                if (item == null || Player.m_localPlayer != __instance)
                    return;

                if (SceneManager.GetActiveScene().name.Equals("start"))
                    return;

                if (!CustomItemType.IsBackpack(item))
                    return;

                if ((m_shoulderItem = __instance.m_shoulderItem) == null)
                    return;

                __instance.m_shoulderItem = __instance.GetAdventureBackpack();
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
        public static class Humanoid_UnequipItem_CustomItemType_LastPrefix
        {
            public static bool Prepare() => Chainloader.PluginInfos.ContainsKey(AdventureBackpacksSlot.pluginID);

            [HarmonyPriority(Priority.Last)]
            [HarmonyAfter(AdventureBackpacksSlot.pluginID)]
            private static void Prefix(Humanoid __instance)
            {
                if (Humanoid_UnequipItem_CustomItemType_FirstPrefix.m_shoulderItem == null)
                    return;

                __instance.m_shoulderItem = Humanoid_UnequipItem_CustomItemType_FirstPrefix.m_shoulderItem;
            }
        }

        [HarmonyPatch]
        public static class EpicLoot_Player_GetEquipment_AddBackpackItem
        {
            public const string epicLootGUID = "randyknapp.mods.epicloot";

            public static MethodBase target;

            public static bool Prepare()
            {
                if (!Chainloader.PluginInfos.ContainsKey(epicLootGUID))
                    return false;

                target = AccessTools.Method("EpicLoot.PlayerExtensions:GetEquipment");
                if (target == null)
                    return false;

                LogInfo("EpicLoot.PlayerExtensions:GetEquipment method will be patched to add adventure backpack");
                return true;
            }

            public static MethodBase TargetMethod() => target;

            public static void Postfix(Player player, List<ItemDrop.ItemData> __result)
            {
                if (!AdventureBackpacksSlot.IsActive)
                    return;

                if (player.GetAdventureBackpack() is ItemDrop.ItemData backpack)
                    __result.Add(backpack);
            }
        }
    }
}