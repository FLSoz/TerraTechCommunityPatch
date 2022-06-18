using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using System.Reflection.Emit;

namespace CommunityPatch.patches
{
    [HarmonyPatch(typeof(ManMods), "InjectModdedBlocks")]
    public static class DeathExplosionOverridePatch
    {
        public static void AddDeathExplosionIfAbsent(ModuleDamage moduleDamage)
        {
            if (moduleDamage.deathExplosion == null)
            {
                Console.WriteLine($"DEATH EXPLOSION OVERRIDEN FOR {moduleDamage.name}");
                moduleDamage.deathExplosion = Singleton.Manager<ManMods>.inst.m_DefaultBlockExplosion;
            }
            else
            {
                Console.WriteLine($"DEATH EXPLOSION SAVED FOR {moduleDamage.name}");
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilgen)
        {
            List<CodeInstruction> originalInstructions = new List<CodeInstruction>(instructions);
            List<CodeInstruction> patchedInstructions = new List<CodeInstruction>();

            bool patchingDeathExplosion = false;
            int opCodesToRemove = 3;

            for (int i = 0; i < originalInstructions.Count; i++)
            {
                CodeInstruction instruction = originalInstructions[i];
                if (!patchingDeathExplosion)
                {
                    if (
                        instruction.opcode == OpCodes.Ldarg_0 && i < originalInstructions.Count - 2 &&
                        originalInstructions[i+1].opcode == OpCodes.Ldfld && ((FieldInfo) originalInstructions[i+1].operand) == typeof(ManMods).GetField("m_DefaultBlockExplosion") &&
                        originalInstructions[i+2].opcode == OpCodes.Stfld && ((FieldInfo) originalInstructions[i+2].operand) == typeof(ModuleDamage).GetField("deathExplosion")
                    )
                    {
                        // Insert call to custom method
                        patchedInstructions.Add(CodeInstruction.Call(typeof(DeathExplosionOverridePatch), "AddDeathExplosionIfAbsent"));
                        // yield return new CodeInstruction(OpCodes.Call, InjectLegacyBlocks);
                        patchingDeathExplosion = true;
                    }
                }
                if (!patchingDeathExplosion || opCodesToRemove == 0)
                {
                    patchedInstructions.Add(instruction);
                }
                else if (patchingDeathExplosion)
                {
                    opCodesToRemove--;
                }
            }

            foreach (CodeInstruction instruction in patchedInstructions)
            {
                yield return instruction;
            }
        }
    }
}
