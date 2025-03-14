﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ExtraSlotsCustomSlots.JudesEquipmentBackpacksCustomSlot
{
    [Serializable]
    public class HumanoidJudesEquipmentBackpack
    {
        public ItemDrop.ItemData backpack;
    }

    public static class HumanoidExtension
    {
        private static readonly ConditionalWeakTable<Humanoid, HumanoidJudesEquipmentBackpack> data = new ConditionalWeakTable<Humanoid, HumanoidJudesEquipmentBackpack>();

        private static HumanoidJudesEquipmentBackpack GetBackpackData(this Humanoid humanoid) => data.GetOrCreateValue(humanoid);

        public static ItemDrop.ItemData GetJudesEquipmentBackpack(this Humanoid humanoid) => humanoid.GetBackpackData().backpack;

        public static ItemDrop.ItemData SetJudesEquipmentBackpack(this Humanoid humanoid, ItemDrop.ItemData item) => humanoid.GetBackpackData().backpack = item;
    }

    [Serializable]
    public class VisEquipmentJudesEquipmentBackpack
    {
        public string m_backpackItem = "";
        public List<GameObject> m_backpackItemInstances;
        public int m_currentbackpackItemHash = 0;

        public static readonly int s_backpackItem = "JudesEquipmentBackpackItem".GetStableHashCode();
    }

    public static class VisEquipmentExtension
    {
        private static readonly ConditionalWeakTable<VisEquipment, VisEquipmentJudesEquipmentBackpack> data = new ConditionalWeakTable<VisEquipment, VisEquipmentJudesEquipmentBackpack>();

        private static VisEquipmentJudesEquipmentBackpack GetBackpackData(this VisEquipment visEquipment) => data.GetOrCreateValue(visEquipment);

        private static void SetBackpackItem(this VisEquipment visEquipment, string name)
        {
            VisEquipmentJudesEquipmentBackpack backpackData = visEquipment.GetBackpackData();

            if (!(backpackData.m_backpackItem == name))
            {
                backpackData.m_backpackItem = name;
                if (visEquipment.m_nview.GetZDO() != null && visEquipment.m_nview.IsOwner())
                    visEquipment.m_nview.GetZDO().Set(VisEquipmentJudesEquipmentBackpack.s_backpackItem, (!string.IsNullOrEmpty(name)) ? name.GetStableHashCode() : 0);
            }
        }

        private static bool SetBackpackEquipped(this VisEquipment visEquipment, int hash)
        {
            VisEquipmentJudesEquipmentBackpack backpackData = visEquipment.GetBackpackData();
            if (backpackData.m_currentbackpackItemHash == hash)
                return false;

            if (backpackData.m_backpackItemInstances != null)
            {
                foreach (GameObject utilityItemInstance in backpackData.m_backpackItemInstances)
                {
                    if ((bool)visEquipment.m_lodGroup)
                        Utils.RemoveFromLodgroup(visEquipment.m_lodGroup, utilityItemInstance);

                    UnityEngine.Object.Destroy(utilityItemInstance);
                }

                backpackData.m_backpackItemInstances = null;
            }

            backpackData.m_currentbackpackItemHash = hash;
            if (hash != 0)
            {
                backpackData.m_backpackItemInstances = visEquipment.AttachArmor(hash);
                CustomItemType.ReorderBones?.Invoke(visEquipment, hash, backpackData.m_backpackItemInstances);
            }

            return true;
        }

        [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.UpdateEquipmentVisuals))]
        public static class VisEquipment_UpdateEquipmentVisuals_CustomItemType
        {
            private static void Prefix(VisEquipment __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                int backpackEquipped = 0;
                ZDO zDO = __instance.m_nview.GetZDO();
                if (zDO != null)
                {
                    backpackEquipped = zDO.GetInt(VisEquipmentJudesEquipmentBackpack.s_backpackItem);
                }
                else
                {
                    VisEquipmentJudesEquipmentBackpack backpackData = __instance.GetBackpackData();
                    if (!string.IsNullOrEmpty(backpackData.m_backpackItem))
                    {
                        backpackEquipped = backpackData.m_backpackItem.GetStableHashCode();
                    }
                }

                if (__instance.SetBackpackEquipped(backpackEquipped))
                    __instance.UpdateLodgroup();
            }
        }
        
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.SetupVisEquipment))]
        public static class Humanoid_SetupVisEquipment_CustomItemType
        {
            private static void Postfix(Humanoid __instance, VisEquipment visEq)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                ItemDrop.ItemData itemData = __instance.GetJudesEquipmentBackpack();

                visEq.SetBackpackItem((itemData != null && itemData.m_dropPrefab != null) ? itemData.m_dropPrefab.name : "");
            }
        }
    }

    public static class CustomItemType
    {
        private static readonly HashSet<StatusEffect> tempEffects = new HashSet<StatusEffect>();
        public static Func<ItemDrop.ItemData, bool> IsBackpack;
        public static Action<VisEquipment, int, List<GameObject>> ReorderBones;

        internal static void InitBackpackFunc(Func<ItemDrop.ItemData, bool> isValid)
        {
            IsBackpack = isValid;

            /*MethodInfo reorderBones = AccessTools.Method(JudesEquipmentBackpackSlot.assembly.GetType("Vapok.Common.Tools.BoneReorder"), "ReorderBones");
            if (reorderBones != null)
                ReorderBones = (VisEquipment visEq, int hash, List<GameObject> gameObjects) => reorderBones.Invoke(null, new object[] { visEq, hash, gameObjects });
            else
                LogWarning("JudesEquipmentBackpacks mod is loaded but Vapok.Common.Tools:ReorderBones is not found");*/
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
        public static class Humanoid_EquipItem_CustomItemType
        {
            [HarmonyPriority(Priority.First)]
            [HarmonyBefore(JudesEquipmentBackpackSlot.pluginID)]
            private static void Postfix(Humanoid __instance, ItemDrop.ItemData item, ref bool __result, bool triggerEquipEffects)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance.IsItemEquiped(item))
                    return;

                if (!IsBackpack(item))
                    return;

                if (__instance.GetJudesEquipmentBackpack() != null)
                {
                    __instance.UnequipItem(__instance.GetJudesEquipmentBackpack(), triggerEquipEffects);
                    __instance.m_visEquipment.UpdateEquipmentVisuals();
                }

                __instance.SetJudesEquipmentBackpack(item);

                if (__instance.IsItemEquiped(item))
                {
                    item.m_equipped = true;
                    __result = true;
                }

                __instance.SetupEquipment();
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
        public static class Humanoid_UnequipItem_CustomItemType
        {
            private static void Postfix(Humanoid __instance, ItemDrop.ItemData item)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (!IsBackpack(item))
                    return;

                if (__instance.GetJudesEquipmentBackpack() == item)
                {
                    __instance.SetJudesEquipmentBackpack(null);
                    __instance.SetupEquipment();
                }
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipAllItems))]
        public class Humanoid_UnequipAllItems_CustomItemType
        {
            public static void Postfix(Humanoid __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                __instance.UnequipItem(__instance.GetJudesEquipmentBackpack(), triggerEquipEffects: false);
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.IsItemEquiped))]
        public static class Humanoid_IsItemEquiped_CustomItemType
        {
            private static void Postfix(Humanoid __instance, ItemDrop.ItemData item, ref bool __result)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (!IsBackpack(item))
                    return;

                __result = __result || __instance.GetJudesEquipmentBackpack() == item;
            }
        }

        [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.IsEquipable))]
        public static class ItemDropItemData_IsEquipable_CustomItemType
        {
            private static void Postfix(ItemDrop.ItemData __instance, ref bool __result)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                __result = __result || __instance.m_shared.m_itemType == JudesEquipmentBackpackItem.GetItemType() && IsBackpack(__instance);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UnequipDeathDropItems))]
        private static class Player_UnequipDeathDropItems_JudesEquipmentBackpack
        {
            private static void Prefix(Player __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance.GetJudesEquipmentBackpack() is ItemDrop.ItemData item)
                {
                    item.m_equipped = false;
                    __instance.SetJudesEquipmentBackpack(null);
                    __instance.SetupEquipment();
                }
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.GetEquipmentEitrRegenModifier))]
        private static class Player_GetEquipmentEitrRegenModifier_JudesEquipmentBackpack
        {
            private static void Postfix(Player __instance, ref float __result)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance.GetJudesEquipmentBackpack() is ItemDrop.ItemData item)
                    __result += item.m_shared.m_eitrRegenModifier;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.Changed))]
        public static class Inventory_Changed_CustomItemType
        {
            private static void Prefix(Inventory __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance != Player.m_localPlayer?.GetInventory())
                    return;

                if (Player.m_localPlayer.GetJudesEquipmentBackpack() is ItemDrop.ItemData item && !__instance.ContainsItem(item))
                {
                    Player.m_localPlayer.SetJudesEquipmentBackpack(null);
                    Player.m_localPlayer.SetupEquipment();
                }
            }
        }
        
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GetEquipmentWeight))]
        public static class Humanoid_GetEquipmentWeight_CustomItemType
        {
            private static void Postfix(Humanoid __instance, ref float __result)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance.GetJudesEquipmentBackpack() is ItemDrop.ItemData item)
                    __result += item.m_shared.m_weight;
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipment))]
        private static class Humanoid_UpdateEquipment_JudesEquipmentBackpackDurabilityDrain
        {
            private static void Postfix(Humanoid __instance, float dt)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance.GetJudesEquipmentBackpack() is ItemDrop.ItemData item && item.m_shared.m_useDurability)
                    __instance.DrainEquipedItemDurability(item, dt);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.ApplyArmorDamageMods))]
        private static class Player_ApplyArmorDamageMods_JudesEquipmentBackpack
        {
            private static void Postfix(Player __instance, ref HitData.DamageModifiers mods)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance.GetJudesEquipmentBackpack() is ItemDrop.ItemData item)
                    mods.Apply(item.m_shared.m_damageModifiers);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.UpdateModifiers))]
        private static class Player_UpdateModifiers_JudesEquipmentBackpack
        {
            private static void Postfix(Player __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (Player.s_equipmentModifierSourceFields == null)
                    return;

                if (__instance.GetJudesEquipmentBackpack() is ItemDrop.ItemData item)
                    for (int i = 0; i < __instance.m_equipmentModifierValues.Length; i++)
                        __instance.m_equipmentModifierValues[i] += (float)Player.s_equipmentModifierSourceFields[i].GetValue(item.m_shared);
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GetSetCount))]
        private static class Humanoid_GetSetCount_JudesEquipmentBackpack
        {
            private static void Postfix(Humanoid __instance, string setName, ref int __result)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance.GetJudesEquipmentBackpack() is ItemDrop.ItemData item && item.m_shared.m_setName == setName)
                    __result++;
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UpdateEquipmentStatusEffects))]
        private static class Humanoid_UpdateEquipmentStatusEffects_JudesEquipmentBackpack
        {
            private static void Prefix(Humanoid __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                tempEffects.Clear();

                if (__instance.GetJudesEquipmentBackpack() is ItemDrop.ItemData item)
                {
                    if ((bool)item.m_shared.m_equipStatusEffect)
                        tempEffects.Add(item.m_shared.m_equipStatusEffect);

                    if (__instance.HaveSetEffect(item))
                        tempEffects.Add(item.m_shared.m_setStatusEffect);
                }
            }

            private static void Postfix(Humanoid __instance)
            {
                foreach (StatusEffect item in tempEffects.Where(item => !__instance.m_equipmentStatusEffects.Contains(item)))
                    __instance.m_seman.AddStatusEffect(item);

                __instance.m_equipmentStatusEffects.UnionWith(tempEffects);

                tempEffects.Clear();
            }
        }

        [HarmonyPatch(typeof(SEMan), nameof(SEMan.RemoveStatusEffect), typeof(int), typeof(bool))]
        private static class SEMan_RemoveStatusEffect_JudesEquipmentBackpackPreventRemoval
        {
            private static void Prefix(SEMan __instance, ref int nameHash)
            {
                if (__instance != Player.m_localPlayer?.GetSEMan() || tempEffects.Count == 0)
                    return;

                foreach (StatusEffect se in tempEffects)
                    if (se.NameHash() == nameHash)
                        nameHash = 0;
            }
        }
    }

    public static class JudesEquipmentBackpackItem
    {
        public static ItemDrop.ItemData.ItemType GetItemType()
        {
            if (!JudesEquipmentBackpackSlot.IsActive)
                return ItemDrop.ItemData.ItemType.Shoulder;

            return ItemDrop.ItemData.ItemType.Misc;
        }

        public static void PatchBackpackItemData(ItemDrop.ItemData itemData)
        {
            if (itemData == null)
                return;

            itemData.m_shared.m_itemType = GetItemType();
            itemData.m_shared.m_attachOverride = GetItemType();
        }

        public static void PatchInventory(Inventory inventory, bool force = false)
        {
            if (!JudesEquipmentBackpackSlot.IsActive && !force)
                return;

            if (inventory == null)
                return;

            foreach (ItemDrop.ItemData item in inventory.GetAllItems().Where(item => CustomItemType.IsBackpack(item)))
                PatchBackpackItemData(item);
        }

        public static void PatchBackpackItemOnConfigChange()
        {
            UpdateBackpacksItemType(force: true);
            PatchInventory(Player.m_localPlayer?.GetInventory(), force: true);
        }

        public static void UpdateBackpacksItemType(bool force = false)
        {
            if (!JudesEquipmentBackpackSlot.IsActive && !force)
                return;

            if (!ObjectDB.instance)
                return;

            foreach (GameObject item in ObjectDB.instance.m_items)
                if (item != null && item.GetComponent<ItemDrop>()?.m_itemData is ItemDrop.ItemData itemData && CustomItemType.IsBackpack(itemData))
                    PatchBackpackItemData(itemData);
        }

        [HarmonyPatch(typeof(Player), nameof(Player.AddKnownItem))]
        public static class Player_AddKnownItem_JudesEquipmentBackpackStats
        {
            private static void Postfix(Player __instance, ref ItemDrop.ItemData item)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance.m_knownMaterial.Contains(item.m_shared.m_name))
                    return;

                if (CustomItemType.IsBackpack(item))
                    PatchBackpackItemData(item);
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        public class Player_OnSpawned_JudesEquipmentBackpackStats
        {
            public static void Postfix(Player __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance != Player.m_localPlayer)
                    return;

                PatchInventory(__instance.GetInventory());
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.Load))]
        public class Inventory_Load_JudesEquipmentBackpackStats
        {
            public static void Postfix(Inventory __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                PatchInventory(__instance);
            }
        }

        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Start))]
        public static class ItemDrop_Start_JudesEquipmentBackpackStats
        {
            private static void Postfix(ref ItemDrop __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (CustomItemType.IsBackpack(__instance.m_itemData))
                    PatchBackpackItemData(__instance.m_itemData);
            }
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        public static class ObjectDB_Awake_ChangeBackpackItemType
        {
            [HarmonyPriority(Priority.Last)]
            [HarmonyAfter(JudesEquipmentBackpackSlot.pluginID)]
            private static void Postfix(ObjectDB __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
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
            [HarmonyAfter(JudesEquipmentBackpackSlot.pluginID)]
            private static void Postfix(ObjectDB __instance)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__instance.m_items.Count == 0 || __instance.GetItemPrefab("Wood") == null)
                    return;

                UpdateBackpacksItemType();
            }
        }
    }
}
