using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
namespace CommunityPatch.patches
{
    internal static class FindCorpPatch
    {
        internal static FieldInfo m_Mods = typeof(ManMods).GetField("m_Mods", CommunityPatchMod.InstanceFlags);

        internal static bool Prefix(ManMods __instance, string corpID, ref ModdedCorpDefinition __result)
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
