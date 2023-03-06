using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MYTYKit.MotionTemplates
{
    [Serializable]
    public class MTItem
    {
        public string name;
        public MotionTemplate template;
    }
    [DisallowMultipleComponent]
    public class MotionTemplateMapper : MonoBehaviour
    {
        [SerializeField] List<MTItem> templates;

        public MotionTemplate GetTemplate(string name)
        {
            foreach (var item in templates)
            {
                if (item.name == name)
                {
                    return item.template;
                }
            }

            return null;
        }

        public void SetTemplate(string name, MotionTemplate templateObj)
        {
            foreach (var item in templates)
            {
                if (item.name == name)
                {
                    item.template = templateObj;
                    return;
                }
            }

            templates.Add(new MTItem()
            {
                name = name,
                template = templateObj
            });
        }

        public List<string> GetNames()
        {
            List<string> ret = new();
            foreach (var item in templates)
            {
                ret.Add(item.name);
            }

            return ret;
        }

        public string GetName(MotionTemplate template)
        {
            var item = templates.FirstOrDefault(item => item.template == template);
            return (item == null) ? "" : item.name;
        }

        public void Clear()
        {
            templates.Clear();
        }

    }
}