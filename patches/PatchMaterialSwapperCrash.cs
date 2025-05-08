using HarmonyLib;
using System;
using UnityEngine;

namespace CommunityPatch.patches
{
    [HarmonyPatch(typeof(TankBlock), "IsCustomMaterialOverride")]
    internal class PatchNoMaterialOverride
    {
        internal static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
    [HarmonyPatch(typeof(ManTechMaterialSwap), "GetMaterial")]
    internal class PatchFailMaterial
    {
        internal static Exception Finalizer(ref Material __result)
        {
            __result = null;
            return null;
        }
    }
}
