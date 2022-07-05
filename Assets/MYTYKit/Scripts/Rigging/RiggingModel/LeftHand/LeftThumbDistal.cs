using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftThumbDistal : JointModel
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

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

        var proximal = rawPoints[2] - rawPoints[1];
        var indexprox = rawPoints[5] - rawPoints[1];
        var thumbPlane = Vector3.Cross(proximal, indexprox);
        var distal = rawPoints[4] - rawPoints[3];

        thumbPlane.Normalize();
        indexprox.Normalize();
        proximal.Normalize();
        distal.Normalize();

        _up = distal;
        _lookAt = Vector3.Cross(thumbPlane, proximal); ;


    }
}
