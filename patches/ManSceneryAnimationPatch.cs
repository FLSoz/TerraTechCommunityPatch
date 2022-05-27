using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CommunityPatch.patches
{
    [HarmonyPatch(typeof(ManSceneryAnimation), "Update")]
    internal class ManSceneryAnimationPatch
    {
		internal static FieldInfo m_PlayingAnimations = typeof(ManSceneryAnimation).GetField("m_PlayingAnimations", CommunityPatchMod.InstanceFlags);
		internal static FieldInfo m_Animations = typeof(ManSceneryAnimation).GetField("m_Animations", CommunityPatchMod.InstanceFlags);

		internal static FieldInfo targetGO = null;
		internal static FieldInfo animType = null;
		internal static FieldInfo time = null;
		internal static FieldInfo animFinishedEvent = null;

        internal static bool Prefix(ref ManSceneryAnimation __instance)
		{
			AnimationClip[] animations = (AnimationClip[]) m_Animations.GetValue(__instance);
			IList playingAnimations = (IList) m_PlayingAnimations.GetValue(__instance);
			float deltaTime = Time.deltaTime;

			if (playingAnimations.Count > 0)
			{
				object firstAnimation = playingAnimations[0];

				// Get fields of ManSceneryAnimation.AnimState by reflection, because Payload wants us to suffer
				// We assume all the fields are fetched successfully, or none of them are, and we throw an exception
				if (targetGO == null)
				{
					targetGO = firstAnimation.GetType().GetField("targetGO", CommunityPatchMod.InstanceFlags);
					animType = firstAnimation.GetType().GetField("animType", CommunityPatchMod.InstanceFlags);
					time = firstAnimation.GetType().GetField("time", CommunityPatchMod.InstanceFlags);
					animFinishedEvent = firstAnimation.GetType().GetField("animFinishedEvent", CommunityPatchMod.InstanceFlags);
				}

				for (int i = playingAnimations.Count - 1; i >= 0; i--)
				{
					object animState = playingAnimations[i];
					float num = ((float) time.GetValue(animState)) + deltaTime;
					AnimationClip animationClip = animations[(int)(ManSceneryAnimation.AnimTypes)animType.GetValue(animState)];
					float length = animationClip.length;
					bool flag = false;
					if (num >= length)
					{
						num = length;
						flag = true;
					}
					GameObject go = (GameObject) targetGO.GetValue(animState);
					if (go != null)
					{
						animationClip.SampleAnimation(go, num);
					}
					if (flag)
					{
						Action finishedEvent = (Action)animFinishedEvent.GetValue(animState);
						if (finishedEvent != null)
						{
							finishedEvent();
						}
						playingAnimations.RemoveAt(i);
					}
					else
					{
						time.SetValue(animState, num);
					}
				}
			}
			return false;
		}
    }
}
