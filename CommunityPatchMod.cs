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
        internal static BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        internal static BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        const int CurrentStable = 9014917;

        internal static bool PatchedModLoading = false;
        internal static bool IsUnstableBuild = false;

        public override bool HasEarlyInit()
        {
            return true;
        }

        internal static FieldInfo m_Mods = typeof(ManMods).GetField("m_Mods", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        internal static FieldInfo m_CurrentSession = typeof(ManMods).GetField("m_CurrentSession", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        public void ManagedEarlyInit()
        {
            int currentBuild = SteamApps.GetAppBuildId();
            Console.WriteLine("[CommunityPatch] ManagedEarlyInit");
            Console.WriteLine($"[CommunityPatch] Current Build: {currentBuild}");

            IsUnstableBuild = currentBuild != CurrentStable;
            OrthoRotPatch.SetupOrthoRotMaps();
        }

        public override void EarlyInit()
        {
            this.ManagedEarlyInit();
            if (!PatchedModLoading)
            {
                // We only need to patch this once, since workshop page searching only happens once per game session
                MethodInfo ShouldReadFromRawJSONMethod = typeof(ManMods).GetMethod("OnSteamModsFetchComplete", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(SteamDownloadData) }, null);
                MethodInfo replacementMethod = typeof(SubscribedModsPatch).GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(SteamDownloadData) }, null);
                harmony.Patch(ShouldReadFromRawJSONMethod, prefix: new HarmonyMethod(replacementMethod));

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
            harmony.PatchAll();
            if (IsUnstableBuild)
            {
                PatchForUnstable();
            }
            else {
                PatchForStable();
            }
        }

        internal void PatchForStable()
        {
            Console.WriteLine("[CommunityPatch] Patching for stable");
            // bullet spread fix
            MethodInfo fire = AccessTools.Method(typeof(Projectile), "Fire");
            harmony.Patch(fire, postfix: new HarmonyMethod(AccessTools.Method(typeof(SpreadFix), "Postfix")));
            // whisper props
            MethodInfo boosterOnPool = AccessTools.Method(typeof(ModuleBooster), "OnPool");
            harmony.Patch(boosterOnPool, postfix: new HarmonyMethod(AccessTools.Method(typeof(WhisperProps), "Postfix")));
            // Death Explosion fix
            MethodInfo injectModdedBlocks = AccessTools.Method(typeof(ManMods), "InjectModdedBlocks");
            harmony.Patch(injectModdedBlocks, transpiler: new HarmonyMethod(AccessTools.Method(typeof(DeathExplosionOverridePatch), "Transpiler")));
            // ManSceneryAnimation fix
            MethodInfo manSceneryAnimationUpdate = AccessTools.Method(typeof(ManSceneryAnimation), "Update");
            harmony.Patch(
                manSceneryAnimationUpdate,
                prefix: new HarmonyMethod(AccessTools.Method(typeof(ManSceneryAnimationPatch), "Prefix")),
                finalizer: new HarmonyMethod(AccessTools.Method(typeof(ManSceneryAnimationPatch), "Finalizer"))
            );
            // ModuleVariableMass patch
            MethodInfo setMassCubeScale = AccessTools.Method(typeof(ModuleVariableMass), "SetMassCubeScale");
            harmony.Patch(setMassCubeScale, postfix: new HarmonyMethod(AccessTools.Method(typeof(ModuleVariableMassPatch), "Postfix")));
            // Find Corp fix
            MethodInfo findCorp = AccessTools.Method(typeof(ManMods), "FindCorp", new Type[] { typeof(string) });
            harmony.Patch(findCorp, prefix: new HarmonyMethod(AccessTools.Method(typeof(FindCorpPatch), "Prefix")));
        }

        internal void PatchForUnstable()
        {
            // For any unstable-only patches
        }
    }
}
