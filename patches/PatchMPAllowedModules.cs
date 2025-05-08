using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CommunityPatch.patches
{
    [HarmonyPatch(typeof(BlockFilterTable), "CheckBlockAllowed")]
    internal class PatchMPAllowedModules
    {
        internal static readonly FieldInfo CorpBlacklist = AccessTools.Field(typeof(BlockFilterTable), "m_CorpsBlackList");
        internal static readonly FieldInfo ComponentWhitelist = AccessTools.Field(typeof(BlockFilterTable), "m_ComponentsWhiteList");
        internal static readonly FieldInfo m_Lookup = AccessTools.Field(typeof(BlockFilterTable), "m_Lookup");
        private static List<Module> s_ModuleList = new List<Module>();
        private static Dictionary<BlockFilterTable, HashSet<Type>> CachedBlacklist = new Dictionary<BlockFilterTable, HashSet<Type>>();
        private static HashSet<Type> AllVanillaModuleTypes = new HashSet<Type>();
        
        internal static void SetupModuleSet()
        {
            CommunityPatchMod.logger.Debug($"Setting up module set");
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assmbly => assmbly.FullName.Contains("Assembly-CSharp") && !assmbly.FullName.Contains("firstpass")).First();
            Type[] types = AccessTools.GetTypesFromAssembly(assembly);
            foreach (Type type in types)
            {
                if (type != typeof(Module) && typeof(Module).IsAssignableFrom(type))
                {
                    CommunityPatchMod.logger.Trace($"  Found Module {type}");
                    AllVanillaModuleTypes.Add(type);
                }
            }
        }

        internal static void PurgeMetadata()
        {
            CachedBlacklist.Clear();
        }

        internal static bool Prefix(BlockTypes blockType, BlockFilterTable __instance, ref bool __result)
        {
            if (Singleton.Manager<ManMods>.inst.IsModdedBlock(blockType, false))
            {
                __result = false;
                CommunityPatchMod.logger.Debug($"Overriding default block filtering for modded block {blockType}");
                TankBlock blockPrefab = Singleton.Manager<ManSpawn>.inst.GetBlockPrefab(blockType);
                if (blockPrefab == null)
                {
                    CommunityPatchMod.logger.Warn(string.Format("Modded block {0} was not correctly injected into ManSpawn", blockType));
                    return false;
                }

                bool passCorpBlacklist = ((BlockFilterLookup.IFilterBuilder) CorpBlacklist.GetValue(__instance)).CheckBlockAllowed(blockType, blockPrefab, false);
                if (passCorpBlacklist)
                {
                    object componentWhitelist = ComponentWhitelist.GetValue(__instance);
                    if (!CachedBlacklist.TryGetValue(__instance, out HashSet<Type> blacklist))
                    {
                        CommunityPatchMod.logger.Debug("Constructing module blacklist");
                        blacklist = new HashSet<Type>();
                        List<Type> whitelist = (List<Type>)AccessTools.Method(componentWhitelist.GetType(), "ListAllowedTypes").Invoke(componentWhitelist, null);
                        foreach (Type type in AllVanillaModuleTypes)
                        {
                            if (!whitelist.Contains(type))
                            {
                                CommunityPatchMod.logger.Trace($"  Adding module {type} to blacklist");
                                blacklist.Add(type);
                            }
                        }
                        CachedBlacklist.Add(__instance, blacklist);
                    }

                    // check against blacklist instead of whitelist
                    blockPrefab.GetComponentsInChildren<Module>(true, s_ModuleList);
                    List<Type> illegalTypes = new List<Type>();
                    CommunityPatchMod.logger.Debug($"Checking block {blockType} for illegal modules");
                    foreach (Module module in s_ModuleList)
                    {
                        Type moduleType = module.GetType();
                        if (blacklist.Contains(moduleType)) {
                            illegalTypes.Add(moduleType);
                            CommunityPatchMod.logger.Debug($"  Illegal type {moduleType} found!");
                        }
                        else
                        {
                            string moddedStatus = AllVanillaModuleTypes.Contains(moduleType) ? "VANILLA" : "MODDED";
                            CommunityPatchMod.logger.Trace($"  Found allowed {moddedStatus} module {moduleType}");
                        }
                    }
                    __result = illegalTypes.Count == 0;
                    s_ModuleList.Clear();
                }
                else
                {
                    
                    __result = false;
                }
            }
            else
            {
                BlockFilterLookup lookup = (BlockFilterLookup)m_Lookup.GetValue(__instance);
                __result = lookup.CheckBlockAllowed(blockType);
            }
            return false;
        }
    }
}
