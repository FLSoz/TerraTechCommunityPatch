using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
namespace CommunityPatch.patches
{
    [HarmonyPatch(typeof(ManMods), "FindCorp")]
    [HarmonyPatch(new Type[] { typeof(string) })]
    class FindCorpPatch
    {
        internal static FieldInfo m_Mods = typeof(ManMods).GetField("m_Mods", CommunityPatchMod.InstanceFlags);

        public static bool Prefix(ref ManMods __instance, ref string corpID, ref ModdedCorpDefinition __result)
        {
            Dictionary<string, ModContainer> mods = (Dictionary<string, ModContainer>)m_Mods.GetValue(__instance);
            foreach (ModContainer modContainer in mods.Values)
            {
                if (modContainer.IsLoaded && modContainer.Contents != null)
                {
                    foreach (ModdedCorpDefinition moddedCorpDefinition in modContainer.Contents.m_Corps)
                    {
                        if (moddedCorpDefinition.m_ShortName == corpID)
                        {
                            __result = moddedCorpDefinition;
                            return false;
                        }
                    }
                }
            }
            __result = null;
            return false;
        }
    }
}
