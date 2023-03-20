using System;
using MYTYKit.Controllers;
using MYTYKit.MotionTemplates;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.MotionAdapters
{
    public class DirectJointAdapter : NativeAdapter , ITemplateObserver

    {
        public AnchorTemplate anchor;
        public Transform target;

        void Start()
        {
            ListenToMotionTemplate();
        }

        public void TemplateUpdated()
        {
            if (anchor == null || target == null) return;
            target.position = anchor.position;
            target.localScale = anchor.scale;
            target.rotation = Quaternion.LookRotation(anchor.lookAt, anchor.up);
        }

        public void ListenToMotionTemplate()
        {
            anchor.SetUpdateCallback(TemplateUpdated);
        }
        
    }
}