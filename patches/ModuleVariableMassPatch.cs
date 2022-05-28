using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using UnityEngine;

namespace CommunityPatch.patches
{
    [HarmonyPatch(typeof(ModuleVariableMass), "SetMassCubeScale")]
    internal static class ModuleVariableMassPatch
    {
        internal static BindingFlags InstanceFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        internal static FieldInfo m_AdditionalMassCategories = typeof(TankBlock).GetField("m_AdditionalMassCategories", InstanceFlags);
        internal static FieldInfo m_DefaultMasss = typeof(TankBlock).GetField("m_DefaultMass", InstanceFlags);
        internal static FieldInfo m_CurrentFulfillment = typeof(ModuleVariableMass).GetField("m_CurrentFulfillment", InstanceFlags);
        internal static FieldInfo m_MassRange = typeof(ModuleVariableMass).GetField("m_MassRange", InstanceFlags);

        [HarmonyPostfix]
        internal static void Postfix(ref ModuleVariableMass __instance)
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
