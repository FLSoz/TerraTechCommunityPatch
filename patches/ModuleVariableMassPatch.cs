using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace CommunityPatch.patches
{
    internal static class ModuleVariableMassPatch
    {
        internal static BindingFlags InstanceFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        internal static FieldInfo m_AdditionalMassCategories = AccessTools.Field(typeof(TankBlock), "m_AdditionalMassCategories");
        internal static FieldInfo m_DefaultMasss = AccessTools.Field(typeof(TankBlock), "m_DefaultMass");
        internal static FieldInfo m_CurrentFulfillment = AccessTools.Field(typeof(ModuleVariableMass), "m_CurrentFulfillment");
        internal static FieldInfo m_MassRange = AccessTools.Field(typeof(ModuleVariableMass), "m_MassRange");

        internal static void Postfix(ModuleVariableMass __instance)
        {
            TankBlock block = __instance.block;
            Dictionary<TankBlock.MassCategoryType, double> additionalMass = (Dictionary<TankBlock.MassCategoryType, double>)m_AdditionalMassCategories.GetValue(block);
            if (additionalMass != null && additionalMass.TryGetValue(TankBlock.MassCategoryType.VariableMass, out double addedMass))
            {
                MinMaxFloat massRange = (MinMaxFloat)m_MassRange.GetValue(__instance);
                float fulfillment = Mathf.Clamp01(((float)addedMass - massRange.Min) / (massRange.Max - massRange.Min));
                m_CurrentFulfillment.SetValue(__instance, fulfillment);
            }
            else
            {
                m_CurrentFulfillment.SetValue(__instance, 0.0f);
            }
        }
    }
}
