using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ExtraSlotsCustomSlots.ExtraSlotsCustomSlots;

namespace ExtraSlotsCustomSlots
{
    internal static class EpicLootCompatibility
    {
        internal const string EpicLootGuid = "randyknapp.mods.epicloot";

        private const string ProviderId = global::ExtraSlotsCustomSlots.ExtraSlotsCustomSlots.pluginID;
        private const string ItemTypeClassifierTypeName = "EpicLoot.GatedItemType.ItemTypeClassifier";
        private const string ShoulderItemInfoType = "ShouldersArmor";
        private const string UtilityItemInfoType = "Utility";

        private static readonly System.Version Version013 = new System.Version(0, 13, 0);

        private static readonly string[] EnchantingMethods =
        {
            "GetSacrificeProducts",
            "GetEnchantCost",
            "GetRuneCost",
            "GetAugmentCost",
            "GetReAugmentCost"
        };

        // Epic Loot 0.13 introduced the supported equipment-provider API and the item classifier
        // used by Shardstones. Earlier releases need direct integration with their internal methods.
        private enum CompatibilityBranch
        {
            None,
            Legacy,
            Version013OrLater
        }

        private static CompatibilityBranch compatibilityBranch;
        private static bool compatibilityBranchResolved;
        private static System.Version epicLootVersion;
        private static Assembly epicLootAssembly;
        private static MethodInfo registerEquipmentProvider;
        private static MethodInfo unregisterEquipmentProvider;
        private static MethodInfo invalidatePlayerEffectCache;
        private static MethodInfo resetLegacyEquipmentEffectCache;
        private static Func<ItemDrop.ItemData, bool> isMagicItem;
        private static bool equipmentProviderRegistered;

        internal static void Prepare()
        {
            ResolveCompatibilityBranch();
        }

        internal static void Initialize()
        {
            if (ResolveCompatibilityBranch() != CompatibilityBranch.Version013OrLater || equipmentProviderRegistered || registerEquipmentProvider == null)
                return;

            try
            {
                equipmentProviderRegistered = registerEquipmentProvider.Invoke(null, new object[]
                {
                    ProviderId,
                    new Func<Player, List<ItemDrop.ItemData>>(GetExtraEquippedItems)
                }) is bool registered && registered;

                if (equipmentProviderRegistered)
                {
                    LogInfo("Registered Extra Slots Custom Slots equipment provider through the Epic Loot 0.13+ API.");
                    InvalidatePlayerEffectCache(Player.m_localPlayer);
                }
                else
                {
                    LogWarning("Epic Loot rejected the Extra Slots Custom Slots equipment provider registration.");
                }
            }
            catch (Exception exception)
            {
                LogWarning($"Failed to register the Extra Slots Custom Slots equipment provider through Epic Loot API: {GetExceptionMessage(exception)}");
            }
        }

        internal static void Shutdown()
        {
            if (!equipmentProviderRegistered)
                return;

            try
            {
                unregisterEquipmentProvider?.Invoke(null, new object[] { ProviderId });
            }
            catch (Exception exception)
            {
                LogWarning($"Failed to unregister the Extra Slots Custom Slots equipment provider from Epic Loot API: {GetExceptionMessage(exception)}");
            }
            finally
            {
                equipmentProviderRegistered = false;
            }
        }

        internal static void InvalidatePlayerEffectCache(Player player)
        {
            if (player == null)
                return;

            ResolveCompatibilityBranch();

            MethodInfo invalidateMethod = compatibilityBranch == CompatibilityBranch.Version013OrLater
                ? invalidatePlayerEffectCache ?? resetLegacyEquipmentEffectCache
                : resetLegacyEquipmentEffectCache;

            if (invalidateMethod == null)
                return;

            try
            {
                invalidateMethod.Invoke(null, new object[] { player });
            }
            catch (Exception exception)
            {
                LogWarning($"Failed to invalidate the Epic Loot equipment effect cache: {GetExceptionMessage(exception)}");
            }
        }

