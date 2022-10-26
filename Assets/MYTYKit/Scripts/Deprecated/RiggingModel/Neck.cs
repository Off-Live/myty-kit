using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neck : JointModel
{
    // Update is called once per frame
    void Update()
    {
        if (rawPoints == null) return;
        _rotation = Quaternion.LookRotation(_lookAt, _up);
        _translation = new Vector3();
    }

    private void LateUpdate()
    {
        if (rawPoints == null) return;
        var shoulderLR = rawPoints[11] - rawPoints[12];
        var shoulderHalf = (rawPoints[11] + rawPoints[12]) / 2;
        var headCenter = (rawPoints[7] + rawPoints[8]) / 2;
        _up = headCenter - shoulderHalf;
        _up.Normalize();
        shoulderLR.Normalize();
        _lookAt = Vector3.Cross(shoulderLR, _up);

    }
}
