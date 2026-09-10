using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ExtraSlotsCustomSlots
{
    internal static class InventoryCompatibility
    {
        // Inventory(bool) always enables temporary mode, even when its argument is false.
        // Its load path creates serialization records without the prefab's SharedData.
        // Never treat those records as live equipment or use that inventory for item recovery.
        internal static bool IsRuntimeInventory(Inventory inventory) => inventory != null && !inventory.m_temoraryInventory;

        internal static bool IsRuntimeItem(ItemDrop.ItemData item) => item?.m_shared != null && !string.IsNullOrEmpty(item.m_shared.m_name);

        internal static IEnumerable<MethodBase> GetLoadMethods()
        {
            yield return AccessTools.DeclaredMethod(typeof(Inventory), nameof(Inventory.Load), new[] { typeof(ZPackage) })
                ?? throw new MissingMethodException("Inventory.Load(ZPackage) was not found. Valheim 1.0.7 is required.");
            yield return AccessTools.DeclaredMethod(typeof(Inventory), nameof(Inventory.Load), new[] { typeof(ZPackage), typeof(bool) })
                ?? throw new MissingMethodException("Inventory.Load(ZPackage, bool) was not found. Valheim 1.0.7 is required.");
        }
    }
}