        private static CompatibilityBranch ResolveCompatibilityBranch()
        {
            if (compatibilityBranchResolved)
                return compatibilityBranch;

            compatibilityBranchResolved = true;

            if (!Chainloader.PluginInfos.TryGetValue(EpicLootGuid, out PluginInfo pluginInfo))
                return compatibilityBranch = CompatibilityBranch.None;

            epicLootAssembly = pluginInfo.Instance?.GetType().Assembly;
            epicLootVersion = pluginInfo.Metadata?.Version;

            if (epicLootAssembly == null)
                return compatibilityBranch = CompatibilityBranch.None;

            Type apiType = epicLootAssembly.GetType("EpicLoot.API");
            Type equipmentProviderType = typeof(Func<Player, List<ItemDrop.ItemData>>);

            registerEquipmentProvider = FindMethod(apiType, "RegisterEquipmentProvider", typeof(string), equipmentProviderType);
            unregisterEquipmentProvider = FindMethod(apiType, "UnregisterEquipmentProvider", typeof(string));
            invalidatePlayerEffectCache = FindMethod(apiType, "InvalidatePlayerEffectCache", typeof(Player));

            Type equipmentEffectCacheType = epicLootAssembly.GetType("EpicLoot.EquipmentEffectCache");
            resetLegacyEquipmentEffectCache = FindMethod(equipmentEffectCacheType, "Reset", typeof(Player));

            Type itemDataExtensionsType = epicLootAssembly.GetType("EpicLoot.ItemDataExtensions");
            MethodInfo isMagicMethod = FindMethod(itemDataExtensionsType, "IsMagic", typeof(ItemDrop.ItemData));
            if (isMagicMethod != null)
            {
                try
                {
                    isMagicItem = (Func<ItemDrop.ItemData, bool>)Delegate.CreateDelegate(
                        typeof(Func<ItemDrop.ItemData, bool>),
                        isMagicMethod);
                }
                catch
                {
                    isMagicItem = null;
                }
            }

            bool providerApiAvailable = registerEquipmentProvider != null;

            // Prefer the declared plugin version. The endpoint probe is only a fallback for builds
            // that do not expose a usable metadata version.
            compatibilityBranch = epicLootVersion != null
                ? epicLootVersion.CompareTo(Version013) >= 0
                    ? CompatibilityBranch.Version013OrLater
                    : CompatibilityBranch.Legacy
                : providerApiAvailable
                    ? CompatibilityBranch.Version013OrLater
                    : CompatibilityBranch.Legacy;

            string versionText = epicLootVersion?.ToString() ?? "unknown version";
            string branchText = compatibilityBranch == CompatibilityBranch.Version013OrLater
                ? "0.13 or later compatibility"
                : "legacy pre-0.13 compatibility";

            LogInfo($"Epic Loot {versionText}: using {branchText}.");
            return compatibilityBranch;
        }

        private static MethodInfo FindMethod(Type type, string methodName, params Type[] parameterTypes)
        {
            return type?.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
                null,
                parameterTypes,
                null);
        }

        private static Assembly GetEpicLootAssembly()
        {
            ResolveCompatibilityBranch();
            return epicLootAssembly;
        }

        private static bool UsesDirectEquipmentPatches()
        {
            // Legacy releases have no provider API. Keep a silent direct-patch fallback for an
            // unversioned or non-standard 0.13+ build that does not expose the endpoint either.
            CompatibilityBranch branch = ResolveCompatibilityBranch();
            return branch == CompatibilityBranch.Legacy ||
                   (branch == CompatibilityBranch.Version013OrLater && registerEquipmentProvider == null);
        }

        private static bool UsesVersion013OrLaterCompatibility()
        {
            return ResolveCompatibilityBranch() == CompatibilityBranch.Version013OrLater;
        }

        private static bool HasSupportedBackpackMod()
        {
            return AdventureBackpacksSlot.IsLoaded || JudesEquipmentBackpackSlot.IsLoaded;
        }

