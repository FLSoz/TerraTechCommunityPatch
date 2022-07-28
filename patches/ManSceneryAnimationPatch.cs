using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CommunityPatch.patches
{
    internal static class ManSceneryAnimationPatch
    {
		internal static FieldInfo m_PlayingAnimations = AccessTools.Field(typeof(ManSceneryAnimation), "m_PlayingAnimations");
		internal static FieldInfo m_Animations = AccessTools.Field(typeof(ManSceneryAnimation), "m_Animations");

		internal static FieldInfo targetGO = null;
		internal static FieldInfo animType = null;
		internal static FieldInfo time = null;
		internal static FieldInfo animFinishedEvent = null;

        internal static bool Prefix(ManSceneryAnimation __instance)
		{
			AnimationClip[] animations = (AnimationClip[]) m_Animations.GetValue(__instance);
			IList playingAnimations = (IList) m_PlayingAnimations.GetValue(__instance);
			float deltaTime = Time.deltaTime;

			if (playingAnimations != null && playingAnimations.Count > 0)
			{
				if (animations != null)
				{
					object firstAnimation = playingAnimations[0];

					// Get fields of ManSceneryAnimation.AnimState by reflection, because Payload wants us to suffer
					// We assume all the fields are fetched successfully, or none of them are, and we throw an exception
					if (targetGO == null)
					{
						CommunityPatchMod.logger.Info("FETCHING PATCH REFLECTION");
						targetGO = AccessTools.Field(firstAnimation.GetType(), "targetGO");
						animType = AccessTools.Field(firstAnimation.GetType(), "animType");
						time = AccessTools.Field(firstAnimation.GetType(), "time");
						animFinishedEvent = AccessTools.Field(firstAnimation.GetType(), "animFinishedEvent");
					}

					for (int i = playingAnimations.Count - 1; i >= 0; i--)
					{
						object animState = playingAnimations[i];
						float num = ((float)time.GetValue(animState)) + deltaTime;
						int animIndex = (int)(ManSceneryAnimation.AnimTypes)animType.GetValue(animState);
						if (animIndex < animations.Length)
						{
							AnimationClip animationClip = animations[animIndex];
							if (animationClip != null)
							{
								float length = animationClip.length;
								bool flag = false;
								if (num >= length)
								{
									num = length;
									flag = true;
								}
								GameObject go = (GameObject)targetGO.GetValue(animState);
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
							else
							{
								CommunityPatchMod.logger.Warn("NULL ANIMATIONCLIP FOUND");
								playingAnimations.RemoveAt(i);
							}
						}
						else
						{
							CommunityPatchMod.logger.Warn($"Animation of INVALID type {animIndex}");
							playingAnimations.RemoveAt(i);
						}
					}
				}
				else
                {
					CommunityPatchMod.logger.Warn("NULL ANIMATION CLIPS");
                }
			}
			return false;
		}

		internal static Exception Finalizer(Exception __exception)
		{
			if (__exception != null)
			{
				CommunityPatchMod.logger.Error("ERROR in ManSceneryAnimation");
				CommunityPatchMod.logger.Error(__exception);
			}
			return null;
		}
	}
}
