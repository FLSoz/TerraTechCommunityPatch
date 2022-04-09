using HarmonyLib;
using System.Reflection;

namespace CommunityPatch.patches
{
    internal class WhisperProps
    {
        // Sourced from Legionite with permission: https://github.com/LegioniteTerraTech/WhisperProps
        [HarmonyPatch(typeof(ModuleBooster))]
        [HarmonyPatch("OnPool")]
        private class ShhhhhhushMyChild
        {
            private static void Postfix(ModuleBooster __instance)
            {
                FieldInfo lockGet = typeof(ModuleBooster).GetField("m_BoosterAudioType", BindingFlags.NonPublic | BindingFlags.Instance);
                int m_BoosterAudioType = (int)lockGet.GetValue(__instance);
                if (m_BoosterAudioType == (int)TechAudio.BoosterEngineType.Propeller)
                {
                    lockGet.SetValue(__instance, (int)TechAudio.BoosterEngineType.MediumRotor);
                }
            }
        }
    }
}
