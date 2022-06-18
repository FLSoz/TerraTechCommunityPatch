using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using System.Reflection.Emit;


namespace CommunityPatch.patches
{
    [HarmonyPatch(typeof(ModuleDamage), "Explode")]
    class DeathExplosiondamagePatch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilgen)
        {
            Label returnLabel = ilgen.DefineLabel();


            List<CodeInstruction> originalInstructions = new List<CodeInstruction>(instructions);
            List<CodeInstruction> patchedInstructions = new List<CodeInstruction>();

            for (int i = originalInstructions.Count - 1; i >= 0; i--)
            {
                CodeInstruction instruction = originalInstructions[i];
                if (instruction.opcode == OpCodes.Ret)
                {
                    returnLabel = instruction.labels[0];
                    break;
                }
            }

            bool inLastIf = false;
            bool patchedLastIf = false;
            foreach (CodeInstruction instruction in originalInstructions)
            {
                if (!inLastIf && instruction.opcode == OpCodes.Brfalse_S)
                {
                    if ((Label)instruction.operand == returnLabel)
                    {
                        inLastIf = true;
                        Console.WriteLine("Properly found skip label");
                    }
                    patchedInstructions.Add(instruction);
                }
                else if (inLastIf)
                {
                    if (instruction.opcode == OpCodes.Callvirt && ((MethodInfo)instruction.operand) == typeof(Explosion).GetMethod("SetDamageSource"))
                    {
                        patchedInstructions.Add(new CodeInstruction(OpCodes.Ldloc_2));
                        patchedInstructions.Add(new CodeInstruction(OpCodes.Ldnull));
                        patchedLastIf = true;
                    }

                    if (patchedLastIf)
                    {
                        patchedInstructions.Add(instruction);
                    }
                }
                else
                {
                    patchedInstructions.Add(instruction);
                }
            }

            foreach (CodeInstruction instruction in patchedInstructions)
            {
                yield return instruction;
            }
        }
    }
}
