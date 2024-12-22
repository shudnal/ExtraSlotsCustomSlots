using BepInEx.Bootstrap;
using HarmonyLib;
using System.Collections.Generic;
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

            if (!PluginInstalled)
                return;

            assembly = Assembly.GetAssembly(Chainloader.PluginInfos[pluginID].Instance.GetType());

            slotID = ID;

            itemIsValid = (ItemDrop.ItemData item) => item != null && item.m_dropPrefab != null && (item.m_dropPrefab.name == "BackpackSimple" || item.m_dropPrefab.name == "BackpackHeavy");

            getName = () => judesEquipmentBackpackSlotName.Value;

            isActive = () => ExtraSlots.API.IsAnyGlobalKeyActive(judesEquipmentBackpackSlotGlobalKey.Value) || ExtraSlots.API.IsAnyMaterialDiscovered(judesEquipmentBackpackSlotItemDiscovered.Value);

            initialized = true;

            CustomItemType.InitBackpackFunc(itemIsValid);
        }
    }

    public static class EpicLootCompat
    {
        public const string epicLootGUID = "randyknapp.mods.epicloot";
        public static Assembly assembly;

        [HarmonyPatch]
        public static class EpicLoot_EnchantCostsHelper_CanBeMagicItem_TreatBackpackAsShoulder
        {
            public static List<MethodBase> targets;

            public static List<MethodBase> GetTargets()
            {
                assembly ??= Assembly.GetAssembly(Chainloader.PluginInfos[epicLootGUID].Instance.GetType());

                List<MethodBase> list = new List<MethodBase>();

                if (AccessTools.Method(assembly.GetType("EpicLoot.Crafting.EnchantCostsHelper"), "GetSacrificeProducts", new System.Type[] { typeof(ItemDrop.ItemData) }) is MethodInfo method0)
                {
                    LogInfo("EpicLoot.Crafting.EnchantCostsHelper:GetSacrificeProducts method is patched to make it work with custom backpack item type");
                    list.Add(method0);
                }
                else
                    LogWarning("EpicLoot.Crafting.EnchantCostsHelper:GetSacrificeProducts method was not found");

                if (AccessTools.Method(assembly.GetType("EpicLoot.Crafting.EnchantCostsHelper"), "GetEnchantCost") is MethodInfo method2)
                {
                    LogInfo("EpicLoot.Crafting.EnchantCostsHelper:GetEnchantCost method is patched to make it work with custom backpack item type");
                    list.Add(method2);
                }
                else
                    LogWarning("EpicLoot.Crafting.EnchantCostsHelper:GetEnchantCost method was not found");

                if (AccessTools.Method(assembly.GetType("EpicLoot.Crafting.EnchantCostsHelper"), "GetAugmentCost") is MethodInfo method3)
                {
                    LogInfo("EpicLoot.Crafting.EnchantCostsHelper:GetAugmentCost method is patched to make it work with custom backpack item type");
                    list.Add(method3);
                }
                else
                    LogWarning("EpicLoot.Crafting.EnchantCostsHelper:GetAugmentCost method was not found");

                if (AccessTools.Method(assembly.GetType("EpicLoot.Crafting.EnchantCostsHelper"), "GetReAugmentCost") is MethodInfo method4)
                {
                    LogInfo("EpicLoot.Crafting.EnchantCostsHelper:GetReAugmentCost method is patched to make it work with custom backpack item type");
                    list.Add(method4);
                }
                else
                    LogWarning("EpicLoot.Crafting.EnchantCostsHelper:GetReAugmentCost method was not found");

                if (AccessTools.Method(assembly.GetType("EpicLoot.EpicLoot"), "CanBeMagicItem") is MethodInfo method5)
                {
                    LogInfo("EpicLoot.EpicLoot:CanBeMagicItem method is patched to make it work with custom backpack item type");
                    list.Add(method5);
                }
                else
                    LogWarning("EpicLoot.EpicLoot:CanBeMagicItem method was not found");

                return list;
            }

            public static bool Prepare() => JudesEquipmentBackpackSlot.IsLoaded && Chainloader.PluginInfos.ContainsKey(epicLootGUID) && (targets ??= GetTargets()).Count > 0;
            
            private static IEnumerable<MethodBase> TargetMethods() => targets;

            public static void Prefix(ItemDrop.ItemData item, ref bool __state)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__state = CustomItemType.IsBackpack(item))
                    item.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Shoulder;
            }

            public static void Postfix(ItemDrop.ItemData item, bool __state)
            {
                if (__state)
                    JudesEquipmentBackpackItem.PatchBackpackItemData(item);
            }
        }

        [HarmonyPatch]
        public static class EpicLoot_MagicItemEffectRequirements_argItemData_TreatBackpackAsShoulder
        {
            public static List<MethodBase> targets;

            public static List<MethodBase> GetTargets()
            {
                assembly ??= Assembly.GetAssembly(Chainloader.PluginInfos[epicLootGUID].Instance.GetType());

                List<MethodBase> list = new List<MethodBase>();

                if (AccessTools.Method(assembly.GetType("EpicLoot.MagicItemEffectRequirements"), "AllowByItemType") is MethodInfo method6)
                {
                    LogInfo("EpicLoot.MagicItemEffectRequirements:AllowByItemType method is patched to make it work with custom backpack item type");
                    list.Add(method6);
                }
                else
                    LogWarning("EpicLoot.MagicItemEffectRequirements:AllowByItemType method was not found");

                if (AccessTools.Method(assembly.GetType("EpicLoot.MagicItemEffectRequirements"), "ExcludeByItemType") is MethodInfo method7)
                {
                    LogInfo("EpicLoot.MagicItemEffectRequirements:ExcludeByItemType method is patched to make it work with custom backpack item type");
                    list.Add(method7);
                }
                else
                    LogWarning("EpicLoot.MagicItemEffectRequirements:ExcludeByItemType method was not found");

                if (AccessTools.Method(assembly.GetType("EpicLoot.MagicItemEffectRequirements"), "CheckRequirements") is MethodInfo method8)
                {
                    LogInfo("EpicLoot.MagicItemEffectRequirements:CheckRequirements method is patched to make it work with custom backpack item type");
                    list.Add(method8);
                }
                else
                    LogWarning("EpicLoot.MagicItemEffectRequirements:CheckRequirements method was not found");

                return list;
            }

            public static bool Prepare() => JudesEquipmentBackpackSlot.IsLoaded && Chainloader.PluginInfos.ContainsKey(epicLootGUID) && (targets ??= GetTargets()).Count > 0;

            private static IEnumerable<MethodBase> TargetMethods() => targets;

            public static void Prefix(ItemDrop.ItemData itemData, ref bool __state)
            {
                if (!JudesEquipmentBackpackSlot.IsActive)
                    return;

                if (__state = CustomItemType.IsBackpack(itemData))
                    itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Shoulder;
            }

            public static void Postfix(ItemDrop.ItemData itemData, bool __state)
            {
                if (__state)
                    JudesEquipmentBackpackItem.PatchBackpackItemData(itemData);
            }
        }
    }
}