using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class JointVec3ToVec1Adapter : NativeAdapter
{
    public JointModel joint;
    public JointVector from;
    public ComponentIndex component;
    public bool negate = false;
    public MYTYController controller;
    public float stabilizeTime = 0.1f;
    private float m_elapsed = 0;

    private void Update()
    {
        if (joint == null) return;
        var input = controller as IFloatInput;
        if (input == null) return;
        Vector3 vec3 = Vector3.zero;
        var scale = negate ? -1 : 1;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedFloat());
            return;
        }

        m_elapsed = 0;

        switch (from)
        {
            case JointVector.LookAt:
                vec3 = joint.lookAt;
                break;
            case JointVector.Up:
                vec3 = joint.upVector;
                break;
        }

        float val = 0;

        switch (component)
        {
            case ComponentIndex.X:
                val = scale * vec3.x;
                break;
            case ComponentIndex.Y:
                val = scale * vec3.y;
                break;
            case ComponentIndex.Z:
                val = scale * vec3.z;
                break;
        }

        Stabilize(val);
        input.SetInput(GetStabilizedFloat());
    }
}
