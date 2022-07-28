using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace CommunityPatch.patches
{
    [HarmonyPatch(typeof(ManMods), "DoReparseAll")]
    class ReparseJSONPatch
    {
		[HarmonyPrefix]
        public static void Prefix(ManMods __instance)
        {
			Console.WriteLine("[Mods] Purging outdated modded blocks");
			Singleton.Manager<RecipeManager>.inst.RemoveCustomBlockRecipes();
			BlockUnlockTable blockUnlockTable = Singleton.Manager<ManLicenses>.inst.GetBlockUnlockTable();
			blockUnlockTable.RemoveModdedBlocks();

			// Clear corp block data
			foreach (KeyValuePair<int, BlockUnlockTable.CorpBlockData> keyValuePair in blockUnlockTable)
			{
				BlockUnlockTable.CorpBlockData value = keyValuePair.Value;
				value.m_GradeList = new BlockUnlockTable.GradeData[]
				{
					new BlockUnlockTable.GradeData()
				};
			}

			Singleton.Manager<ManLicenses>.inst.GetRewardPoolTable().ClearModdedBlockRewards();
		}
    }

	[HarmonyPatch(typeof(ManMods), "RequestReparseAllJsons")]
	class DeleteInsteadOfRecycle
    {
		internal static readonly FieldInfo m_ReparseAllPending = typeof(ManMods).GetField("m_ReparseAllPending", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		internal static readonly FieldInfo ShouldReadFromRawJSON = typeof(ManMods).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(field =>
						field.CustomAttributes.Any(attr => attr.AttributeType == typeof(CompilerGeneratedAttribute)) &&
						(field.DeclaringType == typeof(ManMods).GetProperty("ShouldReadFromRawJSON").DeclaringType) &&
						field.FieldType.IsAssignableFrom(typeof(ManMods).GetProperty("ShouldReadFromRawJSON").PropertyType) &&
						field.Name.StartsWith("<" + typeof(ManMods).GetProperty("ShouldReadFromRawJSON").Name + ">")
					);
		internal static readonly MethodInfo GetDelegates = typeof(ComponentPool).GetMethod("GetDelegates", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		[HarmonyPrefix]
		public static bool Prefix(ManMods __instance)
		{
			foreach (TankBlock tankBlock in UnityEngine.Object.FindObjectsOfType<TankBlock>())
			{
				if (__instance.IsModdedBlock(tankBlock.BlockType, false))
				{
					if (tankBlock.tank != null)
					{
						tankBlock.tank.blockman.Detach(tankBlock, false, true, true, null);
					}

					Action<Component>[] recycleDelegates = (Action<Component>[]) GetDelegates.Invoke(Singleton.Manager<ComponentPool>.inst, new object[] { typeof(Transform), "OnRecycle" });
					Action<Component>[] depoolDelegates = (Action<Component>[])GetDelegates.Invoke(Singleton.Manager<ComponentPool>.inst, new object[] { typeof(Transform), "OnDepool" });
					foreach (Action<Component> recycleDelegate in recycleDelegates) {
						recycleDelegate(tankBlock.transform);
					}
					foreach (Action<Component> depoolDelegate in depoolDelegates) {
						depoolDelegate(tankBlock.transform);
					}
					UnityEngine.Object.Destroy(tankBlock.gameObject);
				}
			}
			ShouldReadFromRawJSON.SetValue(__instance, true);
			m_ReparseAllPending.SetValue(__instance, true);
			return false;
        }
    }
}
