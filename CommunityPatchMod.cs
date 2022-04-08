using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using CommunityPatch.patches;
using Steamworks;

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
                harmony.Patch(typeof(ManMods).GetMethod("OnSteamModsFetchComplete"), prefix: new HarmonyMethod(typeof(SubscribedModsPatch).GetMethod("Prefix")));

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
