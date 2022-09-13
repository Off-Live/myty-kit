using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

public class JointVec3ToVec2Adapter : NativeAdapter
{
    public JointModel joint;
    public JointVector from;
    public ProjectionPlane plane;
    public bool flip = false;
    public MYTYController controller;

    public float stabilizeTime = 0.1f;

    public bool negate1st = false;
    public bool negate2nd = false;

    public float min1st = -1.0f;
    public float max1st = 1.0f;

    public float min2nd = -1.0f;
    public float max2nd = 1.0f;



    private float m_elapsed = 0;
    

    private void Update()
    {
        if (joint == null) return;
        var input = controller as IVec2Input;
        if (input == null) return;
        Vector3 vec3 = Vector3.zero;

        m_elapsed += Time.deltaTime;
        if (m_elapsed < stabilizeTime)
        {
            input.SetInput(GetStabilizedVec2());
            return;
        }

        m_elapsed = 0;

        var scale1st = negate1st? -1 : 1;
        var scale2nd = negate2nd ? -1 : 1;

        switch (from)
        {
            case JointVector.LookAt:
                vec3 = joint.lookAt;
                break;
            case JointVector.Up:
                vec3 = joint.upVector;
                break;
        }

        var inputVal = Vector2.zero;

        switch (plane)
        {
            case ProjectionPlane.XY:
                if (flip) inputVal = new Vector2(vec3.y, vec3.x);
                else inputVal = new Vector2(vec3.x, vec3.y);
                break;
            case ProjectionPlane.YZ:
                if (flip) inputVal = new Vector2(vec3.z, vec3.y);
                else inputVal = new Vector2(vec3.y, vec3.z);
                break;
            case ProjectionPlane.XZ:
                if (flip) inputVal = new Vector2(vec3.z, vec3.x);
                else inputVal = new Vector2(vec3.x, vec3.z);
                break;
        }

        var x = Mathf.Clamp(inputVal.x * scale1st, min1st, max1st);
        var y = Mathf.Clamp(inputVal.y * scale2nd, min2nd, max2nd);

        inputVal = new Vector2(x,y);
        Stabilize(inputVal);
        input.SetInput(GetStabilizedVec2());
    }
}