        private static List<ItemDrop.ItemData> GetExtraEquippedItems(Player player)
        {
            List<ItemDrop.ItemData> items = new List<ItemDrop.ItemData>();

            if (player == null || player != Player.m_localPlayer)
                return items;

            foreach (CustomSlot customSlot in CustomSlot.slots)
            {
                if (!customSlot.initialized)
                    continue;

                ExtraSlots.Slots.Slot extraSlot = ExtraSlots.API.FindSlot(CustomSlot.GetSlotID(customSlot.slotID));
                if (extraSlot == null || !extraSlot.IsActive)
                    continue;

                ItemDrop.ItemData item = extraSlot.Item;
                if (item != null && player.IsItemEquiped(item) && !items.Contains(item))
                    items.Add(item);
            }

            return items;
        }

        private static bool IsSupportedBackpack(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null)
                return false;

            if (AdventureBackpacksSlot.IsActive &&
                AdventureBackpacksCustomSlot.CustomItemType.IsBackpack != null &&
                AdventureBackpacksCustomSlot.CustomItemType.IsBackpack(item))
            {
                return true;
            }

            return JudesEquipmentBackpackSlot.IsActive &&
                   JudesEquipmentBackpacksCustomSlot.CustomItemType.IsBackpack != null &&
                   JudesEquipmentBackpacksCustomSlot.CustomItemType.IsBackpack(item);
        }

        private static string GetExceptionMessage(Exception exception)
        {
            return exception is TargetInvocationException invocationException && invocationException.InnerException != null
                ? invocationException.InnerException.Message
                : exception.Message;
        }

        private static List<MethodBase> FindItemDataMethods(string typeName, IEnumerable<string> methodNames)
        {
            List<MethodBase> targets = new List<MethodBase>();
            Type type = GetEpicLootAssembly()?.GetType(typeName);

            if (type == null)
                return targets;

            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (string methodName in methodNames)
            {
                List<MethodInfo> matches = methods
                    .Where(method => method.Name == methodName)
                    .Where(method => method.GetParameters().FirstOrDefault()?.ParameterType == typeof(ItemDrop.ItemData))
                    .ToList();

                foreach (MethodInfo method in matches)
                {
                    if (!targets.Contains(method))
                        targets.Add(method);
                }

                if (matches.Count > 0)
                    LogInfo($"{typeName}:{methodName} method{(matches.Count == 1 ? "" : " overloads")} will treat supported custom backpacks as shoulder items.");
            }

            return targets;
        }

        private static List<MethodBase> FindItemDataMethods(string typeName, params string[] methodNames)
        {
            return FindItemDataMethods(typeName, (IEnumerable<string>)methodNames);
        }

        private struct ItemTypePatchState
        {
            public bool Changed;
            public ItemDrop.ItemData.ItemType OriginalType;
        }

        [HarmonyPatch]
        private static class EpicLoot_DirectPlayerEquipmentMethods_AddCustomSlotItems
        {
            private static List<MethodBase> targets;

            private static bool Prepare()
            {
                if (!UsesDirectEquipmentPatches())
                    return false;

                targets = GetTargets();
                return targets.Count > 0;
            }

            private static IEnumerable<MethodBase> TargetMethods() => targets ?? (targets = GetTargets());

            private static List<MethodBase> GetTargets()
            {
                List<MethodBase> result = new List<MethodBase>();
                Type playerExtensionsType = GetEpicLootAssembly()?.GetType("EpicLoot.PlayerExtensions");

                MethodInfo method = FindMethod(playerExtensionsType, "GetMagicEquipment", typeof(Player));
                string methodName = "GetMagicEquipment";

                if (method == null)
                {
                    method = FindMethod(playerExtensionsType, "GetEquipment", typeof(Player));
                    methodName = "GetEquipment";
                }

                if (method != null)
                {
                    result.Add(method);
                    LogInfo($"EpicLoot.PlayerExtensions:{methodName} will include items equipped in Extra Slots Custom Slots.");
                }

                return result;
            }

            [HarmonyPriority(Priority.Last)]
            private static void Postfix(Player __0, List<ItemDrop.ItemData> __result)
            {
                if (__result == null)
                    return;

                foreach (ItemDrop.ItemData item in GetExtraEquippedItems(__0))
                {
                    if (isMagicItem != null && !isMagicItem(item))
                        continue;

                    if (!__result.Contains(item))
                        __result.Add(item);
                }
            }
        }

        [HarmonyPatch]
        private static class EpicLoot_ItemTypeChecks_TreatCustomBackpacksAsShoulder
        {
            private static List<MethodBase> targets;

            private static bool Prepare()
            {
                if (ResolveCompatibilityBranch() == CompatibilityBranch.None || !HasSupportedBackpackMod())
                    return false;

                targets = GetTargets();
                return targets.Count > 0;
            }

            private static IEnumerable<MethodBase> TargetMethods() => targets ?? (targets = GetTargets());

            private static List<MethodBase> GetTargets()
            {
                List<MethodBase> result = new List<MethodBase>();
                result.AddRange(FindItemDataMethods(
                    "EpicLoot.Crafting.EnchantCostsHelper",
                    EnchantingMethods));

                result.AddRange(FindItemDataMethods(
                    "EpicLoot.EpicLoot",
                    "CanBeMagicItem"));

                result.AddRange(FindItemDataMethods(
                    "EpicLoot.MagicItemEffectRequirements",
                    "AllowByItemType",
                    "ExcludeByItemType",
                    "CheckRequirements"));

                return result.Distinct().ToList();
            }

            [HarmonyPriority(Priority.First)]
            private static void Prefix(ItemDrop.ItemData __0, ref ItemTypePatchState __state)
            {
                if (!IsSupportedBackpack(__0))
                    return;

                __state.Changed = true;
                __state.OriginalType = __0.m_shared.m_itemType;
                __0.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Shoulder;
            }

            private static Exception Finalizer(ItemDrop.ItemData __0, ItemTypePatchState __state, Exception __exception)
            {
                if (__state.Changed && __0?.m_shared != null)
                    __0.m_shared.m_itemType = __state.OriginalType;

                return __exception;
            }
        }

        [HarmonyPatch]
        private static class EpicLoot_Version013OrLater_ItemTypeClassifier_ClassifyCustomBackpacksAsShoulder
        {
            private static List<MethodBase> targets;

            private static bool Prepare()
            {
                if (!UsesVersion013OrLaterCompatibility() || !HasSupportedBackpackMod())
                    return false;

                targets = FindItemDataMethods(
                    ItemTypeClassifierTypeName,
                    "ClassifyFromFields");

                return targets.Count > 0;
            }

            private static IEnumerable<MethodBase> TargetMethods() => targets ?? (targets = FindItemDataMethods(
                ItemTypeClassifierTypeName,
                "ClassifyFromFields"));

            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ItemDrop.ItemData __0, ref string __result)
            {
                if (IsSupportedBackpack(__0))
                    __result = ShoulderItemInfoType;
            }
        }

        [HarmonyPatch]
        private static class EpicLoot_Version013OrLater_ItemTypeClassifier_TryGetConfiguredType_TreatCustomBackpacksAsShoulder
        {
            private static MethodBase target;

            private static bool Prepare()
            {
                if (!UsesVersion013OrLaterCompatibility() || !HasSupportedBackpackMod())
                    return false;

                Type classifierType = GetEpicLootAssembly()?.GetType(ItemTypeClassifierTypeName);
                target = FindMethod(classifierType, "TryGetConfiguredType", typeof(ItemDrop.ItemData), typeof(string).MakeByRefType());

                if (target != null)
                    LogInfo("EpicLoot.GatedItemType.ItemTypeClassifier:TryGetConfiguredType will classify supported custom backpacks as shoulder armor.");

                return target != null;
            }

            private static MethodBase TargetMethod() => target;

            [HarmonyPriority(Priority.Last)]
            private static void Postfix(ItemDrop.ItemData __0, ref string __1, ref bool __result)
            {
                if (!IsSupportedBackpack(__0))
                    return;

                // Preserve an explicit non-utility iteminfo classification. Utility is the automatic
                // classification caused by this mod changing an active custom backpack to ItemType.Misc.
                if (__result && !string.IsNullOrEmpty(__1) && !string.Equals(__1, UtilityItemInfoType, StringComparison.OrdinalIgnoreCase))
                    return;

                __1 = ShoulderItemInfoType;
                __result = true;
            }
        }
    }
}
