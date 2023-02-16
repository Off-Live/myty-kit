using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MYTYKit
{
	public class HumanoidAvatarBuilder : MonoBehaviour
	{
		public static readonly string[] FINGER_NAME = new string[]
		{
			"Thumb Proximal", "Thumb Intermediate","Thumb Distal",
			"Index Proximal", "Index Intermediate","Index Distal",
			"Middle Proximal", "Middle Intermediate","Middle Distal",
			"Ring Proximal", "Ring Intermediate","Ring Distal",
			"Little Proximal", "Little Intermediate","Little Distal"
		};

		//
		// { "Bip001 L Finger42", "Left Little Distal" },
		// { "Bip001 L Finger41", "Left Little Intermediate" },
		// { "Bip001 L Finger4", "Left Little Proximal" },
		// { "Bip001 L Finger22", "Left Middle Distal" },
		// { "Bip001 L Finger21", "Left Middle Intermediate" },
		// { "Bip001 L Finger2", "Left Middle Proximal" },
		// { "Bip001 L Finger32", "Left Ring Distal" },
		// { "Bip001 L Finger31", "Left Ring Intermediate" },
		// { "Bip001 L Finger3", "Left Ring Proximal" },
		// { "Bip001 L Finger02", "Left Thumb Distal" },
		// { "Bip001 L Finger01", "Left Thumb Intermediate" },
		// { "Bip001 L Finger0", "Left Thumb Proximal" },
		//

		public Transform avatarRoot;
		
		public Transform hips;
		public Transform spine;
		public Transform chest;
		public Transform upperChest;

		public Transform neck;
		public Transform head;
		
		public Transform leftShoulder;
		public Transform leftUpperArm;
		public Transform leftLowerArm;
		public Transform leftHand;
		
		public Transform rightShoulder;
		public Transform rightUpperArm;
		public Transform rightLowerArm;
		public Transform rightHand;

		public Transform leftUpperLeg;
		public Transform leftLowerLeg;
		public Transform leftFoot;
		public Transform leftToe;

		public Transform rightUpperLeg;
		public Transform rightLowerLeg;
		public Transform rightFoot;
		public Transform rightToe;

		public Transform[] leftFingers;
		public Transform[] rightFingers;

		Mesh m_mesh;

		Transform[][] leftFingerArray;
		Transform[][] rightFingerArray;
		public Avatar BuildAvatar()
		{
			var desc = CreateDescription(avatarRoot.gameObject, BuildBoneMap());
			Avatar avatar = AvatarBuilder.BuildHumanAvatar(avatarRoot.gameObject, desc);
			avatar.name = avatarRoot.name;
			
			Debug.Log(avatar.isHuman ? "is human" : "is generic");
			return avatar;
		}
		
	
		public void AutoBody()
		{
			//Mandatory : hips, spine, head, upperarms, upperlegs
			if (hips == null || spine == null || head == null || leftShoulder == null || rightShoulder == null ||
			    leftUpperLeg == null || rightUpperLeg == null)
			{
				Debug.LogWarning("Please setup the mandatory bones (hips, spine, head, shoulders, upperlegs) to use auto body.");
				return;
			}
			

			neck = head.parent;
			upperChest = neck.parent;
			chest = upperChest.parent;

			leftUpperArm = leftShoulder.GetChild(0);
			leftLowerArm = leftUpperArm.GetChild(0);
			leftHand = leftLowerArm.GetChild(0);

			rightUpperArm = rightShoulder.GetChild(0);
			rightLowerArm = rightUpperArm.GetChild(0);
			rightHand = rightLowerArm.GetChild(0);
			
			leftLowerLeg = leftUpperLeg.GetChild(0);
			leftFoot = leftLowerLeg.GetChild(0);
			if (leftFoot.childCount > 0) leftToe = leftFoot.GetChild(0);

			rightLowerLeg = rightUpperLeg.GetChild(0);
			rightFoot = rightLowerLeg.GetChild(0);
			if (rightFoot.childCount > 0) rightToe = rightFoot.GetChild(0);
		
		}

		public void TPose() //Only Arms.
		{
			var leftChain = new Transform[] { leftShoulder, leftUpperArm, leftLowerArm, leftHand };
			var rightChain = new Transform[] { rightShoulder, rightUpperArm, rightLowerArm, rightHand };
			FlattenChainInDirection(leftChain, Vector3.left);
			FlattenChainInDirection(rightChain, Vector3.right);

			if (CheckValidityOfFingers(leftFingers, leftHand))
			{
				BuildFingers(leftFingers, ref leftFingerArray);
				Enumerable.Range(1, 4).ToList().ForEach(idx =>
				{
					FlattenChainInDirection(leftFingerArray[idx],Vector3.left);
				});
			}
			if (CheckValidityOfFingers(rightFingers, rightHand))
			{
				BuildFingers(rightFingers, ref rightFingerArray);
				Enumerable.Range(1, 4).ToList().ForEach(idx =>
				{
					FlattenChainInDirection(rightFingerArray[idx],Vector3.right);
				});
			}
			
		}

		void FlattenChainInDirection(Transform[] chain, Vector3 alignDirection)
		{
			Enumerable.Range(0, chain.Length-1).Select(idx => (chain[idx], chain[idx + 1])).ToList().ForEach(
				pair =>
				{
					var parent = pair.Item1;
					var child = pair.Item2;

					var direction = (child.position - parent.position).normalized;
					var rot = Quaternion.FromToRotation(direction, alignDirection);
					parent.rotation = rot * parent.rotation;
				});
		}

		bool CheckValidityOfFingers(Transform[] fingers, Transform hand)
		{
			if (fingers == null) return false;
			if (fingers.Length != 5) return false;

			var nodeCount = fingers.Select(fingerTip =>
			{
				var cnt = 0;
				do
				{
					if (fingerTip == null) break;
					fingerTip = fingerTip.parent;
					
				} while (fingerTip != hand && ++cnt<2);
				return cnt;
			}).ToList();

			return nodeCount.Count(count => count != 2) == 0;
		}

		void BuildFingers(Transform[] fingers,ref Transform[][] fingerArray)
		{
			fingerArray = new Transform[5][];
			var array = fingerArray;
			Enumerable.Range(0,5).ToList().ForEach(idx =>
			{
				array[idx] = new Transform[]
				{
					fingers[idx].parent.parent,
					fingers[idx].parent,
					fingers[idx]
				};
			});
		}

		Dictionary<string, string> BuildBoneMap()
		{
			var reverseDict = new Dictionary<string, Transform>();
			reverseDict["Hips"] = hips;
			reverseDict["Spine"] = spine;
			reverseDict["Chest"] = chest;
			reverseDict["UpperChest"] = upperChest;

			reverseDict["Head"] = head;
			reverseDict["Neck"] = neck;

			reverseDict["LeftShoulder"] = leftShoulder;
			reverseDict["LeftUpperArm"] = leftUpperArm;
			reverseDict["LeftLowerArm"] = leftLowerArm;
			reverseDict["LeftHand"] = leftHand;

			reverseDict["RightShoulder"] = rightShoulder;
			reverseDict["RightUpperArm"] = rightUpperArm;
			reverseDict["RightLowerArm"] = rightLowerArm;
			reverseDict["RightHand"] = rightHand;

			reverseDict["LeftUpperLeg"] = leftUpperLeg;
			reverseDict["LeftLowerLeg"] = leftLowerLeg;
			reverseDict["LeftFoot"] = leftFoot;
			reverseDict["LeftToes"] = leftToe;

			reverseDict["RightUpperLeg"] = rightUpperLeg;
			reverseDict["RightLowerLeg"] = rightLowerLeg;
			reverseDict["RightFoot"] = rightFoot;
			reverseDict["RightToes"] = rightToe;
			
			
			if (CheckValidityOfFingers(leftFingers, leftHand))
			{
				BuildFingers(leftFingers, ref leftFingerArray);
				Enumerable.Range(0,15).ToList().ForEach(idx =>
				{
					reverseDict["Left " + FINGER_NAME[idx]] = leftFingerArray[idx / 3][idx % 3];
				});
			}
			if (CheckValidityOfFingers(rightFingers, rightHand))
			{
				BuildFingers(rightFingers, ref rightFingerArray);
				Enumerable.Range(0,15).ToList().ForEach(idx =>
				{
					reverseDict["Right " + FINGER_NAME[idx]] = rightFingerArray[idx / 3][idx % 3];
				});
			}
			
			return reverseDict.Where(pair => pair.Value != null).ToDictionary(pair => pair.Value.name, pair => pair.Key);
		}


		static HumanDescription CreateDescription(GameObject avatarRoot, Dictionary<string,string > boneMap)
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
				human = CreateHuman(avatarRoot, boneMap),
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

		static HumanBone[] CreateHuman(GameObject avatarRoot, Dictionary<string, string> boneMap)
		{
			List<HumanBone> human = new List<HumanBone>();

			Transform[] avatarTransforms = avatarRoot.GetComponentsInChildren<Transform>();
			foreach (Transform avatarTransform in avatarTransforms)
			{
				if (boneMap.TryGetValue(avatarTransform.name, out string humanName))
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

		public void InitMesh()
		{
			m_mesh = new Mesh();
			m_mesh.vertices = new Vector3[]
			{
				new Vector3(-0.5f, -Mathf.Sqrt(3) / 6, 0),
				new Vector3(0.5f, -Mathf.Sqrt(3) / 6, 0),
				new Vector3(0, Mathf.Sqrt(3) / 3, 0),
				new Vector3(0, 0 , 10)
			};

			m_mesh.triangles = new int[]
			{
				0, 2, 1,
				0, 1, 3,
				2,0,3,
				1,2,3,
			};

			m_mesh.RecalculateNormals();
		}
		void OnDrawGizmos()
		{
			if (m_mesh==null) InitMesh();
			if (hips == null) return;
			
			DrawBones(hips);
		}
		
		void DrawBones(Transform parent)
		{
			var position = parent.position;

			var children = Enumerable.Range(0, parent.childCount).Select(idx => parent.GetChild(idx)).ToList();
			children.ForEach(child =>
			{
				var diff = child.position - position;
				var dist = diff.magnitude;
				if (dist > 0.001f && parent.gameObject.activeInHierarchy)
				{
					var scale = new Vector3(dist / 10, dist / 10, dist / 10);
					Gizmos.color = Color.green;
					Gizmos.DrawMesh(m_mesh, position, Quaternion.LookRotation(diff), scale);
				}

				DrawBones(child);
			});


		}
	}
}