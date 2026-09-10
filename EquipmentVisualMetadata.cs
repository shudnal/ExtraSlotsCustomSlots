using UnityEngine;

namespace ExtraSlotsCustomSlots
{
    internal sealed class EquipmentVisualMetadata
    {
        private readonly int variantKey;
        private readonly int qualityKey;
        private int variant;
        private int quality;
        private int renderedVariant = -1;
        private int renderedQuality = -1;

        internal EquipmentVisualMetadata(string key)
        {
            variantKey = (key + "Variant").GetStableHashCode();
            qualityKey = (key + "Quality").GetStableHashCode();
        }

        internal void Set(VisEquipment equipment, int itemVariant, int itemQuality)
        {
            variant = itemVariant;
            quality = itemQuality;
            if (equipment.m_nview && equipment.m_nview.IsValid() && equipment.m_nview.IsOwner())
            {
                ZDO zdo = equipment.m_nview.GetZDO();
                zdo.Set(variantKey, variant);
                zdo.Set(qualityKey, quality);
            }
        }

        internal bool HasChanged(VisEquipment equipment)
        {
            Read(equipment, out int currentVariant, out int currentQuality);
            return renderedVariant != currentVariant || renderedQuality != currentQuality;
        }

        internal void Read(VisEquipment equipment, out int currentVariant, out int currentQuality)
        {
            ZDO zdo = equipment.m_nview ? equipment.m_nview.GetZDO() : null;
            currentVariant = zdo != null ? zdo.GetInt(variantKey) : variant;
            currentQuality = zdo != null ? zdo.GetInt(qualityKey) : quality;
        }

        internal void MarkRendered(int currentVariant, int currentQuality)
        {
            renderedVariant = currentVariant;
            renderedQuality = currentQuality;
        }
    }
}
