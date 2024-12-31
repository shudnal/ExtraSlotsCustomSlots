using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    public class BowsBeforeHoesSlot : CustomSlot
    {
        public const string ID = "BowsBeforeHoes";
        public const string pluginID = "Azumatt.BowsBeforeHoes";
        public static bool _isActive;
        private static Func<ItemDrop.ItemData, bool> _isQuiver;

        public static bool IsActive => _isActive && bbhQuiverSlotEnabled.Value;

        public static bool IsQuiver(ItemDrop.ItemData item) => _isQuiver != null && _isQuiver(item);

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

            isActive = () => IsSlotActive(bbhQuiverSlotGlobalKey.Value, bbhQuiverSlotItemDiscovered.Value);

            initialized = true;

            _isActive = true;
            _isQuiver = itemIsValid;
        }
    }

    public static class BowsBeforeHoesCompat
    {
        public static ItemDrop.ItemData.ItemType GetItemType()
        {
            if (!BowsBeforeHoesSlot.IsActive)
                return ItemDrop.ItemData.ItemType.Shoulder;

            return ItemDrop.ItemData.ItemType.Misc;
        }

        public static void PatchBackpackItemData(ItemDrop.ItemData itemData)
        {
            if (itemData == null)
                return;

            itemData.m_shared.m_itemType = GetItemType();
        }

        public static void PatchInventory(Inventory inventory, bool force = false)
        {
            if (!BowsBeforeHoesSlot.IsActive && !force)
                return;

            if (inventory == null)
                return;

            foreach (ItemDrop.ItemData item in inventory.GetAllItems().Where(item => BowsBeforeHoesSlot.IsQuiver(item)))
                PatchBackpackItemData(item);
        }

        public static void PatchBackpackItemOnConfigChange()
        {
            UpdateBackpacksItemType(force: true);
            PatchInventory(Player.m_localPlayer?.GetInventory(), force: true);
        }

        public static void UpdateBackpacksItemType(bool force = false)
        {
            if (!BowsBeforeHoesSlot.IsActive && !force)
                return;

            if (!ObjectDB.instance)
                return;

            foreach (GameObject item in ObjectDB.instance.m_items)
                if (item != null && item.GetComponent<ItemDrop>()?.m_itemData is ItemDrop.ItemData itemData && BowsBeforeHoesSlot.IsQuiver(itemData))
                    PatchBackpackItemData(itemData);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.AddKnownItem))]
        public static class Player_AddKnownItem_BBHQuiverType
        {
            private static void Postfix(Player __instance, ref ItemDrop.ItemData item)
            {
                if (!BowsBeforeHoesSlot.IsActive)
                    return;

                if (__instance.m_knownMaterial.Contains(item.m_shared.m_name))
                    return;

                if (BowsBeforeHoesSlot.IsQuiver(item))
                    PatchBackpackItemData(item);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        public class Player_OnSpawned_BBHQuiverType
        {
            public static void Postfix(Player __instance)
            {
                if (!BowsBeforeHoesSlot.IsActive)
                    return;

                if (__instance != Player.m_localPlayer)
                    return;

                PatchInventory(__instance.GetInventory());
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.Load))]
        public class Inventory_Load_BBHQuiverType
        {
            public static void Postfix(Inventory __instance)
            {
                if (!BowsBeforeHoesSlot.IsActive)
                    return;

                PatchInventory(__instance);
            }
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class ObjectDB_Awake_ChangeBackpackItemType
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ObjectDB __instance)
            {
                if (!BowsBeforeHoesSlot.IsActive)
                    return;

                if (__instance.m_items.Count == 0 || __instance.GetItemPrefab("Wood") == null)
                    return;

                UpdateBackpacksItemType();
            }
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
        public static class ObjectDB_CopyOtherDB_AddPrefab
        {
            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ObjectDB __instance)
            {
                if (!BowsBeforeHoesSlot.IsActive)
                    return;

                if (__instance.m_items.Count == 0 || __instance.GetItemPrefab("Wood") == null)
                    return;

                UpdateBackpacksItemType();
            }
        }
    }
}