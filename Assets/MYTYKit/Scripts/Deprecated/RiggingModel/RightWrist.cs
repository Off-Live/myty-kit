using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightWrist : JointModel
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
        var pinkey = rawPoints[0] - rawPoints[17];
        _lookAt = rawPoints[17] - rawPoints[5];

        _up = -Vector3.Cross(pinkey, _lookAt);
        _lookAt.Normalize();
        _up.Normalize();

        //_lookAt = Vector3.forward;
        //_up = Vector3.up;

    }

}
