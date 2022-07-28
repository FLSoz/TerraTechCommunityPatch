using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace CommunityPatch.patches
{
    // Fix OrthoRotations not being set properly for equivalent rotations.
    internal static class OrthoRotPatch
    {
        internal static OrthoRotation forbiddenRot = new OrthoRotation((OrthoRotation.r) 24);
        internal static Dictionary<Quaternion, OrthoRotation.r> quaternionMap = new Dictionary<Quaternion, OrthoRotation.r>();
        internal static Dictionary<int, OrthoRotation.r> packedMap = new Dictionary<int, OrthoRotation.r>();

        internal static Dictionary<int, int> packedToRootMap = new Dictionary<int, int>() {
            {32, 10},
            {9, 35},
            {11, 33},
            {18, 24},
            {34, 8},
            {20, 19},
            {36, 14},
            {52, 49},
            {40, 2},
            {28, 17},
            {44, 6},
            {60, 51},
            {21, 16},
            {42, 0},
            {22, 17},
            {23, 24},
            {25, 19},
            {29, 24},
            {37, 15},
            {41, 3},
            {43, 1},
            {38, 12},
            {46, 4},
            {26, 16},
            {58, 48},
            {61, 48},
            {62, 49},
            {55, 48},
            {59, 49},
            {31, 16},
            {47, 5},
            {27, 17},
            {30, 19},
            {39, 13},
            {45, 7},
            {54, 51},
            {57, 51},
            {53, 56},
            {63, 56},
            {50, 56}
        };
        internal static Dictionary<int, Quaternion> packedToQuaternionMap = new Dictionary<int, Quaternion>();

        internal const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        internal static MethodInfo FindPacked = typeof(OrthoRotation).GetMethod("FindPacked", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        internal static bool SetupRotMaps = false;

        internal static void SetupOrthoRotMaps()
        {
            if (!SetupRotMaps)
            {
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        for (int z = 0; z < 4; z++)
                        {
                            Quaternion quaternion = Quaternion.Euler(x * 90.0f, y * 90.0f, z * 90.0f);
                            int packed = ((x & 3) << 4) | ((y & 3) << 2) | (z & 3); // bit masking by 3 not strictly necessary
                            int rootPacked;
                            if (!packedToRootMap.TryGetValue(packed, out rootPacked))
                            {
                                rootPacked = packed;
                            }

                            OrthoRotation.r rot = (OrthoRotation.r)FindPacked.Invoke(null, new object[] { rootPacked });

                            if (!quaternionMap.ContainsKey(quaternion))
                            {
                                quaternionMap.Add(quaternion, rot);
                            }
                            packedMap.Add(packed, rot);
                            packedToQuaternionMap.Add(packed, quaternion);
                            Console.WriteLine($"[CommunityPatch] Processing Packed id {packed} (isomorphic to {rootPacked}), <{x} {y} {z}>, {quaternion} as rot {(int)rot}");
                        }
                    }
                }

                foreach (KeyValuePair<int, Quaternion> pair in packedToQuaternionMap)
                {
                    int packed = pair.Key;
                    if (packedToRootMap.TryGetValue(packed, out int rootPacked))
                    {
                        Quaternion quaternion = pair.Value;
                        Quaternion rootQuaternion = packedToQuaternionMap[rootPacked];
                        d.Assert(rootQuaternion.eulerAngles == quaternion.eulerAngles, $"[CommunityPatch] packed value {packed} has quaternion of {quaternion} ({quaternion.eulerAngles}), but root packed value of {rootPacked} has quaternion of {rootQuaternion} ({rootQuaternion.eulerAngles})");
                    }
                }

                SetupRotMaps = true;
            }
        }

        /* Don't add support for forbidden rot until discover why it is forbidden
        [HarmonyPatch(typeof(OrthoRotation), "ToEulers")]
        internal static class PatchToEulers
        {
            [HarmonyPrefix]
            public static bool Prefix(OrthoRotation __instance, ref Vector3 __result)
            {
                if (__instance.rot == (OrthoRotation.r) 24)
                {
                    __result = new Vector3(270.0f, 0.0f, 180.0f);
                    return false;
                }
                return true;
            }
        }
        
        internal static class PatchDecrement
        {

        }

        internal static class PatchIncrement
        {

        }
        */

        [HarmonyPatch(typeof(OrthoRotation), MethodType.Constructor, new Type[] { typeof(Vector3) })]
        internal static class PatchVector3Constructor
        {
            [HarmonyPrefix]
            public static bool Prefix(OrthoRotation __instance, Vector3 eulers)
            {
                IntVector3 baseCheck = eulers / 90.0f;
                int packed = ((3 & baseCheck.x) << 4) | ((3 & baseCheck.y) << 2) | (3 & baseCheck.z);
                packedMap.TryGetValue(packed, out OrthoRotation.r rotEnum);
                __instance = new OrthoRotation(rotEnum);
                // Console.WriteLine($"Vector3 Constructor: Got rotation for eulers {eulers} (packed {packed}): {(int)rotEnum} => set as {__instance.rot} ({(int)__instance.rot})");
                return false;
            }
        }

        [HarmonyPatch(typeof(OrthoRotation), MethodType.Constructor, new Type[] { typeof(Quaternion) })]
        internal static class PatchQuaternionConstructor
        {
            [HarmonyPrefix]
            public static bool Prefix(OrthoRotation __instance, Quaternion rotation)
            {
                quaternionMap.TryGetValue(rotation, out OrthoRotation.r rotEnum);
                __instance = new OrthoRotation(rotEnum);
                // Console.WriteLine($"Quaternion Constructor: Got rotation for quaternion {rotation}: {(int)rotEnum} => set as {__instance.rot} ({(int)__instance.rot})");
                return false;
            }
        }

        [HarmonyPatch(typeof(OrthoRotation), MethodType.Constructor, new Type[] { typeof(int) })]
        internal static class PatchIntConstructor
        {
            [HarmonyPrefix]
            public static bool Prefix(OrthoRotation __instance, int packed)
            {
                packedMap.TryGetValue(packed, out OrthoRotation.r rotEnum);
                __instance = new OrthoRotation(rotEnum);
                // Console.WriteLine($"Packed Constructor: Got rotation for packed {packed}: {(int)rotEnum} => set as {__instance.rot} ({(int) __instance.rot})");
                return false;
            }
        }

        /* Don't add support for forbidden rot until discover why it is forbidden
        [HarmonyPatch(typeof(OrthoRotation), "op_Multiply", new Type[] { typeof(OrthoRotation), typeof(Vector3) })]
        internal static class PatchMultiplyVector3
        {
            [HarmonyPrefix]
            public static bool Prefix(OrthoRotation or, Vector3 v, ref Vector3 __result)
            {
                if (or.rot == (OrthoRotation.r)24)
                {
                    __result = new Vector3(-v.x, v.z, v.y);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(OrthoRotation), "op_Multiply", new Type[] { typeof(OrthoRotation), typeof(IntVector3) })]
        internal static class PatchMultiplyIntVector3
        {
            [HarmonyPrefix]
            public static bool Prefix(OrthoRotation or, IntVector3 v, ref IntVector3 __result)
            {
                if (or.rot == (OrthoRotation.r)24)
                {
                    __result = new IntVector3(-v.x, v.z, v.y);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(OrthoRotation), "op_Multiply", new Type[] { typeof(OrthoRotation), typeof(Bounds) })]
        internal static class PatchMultiplyBounds
        {
            [HarmonyPrefix]
            public static bool Prefix(OrthoRotation or, Bounds b, ref Bounds __result)
            {
                if (or.rot == (OrthoRotation.r)24)
                {
                    __result = default(Bounds);
                    __result.center = new Vector3(-b.center.x, b.center.z, b.center.y);
                    __result.extents = new Vector3(b.extents.x, b.extents.z, b.extents.y);
                    return false;
                }
                return true;
            }
        }
        */
    }
}
