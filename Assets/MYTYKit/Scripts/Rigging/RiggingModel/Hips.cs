using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hips : JointModel
{
    // Update is called once per frame
    void Update()
    {
        if (rawPoints == null) return;
        _rotation = Quaternion.LookRotation(_lookAt, _up);
        _translation = new Vector3(0, 0, 0);
    }

    private void LateUpdate()
    {
        if (rawPoints == null) return;
        var hipLR = rawPoints[23] - rawPoints[24];
        _up = Vector3.up;
        hipLR.Normalize();
        _lookAt = Vector3.Cross(hipLR, _up);
        _up = Vector3.Cross(_lookAt, hipLR);
    }
}
