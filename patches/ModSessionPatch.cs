using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace CommunityPatch.patches
{
    [HarmonyPatch(typeof(ManMods), "PreLobbyCreated")]
    public static class PatchLobbyCreation
    {
        internal static FieldInfo m_CurrentLobbySession = typeof(ManMods).GetField("m_CurrentLobbySession", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        internal static void DeepCopy<T1, T2>(Dictionary<T1, T2> source, Dictionary<T1, T2> target)
        {
            if (source != null && target != null)
            {
                foreach (KeyValuePair<T1, T2> pair in source)
                {
                    target.Add(pair.Key, pair.Value);
                }
            }
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            ModSessionInfo session = (ModSessionInfo)m_CurrentLobbySession.GetValue(Singleton.Manager<ManMods>.inst);
            ModSessionInfo currentSession = (ModSessionInfo) CommunityPatchMod.m_CurrentSession.GetValue(Singleton.Manager<ManMods>.inst);
            if (session != null && currentSession != null)
            {
                CommunityPatchMod.logger.Info("Patched MP lobby loading");

                Dictionary<int, string> currentCorpIDs = currentSession.CorpIDs;
                Dictionary<int, string> targetcorpIDs = session.CorpIDs;
                DeepCopy(currentCorpIDs, targetcorpIDs);

                Dictionary<int, string> currentSkinIDs = currentSession.SkinIDs;
                if (currentSkinIDs != null && currentSkinIDs.Count > 0)
                {
                    Dictionary<int, string> targetSkinIDs = session.SkinIDs;
                    if (targetSkinIDs == null)
                    {
                        targetSkinIDs = new Dictionary<int, string>();
                        session.SkinIDs = targetSkinIDs;
                    }
                    DeepCopy(currentSkinIDs, targetSkinIDs);
                }

                Dictionary<int, string> currentBlockIDs = currentSession.BlockIDs;
                Dictionary<int, string> targetBlockIDs = session.BlockIDs;
                DeepCopy(currentBlockIDs, targetBlockIDs);
            }
        }
    }
}
