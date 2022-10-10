using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplates
{
    public abstract class MotionTemplateBridge : MonoBehaviour
    {
        protected List<MotionTemplate> templateList = new();

        public void AddMotionTemplate(MotionTemplate template)
        {
            templateList.Add(template);
        }

        public void ClearMotionTemplate()
        {
            templateList.Clear();
        }

        public abstract void UpdateTemplate();

    }
}
