using HarmonyLib;
using System.Reflection;

namespace CommunityPatch.patches
{
    [HarmonyPatch(typeof(ModuleRadar), "GetRange")]
    internal class ModuleRadarPatch
    {
        internal static readonly MethodInfo GetScanIndex = AccessTools.Method(typeof(ModuleRadar), "GetScanIndex");
        internal static readonly FieldInfo m_Ranges = AccessTools.Field(typeof(ModuleRadar), "m_Ranges");

        internal static bool Prefix(ModuleRadar __instance, ModuleRadar.RadarScanType type, ref float __result)
        {
            int scanIndex = (int) GetScanIndex.Invoke(__instance, new object[] { type });
            float[] ranges = (float[])m_Ranges.GetValue(__instance);
            if (ranges != null && ranges.Length > scanIndex)
            {
                return true;
            }
            __result = 0.0f;
            return false;
        }
    }
}
