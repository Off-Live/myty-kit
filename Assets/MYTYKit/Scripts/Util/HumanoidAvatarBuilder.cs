using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit
{
	[DisallowMultipleComponent]
	public class HumanoidAvatarBuilder : MonoBehaviour
	{
		public static readonly string[] FingerName = {
			"Thumb Proximal", "Thumb Intermediate","Thumb Distal",
			"Index Proximal", "Index Intermediate","Index Distal",
			"Middle Proximal", "Middle Intermediate","Middle Distal",
			"Ring Proximal", "Ring Intermediate","Ring Distal",
			"Little Proximal", "Little Intermediate","Little Distal"
		};

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

		public Avatar avatar;
		
		Mesh m_mesh;

		Transform[][] m_leftFingerArray;
		Transform[][] m_rightFingerArray;

		Dictionary<Transform, Quaternion> m_poseCache;
		public Avatar BuildAvatar()
		{
			var desc = CreateDescription(avatarRoot.gameObject, BuildBoneMap());
			avatar = AvatarBuilder.BuildHumanAvatar(avatarRoot.gameObject, desc);
			avatar.name = avatarRoot.name;
			
			Debug.Log(avatar.isHuman ? "is human" : "is generic");
			return avatar;
		}

		public static Avatar CreateAvatarFromJson(GameObject root, JObject jObj)
		{
			var human = jObj["humanBones"].ToList()
				.Select(token =>
				{
					var humanBone = new HumanBone()
					{
						boneName = (string)token["boneName"],
						humanName = (string)token["humanName"],
						limit = new HumanLimit()
					};
					humanBone.limit.useDefaultValues = true;
					return humanBone;
				}).ToArray();
			var skeleton = jObj["skeletons"].ToObject<List<SkeletonBone>>().ToArray();
			var desc = CreateDescription(skeleton, human);
			var avatar = AvatarBuilder.BuildHumanAvatar(root, desc);
			avatar.name = root.name;
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
			
			AutoSetupFingers();
		}
		

		public void TPose() //Only Arms.
		{
			var leftChain = new[] { leftShoulder, leftUpperArm, leftLowerArm, leftHand };
			var rightChain = new[] { rightShoulder, rightUpperArm, rightLowerArm, rightHand };
			var leftDirection = (leftShoulder.position - rightShoulder.position).normalized;
			if (Vector3.Dot(leftDirection, Vector3.left) < 0)
			{
				hips.transform.rotation = Quaternion.AngleAxis(180, Vector3.up) * hips.transform.rotation;
			}
			
			FlattenChainInDirection(leftChain, Vector3.left);
			FlattenChainInDirection(rightChain, Vector3.right);

			if (CheckValidityOfFingers(leftFingers, leftHand))
			{
				m_leftFingerArray = BuildFingers(leftFingers);
				Enumerable.Range(1, 4).ToList().ForEach(idx =>
				{
					FlattenChainInDirection(m_leftFingerArray[idx],Vector3.left);
				});
			}
			if (CheckValidityOfFingers(rightFingers, rightHand))
			{
				m_rightFingerArray = BuildFingers(rightFingers);
				Enumerable.Range(1, 4).ToList().ForEach(idx =>
				{
					FlattenChainInDirection(m_rightFingerArray[idx],Vector3.right);
				});
			}
			
		}

		public bool IsCachedPose()
		{
			return m_poseCache != null;
		}
		public void SavePose()
		{
			if (avatarRoot == null)
			{
				Debug.LogWarning("No avatarRoot set");
				return;
			}
			m_poseCache = avatarRoot.GetComponentsInChildren<Transform>().ToDictionary(tf => tf, tf => tf.rotation);
		}

		public void RestorePose()
		{
			if (m_poseCache == null)
			{
				Debug.LogWarning("No saved pose");
				return;
			}
			avatarRoot.GetComponentsInChildren<Transform>().Where(tf => m_poseCache.ContainsKey(tf)).ToList().ForEach(tf => tf.rotation = m_poseCache[tf]);
		}

		void AutoSetupFingers()
		{
			
			var leftFingersTmp = new List<Transform>();
			var rightFingersTmp = new List<Transform>();
			FindLeafNode(leftShoulder, ref leftFingersTmp);
			FindLeafNode(rightShoulder, ref rightFingersTmp);

			leftFingersTmp = leftFingersTmp.Where(fingerTip => CountOfFingerNode(fingerTip,leftHand) == 2).ToList();
			rightFingersTmp = rightFingersTmp.Where(fingerTip => CountOfFingerNode(fingerTip, rightHand) == 2).ToList();
			
			if (CheckValidityOfFingers(leftFingersTmp.ToArray(), leftHand) &&
			    CheckValidityOfFingers(rightFingersTmp.ToArray(), rightHand))
			{
				leftFingers = leftFingersTmp.ToArray();
				rightFingers = rightFingersTmp.ToArray();
			}

		}

		void FindLeafNode(Transform root, ref List<Transform> leafList)
		{
			var list = leafList;
			Enumerable.Range(0, root.childCount).Select(idx => root.GetChild(idx)).ToList().ForEach(child =>
			{
				if(child.childCount==0) list.Add(child);
				FindLeafNode(child,ref list);
			});
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

			var nodeCount = fingers.Select(fingerTip => CountOfFingerNode(fingerTip,hand)).ToList();

			return nodeCount.Count(count => count != 2) == 0;
		}

		int CountOfFingerNode(Transform fingerTip, Transform hand)
		{
			var cnt = 0;
			do
			{
				if (fingerTip == null) break;
				fingerTip = fingerTip.parent;
					
			} while (fingerTip != hand && ++cnt<2);
			return cnt;
		}

		static Transform[][] BuildFingers(Transform[] fingers)
		{
			var fingerArray = new Transform[5][];
			var array = fingerArray;
			Enumerable.Range(0,5).ToList().ForEach(idx =>
			{
				array[idx] = new[]
				{
					fingers[idx].parent.parent,
					fingers[idx].parent,
					fingers[idx]
				};
			});
			return fingerArray;
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
				m_leftFingerArray = BuildFingers(leftFingers);
				Enumerable.Range(0,15).ToList().ForEach(idx =>
				{
					reverseDict["Left " + FingerName[idx]] = m_leftFingerArray[idx / 3][idx % 3];
				});
			}
			if (CheckValidityOfFingers(rightFingers, rightHand))
			{
				m_rightFingerArray = BuildFingers(rightFingers);
				Enumerable.Range(0,15).ToList().ForEach(idx =>
				{
					reverseDict["Right " + FingerName[idx]] = m_rightFingerArray[idx / 3][idx % 3];
				});
			}
			
			return reverseDict.Where(pair => pair.Value != null).ToDictionary(pair => pair.Value.name, pair => pair.Key);
		}


		static HumanDescription CreateDescription(GameObject avatarRoot, Dictionary<string,string > boneMap)
		{
			return CreateDescription(CreateSkeleton(avatarRoot), CreateHuman(avatarRoot, boneMap));
		}

		static HumanDescription CreateDescription(SkeletonBone[] skeleton, HumanBone[] human)
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
				skeleton = skeleton,
				human = human,
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

		void InitMesh()
		{
			m_mesh = new Mesh();
			m_mesh.vertices = new[]
			{
				new Vector3(-0.5f, -Mathf.Sqrt(3) / 6, 0),
				new Vector3(0.5f, -Mathf.Sqrt(3) / 6, 0),
				new Vector3(0, Mathf.Sqrt(3) / 3, 0),
				new Vector3(0, 0 , 10)
			};

			m_mesh.triangles = new[]
			{
				0, 2, 1,
				0, 1, 3,
				2, 0, 3,
				1, 2, 3,
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

		public JObject ExportToJObject()
		{
			var humanBones = avatar.humanDescription.human.Select(humanBone => JObject.FromObject(new
			{
				humanBone.boneName,
				humanBone.humanName
			})).ToArray();

			var skeletons = avatar.humanDescription.skeleton.Select(skeleton=> JObject.FromObject(new
			{
				skeleton.name,
				position = JObject.FromObject(new
				{
					skeleton.position.x,skeleton.position.y, skeleton.position.z
				}),
				rotation = JObject.FromObject(new
				{
					skeleton.rotation.x, skeleton.rotation.y, skeleton.rotation.z, skeleton.rotation.w	
				}),
				scale = JObject.FromObject(new
				{
					skeleton.scale.x, skeleton.scale.y, skeleton.scale.z
				})
			}));
			return JObject.FromObject(new
			{
				humanBones,skeletons
			});
		}
	}
}