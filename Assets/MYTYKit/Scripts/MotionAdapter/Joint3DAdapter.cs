using System;
using MYTYKit.Controllers;
using MYTYKit.MotionTemplates;
using UnityEngine;

namespace MYTYKit.MotionAdapters
{
    public class Joint3DAdapter : NativeAdapter , ITemplateObserver

    {
        public AnchorTemplate anchor;
        public ByPassController target;

        void Start()
        {
            ListenToMotionTemplate();
        }

        public void TemplateUpdated()
        {
            if (anchor == null || target == null) return;
            target.position = anchor.position;
            target.scale = anchor.scale;
            target.rotation = Quaternion.LookRotation(anchor.lookAt, anchor.up);
        }

        public void ListenToMotionTemplate()
        {
            anchor.SetUpdateCallback(TemplateUpdated);
        }
    }
}