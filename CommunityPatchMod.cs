using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using CommunityPatch.patches;
using Steamworks;
using Payload.UI.Commands.Steam;

namespace CommunityPatch
{
    public class CommunityPatchMod : ModBase
    {
        const string HarmonyID = "com.flsoz.ttmods.communitypatch";
        internal static Harmony harmony = new Harmony(HarmonyID);

        internal static bool PatchedModLoading = false;

        public override bool HasEarlyInit()
        {
            return true;
        }

        internal static FieldInfo m_Mods = typeof(ManMods).GetField("m_Mods", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        public void ManagedEarlyInit()
        {
            OrthoRotPatch.SetupOrthoRotMaps();
        }
        public override void EarlyInit()
        {
            this.ManagedEarlyInit();
            if (!PatchedModLoading)
            {
                // We only need to patch this once, since workshop page searching only happens once per game session
                MethodInfo targetMethod = typeof(ManMods).GetMethod("OnSteamModsFetchComplete", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(SteamDownloadData) }, null);
                MethodInfo replacementMethod = typeof(SubscribedModsPatch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(SteamDownloadData) }, null);
                harmony.Patch(targetMethod, prefix: new HarmonyMethod(replacementMethod));

                Dictionary<string, ModContainer> mods = (Dictionary<string, ModContainer>)m_Mods.GetValue(Singleton.Manager<ManMods>.inst);
                if (mods.Count >= Constants.kNumUGCResultsPerPage)
                {
                    SubscribedModsPatch.CheckForMoreSteamMods(2u);
                }
                PatchedModLoading = true;
            }
        }

        public override void DeInit()
        {
            harmony.UnpatchAll(HarmonyID);
        }

        public override void Init()
        {
            harmony.PatchAll(); // Patches in this mod are safe - keep them permanently applied
        }
    }
}
