using System.Reflection;
using UnityEngine;


namespace CommunityPatch.patches
{
    // Taken from TT Spread fix: https://github.com/Fireflywater/TerraTech_SpreadFix/blob/master/FFW_TT_SpreadFix/Spread_Patch.cs
    internal static class SpreadFix
    {
        static Vector3 Random2(Vector3 v, float variance)
        {
            return new Vector3(
                v.x + Vector3.Magnitude(v) * 0.5f * UnityEngine.Random.Range(-variance, variance),
                v.y + Vector3.Magnitude(v) * 0.5f * UnityEngine.Random.Range(-variance, variance),
                v.z + Vector3.Magnitude(v) * 0.5f * UnityEngine.Random.Range(-variance, variance)
            );
        }

        internal static void Postfix(Projectile __instance, Vector3 fireDirection, FireData fireData, Tank shooter = null, bool replayRounds = false)
        {
            FieldInfo field_LastFireDirection = typeof(Projectile)
                .GetField("m_LastFireDirection", BindingFlags.NonPublic | BindingFlags.Instance);

            Vector3 vector = fireDirection * fireData.m_MuzzleVelocity;
            if (!replayRounds)
            {
                field_LastFireDirection.SetValue(__instance, Random2(vector, fireData.m_BulletSprayVariance));
                vector = (Vector3)field_LastFireDirection.GetValue(__instance);
            }
            else
            {
                vector = (Vector3)field_LastFireDirection.GetValue(__instance);
            }
            if (shooter != null)
            {
                vector += shooter.rbody.velocity;
            }
            __instance.rbody.velocity = vector;
        }
    }
}
