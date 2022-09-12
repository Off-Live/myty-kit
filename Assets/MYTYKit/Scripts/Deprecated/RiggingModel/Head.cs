using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : JointModel
{
    private void Update()
    {
        if (rawPoints == null) return;
        _rotation = Quaternion.LookRotation(_lookAt, _up);
        _translation = new Vector3(0, 0, 0);
    }

    private void LateUpdate()
    {
        if (rawPoints == null) return;

        var faceLR = rawPoints[263] - rawPoints[33]; //33 263 are end point of eyes
        _up = rawPoints[151] - rawPoints[200];
        faceLR.Normalize();
        _up.Normalize();
        _lookAt = -Vector3.Cross(_up.normalized, faceLR.normalized);
        _up = -Vector3.Cross(faceLR, _lookAt);
    }
}


