using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
    [CustomEditor(typeof(HumanoidAvatarBuilder))]
    public class HumanoidAvatarBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var builder = (HumanoidAvatarBuilder)target;
            if (GUILayout.Button("Auto body"))
            {
                builder.AutoBody();
                serializedObject.FindProperty("neck").objectReferenceValue = builder.neck;
                serializedObject.FindProperty("upperChest").objectReferenceValue = builder.upperChest;
                serializedObject.FindProperty("chest").objectReferenceValue = builder.chest;
                serializedObject.FindProperty("leftUpperArm").objectReferenceValue = builder.leftUpperArm;
                serializedObject.FindProperty("leftLowerArm").objectReferenceValue = builder.leftLowerArm;
                serializedObject.FindProperty("leftHand").objectReferenceValue = builder.leftHand;
                serializedObject.FindProperty("rightUpperArm").objectReferenceValue = builder.rightUpperArm;
                serializedObject.FindProperty("rightLowerArm").objectReferenceValue = builder.rightLowerArm;
                serializedObject.FindProperty("rightHand").objectReferenceValue = builder.rightHand;
                serializedObject.FindProperty("leftLowerLeg").objectReferenceValue = builder.leftLowerLeg;
                serializedObject.FindProperty("leftFoot").objectReferenceValue = builder.leftFoot;
                serializedObject.FindProperty("leftToe").objectReferenceValue = builder.leftToe;
                serializedObject.FindProperty("rightLowerLeg").objectReferenceValue = builder.rightLowerLeg;
                serializedObject.FindProperty("rightFoot").objectReferenceValue = builder.rightFoot;
                serializedObject.FindProperty("rightToe").objectReferenceValue = builder.rightToe;

                serializedObject.ApplyModifiedProperties();

                EditorUtility.SetDirty(builder);
            }

            if (GUILayout.Button("T Pose"))
            {
                builder.TPose();
            }

            if (GUILayout.Button("Build AvatarAsset"))
            {
                var avatar = builder.BuildAvatar();
                var path = string.Format("Assets/{0}.ht", avatar.name.Replace(':', '_'));
                AssetDatabase.CreateAsset(avatar, path);
            }
        }
    }
}