using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
	public class HumanoidAvatarMaker
	{

		public static Dictionary<string, string> CloneXMaleNames = new Dictionary<string, string>()
		{
			{ "abdomenUpper", "Chest" },
			{ "head", "Head" },
			{ "Genesis8_1Male", "Hips" },
			{ "Genesis8_1Female", "Hips" },
			//{"hip","Hips"},
			{ "lIndex3", "Left Index Distal" },
			{ "lIndex2", "Left Index Intermediate" },
			{ "lIndex1", "Left Index Proximal" },
			{ "lPinky3", "Left Little Distal" },
			{ "lPinky2", "Left Little Intermediate" },
			{ "lPinky1", "Left Little Proximal" },
			{ "lMid3", "Left Middle Distal" },
			{ "lMid2", "Left Middle Intermediate" },
			{ "lMid1", "Left Middle Proximal" },
			{ "lRing3", "Left Ring Distal" },
			{ "lRing2", "Left Ring Intermediate" },
			{ "lRing1", "Left Ring Proximal" },
			{ "lThumb3", "Left Thumb Distal" },
			{ "lThumb2", "Left Thumb Intermediate" },
			{ "lThumb1", "Left Thumb Proximal" },
			{ "lFoot", "LeftFoot" },
			{ "lHand", "LeftHand" },
			{ "lForearmBend", "LeftLowerArm" },
			{ "lShin", "LeftLowerLeg" },
			{ "lCollar", "LeftShoulder" },
			{ "lToe", "LeftToes" },
			{ "lShldrBend", "LeftUpperArm" },
			{ "lThighBend", "LeftUpperLeg" },
			{ "neckLower", "Neck" },
			{ "rIndex3", "Right Index Distal" },
			{ "rIndex2", "Right Index Intermediate" },
			{ "rIndex1", "Right Index Proximal" },
			{ "rPinky3", "Right Little Distal" },
			{ "rPinky2", "Right Little Intermediate" },
			{ "rPinky1", "Right Little Proximal" },
			{ "rMid3", "Right Middle Distal" },
			{ "rMid2", "Right Middle Intermediate" },
			{ "rMid1", "Right Middle Proximal" },
			{ "rRing3", "Right Ring Distal" },
			{ "rRing2", "Right Ring Intermediate" },
			{ "rRing1", "Right Ring Proximal" },
			{ "rThumb3", "Right Thumb Distal" },
			{ "rThumb2", "Right Thumb Intermediate" },
			{ "rThumb1", "Right Thumb Proximal" },
			{ "rFoot", "RightFoot" },
			{ "rHand", "RightHand" },
			{ "rForearmBend", "RightLowerArm" },
			{ "rShin", "RightLowerLeg" },
			{ "rCollar", "RightShoulder" },
			{ "rToe", "RightToes" },
			{ "rShldrBend", "RightUpperArm" },
			{ "rThighBend", "RightUpperLeg" },
			{ "abdomenLower", "Spine" },
			{ "chestLower", "UpperChest" }
		};

		public static Dictionary<string, string> XOCIETYBones = new Dictionary<string, string>()
		{
			{ "Bip001 Spine1", "Chest" },
			{ "Bip001 Head", "Head" },
			{ "Bip001 Pelvis", "Hips" },
			{ "Bip001 L Finger12", "Left Index Distal" },
			{ "Bip001 L Finger11", "Left Index Intermediate" },
			{ "Bip001 L Finger1", "Left Index Proximal" },
			{ "Bip001 L Finger42", "Left Little Distal" },
			{ "Bip001 L Finger41", "Left Little Intermediate" },
			{ "Bip001 L Finger4", "Left Little Proximal" },
			{ "Bip001 L Finger22", "Left Middle Distal" },
			{ "Bip001 L Finger21", "Left Middle Intermediate" },
			{ "Bip001 L Finger2", "Left Middle Proximal" },
			{ "Bip001 L Finger32", "Left Ring Distal" },
			{ "Bip001 L Finger31", "Left Ring Intermediate" },
			{ "Bip001 L Finger3", "Left Ring Proximal" },
			{ "Bip001 L Finger02", "Left Thumb Distal" },
			{ "Bip001 L Finger01", "Left Thumb Intermediate" },
			{ "Bip001 L Finger0", "Left Thumb Proximal" },
			{ "Bip001 L Foot", "LeftFoot" },
			{ "Bip001 L Hand", "LeftHand" },
			{ "Bip001 L Forearm", "LeftLowerArm" },
			{ "Bip001 L Calf", "LeftLowerLeg" },
			{ "Bip001 L Clavicle", "LeftShoulder" },
			{ "Bip001 L Toe0", "LeftToes" },
			{ "Bip001 L UpperArm", "LeftUpperArm" },
			{ "Bip001 L Thigh", "LeftUpperLeg" },
			{ "Bip001 Neck", "Neck" },
			{ "Bip001 R Finger12", "Right Index Distal" },
			{ "Bip001 R Finger11", "Right Index Intermediate" },
			{ "Bip001 R Finger1", "Right Index Proximal" },
			{ "Bip001 R Finger42", "Right Little Distal" },
			{ "Bip001 R Finger41", "Right Little Intermediate" },
			{ "Bip001 R Finger4", "Right Little Proximal" },
			{ "Bip001 R Finger22", "Right Middle Distal" },
			{ "Bip001 R Finger21", "Right Middle Intermediate" },
			{ "Bip001 R Finger2", "Right Middle Proximal" },
			{ "Bip001 R Finger32", "Right Ring Distal" },
			{ "Bip001 R Finger31", "Right Ring Intermediate" },
			{ "Bip001 R Finger3", "Right Ring Proximal" },
			{ "Bip001 R Finger02", "Right Thumb Distal" },
			{ "Bip001 R Finger01", "Right Thumb Intermediate" },
			{ "Bip001 R Finger0", "Right Thumb Proximal" },
			{ "Bip001 R Foot", "RightFoot" },
			{ "Bip001 R Hand", "RightHand" },
			{ "Bip001 R Forearm", "RightLowerArm" },
			{ "Bip001 R Calf", "RightLowerLeg" },
			{ "Bip001 R Clavicle", "RightShoulder" },
			{ "Bip001 R Toe0", "RightToes" },
			{ "Bip001 R UpperArm", "RightUpperArm" },
			{ "Bip001 R Thigh", "RightUpperLeg" },
			{ "Bip001 Spine", "Spine" },
			{ "Bip001 Spine2", "UpperChest" }
		};

		public static Avatar MakeAvatar(GameObject root)
		{
			var desc = CreateDescription(root);
			Avatar avatar = AvatarBuilder.BuildHumanAvatar(root, desc);
			avatar.name = root.name;
			return avatar;
		}

		[MenuItem("MYTY Kit/Make Humanoid Avatar")]
		static void MakeAvatarMenu()
		{
			GameObject activeGameObject = Selection.activeGameObject;
			
			if (activeGameObject != null)
			{
				var avatar = MakeAvatar(activeGameObject);
				Debug.Log(avatar.isHuman ? "is human" : "is generic");

				var path = string.Format("Assets/{0}.ht", avatar.name.Replace(':', '_'));
				AssetDatabase.CreateAsset(avatar, path);

			}
		}

		static HumanDescription CreateDescription(GameObject avatarRoot)
		{
			HumanDescription description = new HumanDescription()
			{
				armStretch = 0.05f,
				feetSpacing = 0f,
				hasTranslationDoF = false,
				legStretch = 0.05f,
				lowerArmTwist = 0.5f,
				lowerLegTwist = 0.5f,
				upperArmTwist = 0.5f,
				upperLegTwist = 0.5f,
				skeleton = CreateSkeleton(avatarRoot),
				human = CreateHuman(avatarRoot),
			};
			return description;
		}

		static SkeletonBone[] CreateSkeleton(GameObject avatarRoot)
		{
			List<SkeletonBone> skeleton = new List<SkeletonBone>();

			Transform[] avatarTransforms = avatarRoot.GetComponentsInChildren<Transform>();

			foreach (Transform avatarTransform in avatarTransforms)
			{

				SkeletonBone bone = new SkeletonBone()
				{
					name = avatarTransform.name,
					position = avatarTransform.localPosition,
					rotation = avatarTransform.localRotation,
					scale = avatarTransform.localScale
				};

				skeleton.Add(bone);
			}

			return skeleton.ToArray();
		}

		static HumanBone[] CreateHuman(GameObject avatarRoot)
		{
			List<HumanBone> human = new List<HumanBone>();

			Transform[] avatarTransforms = avatarRoot.GetComponentsInChildren<Transform>();
			foreach (Transform avatarTransform in avatarTransforms)
			{
				if (XOCIETYBones.TryGetValue(avatarTransform.name, out string humanName))
				{
					HumanBone bone = new HumanBone
					{
						boneName = avatarTransform.name,
						humanName = humanName,
						limit = new HumanLimit()
					};
					bone.limit.useDefaultValues = true;

					human.Add(bone);
				}
			}

			return human.ToArray();
		}


	}
}