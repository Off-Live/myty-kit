using UnityEditor;

namespace MYTYKit
{
    [CustomEditor(typeof(RiggingObject))]
    public class RiggingObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var pc = (RiggingObject)target;

        }
    }
}
